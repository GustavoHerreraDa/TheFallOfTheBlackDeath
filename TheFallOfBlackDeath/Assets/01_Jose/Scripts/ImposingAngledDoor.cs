using UnityEngine;
using UnityEngine.AI;

public class ImposingAngledDoor : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private Transform doorObject;

    [Header("Diagonal Opening Movement")]
    [SerializeField] private float openDistance = 3f;

    [Tooltip("45 means the door moves equally upward and backward.")]
    [SerializeField] private float diagonalAngle = 45f;

    [SerializeField] private float openSpeed = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool useRandomPitch = false;

    [Header("Navigation")]
    [SerializeField] private NavMeshObstacle obstacle;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private float openAmount;
    [SerializeField] private bool isOpen;

    private void Awake()
    {
        if (doorObject == null)
        {
            doorObject = transform;
        }

        closedPosition = doorObject.position;
        openPosition = closedPosition + GetDiagonalOpenOffset();

        if (obstacle != null)
        {
            obstacle.enabled = true;
        }
    }

    private void Update()
    {
        float targetAmount = isOpen ? 1f : 0f;

        openAmount = Mathf.MoveTowards(
            openAmount,
            targetAmount,
            openSpeed * Time.deltaTime
        );

        doorObject.position = Vector3.Lerp(
            closedPosition,
            openPosition,
            openAmount
        );

        if (obstacle != null)
        {
            obstacle.enabled = openAmount < 0.95f;
        }
    }

    public void OpenDoor()
    {
        if (isOpen)
        {
            return;
        }

        isOpen = true;
        PlaySound(openSound);
    }

    public void CloseDoor()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;
        PlaySound(closeSound);
    }

    public void ToggleDoor()
    {
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    private Vector3 GetDiagonalOpenOffset()
    {
        float angleInRadians = diagonalAngle * Mathf.Deg2Rad;

        Vector3 backwardMovement = doorObject.right * Mathf.Cos(angleInRadians);
        Vector3 upwardMovement = doorObject.up * Mathf.Sin(angleInRadians);

        Vector3 diagonalDirection = (backwardMovement + upwardMovement).normalized;

        return diagonalDirection * openDistance;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("No AudioManager found in scene.");
            return;
        }

        AudioManager.Instance.PlaySFX(
            clip,
            volume,
            useRandomPitch
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Charecter"))
        {
            return;
        }

        OpenDoor();
    }

}