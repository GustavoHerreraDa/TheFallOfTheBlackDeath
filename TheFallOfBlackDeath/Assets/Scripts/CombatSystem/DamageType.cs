using UnityEngine;

public enum DamageType 
{ 
    Kinetic, // Balas, golpes físicos
    Thermal, // Fuego, láser
    EMP,     // Eléctrico, hackeo
    Chemical // Ácido, veneno
}

public enum PartStatus 
{ 
    None, 
    Corroded, 
    Electrified, 
    Burning, 
    Bleeding 
}