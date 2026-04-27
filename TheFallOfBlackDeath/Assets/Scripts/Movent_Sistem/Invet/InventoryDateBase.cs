using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TP2 AUGUSTO NANINI
[CreateAssetMenu(fileName = "Date", menuName = "Inventory/List", order = 1)]
/// <summary>
/// Stores the item catalog used by the inventory, equipment, and consumable systems.
/// </summary>
public class InventoryDateBase : ScriptableObject
{
    [Tooltip("Objetos equipables aumentan las stats del personaje que eligen el jugador por ejemplo la tiara o la bota que suben la vida, o la scimitarra." +
        "Objetos Usables aumentan las stats del personaje que eligen el jugador, por ejemplo la pocion de fuerza o la pocion de vida." +
        "Objetos Consumibles para utilizar al menos vez, por ejemplo la llave. Para abrir una puierta" +
        "Objetos Skillneed habilitan alguna skill o la potencian."
        )]
    [System.Serializable]
    /// <summary>
    /// Stores the data used by object.
    /// </summary>
    public struct Object
    {
        public Animation animation;
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
    /// <summary>
    /// Defines the named values used by uso.
    /// </summary>
    public enum Uso { Equipable, Usable, Consumable, SkillNeed, BodyPartHeal }
    /// <summary>
    /// Defines the named values used by stats upgrade.
    /// </summary>
    public enum StatsUpgrade {None, Health, Attack, Defense, Speed, Spirit }


    public Object[] DateBase;
}

