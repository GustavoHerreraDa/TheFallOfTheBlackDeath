using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUI : MonoBehaviour
{
    public Skill skill;
    public StatusMod statusMod;
    public HealthModSkill healthModSkill;
    public TextMeshProUGUI shortDescripcion;
    public TextMeshProUGUI skillPower;
    public Image skillIcon;

    public void Start()
    {
        if (skill == null)
            return;

        shortDescripcion.text = skill.SkillDesc;
        if (statusMod == null && healthModSkill == null)
            return;

        if (statusMod != null)
            skillPower.text = statusMod.amount.ToString();

        if (healthModSkill != null)
            skillPower.text = healthModSkill.amount.ToString();

        skillIcon.sprite = skill.iconUI;

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

