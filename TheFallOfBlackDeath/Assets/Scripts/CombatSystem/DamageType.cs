using UnityEngine;

/// <summary>
/// Defines the named values used by damage type.
/// </summary>
public enum DamageType 
{ 
    Kinetic, // Balas, golpes fÃ­sicos
    Thermal, // Fuego, lÃ¡ser
    EMP,     // ElÃ©ctrico, hackeo
    Chemical // Ãcido, veneno
}

/// <summary>
/// Defines the named values used by part status.
/// </summary>
public enum PartStatus 
{ 
    None, 
    Corroded, 
    Electrified, 
    Burning, 
    Bleeding 
}
