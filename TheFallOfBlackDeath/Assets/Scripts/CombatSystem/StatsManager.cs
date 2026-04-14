using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//TP2 GUSTAVO TORRES/FACUNDO FERREIRO

/// <summary>
/// Supports the combat system by handling stats manager.
/// </summary>
public class StatsManager : MonoBehaviour
{
    [Header("Referencias")]
    public Fighter fighter;

    [Header("UI Elements")]
    public TextMeshProUGUI actualDefense;
    public TextMeshProUGUI actualAttack;

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    private void Awake()
    {
        // Buscar elementos UI solo si no estÃ¡n asignados en el inspector
        if (actualAttack == null)
        {
            GameObject attackObj = GameObject.Find("Txt_Attack");
            if (attackObj != null)
                actualAttack = attackObj.GetComponent<TextMeshProUGUI>();
            else
                Debug.LogWarning("No se encontrÃ³ el objeto 'Txt_Attack'");
        }
        if (actualDefense == null)
        {
            GameObject defenseObj = GameObject.Find("Txt_Defense");
            if (defenseObj != null)
                actualDefense = defenseObj.GetComponent<TextMeshProUGUI>();
            else
                Debug.LogWarning("No se encontrÃ³ el objeto 'Txt_Defense'");
        }
    }

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
        // Validar que tenemos todas las referencias necesarias
        if (ValidateReferences())
        {
            UpdateUI();
        }
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    void Update()
    {
        // Descomenta si necesitas actualizaciÃ³n constante
        // if (ValidateReferences())
        //     UpdateUI();
    }

    /// <summary>
    /// Executes the validate references workflow.
    /// </summary>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    private bool ValidateReferences()
    {
        if (fighter == null)
        {
            Debug.LogError("Fighter no estÃ¡ asignado en StatsManager");
            return false;
        }

        if (actualAttack == null || actualDefense == null)
        {
            Debug.LogError("Referencias de UI no estÃ¡n completas en StatsManager");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Updates the ui.
    /// </summary>
    public void UpdateUI()
    {
        if (!ValidateReferences())
            return;

        var currentStats = fighter.GetCurrentStats();

        if (currentStats != null)
        {
            SetDefense(currentStats.deffense);
            SetAttack(currentStats.attack);
            Debug.Log($"EstadÃ­sticas actualizadas para el jugador: {fighter.name}");
        }
        else
        {
            Debug.LogError("No se pudieron obtener las estadÃ­sticas del fighter");
        }
    }

    /// <summary>
    /// Sets the defense.
    /// </summary>
    /// <param name="deffense">The deffense.</param>
    public void SetDefense(float deffense)
    {
        if (actualDefense == null)
            return;

        actualDefense.text = deffense.ToString("F0");

        // Resetear color por defecto
        actualDefense.color = Color.white;

        // Aplicar colores segÃºn el valor
        if (deffense >= 80)
        {
            actualDefense.color = Color.yellow;
        }
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="<">The <.</param>
        /// <returns>The resulting value.</returns>
        else if (deffense <= 20)
        {
            actualDefense.color = Color.red;
        }
        else
        {
            actualDefense.color = Color.green; // Color para valores medios
        }
    }

    /// <summary>
    /// Sets the attack.
    /// </summary>
    /// <param name="attack">The attack.</param>
    public void SetAttack(float attack)
    {
        if (actualAttack == null)
            return;

        actualAttack.text = attack.ToString("F0");

        // Resetear color por defecto
        actualAttack.color = Color.white;

        // Aplicar colores segÃºn el valor
        if (attack >= 80)
        {
            actualAttack.color = Color.yellow;
        }
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="<">The <.</param>
        /// <returns>The resulting value.</returns>
        else if (attack <= 20)
        {
            actualAttack.color = Color.red;
        }
        else
        {
            actualAttack.color = Color.green; // Color para valores medios
        }
    }

    // MÃ©todo pÃºblico para actualizar UI desde otros scripts
    /// <summary>
    /// Refreshes the ui.
    /// </summary>
    public void RefreshUI()
    {
        UpdateUI();
    }
}
