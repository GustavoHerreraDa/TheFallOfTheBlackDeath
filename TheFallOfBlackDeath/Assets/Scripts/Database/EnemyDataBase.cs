using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyDB", menuName = "Enemy/List", order = 1)]
public class EnemyDataBase : ScriptableObject
{
    [System.Serializable]
    public struct EnemyStats
    {
        public GameObject enemyPrefab;
        public int prefabIndex;
        public float maxHealth;
        public int level;
        public float attack;
        public float deffense;
        public float spirit;
        public float speed;
        public string Description;
        public string LargeDescription;
        public string Name;
          
    }

    public void UpdateFighterStats(int index, float amountAffected, string statAffected)
    {
        switch (statAffected)
        {
            case "Attack":

                EnemyStats newAttackStats = new EnemyStats
                {
                    maxHealth = EnemyDB[index].maxHealth, 
                    level = EnemyDB[index].level, 
                    attack = EnemyDB[index].attack + amountAffected, 
                    deffense = EnemyDB[index].deffense, 
                    spirit = EnemyDB[index].spirit, 
                    speed = EnemyDB[index].speed, 
                    Description = EnemyDB[index].Description, 
                    LargeDescription = EnemyDB[index].LargeDescription, 
                    Name = EnemyDB[index].Name
                };
                EnemyDB[index] = newAttackStats;
                break;

            case "Defense":

                EnemyStats newDefenseStats = new EnemyStats
                {
                    maxHealth = EnemyDB[index].maxHealth, 
                    level = EnemyDB[index].level, 
                    attack = EnemyDB[index].attack, 
                    deffense = EnemyDB[index].deffense + amountAffected, 
                    spirit = EnemyDB[index].spirit, 
                    speed = EnemyDB[index].speed, 
                    Description = EnemyDB[index].Description, 
                    LargeDescription = EnemyDB[index].LargeDescription, 
                    Name = EnemyDB[index].Name
                };
                EnemyDB[index] = newDefenseStats;
                break;
        }
    }


    //public EnemyStats[] EnemyDB;
    public List<EnemyStats> EnemyDB = new List<EnemyStats>(); // Cambie el array por una lista
}

