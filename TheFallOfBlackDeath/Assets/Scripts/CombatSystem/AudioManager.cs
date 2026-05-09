using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Supports the combat system by handling audio manager.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer Configuration")]
    public AudioMixer mainMixer;         // Referencia al AudioMixer principal
    public AudioMixerGroup sfxGroup;    // Grupo para efectos de sonido

    [Header("Sonidos de Combate")]
    public AudioClip shootSound;      // Disparo del arma
    public AudioClip hitNormalSound;  // Impacto normal en carne
    public AudioClip hitCriticalSound;// Impacto crÃ­tico (mÃ¡s fuerte)
    public AudioClip armorBreakSound; // Extremidad destruida (metÃ¡lico/crujiente)

    [Header("Sonidos de UI")]
    public AudioClip uiHoverSound;    // Pasar el ratÃ³n
    public AudioClip uiClickSound;    // Hacer clic

    [Header("ConfiguraciÃ³n de Pitch")]
    [Range(0.1f, 0.5f)]
    public float pitchVariation = 0.15f; // CuÃ¡nto varÃ­a el tono aleatoriamente

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    private void Awake()
    {
        // Configuramos el Singleton
        if (Instance == null)
        {
            Instance = this;
            // Opcional: DontDestroyOnLoad(gameObject); si quieres que persista entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // FunciÃ³n principal para reproducir sonidos
    /// <summary>
    /// Executes the play sfx workflow.
    /// </summary>
    /// <param name="clip">The clip.</param>
    /// <param name="volume">The volume.</param>
    /// <param name="useRandomPitch">The use random pitch.</param>
    public void PlaySFX(AudioClip clip, float volume = 1f, bool useRandomPitch = true)
    {
        if (clip == null) return;

        // 1. Creamos un GameObject temporal vacÃ­o
        GameObject soundObj = new GameObject("TempAudio_" + clip.name);
        
        // 2. Le aÃ±adimos un componente AudioSource
        AudioSource source = soundObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;

        // Asignamos el grupo del mixer si estÃ¡ configurado
        if (sfxGroup != null)
        {
            source.outputAudioMixerGroup = sfxGroup;
        }

        // 3. LA MAGIA: VariaciÃ³n dinÃ¡mica de Pitch para que no suene repetitivo
        if (useRandomPitch)
        {
            source.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
        }

        // 4. Reproducimos el sonido
        source.Play();

        // 5. Destruimos el objeto exactamente cuando el sonido termina
        Destroy(soundObj, clip.length);
    }

    /// <summary>
    /// Cambia el volumen de un parÃ¡metro del mixer (en dB).
    /// </summary>
    public void SetMixerVolume(string parameterName, float sliderValue)
    {
        if (mainMixer == null) return;
        
        // ConversiÃ³n de valor de slider (0 a 1) a decibelios (-80 a 0)
        float dB = sliderValue > 0.0001f ? Mathf.Log10(sliderValue) * 20f : -80f;
        mainMixer.SetFloat(parameterName, dB);
    }
}
