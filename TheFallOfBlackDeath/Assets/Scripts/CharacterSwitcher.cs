using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    public List<GameObject> characters;
    public EnemyDataBase fightersDateBase;
    public int currentMainCharacterIndex;
    public int currentSecondaryCharacterIndex;

    /* public delegate void UpdateInventory();
    public static event UpdateInventory OnInventoryUpdate;

    public delegate void UpdateStatsUI();
    public static event UpdateStatsUI OnStatsUpdate; */

    void Start()
    {
        SetIndex();
        SwitchMainCharacter(currentMainCharacterIndex, true);
        SwitchSecondaryCharacter(currentSecondaryCharacterIndex);
    }

    public void SwitchMainCharacter(int characterIndex, bool isFirstTime)     //ARREGLAR: PROBLEMA SI EL DOC ES EL PRINCIPAL CUANDO EMPIEZA LA ESCENA
    {
        //Si selecciona como principal el personaje que ya es principal, retornamos.
        if (currentMainCharacterIndex == characterIndex && !isFirstTime)
        {
            Debug.Log("Hola");
            return;
        }

        //Si selecciona como principal el personaje que es secundario, return.
        if(characterIndex == currentSecondaryCharacterIndex)
        {
            Debug.Log("El personaje ya está seleccionado.");
            return;
        }

        //Es complicado conseguir la referencia del Doctor porque está en el doctor que flota en el escenario, no en el que manejas.
        if(characterIndex == 0)
        {
            fightersDateBase.SetMainCharacter(GameManager.Instance.character1.figherIndex, false);
            fightersDateBase.SetMainCharacter(GameManager.Instance.BadDoctor.figherIndex, true);

            //characters[currentMainCharacterIndex].SetActive(false);
            if(!isFirstTime) //Probar esto, osea meter el for aca  adentro
            {
                for (int i = 0; i < characters[currentMainCharacterIndex].transform.childCount; i++)
                {
                    characters[currentMainCharacterIndex].transform.GetChild(i).gameObject.SetActive(false);
                }
            }
            currentMainCharacterIndex = characterIndex;
            characters[characterIndex].SetActive(true);
            GameManager.Instance.character1 = GameManager.Instance.BadDoctor;
            return;
        }

        //Es complicado conseguir la referencia del Assassin porque está en el assassin que flota en el escenario, no en el que manejas.
        if(characterIndex == 1)
        {
            fightersDateBase.SetMainCharacter(GameManager.Instance.character1.figherIndex, false);
            fightersDateBase.SetMainCharacter(GameManager.Instance.Assassin.figherIndex, true);

            //characters[currentMainCharacterIndex].SetActive(false);

            if(currentMainCharacterIndex == 0)
            {
                characters[currentMainCharacterIndex].SetActive(false);
            }
            else
            {
                for (int i = 0; i < characters[currentMainCharacterIndex].transform.childCount; i++)
                {
                    characters[currentMainCharacterIndex].transform.GetChild(i).gameObject.SetActive(false);
                }
            }

            currentMainCharacterIndex = characterIndex;
            //characters[characterIndex].SetActive(true);
            for (int i = 0; i < characters[currentMainCharacterIndex].transform.childCount; i++)
            {
                characters[currentMainCharacterIndex].transform.GetChild(i).gameObject.SetActive(true);
            }
            GameManager.Instance.character1 = GameManager.Instance.Assassin;
            return;
        }

        fightersDateBase.SetMainCharacter(GameManager.Instance.character1.figherIndex, false);

        if(currentMainCharacterIndex == 0)
        {
            characters[currentMainCharacterIndex].SetActive(false);
        }
        else
        {
            for (int i = 0; i < characters[currentMainCharacterIndex].transform.childCount; i++)
            {
                characters[currentMainCharacterIndex].transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        
        currentMainCharacterIndex = characterIndex;
        //characters[characterIndex].SetActive(true);
        for (int i = 0; i < characters[currentMainCharacterIndex].transform.childCount; i++)
        {
            characters[currentMainCharacterIndex].transform.GetChild(i).gameObject.SetActive(true);
        }
        GameManager.Instance.character1 = characters[characterIndex].GetComponent<PlayerFighter>();

        fightersDateBase.SetMainCharacter(GameManager.Instance.character1.figherIndex, true);

        //Evento para que player UI ponga las pestañas de UI Adecuadas.
        //OnStatsUpdate();
        //Evento para desequipar los items equipables(armas, armadura)
        //OnInventoryUpdate();
        //Evento.
    }

    public void SwitchSecondaryCharacter(int characterIndex)
    {
        //Si selecciona como secundario el personaje que ya es secundario, retornamos.
        /* if(currentSecondaryCharacterIndex == characterIndex)
        {
            return;
        } */

        //Si selecciona como secundario el personaje que es principal, return.
        if(characterIndex == currentMainCharacterIndex)
        {
            Debug.Log("El personaje ya está seleccionado.");
            return;
        }

        if(characterIndex == 0)
        {
            fightersDateBase.SetSecondaryCharacter(GameManager.Instance.character2.figherIndex, false);
            fightersDateBase.SetSecondaryCharacter(GameManager.Instance.BadDoctor.figherIndex, true);

            GameManager.Instance.character2 = GameManager.Instance.BadDoctor;
            currentSecondaryCharacterIndex = characterIndex;
            return;
        }

        if(characterIndex == 1)
        {
            fightersDateBase.SetSecondaryCharacter(GameManager.Instance.character2.figherIndex, false);
            fightersDateBase.SetSecondaryCharacter(GameManager.Instance.Assassin.figherIndex, true);

            GameManager.Instance.character2 = GameManager.Instance.Assassin;
            currentSecondaryCharacterIndex = characterIndex;
            return;
        }


        fightersDateBase.SetSecondaryCharacter(GameManager.Instance.character2.figherIndex, false);

        currentSecondaryCharacterIndex = characterIndex;
        GameManager.Instance.character2 = characters[characterIndex].GetComponent<PlayerFighter>();

        fightersDateBase.SetSecondaryCharacter(GameManager.Instance.character2.figherIndex, true);
    }

    private void SetIndex()
    {
        for (int i = 0; i < fightersDateBase.EnemyDB.Count; i++)
        {
            if(fightersDateBase.EnemyDB[i].isMainCharacter)
            {
                currentMainCharacterIndex = fightersDateBase.EnemyDB[i].CharacterSwitcherIndex;
            }
        }

        for (int i = 0; i < fightersDateBase.EnemyDB.Count; i++)
        {
            if(fightersDateBase.EnemyDB[i].isSecondaryCharacter)
            {
                currentSecondaryCharacterIndex = fightersDateBase.EnemyDB[i].CharacterSwitcherIndex;
            }
        }
    }
}
