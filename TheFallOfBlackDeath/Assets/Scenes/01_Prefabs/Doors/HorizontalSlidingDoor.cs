using System;
using UnityEngine;

/// Two-piece horizontal sliding door (left/right separation)
/// Attach this to an empty parent object.
/// Assign the left and right door parts, and a trigger collider.
public class HorizontalSlidingDoor : MonoBehaviour
{
    [Header("Door References")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [Header("Settings")]
    [SerializeField] private float openDistance = 1.5f;
    [SerializeField] private float speed = 3f;
    [SerializeField] public bool open = false;

    [Header("Audio (Optional Overrides)")]
    [SerializeField] private AudioClip customOpenSound;
    [SerializeField] private AudioClip customCloseSound;

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;

    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;

    private void Awake()
    {
        leftClosedPos = leftDoor.localPosition;
        rightClosedPos = rightDoor.localPosition;

        // Left moves left, Right moves right
        leftOpenPos = leftClosedPos + Vector3.forward * openDistance;
        rightOpenPos = rightClosedPos + Vector3.back * openDistance;
    }

    private void Update()
    {
        Vector3 leftTarget = open ? leftOpenPos : leftClosedPos;
        Vector3 rightTarget = open ? rightOpenPos : rightClosedPos;

        leftDoor.localPosition = Vector3.Lerp(
            leftDoor.localPosition,
            leftTarget,
            Time.deltaTime * speed
        );

        rightDoor.localPosition = Vector3.Lerp(
            rightDoor.localPosition,
            rightTarget,
            Time.deltaTime * speed
        );
    }

    public void Toggle()
    {
        if (open)
            Close();
        else
            Open();
    }

    public void Open()
    {
        if (open) return;

        open = true;
        PlayDoorSound(true);
    }

    public void Close()
    {
        if (!open) return;

        open = false;
        PlayDoorSound(false);
    }

    private void PlayDoorSound(bool isOpen)
    {
        if (AudioManager.Instance == null) return;

        AudioClip clipToPlay = isOpen
            ? customOpenSound
            : customCloseSound;

        if (clipToPlay == null)
        {
            clipToPlay = isOpen
                ? AudioManager.Instance.doorOpenSound
                : AudioManager.Instance.doorCloseSound;
        }

        if (clipToPlay != null)
        {
            AudioManager.Instance.PlaySFX(clipToPlay);
        }
    }

   

   
}