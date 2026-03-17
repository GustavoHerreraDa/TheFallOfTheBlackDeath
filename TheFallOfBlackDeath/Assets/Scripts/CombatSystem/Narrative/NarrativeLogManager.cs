using UnityEngine;
using System.Linq;

// Attach this to a combat scene GameObject. Set the database and player reference via Inspector.
// It will produce atmospheric combat log messages routed through LogPanel.Write().
public class NarrativeLogManager : MonoBehaviour
{
    [Header("Database")]
    public NarrativeLogDatabase database;

    [Header("References")]
    [Tooltip("Primary player character for contextual checks.")]
    public PlayerFighter player; // Optional; if null, contextual checks are skipped.

    [Header("Context Thresholds")] 
    [Range(0.05f, 0.75f)] public float lowHealthThreshold = 0.3f;
    
    void Awake()
    {
        if (player == null)
        {
            PlayerFighter[] players = FindObjectsOfType<PlayerFighter>();

            foreach (var p in players)
            {
                if (p.team == Team.PLAYERS)
                {
                    player = p;
                    break;
                }
            }
        }
    }

    public void EnemyEncounter(EnemyFighter enemy)
    {
        var entry = GetEntry(enemy);
        string msg = entry?.GetRandom(entry?.encounterMessages);
        if (!string.IsNullOrEmpty(msg))
            LogPanel.Write(msg);
    }

    public void EnemyTurn(EnemyFighter enemy)
    {
        string msg = GetContextualMessage(enemy);
        if (string.IsNullOrEmpty(msg))
        {
            var entry = GetEntry(enemy);
            msg = entry?.GetRandom(entry?.turnMessages);
        }
        if (!string.IsNullOrEmpty(msg))
            LogPanel.Write(msg);
    }

    public void EnemyPreparing(EnemyFighter enemy)
    {
        var entry = GetEntry(enemy);
        string msg = entry?.GetRandom(entry?.preparingMessages);
        if (!string.IsNullOrEmpty(msg))
            LogPanel.Write(msg);
    }

    private EnemyNarrativeEntry GetEntry(EnemyFighter enemy)
    {
        if (enemy == null) return null;
        if (enemy.narrativeData != null) return enemy.narrativeData; // per prefab override
        if (database == null) return null;
        return database.GetById(enemy.idName);
    }

    private string GetContextualMessage(EnemyFighter enemy)
    {
        if (player == null) return null;
        var entry = GetEntry(enemy);
        if (entry == null) return null;

        // 1) Bleeding (StatusCondition-based or body part statuses)
        bool playerBleeding = IsPlayerBleeding(player);
        if (playerBleeding)
        {
            var m = entry.GetRandom(entry.playerBleedingMessages);
            if (!string.IsNullOrEmpty(m)) return m;
        }

        // 2) Low Health
        float ratio = (player.stats != null && player.stats.maxHealth > 0) ? (player.stats.health / player.stats.maxHealth) : 1f;
        if (ratio <= lowHealthThreshold)
        {
            var m = entry.GetRandom(entry.playerLowHealthMessages);
            if (!string.IsNullOrEmpty(m)) return m;
        }

        // 3) Injured (destroyed) arm
        if (IsArmInjured(player))
        {
            var m = entry.GetRandom(entry.playerInjuredArmMessages);
            if (!string.IsNullOrEmpty(m)) return m;
        }

        return null; // fall back to default messages
    }

    private bool IsPlayerBleeding(PlayerFighter p)
    {
        // If there's a live StatusCondition of bleeding type, we could inspect type name.
        // But to avoid coupling, also check any body part with PartStatus.Bleeding.
        if (p == null) return false;
        if (p.statusCondition is BleedingCondition) return true;
        if (p.bodyParts != null)
        {
            foreach (var bp in p.bodyParts)
                if (bp != null && bp.currentStatus == PartStatus.Bleeding)
                    return true;
        }
        return false;
    }

    private bool IsArmInjured(PlayerFighter p)
    {
        if (p == null || p.bodyParts == null) return false;
        var left = p.GetBodyPart(BodyPart.LeftArm);
        var right = p.GetBodyPart(BodyPart.RightArm);
        bool injured = (left != null && left.IsDestroyed) || (right != null && right.IsDestroyed);
        return injured;
    }
}
