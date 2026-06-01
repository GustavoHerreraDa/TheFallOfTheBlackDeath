using UnityEngine;

/// <summary>
/// Plays a looping sound while the player is inside the trigger and
/// gradually fades it out when the player leaves.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class SoundTriggerArea : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip[] audioClips;

    [Range(0f, 1f)]
    [SerializeField] private float maxVolume = 1f;

    [SerializeField] private float fadeOutDuration = 2f;

    private AudioSource activeSource;
    private bool isFading;

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void Update()
    {
        if (!isFading || activeSource == null)
        {
            return;
        }

        activeSource.volume -= (maxVolume / fadeOutDuration) * Time.deltaTime;

        if (activeSource.volume <= 0f)
        {
            activeSource.volume = 0f;
            isFading = false;

            AudioManager.Instance.StopPersistentSFX(activeSource);

            activeSource = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Charecter"))
        {
            return;
        }

        if (activeSource != null)
        {
            isFading = false;
            activeSource.volume = maxVolume;
            return;
        }

        if (audioClips == null || audioClips.Length == 0)
        {
            return;
        }

        AudioClip selectedClip = audioClips[
            Random.Range(0, audioClips.Length)
        ];

        activeSource = AudioManager.Instance.PlayPersistentSFX(
            selectedClip,
            maxVolume,
            true
        );

        isFading = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Charecter"))
        {
            return;
        }

        if (activeSource == null)
        {
            return;
        }

        isFading = true;
    }
}