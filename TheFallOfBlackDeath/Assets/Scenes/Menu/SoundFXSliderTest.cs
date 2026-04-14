using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Supports menu and scene loading flow by handling sound fx slider test.
/// </summary>
public class SoundFXSliderTest : MonoBehaviour
{
    public Slider soundFXSlider;
    public AudioSource soundFXAudioSource;
    public AudioClip soundFXClip;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        soundFXSlider.onValueChanged.AddListener(UpdateSoundFXVolume);
    }

    /// <summary>
    /// Updates the sound fx volume.
    /// </summary>
    /// <param name="value">The value.</param>
    void UpdateSoundFXVolume(float value)
    {
        soundFXAudioSource.volume = value;

        // Reproducir el sonido de prueba cuando se ajuste el volumen
        if (soundFXClip != null)
        {
            soundFXAudioSource.PlayOneShot(soundFXClip);
        }
    }
}
