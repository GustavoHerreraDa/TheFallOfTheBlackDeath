using System;
using UnityEngine;

/// Single-piece vertical sliding door (moves up/down like a shutter)
public class VerticalSlidingDoor : MonoBehaviour
{
    [Header("Door Reference")]
    [SerializeField] private Transform door;

    [Header("Settings")]
    [SerializeField] private float openHeight = 2f;
    [SerializeField] private float speed = 3f;
    [SerializeField] private bool open = false;

    [Header("Audio (Optional Overrides)")]
    [SerializeField] private AudioClip customOpenSound;
    [SerializeField] private AudioClip customCloseSound;

    private Vector3 closedPos;
    private Vector3 openPos;

    private void Awake()
    {
        closedPos = door.localPosition;
        openPos = closedPos + Vector3.up * openHeight;
    }

    private void Update()
    {
        Vector3 target = open ? openPos : closedPos;
        door.localPosition = Vector3.Lerp(door.localPosition, target, Time.deltaTime * speed);
    }

    public void Toggle()
    {
        if (open) Close();
        else Open();
    }

    public void Open()
    {
        if (open) return; // Evitar disparar sonido si ya estÃ¡ abierta
        open = true;
        PlayDoorSound(true);
    }

    public void Close()
    {
        if (!open) return; // Evitar disparar sonido si ya estÃ¡ cerrada
        open = false;
        PlayDoorSound(false);
    }

    private void PlayDoorSound(bool isOpen)
    {
        if (AudioManager.Instance == null) return;

        AudioClip clipToPlay = isOpen ? customOpenSound : customCloseSound;
        
        // Si no hay sonido personalizado, usar el del AudioManager
        if (clipToPlay == null)
        {
            clipToPlay = isOpen ? AudioManager.Instance.doorOpenSound : AudioManager.Instance.doorCloseSound;
        }

        if (clipToPlay != null)
        {
            AudioManager.Instance.PlaySFX(clipToPlay);
        }
    }
}