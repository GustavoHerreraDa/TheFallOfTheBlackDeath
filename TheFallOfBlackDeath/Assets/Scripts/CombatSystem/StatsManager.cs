using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//TP2 GUSTAVO TORRES/FACUNDO FERREIRO

public class StatsManager : MonoBehaviour
{
    [Header("Referencias")]
    public Fighter fighter;

    [Header("UI Elements")]
    public TextMeshProUGUI actualDefense;
    public TextMeshProUGUI actualAttack;

    private void Awake()
    {
        // Buscar elementos UI solo si no están asignados en el inspector
        if (actualAttack == null)
        {
            GameObject attackObj = GameObject.Find("Txt_Attack");
            if (attackObj != null)
                actualAttack = attackObj.GetComponent<TextMeshProUGUI>();
            else
                Debug.LogWarning("No se encontró el objeto 'Txt_Attack'");
        }
        if (actualDefense == null)
        {
            GameObject defenseObj = GameObject.Find("Txt_Defense");
            if (defenseObj != null)
                actualDefense = defenseObj.GetComponent<TextMeshProUGUI>();
            else
                Debug.LogWarning("No se encontró el objeto 'Txt_Defense'");
        }
    }

    private void Start()
    {
        // Validar que tenemos todas las referencias necesarias
        if (ValidateReferences())
        {
            UpdateUI();
        }
    }

    void Update()
    {
        // Descomenta si necesitas actualización constante
        // if (ValidateReferences())
        //     UpdateUI();
    }

    private bool ValidateReferences()
    {
        if (fighter == null)
        {
            Debug.LogError("Fighter no está asignado en StatsManager");
            return false;
        }

        if (actualAttack == null || actualDefense == null)
        {
            Debug.LogError("Referencias de UI no están completas en StatsManager");
            return false;
        }

        return true;
    }

    public void UpdateUI()
    {
        if (!ValidateReferences())
            return;

        var currentStats = fighter.GetCurrentStats();

        if (currentStats != null)
        {
            SetDefense(currentStats.deffense);
            SetAttack(currentStats.attack);
            Debug.Log($"Estadísticas actualizadas para el jugador: {fighter.name}");
        }
        else
        {
            Debug.LogError("No se pudieron obtener las estadísticas del fighter");
        }
    }

    public void SetDefense(float deffense)
    {
        if (actualDefense == null)
            return;

        actualDefense.text = deffense.ToString("F0");

        // Resetear color por defecto
        actualDefense.color = Color.white;

        // Aplicar colores según el valor
        if (deffense >= 80)
        {
            actualDefense.color = Color.yellow;
        }
        else if (deffense <= 20)
        {
            actualDefense.color = Color.red;
        }
        else
        {
            actualDefense.color = Color.green; // Color para valores medios
        }
    }

    public void SetAttack(float attack)
    {
        if (actualAttack == null)
            return;

        actualAttack.text = attack.ToString("F0");

        // Resetear color por defecto
        actualAttack.color = Color.white;

        // Aplicar colores según el valor
        if (attack >= 80)
        {
            actualAttack.color = Color.yellow;
        }
        else if (attack <= 20)
        {
            actualAttack.color = Color.red;
        }
        else
        {
            actualAttack.color = Color.green; // Color para valores medios
        }
    }

    // Método público para actualizar UI desde otros scripts
    public void RefreshUI()
    {
        UpdateUI();
    }
}
