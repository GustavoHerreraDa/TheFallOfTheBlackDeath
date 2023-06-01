using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatusWorldPanel : MonoBehaviour
{
    // Start is called before the first frame update
    public Slider healthSlider;
    public Image healthSliderBar;
    public TextMeshProUGUI nameLabel;
    public void SetStats(string name, Stats stats)
    {
        this.nameLabel.text = name;

        //this.levelLabel.text = $"N. {stats.level}";
        this.SetHealth(stats.health, stats.maxHealth);
    }
    public void SetHealth(float health, float maxHealth)
    {
        //Matfh convierte el flotante health en numeros enteros para que no salgan decimales
        //this.healthLabel.text = $"{Mathf.RoundToInt(health)} / {Mathf.RoundToInt(maxHealth)}";
        float percentage = health / maxHealth;

        this.healthSlider.value = percentage;
        //Si el porcentaje de vida es menor al 33% el color de la vida se vuelve rojo
        if (percentage < 0.33f)
        {
            this.healthSliderBar.color = Color.red;
        }
        {

        }
    }
}
