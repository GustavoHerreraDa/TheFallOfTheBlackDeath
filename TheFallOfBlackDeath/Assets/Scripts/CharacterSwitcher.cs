using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Synchronizes the selected main and secondary party members between scene objects, UI, and the fighter database.
/// </summary>
public class CharacterSwitcher : MonoBehaviour
{
    public List<GameObject> characters;
    public globalDataBase fightersDateBase;
    public int currentMainCharacterIndex;
    public int currentSecondaryCharacterIndex;

    public delegate void UpdateMainCharacterUI(bool isPreview, string previewId);
    public static event UpdateMainCharacterUI updateMainCharacterUI;
    public delegate void UpdateSecondaryCharacterUI(bool isPreview, string previewId);
    public static event UpdateSecondaryCharacterUI updateSecondaryCharacterUI;

    public static void NotifyStatsPreview(bool isPreview, string previewId)
    {
        updateMainCharacterUI?.Invoke(isPreview, previewId);
        updateSecondaryCharacterUI?.Invoke(isPreview, previewId);
    }

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        SetIndex();
        SwitchMainCharacter(currentMainCharacterIndex, true);
        SwitchSecondaryCharacter(currentSecondaryCharacterIndex, true);

        if (characters == null)
            Debug.LogError("characters es null");
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="characters.Count">The characters.count.</param>
        /// <returns>The resulting value.</returns>
        else if (characters.Count == 0)
            Debug.LogError("characters est  vac o!");
    }

    /// <summary>
    /// Changes the main party selection in the fighter database and notifies dependent UI.
    /// </summary>
    /// <param name="characterIndex">The character index.</param>
    /// <param name="isFirstTime">The is first time.</param>
    public void SwitchMainCharacter(int characterIndex, bool isFirstTime)
    {
        if (fightersDateBase == null || characters == null || characterIndex < 0 || characterIndex >= characters.Count)
            return;

        // Clear previous main flag without relying on GameManager
        for (int i = 0; i < fightersDateBase.EnemyDB.Count; i++)
        {
            if (fightersDateBase.EnemyDB[i].isMainCharacter)
            {
                fightersDateBase.SetMainCharacter(fightersDateBase.EnemyDB[i].CharacterSwitcherIndex, false);
            }
        }

        currentMainCharacterIndex = characterIndex;
        var pf = characters[characterIndex]?.GetComponent<PlayerFighter>();
        if (pf != null)
        {
            GameManager.Instance?.SetMainCharacter(pf);
            fightersDateBase.SetMainCharacter(pf.figherIndex, true);
        }

        updateMainCharacterUI?.Invoke(false, "");
    }

    /// <summary>
    /// Changes the secondary party selection in the fighter database and notifies dependent UI.
    /// </summary>
    /// <param name="characterIndex">The character index.</param>
    /// <param name="isFirstTime">The is first time.</param>
    public void SwitchSecondaryCharacter(int characterIndex, bool isFirstTime)
    {
        if (fightersDateBase == null || characters == null || characterIndex < 0 || characterIndex >= characters.Count)
            return;

        // Clear previous secondary flag without relying on GameManager
        for (int i = 0; i < fightersDateBase.EnemyDB.Count; i++)
        {
            if (fightersDateBase.EnemyDB[i].isSecondaryCharacter)
            {
                fightersDateBase.SetSecondaryCharacter(fightersDateBase.EnemyDB[i].CharacterSwitcherIndex, false);
            }
        }

        currentSecondaryCharacterIndex = characterIndex;
        var pf = characters[characterIndex]?.GetComponent<PlayerFighter>();
        if (pf != null)
        {
            GameManager.Instance?.SetSecondaryCharacter(pf);
            fightersDateBase.SetSecondaryCharacter(pf.figherIndex, true);
        }

        updateSecondaryCharacterUI?.Invoke(false, "");
    }

    /// <summary>
    /// Sets the index.
    /// </summary>
    private void SetIndex()
    {
        for (int i = 0; i < fightersDateBase.EnemyDB.Count; i++)
        {
            if (fightersDateBase.EnemyDB[i].isMainCharacter)
            {
                currentMainCharacterIndex = fightersDateBase.EnemyDB[i].CharacterSwitcherIndex;
                Debug.Log("Main Character es " + fightersDateBase.EnemyDB[i].Name);
            }
        }

        for (int i = 0; i < fightersDateBase.EnemyDB.Count; i++)
        {
            if (fightersDateBase.EnemyDB[i].isSecondaryCharacter)
            {
                currentSecondaryCharacterIndex = fightersDateBase.EnemyDB[i].CharacterSwitcherIndex;
                Debug.Log("Secondary Character es " + fightersDateBase.EnemyDB[i].Name);
            }
        }
    }
}
