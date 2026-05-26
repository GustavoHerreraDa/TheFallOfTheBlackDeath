using UnityEngine;

/// <summary>
/// Self-contained component that lives on a projectile prefab.
/// After calling <see cref="Initialize"/> the object flies toward the target transform,
/// spawns an impact VFX on arrival, and destroys itself.
/// 
/// Usage: put this component (and nothing else flight-related) on the projectile prefab.
/// The SkillEffectPlayer drives everything else.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SkillProjectile : MonoBehaviour
{
    // ── Runtime state ──────────────────────────────────────────────────────────
    private Transform       _target;
    private Vector3         _fixedTargetPosition;   // fallback if target is destroyed mid-flight
    private bool            _hasTarget;

    private float           _speed;
    private bool            _rotateTowardsTarget;

    private GameObject      _impactPrefab;
    private float           _impactDuration;

    private bool            _arrived;

    // ── Arrival threshold ──────────────────────────────────────────────────────
    private const float ArrivalSqrDistance = 0.08f * 0.08f;   // ~8 cm

    // ── Rigidbody ──────────────────────────────────────────────────────────────
    private Rigidbody _rb;

    // ──────────────────────────────────────────────────────────────────────────
    //  Public API
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by <see cref="SkillEffectPlayer"/> right after Instantiate.
    /// </summary>
    /// <param name="target">Live transform to track (receiver's hit-point).</param>
    /// <param name="speed">Units per second.</param>
    /// <param name="rotateTowards">Should the projectile face its velocity?</param>
    /// <param name="impactPrefab">VFX prefab to spawn on impact (can be null).</param>
    /// <param name="impactDuration">Lifetime of the impact VFX.</param>
    public void Initialize(
        Transform  target,
        float      speed,
        bool       rotateTowards,
        GameObject impactPrefab,
        float      impactDuration)
    {
        _rb                  = GetComponent<Rigidbody>();
        _rb.isKinematic      = true;        // We drive position manually — no physics needed
        _rb.useGravity       = false;
        _rb.interpolation    = RigidbodyInterpolation.Interpolate;

        _target              = target;
        _fixedTargetPosition = target != null ? target.position : transform.position;
        _hasTarget           = target != null;
        _speed               = speed;
        _rotateTowardsTarget = rotateTowards;
        _impactPrefab        = impactPrefab;
        _impactDuration      = impactDuration;
        _arrived             = false;
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Unity loop
    // ──────────────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (_arrived) return;

        // Resolve destination: track live transform or fall back to last known position
        Vector3 destination = (_hasTarget && _target != null)
            ? _target.position
            : _fixedTargetPosition;

        // Cache it every frame so we always have a valid fallback
        if (_hasTarget && _target != null)
            _fixedTargetPosition = _target.position;

        Vector3 direction = destination - transform.position;
        float   sqrDist   = direction.sqrMagnitude;

        // ── Arrival check ──
        if (sqrDist <= ArrivalSqrDistance)
        {
            OnArrival(destination);
            return;
        }

        // ── Move ──
        float step = _speed * Time.deltaTime;

        // Overshoot guard: clamp step so we never fly past the target in one frame
        float dist = Mathf.Sqrt(sqrDist);
        step = Mathf.Min(step, dist);

        transform.position += direction.normalized * step;

        // ── Rotate ──
        if (_rotateTowardsTarget && direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction.normalized);
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Private helpers
    // ──────────────────────────────────────────────────────────────────────────

    private void OnArrival(Vector3 position)
    {
        _arrived = true;

        // Snap to target
        transform.position = position;

        // Spawn impact VFX
        if (_impactPrefab != null)
        {
            GameObject impact = Instantiate(_impactPrefab, position, Quaternion.identity);
            Destroy(impact, _impactDuration);
        }

        Destroy(gameObject);
    }
}
