using System.Collections.Generic;
using UnityEngine;

// Attach this to a Fighter to visualize current PartStatus per body part.
// It only READS Fighter.BodyPartData.currentStatus and never changes gameplay logic.
[DisallowMultipleComponent]
[RequireComponent(typeof(Fighter))]
public class StatusVisualController : MonoBehaviour
{
    [Header("Database")] 
    [SerializeField] private StatusVisualDatabase database;

    [Header("Update Settings")] 
    [Tooltip("How often to poll for status changes on body parts (seconds)")] 
    [SerializeField] private float pollInterval = 0.15f;

    private Fighter fighter;
    private float timer;

    private class PartVisualState
    {
        public PartStatus lastStatus = PartStatus.None;
        public StatusVisualEffect effect; // component living under controller for this part
    }

    private readonly Dictionary<BodyPart, PartVisualState> states = new Dictionary<BodyPart, PartVisualState>();

    private void Awake()
    {
        fighter = GetComponent<Fighter>();
    }

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

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= pollInterval)
        {
            timer = 0f;
            RefreshNow();
        }
    }

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

    public void SetDatabase(StatusVisualDatabase db)
    {
        database = db;
        RefreshNow();
    }

    // Force a scan and update of visuals now
    public void RefreshNow()
    {
        if (fighter == null || fighter.bodyParts == null) return;

        foreach (var bp in fighter.bodyParts)
        {
            UpdatePartVisual(bp);
        }
    }

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

    private void RemoveEffect(PartVisualState s)
    {
        if (s.effect != null)
        {
            s.effect.Cleanup();
            Destroy(s.effect.gameObject);
            s.effect = null;
        }
    }

    private void OnBodyPartDestroyed(BodyPart part)
    {
        if (states.TryGetValue(part, out var s))
        {
            RemoveEffect(s);
            s.lastStatus = PartStatus.None;
        }
    }
}