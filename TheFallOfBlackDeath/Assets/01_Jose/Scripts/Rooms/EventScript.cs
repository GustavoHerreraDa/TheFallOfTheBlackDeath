using UnityEngine;

/// <summary>
/// ScriptableObject containing data for a specific room event.
/// Defines the stat modifications and the type of effect it has on the character.
/// </summary>
[CreateAssetMenu(fileName = "NewRoomEvent", menuName = "RoomSystem/EventScript", order = 1)]
public class EventScript : ScriptableObject
{
    [SerializeField] private string eventName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private EffectType effectType;
    
    [Header("Stat Modifiers")]
    [SerializeField] private int healthModifier;
    [SerializeField] private float speedModifier;
    [SerializeField] private int strengthModifier;

    /// <summary>
    /// Gets the name of the event.
    /// </summary>
    public string EventName => eventName;

    /// <summary>
    /// Gets the description of the event.
    /// </summary>
    public string Description => description;

    /// <summary>
    /// Gets the effect type (Positive or Negative).
    /// </summary>
    public EffectType EffectType => effectType;

    /// <summary>
    /// Gets the health modifier value.
    /// </summary>
    public int HealthModifier => healthModifier;

    /// <summary>
    /// Gets the speed modifier value.
    /// </summary>
    public float SpeedModifier => speedModifier;

    /// <summary>
    /// Gets the strength modifier value.
    /// </summary>
    public int StrengthModifier => strengthModifier;
}