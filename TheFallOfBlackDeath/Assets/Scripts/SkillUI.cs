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

    public void Start()
    {
        skillIcon.color = new Color(1, 1, 1, 0);
        UpdateUI();
    }

    public void UpdateUI()
    {
        Debug.Log("Se actuliza UI" + gameObject.name);

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

        if (statusMod != null)
            skillPower.text = statusMod.amount.ToString();

        if (healthModSkill != null)
            skillPower.text = healthModSkill.amount.ToString();

        skillIcon.sprite = skill.iconUI;
        skillIcon.color = Color.white;
    }

    private void GetSkill()
    {
        var skills = player.GetComponentsInChildren<Skill>();

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

