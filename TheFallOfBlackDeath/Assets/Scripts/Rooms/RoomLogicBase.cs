using UnityEngine;

/// <summary>
/// Abstract base class for modular room logic. 
/// Specific room behaviors must inherit from this and implement ExecuteLogic.
/// </summary>
public abstract class RoomLogicBase : MonoBehaviour
{
    /// <summary>
    /// Executes the primary logic specific to the derived room type.
    /// </summary>
    public abstract void ExecuteLogic();
}