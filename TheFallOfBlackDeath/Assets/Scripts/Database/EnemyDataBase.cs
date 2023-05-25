using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyDB", menuName = "Enemy/List", order = 1)]
public class EnemyDataBase : ScriptableObject
{
    

    [System.Serializable]
    public struct EnemyStats
    {

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
    

    public EnemyStats[] EnemyDB;
}

