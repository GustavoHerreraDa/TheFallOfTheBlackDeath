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

    /// <summary>
    /// Registers runtime listeners when the component becomes active.
    /// </summary>
    void OnEnable()
    {
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
        if (NewInventoryManager.Instance != null)
        {
            NewInventoryManager.Instance.OnInventoryChanged -= UpdateUI;
        }
    }

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    public void Start()
    {
        if (skillIcon != null)
            skillIcon.color = new Color(1, 1, 1, 0);
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
        

        if (skill == null)
            GetSkill();

        if (skill == null || (statusMod == null && healthModSkill == null && applySCSkill == null))
        {
            //this.gameObject.SetActive(false);
            return;
        }
        
        if (skill.ItemsNeeded.Count > 0)
            skill.HasRequiredItems();

        if (!skill.HasItemInInventory && skill.ItemsNeeded.Count > 0)
            return;

        if (skillname != null) skillname.text = skill.skillName;
        if (shortDescripcion != null) shortDescripcion.text = skill.SkillDesc;

        //if (statusMod != null)
        //    skillPower.text = statusMod.amount.ToString();

        //if (healthModSkill != null)
        //    skillPower.text = healthModSkill.amount.ToString();

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
        if (player == null)
        {
            // Debug.Log("PLAYER ES NULL EN SkillUI!!!", this);
            return;
        }

        var skills = player.GetComponentsInChildren<Skill>(true);
        if (skills == null || skillIndex < 0 || skillIndex >= skills.Length)
        {
            // Debug.LogWarning($"[SkillUI.GetSkill] Invalid index {skillIndex} (skillsCount={(skills!=null?skills.Length:0)}) on {gameObject.name}", this);
            return;
        }

        skill = skills[skillIndex];
        
        if (skill == null) return;

        healthModSkill = skill.gameObject.GetComponent<HealthModSkill>();
        statusMod = skill.gameObject.GetComponent<StatusMod>();
        applySCSkill = skill.gameObject.GetComponent<ApplySCSkill>();

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

        shortDescripcion.text = skill.SkillDesc;
        skillIcon.sprite = skill.iconUI;

        if (statusMod == null)
            return;

        skillPower.text = statusMod.amount.ToString();
    }
}

