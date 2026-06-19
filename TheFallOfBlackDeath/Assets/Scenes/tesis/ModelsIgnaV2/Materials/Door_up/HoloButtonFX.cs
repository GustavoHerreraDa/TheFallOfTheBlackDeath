// ============================================================
//  HoloButtonFX.cs
//  Controlador de estado del shader HoloButton_Diegetic v2
//
//  Uso:
//    1. Añade este script al mismo GameObject que tiene el
//       componente Button (o Image con el material holo).
//    2. Asigna el material HoloButton en el campo _mat del
//       Inspector, O déjalo vacío y se buscará automáticamente
//       en el componente Image/MeshRenderer del objeto.
//    3. El botón necesita un componente Button de Unity UI
//       para que los eventos de EventSystem funcionen.
//
//  Requiere: Unity UI (com.unity.ugui)
// ============================================================

using UnityEngine;
using UnityEngine.EventSystems;   // IPointerEnterHandler, etc.
using UnityEngine.UI;

// Implementamos las tres interfaces de eventos de UI que nos interesan
public class HoloButtonFX : MonoBehaviour,
    IPointerEnterHandler,   // Mouse/dedo entra al área del botón
    IPointerExitHandler,    // Mouse/dedo sale del área
    IPointerDownHandler,    // Botón presionado
    IPointerUpHandler       // Botón soltado
{
    // ── Inspector ─────────────────────────────────────────────────────────
    [Header("Material (opcional — se busca automáticamente si está vacío)")]
    [SerializeField] private Material _mat;

    [Header("Velocidad de transición entre estados")]
    [SerializeField] private float _transitionSpeed = 8f;

    // ── IDs de propiedades del shader (cacheados para evitar string lookup) ─
    // Usar el hash integer en vez de strings en SetFloat es ~5x más rápido
    private static readonly int PropState      = Shader.PropertyToID("_ButtonState");
    private static readonly int PropBlend      = Shader.PropertyToID("_StateBlend");

    // ── Máquina de estados ─────────────────────────────────────────────────
    private enum ButtonState { Idle = 0, Hover = 1, Press = 2 }

    private ButtonState _currentState  = ButtonState.Idle;
    private ButtonState _previousState = ButtonState.Idle;
    private float       _blendValue    = 0f;   // 0 = estado anterior, 1 = estado actual
    
// ── Inicialización ─────────────────────────────────────────────────────
    private void Awake()
    {
        // Si se asignó un material en el Inspector (asset), creamos una instancia
        // para no modificar el archivo original y evitar el error al destruirlo.
        if (_mat != null)
        {
            _mat = new Material(_mat);
            
            // Asignamos esta nueva instancia al componente para que la use
            var img = GetComponent<Image>();
            if (img != null) img.material = _mat;
            else
            {
                var mr = GetComponent<MeshRenderer>();
                if (mr != null) mr.material = _mat;
            }
            return;
        }

        // Si no se asignó un material en el Inspector, lo buscamos dinámicamente
        var imageComponent = GetComponent<Image>();
        if (imageComponent != null)
        {
            // Instanciamos el material para no modificar el asset compartido
            _mat = imageComponent.material = new Material(imageComponent.material);
            return;
        }

        // Si no hay Image, intentamos MeshRenderer (mundo 3D)
        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            _mat = meshRenderer.material; // Unity instancia automáticamente al llamar .material
        }
    }

    // ── Update: interpola el blend en CPU cada frame ───────────────────────
    private void Update()
    {
        if (_mat == null) return;

        // Movemos _blendValue hacia 1 a la velocidad configurada
        // Cuando el estado cambia, resetamos a 0 (ver SetState)
        _blendValue = Mathf.MoveTowards(_blendValue, 1f,
                                        Time.deltaTime * _transitionSpeed);

        // Escribimos al material de instancia (no al asset)
        _mat.SetFloat(PropState, (float)_currentState);
        _mat.SetFloat(PropBlend, _blendValue);
    }

    // ── Cambio de estado ──────────────────────────────────────────────────
    private void SetState(ButtonState newState)
    {
        if (newState == _currentState) return;

        _previousState = _currentState;
        _currentState  = newState;

        // Resetamos el blend para que la transición arranque desde 0
        _blendValue = 0f;
    }

    // ── Eventos de UI (interfaces) ─────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetState(ButtonState.Hover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetState(ButtonState.Idle);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetState(ButtonState.Press);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Al soltar volvemos a Hover si el cursor sigue dentro, o a Idle si salió
        // IsPointerOverGameObject comprueba si el cursor sigue sobre este objeto
        bool stillOver = RectTransformUtility.RectangleContainsScreenPoint(
            GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera);

        SetState(stillOver ? ButtonState.Hover : ButtonState.Idle);
    }

    // ── Limpieza ───────────────────────────────────────────────────────────
    private void OnDestroy()
    {
        // Destruimos la instancia del material para no tener memory leaks
        if (_mat != null)
            Destroy(_mat);
    }
}
