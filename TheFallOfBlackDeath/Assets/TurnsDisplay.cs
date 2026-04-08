using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnsDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    public CombatManager CombatManager;
    TextMeshProUGUI[] textComponents;
    void Awake()
    {
        // Obtener todos los componentes Text en los hijos del objeto actual
        textComponents = GetComponentsInChildren<TextMeshProUGUI>();

        // Iterar sobre los componentes encontrados
       
    }

    public void SetText(Fighter[] fighters)
    {
        if (textComponents == null) return;

        int numFighters = fighters != null ? fighters.Length : 0;
        int numTextSlots = textComponents.Length;

        // Limpiar todos los textos primero y desactivarlos
        foreach (var otext in textComponents)
        {
            if (otext != null)
            {
                otext.text = string.Empty;
                otext.gameObject.SetActive(false);
            }
        }

        // Asignar nombres hasta el límite de slots o de luchadores
        for (int i = 0; i < Mathf.Min(numFighters, numTextSlots); i++)
        {
            if (fighters[i] != null && textComponents[i] != null)
            {
                textComponents[i].text = fighters[i].idName;
                textComponents[i].gameObject.SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
