using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnsDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    public CombatManager CombatManager;
    Text[] textComponents;
    void Awake()
    {
        // Obtener todos los componentes Text en los hijos del objeto actual
        textComponents = GetComponentsInChildren<Text>();

        // Iterar sobre los componentes encontrados
       
    }

    public void SetText(Fighter[] fighters)
    {
        int i = 0;
        foreach (Fighter oFighter in fighters)
        {
            textComponents[i].text = fighters[i].idName;
            i++;
        }
        foreach (Text otext in textComponents)
        {
            if (otext.text == string.Empty)
                otext.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
