using UnityEngine;

/// <summary>
/// Defines how a skill's visual effect should behave in the world.
/// Create one per skill via Assets > Create > Combat > Skill Effect Config.
/// </summary>
[CreateAssetMenu(menuName = "Combat/Skill Effect Config")]
public class SkillEffectConfig : ScriptableObject
{
    [Header("Effect Type")]
    public SkillEffectMode mode = SkillEffectMode.OnTarget;

    [Header("Prefabs")]
    [Tooltip("VFX spawned at the emitter's origin (muzzle flash, cast glow, etc.)")]
    public GameObject emitterEffectPrefab;

    [Tooltip("The projectile prefab that travels toward the target (bullet, fireball, arrow…). Only used in Projectile mode.")]
    public GameObject projectilePrefab;

    [Tooltip("VFX spawned at the hit point on the receiver (explosion, blood, spark, etc.)")]
    public GameObject impactEffectPrefab;

    [Header("Emitter Spawn")]
    [Tooltip("Which transform on the emitter to use as spawn origin.")]
    public EmitterSpawnPoint emitterSpawnPoint = EmitterSpawnPoint.DamagePivot;

    [Tooltip("Local offset added on top of the resolved spawn transform.")]
    public Vector3 emitterSpawnOffset = Vector3.zero;

    [Header("Projectile Settings")]
    public float projectileSpeed = 12f;

    [Tooltip("If true the projectile rotates to always face its travel direction.")]
    public bool rotateTowardsTarget = true;

    [Header("Durations (seconds)")]
    public float emitterEffectDuration = 0.5f;
    public float impactEffectDuration  = 0.8f;

    [Header("Delay")]
    [Tooltip("Seconds to wait after Run() before the effect starts. Useful to sync with attack animations.")]
    public float launchDelay = 0f;
}

/// <summary>How the visual travels from emitter to target.</summary>
public enum SkillEffectMode
{
    /// <summary>Effect spawns directly on the target (melee hit, heal aura, etc.).</summary>
    OnTarget,

    /// <summary>A projectile is instantiated at the emitter and moves toward the target.</summary>
    Projectile,

    /// <summary>Effect spawns at the emitter only (self-buff, AOE cast, etc.).</summary>
    OnEmitter,

    /// <summary>Effect spawns at both emitter and target simultaneously.</summary>
    OnBoth
}

/// <summary>Which point on the emitter Fighter to use as the projectile/effect spawn origin.</summary>
public enum EmitterSpawnPoint
{
    /// <summary>Fighter.DamagePivot — general centre-mass point.</summary>
    DamagePivot,

    /// <summary>Fighter.GetHitPoint(RightArm) — weapon hand.</summary>
    RightHand,

    /// <summary>Fighter.GetHitPoint(LeftArm) — off-hand / shield.</summary>
    LeftHand,

    /// <summary>Fighter.GetHitPoint(Head) — eye / forehead for beam effects.</summary>
    Head
}
