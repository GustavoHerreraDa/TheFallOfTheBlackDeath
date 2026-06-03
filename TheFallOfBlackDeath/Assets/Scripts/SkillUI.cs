using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using InventoryNew;

/// <summary>
/// Handles skill ui for the current project workflow.
/// </summary>
public class SkillUI : MonoBehaviour
{
    private enum SkillDisplaySource // NUEVO
    {
        LearnedPool,
        ActiveLoadout,
        SceneChildrenFallback
    }

    public GameObject player;
    public int skillIndex;
    public Skill skill;
    public StatusMod statusMod;
    public HealthModSkill healthModSkill;
    public ApplySCSkill applySCSkill;
    public TextMeshProUGUI skillname;
    public TextMeshProUGUI shortDescripcion;
    public TextMeshProUGUI skillPower;
    public Image skillIcon;

    [Header("Skill Source")]
    [SerializeField] private SkillDisplaySource skillDisplaySource = SkillDisplaySource.LearnedPool; // NUEVO

    [Header("Equipment Skill Feedback")]
    [SerializeField] private bool useCurrentNameColorAsBase = true; // NUEVO
    [SerializeField] private Color baseSkillNameColor = Color.white; // NUEVO
    [SerializeField] private Color equipmentGrantedSkillNameColor = new Color(0.25f, 0.85f, 1f); // NUEVO
    [SerializeField] private Color newlyGrantedSkillNameColor = new Color(1f, 0.85f, 0.25f); // NUEVO
    [SerializeField] private float newlyGrantedHighlightSeconds = 2f; // NUEVO

    [Header("Empty / Unavailable State")]
    [SerializeField] private bool hideSlotWhenEmpty = false; // NUEVO
    [SerializeField] private bool showSkillsMissingRequiredItems = true; // NUEVO
    [SerializeField] private Color missingRequiredItemsNameColor = Color.gray; // NUEVO
    [SerializeField] private Color emptyIconColor = new Color(1, 1, 1, 0); // NUEVO

    private PlayerFighter boundFighter; // NUEVO
    private EquipmentHandler subscribedEquipmentHandler; // NUEVO
    private readonly HashSet<string> knownGrantedSkillIds = new HashSet<string>(); // NUEVO
    private readonly HashSet<string> newlyGrantedSkillIds = new HashSet<string>(); // NUEVO
    private Coroutine clearNewFeedbackRoutine; // NUEVO
    private bool capturedBaseNameColor; // NUEVO

    private void Awake() // NUEVO
    {
        CaptureBaseNameColor();
    }

    /// <summary>
    /// Registers runtime listeners when the component becomes active.
    /// </summary>
    void OnEnable()
    {
        RefreshBoundFighter(); // NUEVO

        if (NewInventoryManager.Instance != null)
        {
            NewInventoryManager.Instance.OnInventoryChanged += UpdateUI;
        }
    }

    /// <summary>
    /// Unregisters runtime listeners when the component becomes inactive.
    /// </summary>
    void OnDisable()
    {
        UnsubscribeFromEquipmentHandler(); // NUEVO

        if (NewInventoryManager.Instance != null)
        {
            NewInventoryManager.Instance.OnInventoryChanged -= UpdateUI;
        }

        if (clearNewFeedbackRoutine != null)
        {
            StopCoroutine(clearNewFeedbackRoutine);
            clearNewFeedbackRoutine = null;
        }
    }

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    public void Start()
    {
        CaptureBaseNameColor(); // NUEVO

        if (skillIcon != null)
            skillIcon.color = emptyIconColor; // MODIFICADO
        if (skillPower != null)
            skillPower.text = "";
        UpdateUI();
    }

    /// <summary>
    /// Executes the on character changed workflow.
    /// </summary>
    /// <param name="fighter">The fighter.</param>
    private void OnCharacterChanged(PlayerFighter fighter)
    {
        // If this UI is bound to the active player, refresh skills reference
        if (player == null)
            player = fighter != null ? fighter.gameObject : player;
        RefreshBoundFighter(); // NUEVO
        skill = null; // force re-fetch
        UpdateUI();
    }

