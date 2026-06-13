using UnityEngine;

/// Rotates a vent with smooth acceleration and deceleration.
public class RotatingVent : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;
    [SerializeField] private float maxSpeed = 720f;
    [SerializeField] private float acceleration = 300f;

    private bool isRunning;
    private float currentSpeed;

    private void Update()
    {
        float targetSpeed = isRunning ? maxSpeed : 0f;

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            acceleration * Time.deltaTime);

        transform.Rotate(
            rotationAxis.normalized * currentSpeed * Time.deltaTime,
            Space.Self);
    }

    public void SetRunning(bool running)
    {
        isRunning = running;
    }
}