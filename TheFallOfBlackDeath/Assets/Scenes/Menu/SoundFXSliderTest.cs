using UnityEngine;
using UnityEngine.UI;

public class SoundFXSliderTest : MonoBehaviour
{
    public Slider soundFXSlider;
    public AudioSource soundFXAudioSource;
    public AudioClip soundFXClip;

    void Start()
    {
        soundFXSlider.onValueChanged.AddListener(UpdateSoundFXVolume);
    }

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