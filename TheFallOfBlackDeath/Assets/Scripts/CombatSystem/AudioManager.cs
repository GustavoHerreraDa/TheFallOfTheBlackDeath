using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sonidos de Combate")]
    public AudioClip shootSound;      // Disparo del arma
    public AudioClip hitNormalSound;  // Impacto normal en carne
    public AudioClip hitCriticalSound;// Impacto crítico (más fuerte)
    public AudioClip armorBreakSound; // Extremidad destruida (metálico/crujiente)

    [Header("Sonidos de UI")]
    public AudioClip uiHoverSound;    // Pasar el ratón
    public AudioClip uiClickSound;    // Hacer clic

    [Header("Configuración de Pitch")]
    [Range(0.1f, 0.5f)]
    public float pitchVariation = 0.15f; // Cuánto varía el tono aleatoriamente

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

    // Función principal para reproducir sonidos
    public void PlaySFX(AudioClip clip, float volume = 1f, bool useRandomPitch = true)
    {
        if (clip == null) return;

        // 1. Creamos un GameObject temporal vacío
        GameObject soundObj = new GameObject("TempAudio_" + clip.name);
        
        // 2. Le añadimos un componente AudioSource
        AudioSource source = soundObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;

        // 3. LA MAGIA: Variación dinámica de Pitch para que no suene repetitivo
        if (useRandomPitch)
        {
            source.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
        }

        // 4. Reproducimos el sonido
        source.Play();

        // 5. Destruimos el objeto exactamente cuando el sonido termina
        Destroy(soundObj, clip.length);
    }
}