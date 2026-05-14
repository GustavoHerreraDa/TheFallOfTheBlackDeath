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
    public void UpdateFighterStats(int index, float amountAffected, InventoryDateBase.StatType statAffected)
    {
        if (index < 0 || index >= EnemyDB.Count) return;
        
        EnemyStats stats = EnemyDB[index];
        switch (statAffected)
        {
            case InventoryDateBase.StatType.Attack:
                stats.attack += amountAffected;
                break;
            case InventoryDateBase.StatType.Defense:
                stats.deffense += amountAffected;
                break;
            case InventoryDateBase.StatType.MaxHealth:
                stats.maxHealth += amountAffected;
                break;
            case InventoryDateBase.StatType.Health:
                stats.hp = Mathf.Clamp(stats.hp + amountAffected, 0, stats.maxHealth);
                break;
            case InventoryDateBase.StatType.Speed:
                stats.speed += amountAffected;
                break;
            case InventoryDateBase.StatType.Spirit:
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
        EnemyStats newCharacterStats = new EnemyStats
        {
            isMainCharacter = isCharacter,
            isSecondaryCharacter = EnemyDB[index].isSecondaryCharacter,
            enemyPrefab = EnemyDB[index].enemyPrefab,
            prefabIndex = EnemyDB[index].prefabIndex,
            maxHealth = EnemyDB[index].maxHealth, 
            hp = EnemyDB[index].hp,
            level = EnemyDB[index].level, 
            attack = EnemyDB[index].attack, 
            deffense = EnemyDB[index].deffense, 
            spirit = EnemyDB[index].spirit, 
            speed = EnemyDB[index].speed,
            experience = EnemyDB[index].experience,
            experienceToNextLevel = EnemyDB[index].experienceToNextLevel,
            Description = EnemyDB[index].Description, 
            LargeDescription = EnemyDB[index].LargeDescription, 
            Name = EnemyDB[index].Name,
            CharacterSwitcherIndex = EnemyDB[index].CharacterSwitcherIndex,
            characterImage = EnemyDB[index].characterImage
        };
        EnemyDB[index] = newCharacterStats;
    }

    /// <summary>
    /// Sets the secondary character.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="isCharacter">The is character.</param>
    public void SetSecondaryCharacter(int index, bool isCharacter)
    {
        EnemyStats newCharacterStats = new EnemyStats
        {
            isMainCharacter = EnemyDB[index].isMainCharacter,
            isSecondaryCharacter = isCharacter,
            enemyPrefab = EnemyDB[index].enemyPrefab,
            prefabIndex = EnemyDB[index].prefabIndex,
            maxHealth = EnemyDB[index].maxHealth, 
            hp = EnemyDB[index].hp,
            level = EnemyDB[index].level, 
            attack = EnemyDB[index].attack, 
            deffense = EnemyDB[index].deffense, 
            spirit = EnemyDB[index].spirit, 
            speed = EnemyDB[index].speed,
            experience = EnemyDB[index].experience,
            experienceToNextLevel = EnemyDB[index].experienceToNextLevel,
            Description = EnemyDB[index].Description, 
            LargeDescription = EnemyDB[index].LargeDescription, 
            Name = EnemyDB[index].Name,
            CharacterSwitcherIndex = EnemyDB[index].CharacterSwitcherIndex,
            characterImage = EnemyDB[index].characterImage
        };
        EnemyDB[index] = newCharacterStats;
    }

    //public EnemyStats[] EnemyDB;
    public List<EnemyStats> EnemyDB = new List<EnemyStats>(); // Cambie el array por una lista
}

