using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour, IIntereractable
{
    public Sprite itemIcon;
    public int amount;

    public void Interact(Movent player)
    {
        Destroy(this.gameObject);
        player.AddToInventory(item:this);
        
    }
}
