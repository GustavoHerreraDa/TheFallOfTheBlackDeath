using System.Collections.Generic;
using UnityEngine;

// Attach this to a Fighter to visualize current PartStatus per body part.
// It only READS Fighter.BodyPartData.currentStatus and never changes gameplay logic.
[DisallowMultipleComponent]
[RequireComponent(typeof(Fighter))]
/// <summary>
/// Applies and removes visual effects that represent the current body-part status conditions of a fighter.
/// </summary>
public class StatusVisualController : MonoBehaviour
{
    [Header("Database")] 
    [SerializeField] private StatusVisualDatabase database;

    [Header("Update Settings")] 
    [Tooltip("How often to poll for status changes on body parts (seconds)")] 
    [SerializeField] private float pollInterval = 0.15f;

    private Fighter fighter;
    private float timer;

    /// <summary>
    /// Supports status-effect presentation by handling part visual state.
    /// </summary>
    private class PartVisualState
    {
        public PartStatus lastStatus = PartStatus.None;
        public StatusVisualEffect effect; // component living under controller for this part
    }

    private readonly Dictionary<BodyPart, PartVisualState> states = new Dictionary<BodyPart, PartVisualState>();

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    private void Awake()
    {
        fighter = GetComponent<Fighter>();
    }

    /// <summary>
    /// Registers runtime listeners when the component becomes active.
    /// </summary>
    private void OnEnable()
    {
        if (fighter == null) fighter = GetComponent<Fighter>();
        if (fighter != null)
        {
            fighter.OnBodyPartDestroyedEvent += OnBodyPartDestroyed;
        }

        BuildInitialState();
        RefreshNow();
    }

    /// <summary>
    /// Unregisters runtime listeners when the component becomes inactive.
    /// </summary>
    private void OnDisable()
    {
        if (fighter != null)
        {
            fighter.OnBodyPartDestroyedEvent -= OnBodyPartDestroyed;
        }

        // Clean all effects
        foreach (var kvp in states)
        {
            if (kvp.Value.effect != null)
            {
                kvp.Value.effect.Cleanup();
                Destroy(kvp.Value.effect.gameObject);
                kvp.Value.effect = null;
            }
        }

        states.Clear();
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= pollInterval)
        {
            timer = 0f;
            RefreshNow();
        }
    }

    /// <summary>
    /// Executes the build initial state workflow.
    /// </summary>
    private void BuildInitialState()
    {
        states.Clear();
        if (fighter == null || fighter.bodyParts == null) return;

        foreach (var bp in fighter.bodyParts)
        {
            if (!states.ContainsKey(bp.part))
            {
                states[bp.part] = new PartVisualState { lastStatus = PartStatus.None, effect = null };
            }
        }
    }

    /// <summary>
    /// Sets the database.
    /// </summary>
    /// <param name="db">The db.</param>
    public void SetDatabase(StatusVisualDatabase db)
    {
        database = db;
        RefreshNow();
    }

    // Force a scan and update of visuals now
    /// <summary>
    /// Refreshes the now.
    /// </summary>
    public void RefreshNow()
    {
        if (fighter == null || fighter.bodyParts == null) return;

        foreach (var bp in fighter.bodyParts)
        {
            UpdatePartVisual(bp);
        }
    }

    /// <summary>
    /// Updates the part visual.
    /// </summary>
    /// <param name="bp">The bp.</param>
    private void UpdatePartVisual(Fighter.BodyPartData bp)
    {
        if (!states.TryGetValue(bp.part, out var s))
        {
            s = new PartVisualState();
            states[bp.part] = s;
        }

        // If the body part was destroyed, ensure visuals are removed
        if (bp.IsDestroyed)
        {
            RemoveEffect(s);
            s.lastStatus = PartStatus.None;
            return;
        }

        var current = bp.currentStatus;
        if (current == s.lastStatus) return; // no change

        // Status changed -> remove old effect
        RemoveEffect(s);

        // Create new effect if needed
        if (current != PartStatus.None && database != null)
        {
            var entry = database.GetEntry(current);
            if (entry != null)
            {
                var go = new GameObject($"StatusVFX_{bp.part}_{current}");
                go.transform.SetParent(this.transform, false);
                var effect = go.AddComponent<StatusVisualEffect>();
                effect.Initialize(fighter, bp.part, entry);
                s.effect = effect;
            }
        }

        s.lastStatus = current;
    }

    /// <summary>
    /// Removes the effect.
    /// </summary>
    /// <param name="s">The s.</param>
    private void RemoveEffect(PartVisualState s)
    {
        if (s.effect != null)
        {
            s.effect.Cleanup();
            Destroy(s.effect.gameObject);
            s.effect = null;
        }
    }

    /// <summary>
    /// Executes the on body part destroyed workflow.
    /// </summary>
    /// <param name="part">The part.</param>
    private void OnBodyPartDestroyed(BodyPart part)
    {
        if (states.TryGetValue(part, out var s))
        {
            RemoveEffect(s);
            s.lastStatus = PartStatus.None;
        }
    }
}
