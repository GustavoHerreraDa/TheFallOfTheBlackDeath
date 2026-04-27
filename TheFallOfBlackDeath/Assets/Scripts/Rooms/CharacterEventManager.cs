using System;
using UnityEngine;

/// <summary>
/// Singleton manager responsible for handling events and applying their effects
/// to the character's statistics.
/// </summary>
public class CharacterEventManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the CharacterEventManager.
    /// </summary>
    public static CharacterEventManager Instance { get; private set; }

    /// <summary>
    /// Event triggered when an effect has been successfully applied to the character.
    /// </summary>
    public event Action<EventScript> OnEffectApplied;

    [SerializeField] private CharacterStats currentStats;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        currentStats ??= new CharacterStats();
    }

    /// <summary>
    /// Applies the modifiers from the provided event script to the character's stats.
    /// </summary>
    /// <param name="roomEvent">The event containing the stat modifiers.</param>
    public void ApplyEvent(EventScript roomEvent)
    {
        if (roomEvent == null) return;

        int sign = roomEvent.EffectType == EffectType.POSITIVE ? 1 : -1;

        currentStats.health += roomEvent.HealthModifier * sign;
        currentStats.speed += roomEvent.SpeedModifier * sign;
        currentStats.strength += roomEvent.StrengthModifier * sign;

        Debug.Log($"[CharacterEventManager] Applied {roomEvent.EffectType} event: {roomEvent.EventName}");
        Debug.Log($"[CharacterEventManager] Current Stats -> Health: {currentStats.health}, Speed: {currentStats.speed}, Strength: {currentStats.strength}");

        OnEffectApplied?.Invoke(roomEvent);
    }
}
