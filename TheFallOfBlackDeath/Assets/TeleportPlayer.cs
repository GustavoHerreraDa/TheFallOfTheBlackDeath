using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
    public GameObject player;
    public Transform destino;
    private CharacterController _characterController;

    void Start()
    {
        _characterController = player.GetComponent<CharacterController>();
    }

    void OnTriggerEnter(Collider other)
    {
        _characterController.enabled = false; //Es necesario desactivar el character controller antes de teletransportar al PJ.
        player.transform.position = destino.position;
        _characterController.enabled = true;
    }

    
}