    /// <summary>
    /// Updates the ui.
    /// </summary>
    public void UpdateUI()
    {
        // Si el objeto ha sido destruido, no hacemos nada
        if (this == null || gameObject == null) return;

        //Debug.Log("Se actuliza UI" + gameObject.name);

        RefreshBoundFighter(); // NUEVO
        GetSkill(); // MODIFICADO: siempre re-resuelve para reflejar cambios de equipo/loadout.

        if (skill == null) // MODIFICADO: los componentes de efecto son opcionales; la Skill es la fuente principal.
        {
            ClearSlot(); // MODIFICADO
            return;
        }

        bool hasItemRequirements = skill.ItemsNeeded != null && skill.ItemsNeeded.Count > 0; // MODIFICADO
        if (hasItemRequirements)
            skill.HasRequiredItems();

        if (!skill.HasItemInInventory && hasItemRequirements && !showSkillsMissingRequiredItems) // MODIFICADO
        {
            ClearSlot();
            return;
        }

        if (hideSlotWhenEmpty && !gameObject.activeSelf) // NUEVO
            gameObject.SetActive(true);

        if (skillname != null) skillname.text = skill.skillName;
        if (shortDescripcion != null) shortDescripcion.text = skill.SkillDesc;

        //if (statusMod != null)
        //    skillPower.text = statusMod.amount.ToString();

        //if (healthModSkill != null)
        //    skillPower.text = healthModSkill.amount.ToString();

        ApplySkillNameFeedback(hasItemRequirements && !skill.HasItemInInventory); // NUEVO

        if (skillIcon != null)
        {
            skillIcon.sprite = skill.iconUI;
            skillIcon.color = Color.white;
        }
    }

    /// <summary>
    /// Gets the skill.
    /// </summary>
    private void GetSkill()
    {
        ClearCachedSkillComponents(); // NUEVO

        Skill[] skills = GetDisplayedSkills(); // MODIFICADO
        if (skills == null || skillIndex < 0 || skillIndex >= skills.Length)
        {
            return;
        }

        skill = skills[skillIndex];

        if (skill == null) return;

        healthModSkill = skill.gameObject.GetComponent<HealthModSkill>();
        statusMod = skill.gameObject.GetComponent<StatusMod>();
        applySCSkill = skill.gameObject.GetComponent<ApplySCSkill>();

    }

    private Skill[] GetDisplayedSkills() // NUEVO
    {
        if (boundFighter == null)
        {
            return player != null
                ? player.GetComponentsInChildren<Skill>(true)
                : null;
        }

        switch (skillDisplaySource)
        {
            case SkillDisplaySource.ActiveLoadout:
                return boundFighter.skills;

            case SkillDisplaySource.SceneChildrenFallback:
                return boundFighter.GetComponentsInChildren<Skill>(true);

            case SkillDisplaySource.LearnedPool:
            default:
                if (boundFighter.allLearnedSkills == null || boundFighter.allLearnedSkills.Length == 0)
                    boundFighter.RebuildSkillPool();

                return boundFighter.allLearnedSkills;
        }
    }

    private void RefreshBoundFighter() // NUEVO
    {
        PlayerFighter resolvedFighter = ResolvePlayerFighter();
        if (resolvedFighter != boundFighter)
        {
            UnsubscribeFromEquipmentHandler();
            boundFighter = resolvedFighter;
            ResetGrantedSkillCache();
            skill = null;
        }

        SubscribeToEquipmentHandler();
    }

    private PlayerFighter ResolvePlayerFighter() // NUEVO
    {
        if (player != null)
            return player.GetComponent<PlayerFighter>();

        if (GameManager.Instance != null && GameManager.Instance.character1 != null)
        {
            player = GameManager.Instance.character1.gameObject;
            return GameManager.Instance.character1;
        }

        return null;
    }

    private void SubscribeToEquipmentHandler() // NUEVO
    {
        EquipmentHandler handler = boundFighter != null ? boundFighter.equipmentHandler : null;
        if (handler == subscribedEquipmentHandler)
            return;

        UnsubscribeFromEquipmentHandler();

        subscribedEquipmentHandler = handler;
        if (subscribedEquipmentHandler != null)
            subscribedEquipmentHandler.OnEquipChanged += HandleEquipmentChanged;
    }

    private void UnsubscribeFromEquipmentHandler() // NUEVO
    {
        if (subscribedEquipmentHandler != null)
            subscribedEquipmentHandler.OnEquipChanged -= HandleEquipmentChanged;

        subscribedEquipmentHandler = null;
    }

    private void HandleEquipmentChanged() // NUEVO
    {
        if (boundFighter != null)
            boundFighter.RebuildSkillPool(); // NUEVO: garantiza que allLearnedSkills ya incluya/quita lo otorgado antes de refrescar.

        var currentGrantedIds = CollectGrantedSkillIds();
        newlyGrantedSkillIds.Clear();

        foreach (string skillId in currentGrantedIds)
        {
            if (!knownGrantedSkillIds.Contains(skillId))
                newlyGrantedSkillIds.Add(skillId);
        }

        knownGrantedSkillIds.Clear();
        foreach (string skillId in currentGrantedIds)
            knownGrantedSkillIds.Add(skillId);

        skill = null;
        UpdateUI();
        StartNewSkillFeedbackTimer();
    }

