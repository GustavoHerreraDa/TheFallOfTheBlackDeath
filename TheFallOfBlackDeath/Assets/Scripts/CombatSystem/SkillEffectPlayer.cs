using System.Collections;
using UnityEngine;

/// <summary>
/// Stateless service that reads a <see cref="SkillEffectConfig"/> and spawns/moves
/// the correct VFX objects for a single skill activation.
///
/// Call <see cref="Play"/> once per receiver during <c>Skill.Run()</c>.
/// Everything is driven through coroutines so there are no allocations per-frame
/// after the initial Instantiate calls.
/// </summary>
public class SkillEffectPlayer : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    //  Public API
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Spawns all VFX defined in <paramref name="config"/> for one emitter → receiver pair.
    /// Safe to call with a null config (silently does nothing).
    /// </summary>
    /// <param name="config">The effect descriptor (ScriptableObject).</param>
    /// <param name="emitter">The Fighter casting the skill.</param>
    /// <param name="receiver">The Fighter receiving the skill.</param>
    /// <param name="targetBodyPart">Which body part on the receiver to aim at.</param>
    public void Play(
        SkillEffectConfig config,
        Fighter           emitter,
        Fighter           receiver,
        BodyPart          targetBodyPart = BodyPart.None)
    {
        if (config == null) return;
        StartCoroutine(PlayRoutine(config, emitter, receiver, targetBodyPart));
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Core coroutine
    // ──────────────────────────────────────────────────────────────────────────

    private IEnumerator PlayRoutine(
        SkillEffectConfig config,
        Fighter           emitter,
        Fighter           receiver,
        BodyPart          targetBodyPart)
    {
        // Optional initial delay to sync with attack animations
        if (config.launchDelay > 0f)
            yield return new WaitForSeconds(config.launchDelay);

        Transform emitterPoint = ResolveEmitterSpawnPoint(emitter, config.emitterSpawnPoint);
        Vector3   spawnPos     = emitterPoint.position + emitterPoint.TransformDirection(config.emitterSpawnOffset);

        Transform targetPoint  = receiver != null
            ? receiver.GetHitPoint(targetBodyPart)
            : null;

        switch (config.mode)
        {
            case SkillEffectMode.OnTarget:
                PlayOnTarget(config, targetPoint);
                break;

            case SkillEffectMode.OnEmitter:
                PlayOnEmitter(config, spawnPos, emitterPoint.rotation);
                break;

            case SkillEffectMode.OnBoth:
                PlayOnEmitter(config, spawnPos, emitterPoint.rotation);
                PlayOnTarget(config, targetPoint);
                break;

            case SkillEffectMode.Projectile:
                PlayProjectile(config, spawnPos, emitterPoint.rotation, targetPoint);
                break;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Mode implementations
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Spawns the emitter-side VFX at the caster's spawn point.</summary>
    private void PlayOnEmitter(SkillEffectConfig config, Vector3 position, Quaternion rotation)
    {
        if (config.emitterEffectPrefab == null) return;

        GameObject fx = Instantiate(config.emitterEffectPrefab, position, rotation);
        Destroy(fx, config.emitterEffectDuration);
    }

    /// <summary>Spawns the impact VFX directly on the receiver's hit point.</summary>
    private void PlayOnTarget(SkillEffectConfig config, Transform targetPoint)
    {
        if (config.impactEffectPrefab == null || targetPoint == null) return;

        GameObject fx = Instantiate(config.impactEffectPrefab, targetPoint.position, targetPoint.rotation);
        Destroy(fx, config.impactEffectDuration);
    }

    /// <summary>
    /// Instantiates the emitter VFX, then launches a <see cref="SkillProjectile"/>
    /// from the spawn point toward the target.
    /// </summary>
    private void PlayProjectile(
        SkillEffectConfig config,
        Vector3           spawnPos,
        Quaternion        spawnRot,
        Transform         targetPoint)
    {
        // 1. Muzzle flash / cast VFX at emitter
        PlayOnEmitter(config, spawnPos, spawnRot);

        // 2. Projectile
        if (config.projectilePrefab == null || targetPoint == null) return;

        // Face the target right from spawn
        Vector3    dir            = (targetPoint.position - spawnPos).normalized;
        Quaternion projectileRot  = dir != Vector3.zero
            ? Quaternion.LookRotation(dir)
            : spawnRot;

        GameObject projectileGO = Instantiate(config.projectilePrefab, spawnPos, projectileRot);

        // The prefab must have a SkillProjectile component; if it doesn't, add one automatically
        SkillProjectile projectile = projectileGO.GetComponent<SkillProjectile>();
        if (projectile == null)
            projectile = projectileGO.AddComponent<SkillProjectile>();

        projectile.Initialize(
            targetPoint,
            config.projectileSpeed,
            config.rotateTowardsTarget,
            config.impactEffectPrefab,
            config.impactEffectDuration);
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Maps an <see cref="EmitterSpawnPoint"/> enum value to the correct
    /// <see cref="Transform"/> on the emitter fighter.
    /// Falls back to <c>DamagePivot</c> when the requested part doesn't exist.
    /// </summary>
    private static Transform ResolveEmitterSpawnPoint(Fighter emitter, EmitterSpawnPoint point)
    {
        if (emitter == null) return null;

        switch (point)
        {
            case EmitterSpawnPoint.RightHand:
            {
                Transform t = emitter.GetHitPoint(BodyPart.RightArm);
                return t != null ? t : emitter.DamagePivot;
            }
            case EmitterSpawnPoint.LeftHand:
            {
                Transform t = emitter.GetHitPoint(BodyPart.LeftArm);
                return t != null ? t : emitter.DamagePivot;
            }
            case EmitterSpawnPoint.Head:
            {
                Transform t = emitter.GetHitPoint(BodyPart.Head);
                return t != null ? t : emitter.DamagePivot;
            }
            case EmitterSpawnPoint.DamagePivot:
            default:
                return emitter.DamagePivot;
        }
    }
}
