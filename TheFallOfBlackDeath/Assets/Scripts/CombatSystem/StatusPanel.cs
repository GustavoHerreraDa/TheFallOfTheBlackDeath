using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusPanel : MonoBehaviour
{
    public Text nameLabel;
    public Text levelLabel;
    public Text healthLabel;
    public TextMeshProUGUI actualDefense;
    public TextMeshProUGUI actualAttack;
    public Slider healthSlider;
    public Image healthSliderBar;
    public TextMeshProUGUI healthLabelPro;
    public TextMeshProUGUI nameTextLabel;



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

        this.SetHealth(stats.health, stats.maxHealth);

    }
    public void SetHealth(float health, float maxHealth)
    {
        //Matfh convierte el flotante health en numeros enteros para que no salgan decimales
        if (healthLabelPro != null)
            this.healthLabelPro.text = $"{Mathf.RoundToInt(health)} / {Mathf.RoundToInt(maxHealth)}";

        if (healthLabel != null)
            this.healthLabel.text = $"{Mathf.RoundToInt(health)} / {Mathf.RoundToInt(maxHealth)}";

        float percentage = health / maxHealth;

        this.healthSlider.value = percentage;
        //Si el porcentaje de vida es menor al 33% el color de la vida se vuelve rojo
        if (percentage < 0.33f)
        {
            this.healthSliderBar.color = Color.red;
        }
    }
}