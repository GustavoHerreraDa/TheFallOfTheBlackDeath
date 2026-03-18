using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    void OnEnable()
    {
        InventoryManager.OnInventoryChanged += UpdateUI;
        InventoryManager.OnCharacterChanged += OnCharacterChanged;
    }

    void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= UpdateUI;
        InventoryManager.OnCharacterChanged -= OnCharacterChanged;
    }

    public void Start()
    {
        if (skillIcon != null)
            skillIcon.color = new Color(1, 1, 1, 0);
        if (skillPower != null)
            skillPower.text = "";
        UpdateUI();
    }

    private void OnCharacterChanged(PlayerFighter fighter)
    {
        // If this UI is bound to the active player, refresh skills reference
        if (player == null)
            player = fighter != null ? fighter.gameObject : player;
        skill = null; // force re-fetch
        UpdateUI();
    }

    public void UpdateUI()
    {
        //Debug.Log("Se actuliza UI" + gameObject.name);
        

        if (skill == null)
            GetSkill();

        if (statusMod == null && healthModSkill == null && applySCSkill == null)
        {
            //this.gameObject.SetActive(false);
            return;
        }
        
        if (skill.ItemsNeeded.Count > 0)
            skill.HasItemsInInventory();

        if (!skill.HasItemInInventory && skill.ItemsNeeded.Count > 0)
            return;

        skillname.text = skill.skillName;
        shortDescripcion.text = skill.SkillDesc;

        //if (statusMod != null)
        //    skillPower.text = statusMod.amount.ToString();

        //if (healthModSkill != null)
        //    skillPower.text = healthModSkill.amount.ToString();

        skillIcon.sprite = skill.iconUI;
        skillIcon.color = Color.white;
    }

    private void GetSkill()
    {
        if (player == null)
            Debug.Log("PLAYER ES NULL EN SkillUI!!!", this);

        var skills = player.GetComponentsInChildren<Skill>(true);
        if (skills == null || skillIndex < 0 || skillIndex >= skills.Length)
        {
            Debug.LogWarning($"[SkillUI.GetSkill] Invalid index {skillIndex} (skillsCount={(skills!=null?skills.Length:0)}) on {gameObject.name}", this);
            return;
        }

        skill = skills[skillIndex];

        healthModSkill = skills[skillIndex].gameObject.GetComponent<HealthModSkill>();
        statusMod = skills[skillIndex].gameObject.GetComponent<StatusMod>();
        applySCSkill = skills[skillIndex].gameObject.GetComponent<ApplySCSkill>();

    }


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

