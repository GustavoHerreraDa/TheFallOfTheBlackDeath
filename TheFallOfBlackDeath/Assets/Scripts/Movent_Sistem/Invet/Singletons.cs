using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singletons : InventoryManager
{
    public static InventoryObjectID Inventory;

    void Awake()
    {
        if(true)
        {

        }
    }

    private void Update()
    {
        _ = Inventory;
    }
}
