using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Supports inventory and interaction flow by handling singletons.
/// </summary>
public class Singletons : InventoryManager
{
    public static InventoryObjectID Inventory;

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    void Awake()
    {
        if(true)
        {

        }
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    private void Update()
    {
        _ = Inventory;
    }
}
