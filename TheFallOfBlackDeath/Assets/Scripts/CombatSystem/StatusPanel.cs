using TMPro;
using UnityEngine;
using UnityEngine.UI;
//TP2 FACUNDO FERREIRO

/// <summary>
/// Supports the combat system by handling status panel.
/// </summary>
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

    /// <summary>
    /// Sets the stats.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="stats">The stats.</param>
    public void SetStats(string name, Stats stats)
    {
        if (nameLabel != null)
            this.nameLabel.text = name;

        if (nameTextLabel != null)
        {
            this.nameTextLabel.text = name;

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
    
    /// <summary>
    /// Sets the health.
    /// </summary>
    /// <param name="health">The health.</param>
    /// <param name="maxHealth">The max health.</param>
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
        
    }
    
}
