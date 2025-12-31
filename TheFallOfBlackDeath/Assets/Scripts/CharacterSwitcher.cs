using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//ignacio chumba
public class CharacterSwitcher : MonoBehaviour
{
    public List<GameObject> characters;
    public EnemyDataBase fightersDateBase;
    public int currentMainCharacterIndex;
    public int currentSecondaryCharacterIndex;

    public delegate void UpdateMainCharacterUI();

    public CharacterSwitcher(List<GameObject> characters, EnemyDataBase fightersDateBase, int currentMainCharacterIndex, int currentSecondaryCharacterIndex)
    {
        this.characters = characters;
        this.fightersDateBase = fightersDateBase;
        this.currentMainCharacterIndex = currentMainCharacterIndex;
        this.currentSecondaryCharacterIndex = currentSecondaryCharacterIndex;
    }

    public static event UpdateMainCharacterUI updateMainCharacterUI;
    public delegate void UpdateSecondaryCharacterUI();
    public static event UpdateSecondaryCharacterUI updateSecondaryCharacterUI;

    void Start()
    {
        SetIndex();
        SwitchMainCharacter(currentMainCharacterIndex, true);
        SwitchSecondaryCharacter(currentSecondaryCharacterIndex, true);

        if (characters == null)
            Debug.LogError("characters es null");
        else if (characters.Count == 0)
            Debug.LogError("characters est� vac�o!");

        SwitchMainCharacter(0, true);
    }

    public void SwitchMainCharacter(int characterIndex, bool isFirstTime)
    {
        if (GameManager.Instance == null || fightersDateBase == null || characters == null || characters.Count == 0)
        {
            Debug.LogWarning("SwitchMainCharacter: missing dependencies");
            return;
        }

        if (characterIndex < 0 || characterIndex >= characters.Count)
        {
            Debug.LogWarning($"SwitchMainCharacter: index {characterIndex} out of range");
            return;
        }

        // Validar que character1 exista antes de intentar quitarle el estado de Main
        if (GameManager.Instance.character1 != null)
        {
            fightersDateBase.SetMainCharacter(GameManager.Instance.character1.figherIndex, false);
        }

        currentMainCharacterIndex = characterIndex;

        // Validar que el objeto en la lista tenga el componente
        var newMain = characters[characterIndex].GetComponent<PlayerFighter>();
        if (newMain != null)
        {
            GameManager.Instance.character1 = newMain;
            fightersDateBase.SetMainCharacter(newMain.figherIndex, true);

            // Notify decoupled listeners (UI, etc.)
            InventoryManager.NotifyCharacterChanged(newMain);
        }

        // Uso del operador ?. para invocar el evento solo si tiene suscriptores
        updateMainCharacterUI?.Invoke();
    }

    public void SwitchSecondaryCharacter(int characterIndex, bool isFirstTime)
    {
        if (GameManager.Instance == null || fightersDateBase == null || characters == null || characters.Count == 0)
        {
            Debug.LogWarning("SwitchSecondaryCharacter: missing dependencies");
            return;
        }

        if (characterIndex < 0 || characterIndex >= characters.Count)
        {
            Debug.LogWarning($"SwitchSecondaryCharacter: index {characterIndex} out of range");
            return;
        }

        if (GameManager.Instance.character2 != null)
        {
            fightersDateBase.SetSecondaryCharacter(GameManager.Instance.character2.figherIndex, false);
        }

        currentSecondaryCharacterIndex = characterIndex;

        var newSecondary = characters[characterIndex].GetComponent<PlayerFighter>();
        if (newSecondary != null)
        {
            GameManager.Instance.character2 = newSecondary;
            fightersDateBase.SetSecondaryCharacter(newSecondary.figherIndex, true);
            // Notify listeners; pass secondary too for general updates
            InventoryManager.NotifyCharacterChanged(newSecondary);
        }

        updateSecondaryCharacterUI?.Invoke();
    }

    private void SetIndex()
    {
        for (int i = 0; i < fightersDateBase.EnemyDB.Count; i++)
        {
            if (fightersDateBase.EnemyDB[i].isMainCharacter)
            {

                currentMainCharacterIndex = fightersDateBase.EnemyDB[i].CharacterSwitcherIndex;
                Debug.Log("Main Character es " + fightersDateBase.EnemyDB[i].Name);
                GameManager.Instance.SavePlayerState(GameManager.Instance.character1);
            }
        }

        for (int i = 0; i < fightersDateBase.EnemyDB.Count; i++)
        {
            if (fightersDateBase.EnemyDB[i].isSecondaryCharacter)
            {
                currentSecondaryCharacterIndex = fightersDateBase.EnemyDB[i].CharacterSwitcherIndex;
                Debug.Log("Secondary Character es " + fightersDateBase.EnemyDB[i].Name);
                GameManager.Instance.SavePlayerState(GameManager.Instance.character2);
            }
        }
    }
}
