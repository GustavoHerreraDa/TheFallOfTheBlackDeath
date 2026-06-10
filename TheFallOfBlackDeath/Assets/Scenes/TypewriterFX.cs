// ============================================================
//  TypewriterFX.cs  —  Efecto typewriter sci-fi para TextMeshPro
//
//  Características:
//    · Revela el texto caracter a caracter a velocidad configurable
//    · Cursor parpadeante estilo terminal al final del texto visible
//    · Pausa automática en signos de puntuación (coma, punto, etc.)
//    · Sonido de "tick" por caracter con variación de pitch aleatoria
//    · Evento OnComplete para encadenar lógica de juego
//    · Soporte para tags rich text de TMP (<color>, <b>, etc.)
//    · Métodos públicos: Play(), Skip(), Pause(), Resume()
//
//  Uso:
//    1. Añade este script al GameObject que tenga el componente TMP_Text.
//    2. Asigna el AudioSource en el Inspector (o se crea automáticamente).
//    3. Asigna un AudioClip corto (tick, clic, bip) en _tickSound.
//    4. Llama a Play("Texto a mostrar") desde otro script o desde el Inspector.
//
//  Requiere: TextMeshPro (com.unity.textmeshpro)
// ============================================================

using System;
using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TypewriterFX : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────

    [Header("Velocidad")]
    [Tooltip("Segundos entre cada caracter. 0.04 = rápido, 0.08 = legible, 0.15 = dramático")]
    [SerializeField] private float _charInterval = 0.05f;

    [Header("Cursor")]
    [SerializeField] private bool   _showCursor       = true;
    [Tooltip("Caracter que actúa como cursor al final del texto visible")]
    [SerializeField] private string _cursorChar       = "_";
    [Tooltip("Segundos entre cada parpadeo del cursor")]
    [SerializeField] private float  _cursorBlinkRate  = 0.5f;
    [Tooltip("Color del cursor en formato TMP rich text (sin los tags)")]
    [SerializeField] private string _cursorColor      = "#00FFCC";

    [Header("Pausas en puntuación")]
    [SerializeField] private bool  _pauseOnPunctuation = true;
    [Tooltip("Multiplicador de _charInterval al encontrar una coma")]
    [SerializeField] private float _comaPauseMult      = 3f;
    [Tooltip("Multiplicador de _charInterval al encontrar un punto o '!'/'?'")]
    [SerializeField] private float _periodPauseMult    = 6f;

    [Header("Sonido")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip   _tickSound;
    [Tooltip("Rango de variación aleatoria del pitch del tick (0 = siempre igual)")]
    [SerializeField] private float       _pitchVariance = 0.15f;
    [Tooltip("Reproducir tick solo cada N caracteres para no saturar el audio")]
    [SerializeField] private int         _tickEveryN    = 1;

    [Header("Comportamiento")]
    [Tooltip("Si está activo, al hacer Play() se interrumpe cualquier typewriter en curso")]
    [SerializeField] private bool _interruptOnPlay = true;
    [Tooltip("Al completarse, el cursor desaparece automáticamente después de N segundos. -1 = nunca")]
    [SerializeField] private float _hideCursorAfter = 2f;

    // ── Eventos ────────────────────────────────────────────────────────────

    /// <summary>Se dispara cuando el texto termina de revelarse por completo.</summary>
    public event Action OnComplete;

    /// <summary>Se dispara en cada caracter revelado. Pasa el índice del char.</summary>
    public event Action<int> OnCharRevealed;

    // ── Estado interno ─────────────────────────────────────────────────────

    private TMP_Text    _label;
    private string      _targetText    = "";
    private int         _visibleCount  = 0;
    private bool        _isPlaying     = false;
    private bool        _isPaused      = false;
    private bool        _cursorVisible = false;
    private int         _charsSinceLastTick = 0;

    private Coroutine   _typeCoroutine;
    private Coroutine   _cursorCoroutine;
    private Coroutine   _hideCursorCoroutine;

    // ── Propiedades públicas ───────────────────────────────────────────────

    /// <summary>True si el typewriter todavía está revelando caracteres.</summary>
    public bool IsPlaying => _isPlaying;

    /// <summary>True si está pausado a mitad de revelación.</summary>
    public bool IsPaused  => _isPaused;

    /// <summary>Progreso de revelación entre 0 y 1.</summary>
    public float Progress => _targetText.Length == 0 ? 1f
                           : (float)_visibleCount / _targetText.Length;

    // ── Inicialización ─────────────────────────────────────────────────────

    private void Awake()
    {
        _label = GetComponent<TMP_Text>();

        // Crear AudioSource automáticamente si no se asignó uno
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f; // 2D — UI no necesita 3D audio
    }
    
    private void Start()
    {
        // Si hay texto escrito en el componente, lo anima automáticamente al iniciar
        if (!string.IsNullOrEmpty(_label.text))
        {
            Play();
        }
    }

    // ── API Pública ────────────────────────────────────────────────────────

    /// <summary>
    /// Inicia el efecto typewriter con el texto dado.
    /// Si ya hay uno en curso y _interruptOnPlay está activo, lo cancela.
    /// </summary>
    public void Play(string text)
    {
        if (_isPlaying && !_interruptOnPlay) return;

        StopAllCoroutinesInternal();

        _targetText   = text;
        _visibleCount = 0;
        _isPlaying    = true;
        _isPaused     = false;

        // Mostramos el texto completo vacío para que TMP reserve el layout
        // maxVisibleCharacters controla cuántos caracteres renderiza TMP
        _label.text               = _targetText;
        _label.maxVisibleCharacters = 0;

        _typeCoroutine   = StartCoroutine(TypeRoutine());

        if (_showCursor)
            _cursorCoroutine = StartCoroutine(CursorBlinkRoutine());
    }

    /// <summary>
    /// Sobrecarga: reutiliza el texto que ya tiene el label (útil si
    /// el texto fue asignado desde el Inspector o desde otro sistema).
    /// </summary>
    public void Play()
    {
        Play(_label.text);
    }

    /// <summary>
    /// Revela todo el texto instantáneamente y dispara OnComplete.
    /// Útil para que el jugador pueda saltear el efecto con un click.
    /// </summary>
    public void Skip()
    {
        if (!_isPlaying) return;

        StopAllCoroutinesInternal();

        _visibleCount = _targetText.Length;
        _label.maxVisibleCharacters = _visibleCount;
        _isPlaying = false;
        _isPaused  = false;

        HideCursorImmediate();
        OnComplete?.Invoke();
    }

    /// <summary>Pausa el typewriter en el caracter actual.</summary>
    public void Pause()
    {
        if (_isPlaying && !_isPaused)
            _isPaused = true;
    }

    /// <summary>Reanuda un typewriter pausado.</summary>
    public void Resume()
    {
        if (_isPlaying && _isPaused)
            _isPaused = false;
    }

    /// <summary>Limpia el texto y detiene cualquier efecto en curso.</summary>
    public void Clear()
    {
        StopAllCoroutinesInternal();
        _label.text = "";
        _label.maxVisibleCharacters = 0;
        _targetText   = "";
        _visibleCount = 0;
        _isPlaying    = false;
        _isPaused     = false;
    }

    // ── Coroutines ─────────────────────────────────────────────────────────

    /// <summary>
    /// Coroutine principal: revela un caracter por iteración,
    /// aplicando pausa en signos de puntuación si está habilitado.
    /// </summary>
    private IEnumerator TypeRoutine()
    {
        // Usamos maxVisibleCharacters de TMP para revelar sin reconstruir el mesh
        // cada frame — mucho más eficiente que modificar el string.
        while (_visibleCount < _targetText.Length)
        {
            // Respetamos la pausa sin salir de la coroutine
            while (_isPaused)
                yield return null;

            _visibleCount++;
            _label.maxVisibleCharacters = _visibleCount;

            OnCharRevealed?.Invoke(_visibleCount - 1);

            // Sonido de tick (cada _tickEveryN caracteres)
            _charsSinceLastTick++;
            if (_charsSinceLastTick >= _tickEveryN)
            {
                PlayTick();
                _charsSinceLastTick = 0;
            }

            // Calcular el intervalo de espera para este caracter
            float waitTime = GetIntervalForChar(_visibleCount - 1);
            yield return new WaitForSeconds(waitTime);
        }

        // ── Texto completo ──────────────────────────────────────────────
        _isPlaying = false;
        OnComplete?.Invoke();

        // Esconder el cursor tras N segundos si está configurado
        if (_showCursor && _hideCursorAfter >= 0f)
            _hideCursorCoroutine = StartCoroutine(HideCursorAfterDelay(_hideCursorAfter));
    }

    /// <summary>
    /// Coroutine del cursor: activa/desactiva el caracter de cursor
    /// al final del texto visible usando un tag TMP inline.
    ///
    /// Estrategia: el texto real lo maneja maxVisibleCharacters,
    /// y el cursor es un sufijo que añadimos/quitamos del string.
    /// Para evitar reflow del layout, lo ponemos con alpha 0 al ocultarlo.
    /// </summary>
    private IEnumerator CursorBlinkRoutine()
    {
        // Tag de cursor con color configurable + alpha según visibilidad
        string CursorTag(bool visible)
        {
            string alpha = visible ? "FF" : "00";
            return $"<color={_cursorColor}{alpha}>{_cursorChar}</color>";
        }

        while (true)
        {
            _cursorVisible = !_cursorVisible;

            // Reconstruimos el texto con el cursor al final
            // Solo modificamos si el label aún está en uso
            if (_label != null)
            {
                _label.text = _targetText + CursorTag(_cursorVisible);
                // Mantenemos maxVisibleCharacters en sync:
                // el caracter del cursor está SIEMPRE visible (es el último char)
                // así que sumamos 1 al final del texto real para incluirlo
                if (!_isPlaying)
                    _label.maxVisibleCharacters = _targetText.Length + 1;
            }

            yield return new WaitForSeconds(_cursorBlinkRate);
        }
    }

    private IEnumerator HideCursorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideCursorImmediate();
    }

    // ── Helpers privados ───────────────────────────────────────────────────

    /// <summary>
    /// Devuelve el tiempo de espera correcto para el caracter en el índice dado.
    /// Los signos de puntuación aplican un multiplicador para dar ritmo natural.
    /// </summary>
    private float GetIntervalForChar(int index)
    {
        if (!_pauseOnPunctuation || index >= _targetText.Length)
            return _charInterval;

        char c = _targetText[index];

        // Pausas dramáticas en final de oración
        if (c == '.' || c == '!' || c == '?' || c == '…')
            return _charInterval * _periodPauseMult;

        // Pausa breve en coma y punto y coma
        if (c == ',' || c == ';' || c == ':')
            return _charInterval * _comaPauseMult;

        return _charInterval;
    }

    /// <summary>Reproduce el tick con pitch aleatorio para variedad orgánica.</summary>
    private void PlayTick()
    {
        if (_tickSound == null || _audioSource == null) return;

        // Variación de pitch: base 1.0 ± _pitchVariance
        // Evita que todos los ticks suenen idénticos (muy robótico)
        _audioSource.pitch  = 1f + UnityEngine.Random.Range(-_pitchVariance, _pitchVariance);
        _audioSource.volume = 1f;
        _audioSource.PlayOneShot(_tickSound);
    }

    private void HideCursorImmediate()
    {
        if (_cursorCoroutine != null)
        {
            StopCoroutine(_cursorCoroutine);
            _cursorCoroutine = null;
        }

        // Restaurar el texto sin el sufijo del cursor
        if (_label != null)
        {
            _label.text = _targetText;
            _label.maxVisibleCharacters = _targetText.Length;
        }
    }

    private void StopAllCoroutinesInternal()
    {
        if (_typeCoroutine   != null) { StopCoroutine(_typeCoroutine);   _typeCoroutine   = null; }
        if (_cursorCoroutine != null) { StopCoroutine(_cursorCoroutine); _cursorCoroutine = null; }
        if (_hideCursorCoroutine != null) { StopCoroutine(_hideCursorCoroutine); _hideCursorCoroutine = null; }
    }

    // ── Limpieza ───────────────────────────────────────────────────────────

    private void OnDestroy()
    {
        StopAllCoroutinesInternal();
        // Desuscribirse de los eventos para evitar memory leaks
        OnComplete       = null;
        OnCharRevealed   = null;
    }
}