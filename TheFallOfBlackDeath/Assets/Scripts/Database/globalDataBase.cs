using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyDB", menuName = "Enemy/List", order = 1)]
/// <summary>
/// Stores fighter and enemy definitions used to spawn party members and combatants and to keep character selection flags.
/// </summary>
public class globalDataBase : ScriptableObject
{
    [System.Serializable]
    /// <summary>
    /// Stores the serialized stats and selection flags associated with a fighter or enemy entry in the global database.
    /// </summary>
    public struct EnemyStats
    {
        public bool isMainCharacter;
        public bool isSecondaryCharacter;
        public GameObject enemyPrefab;
        public int prefabIndex;
        public float maxHealth;
        public float hp;
        public int level;
        public float attack;
        public float deffense;
        public float spirit;
        public float speed;
        public int experience;
        public int experienceToNextLevel;
        public string Description;
        public string LargeDescription;
        public string Name;   
        public int CharacterSwitcherIndex;
        public Sprite characterImage;
        public List<BodyPart> destroyedParts;
        public float currentHealth;
    }

    /// <summary>
    /// Updates the fighter stats.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="amountAffected">The amount affected.</param>
    /// <param name="statAffected">The stat affected.</param>
    public void UpdateFighterStats(int index, float amountAffected, InventoryNew.StatType statAffected)
    {
        if (index < 0 || index >= EnemyDB.Count) return;
        
        EnemyStats stats = EnemyDB[index];
        switch (statAffected)
        {
            case InventoryNew.StatType.Attack:
                stats.attack += amountAffected;
                break;
            case InventoryNew.StatType.Defense:
                stats.deffense += amountAffected;
                break;
            case InventoryNew.StatType.MaxHealth:
                stats.maxHealth += amountAffected;
                break;
            case InventoryNew.StatType.Speed:
                stats.speed += amountAffected;
                break;
            case InventoryNew.StatType.Spirit:
                stats.spirit += amountAffected;
                break;
        }
        EnemyDB[index] = stats;
    }

    /// <summary>
    /// Sets the main character.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="isCharacter">The is character.</param>
    public void SetMainCharacter(int index, bool isCharacter)
    {
        if (index < 0 || index >= EnemyDB.Count) return;

        EnemyStats stats = EnemyDB[index];
        stats.isMainCharacter = isCharacter;
        EnemyDB[index] = stats;
    }

    /// <summary>
    /// Sets the secondary character.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="isCharacter">The is character.</param>
    public void SetSecondaryCharacter(int index, bool isCharacter)
    {
        if (index < 0 || index >= EnemyDB.Count) return;

        EnemyStats stats = EnemyDB[index];
        stats.isSecondaryCharacter = isCharacter;
        EnemyDB[index] = stats;
    }

    //public EnemyStats[] EnemyDB;
    public List<EnemyStats> EnemyDB = new List<EnemyStats>(); // Cambie el array por una lista
}

