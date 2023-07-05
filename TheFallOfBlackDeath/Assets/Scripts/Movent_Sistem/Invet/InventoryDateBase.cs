using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TP2 AUGUSTO NANINI
[CreateAssetMenu(fileName = "Date", menuName = "Inventory/List", order = 1)]
public class InventoryDateBase : ScriptableObject
{
    [Tooltip("Objetos equipables aumentan las stats del personaje que eligen el jugador por ejemplo la tiara o la bota que suben la vida, o la scimitarra." +
        "Objetos Usables aumentan las stats del personaje que eligen el jugador, por ejemplo la pocion de fuerza o la pocion de vida." +
        "Objetos Consumibles para utilizar al menos vez, por ejemplo la llave. Para abrir una puierta" +
        "Objetos Skillneed habilitan alguna skill o la potencian."
        )]
    [System.Serializable]
    public struct Object
    {

        public string name;
        public Sprite sprite;
        public Uso uso;
        public string characteristic;
        public string funtion;
        public StatsUpgrade statsAffected;
        public bool skillAffection;
        public float amountAffected;

    }
    //Objetos equipables aumentan las stats del personaje que eligen el jugador por ejemplo la tiara o la bota que suben la vida, o la scimitarra.
    //Objetos Usables aumentan las stats del personaje que eligen el jugador, por ejemplo la pocion de fuerza o la pocion de vida.
    //Objetos Consumibles para utilizar al menos vez, por ejemplo la llave. Para abrir una puierta
    //Objetos Skillneed habilitan alguna skill o la potencian.
    public enum Uso { Equipable, Usable, Consumable, SkillNeed }
    public enum StatsUpgrade {None, Health, Attack, Defense, Speed, Spirit }


    public Object[] DateBase;
}

