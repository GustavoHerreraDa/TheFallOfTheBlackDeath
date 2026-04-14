using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the named values used by canvas type.
/// </summary>
public enum CanvasType
{
    Object,
    Equipment
}
/// <summary>
/// Handles set equipmnet ui for the current project workflow.
/// </summary>
public class SetEquipmnetUI : MonoBehaviour
{
    // Start is called before the first frame update
    public CanvasType canvasType;

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    void Awake()
    {
        Debug.Log("Actualizo canvas equipmento o objet");
        if (InventoryManager.instance == null)
            return;
        if (canvasType == CanvasType.Equipment)
            InventoryManager.instance.equipmentUI = this.transform;
        if (canvasType == CanvasType.Object)
            InventoryManager.instance.objetsUI = this.transform;



    }
    
}
