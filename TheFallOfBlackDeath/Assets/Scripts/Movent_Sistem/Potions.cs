
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
public class Potions : MonoBehaviour, IIntereractable
{
    private int amount;

    private void Awake()
    {
        amount = Random.Range(1,3);  
    }

    public void Interact(Movent player)
    {
        player.AddPotions(amount);
        Destroy(this.gameObject);
    }
}