    private void ResetGrantedSkillCache() // NUEVO
    {
        knownGrantedSkillIds.Clear();
        newlyGrantedSkillIds.Clear();

        foreach (string skillId in CollectGrantedSkillIds())
            knownGrantedSkillIds.Add(skillId);
    }

    private HashSet<string> CollectGrantedSkillIds() // NUEVO
    {
        var ids = new HashSet<string>();
        if (boundFighter == null || boundFighter.equipmentHandler == null)
            return ids;

        Skill[] grantedSkills = boundFighter.equipmentHandler.GetGrantedSkills();
        if (grantedSkills == null)
            return ids;

        foreach (var grantedSkill in grantedSkills)
        {
            string id = GetSkillIdentifier(grantedSkill);
            if (!string.IsNullOrEmpty(id))
                ids.Add(id);
        }

        return ids;
    }

    private void StartNewSkillFeedbackTimer() // NUEVO
    {
        if (clearNewFeedbackRoutine != null)
            StopCoroutine(clearNewFeedbackRoutine);

        if (newlyGrantedSkillIds.Count == 0)
            return;

        if (newlyGrantedHighlightSeconds <= 0f)
        {
            newlyGrantedSkillIds.Clear();
            UpdateUI();
            return;
        }

        clearNewFeedbackRoutine = StartCoroutine(ClearNewSkillFeedbackAfterDelay());
    }

    private IEnumerator ClearNewSkillFeedbackAfterDelay() // NUEVO
    {
        yield return new WaitForSecondsRealtime(newlyGrantedHighlightSeconds);

        newlyGrantedSkillIds.Clear();
        clearNewFeedbackRoutine = null;
        UpdateUI();
    }

    private void ApplySkillNameFeedback(bool missingRequiredItems) // NUEVO
    {
        if (skillname == null || skill == null)
            return;

        string id = GetSkillIdentifier(skill);
        if (!string.IsNullOrEmpty(id) && newlyGrantedSkillIds.Contains(id))
        {
            skillname.color = newlyGrantedSkillNameColor;
            return;
        }

        if (!string.IsNullOrEmpty(id) && knownGrantedSkillIds.Contains(id))
        {
            skillname.color = equipmentGrantedSkillNameColor;
            return;
        }

        skillname.color = missingRequiredItems ? missingRequiredItemsNameColor : baseSkillNameColor;
    }

    private string GetSkillIdentifier(Skill targetSkill) // NUEVO
    {
        if (targetSkill == null) return string.Empty;
        return !string.IsNullOrEmpty(targetSkill.skillId) ? targetSkill.skillId : targetSkill.skillName;
    }

    private void ClearCachedSkillComponents() // NUEVO
    {
        skill = null;
        statusMod = null;
        healthModSkill = null;
        applySCSkill = null;
    }

    private void ClearSlot() // NUEVO
    {
        ClearCachedSkillComponents();

        if (skillname != null)
        {
            skillname.text = "";
            skillname.color = baseSkillNameColor;
        }

        if (shortDescripcion != null)
            shortDescripcion.text = "";

        if (skillPower != null)
            skillPower.text = "";

        if (skillIcon != null)
        {
            skillIcon.sprite = null;
            skillIcon.color = emptyIconColor;
        }

        if (hideSlotWhenEmpty && gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    private void CaptureBaseNameColor() // NUEVO
    {
        if (capturedBaseNameColor || !useCurrentNameColorAsBase || skillname == null)
            return;

        baseSkillNameColor = skillname.color;
        capturedBaseNameColor = true;
    }


    /// <summary>
    /// Sets the stats.
    /// </summary>
    /// <param name="skill">The skill.</param>
    /// <param name="statusMod">The status mod.</param>
    public void SetStats(Skill skill, StatusMod statusMod)
    {
        if (skill == null)
            return;

        this.skill = skill; // MODIFICADO
        this.statusMod = statusMod; // MODIFICADO
        this.healthModSkill = skill.gameObject.GetComponent<HealthModSkill>(); // NUEVO
        this.applySCSkill = skill.gameObject.GetComponent<ApplySCSkill>(); // NUEVO

        if (skillname != null) // NUEVO
        {
            skillname.text = skill.skillName;
            ApplySkillNameFeedback(false);
        }

        if (shortDescripcion != null) // MODIFICADO
            shortDescripcion.text = skill.SkillDesc;

        if (skillIcon != null) // MODIFICADO
        {
            skillIcon.sprite = skill.iconUI;
            skillIcon.color = Color.white;
        }

        if (skillPower != null) // MODIFICADO
            skillPower.text = statusMod != null ? statusMod.amount.ToString() : "";
    }
}

