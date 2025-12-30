using TMPro;
using UnityEngine;
using UnityEngine.UI;
//TP2 FACUNDO FERREIRO

public class StatusPanel : MonoBehaviour
{
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI levelLabel;
    public TextMeshProUGUI healthLabel;
    public TextMeshProUGUI actualDefense;
    public TextMeshProUGUI actualAttack;
    public Slider healthSlider;
    public Image healthSliderBar;
    public TextMeshProUGUI healthLabelPro;
    public TextMeshProUGUI nameTextLabel;
    private Material glitchMaterial;

    public void SetStats(string name, Stats stats)
    {
        if (nameLabel != null)
            this.nameLabel.text = name;

        if (nameTextLabel != null)
        {
            this.nameTextLabel.text = name;
            this.nameTextLabel.fontSize = name.Length > 8 ?  4 : 6;

        }
        if (levelLabel != null)
            this.levelLabel.text = $"N. {stats.level}";
        if(actualAttack != null)
        {
            this.actualAttack.text = $"{stats.attack}";
        }
        if(actualDefense != null)
        {
            this.actualDefense.text = $"{stats.deffense}";
        }
        

        this.SetHealth(stats.health, stats.maxHealth);

    }

    void Awake()
    {
        if (nameTextLabel != null)
        {
            
            glitchMaterial = nameTextLabel.fontMaterial;
        }
    }

    public void SetHealth(float health, float maxHealth)
    {
        if (healthLabelPro != null)
            healthLabelPro.text = $"{Mathf.RoundToInt(health)} / {Mathf.RoundToInt(maxHealth)}";

        if (healthLabel != null)
            healthLabel.text = $"{Mathf.RoundToInt(health)} / {Mathf.RoundToInt(maxHealth)}";

        float percentage = health / maxHealth;
        healthSlider.value = percentage;

        if (percentage < 0.33f)
            healthSliderBar.color = Color.red;

        UpdateGlitchEffect(percentage);
    }

    void UpdateGlitchEffect(float lifePercent)
    {
        if (glitchMaterial == null) return;

        // Invertimos: poca vida = valor alto
        float glitchStrength = 1f - lifePercent;

        glitchMaterial.SetFloat("_GlitchAmount", Mathf.Lerp(0f, 15f, glitchStrength));
        glitchMaterial.SetFloat("_GlitchTime", Mathf.Lerp(0f, 10f, glitchStrength));
        glitchMaterial.SetFloat("_GlitchOffset", Mathf.Lerp(0f, 0.03f, glitchStrength));
        glitchMaterial.SetFloat("_GlitchOffset2", Mathf.Lerp(0f, 0.02f, glitchStrength));
        glitchMaterial.SetFloat("_ScanLinesAmount", Mathf.Lerp(0f, 120f, glitchStrength));
    }

}