using UnityEngine;

public class Boss_Door_ : MonoBehaviour
{
    [SerializeField] private float openAngle = 45f;   // Ángulo de apertura
    [SerializeField] private float duration = 1.5f;   // Tiempo de animación
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // Eje de rotación

    private bool isOpen = false;

    public void ToggleDoor()
    {
        if (!isOpen)
            StartCoroutine(OpenDoor());
        else
            StartCoroutine(CloseDoor());
    }

    private System.Collections.IEnumerator OpenDoor()
    {
        isOpen = true;
        yield return RotateDoor(openAngle);
    }

    private System.Collections.IEnumerator CloseDoor()
    {
        isOpen = false;
        yield return RotateDoor(-openAngle);
    }

    private System.Collections.IEnumerator RotateDoor(float targetAngle)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.AngleAxis(targetAngle, rotationAxis);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endRotation;
    }
}
