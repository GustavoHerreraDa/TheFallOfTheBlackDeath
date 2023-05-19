using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class seguirjugador : MonoBehaviour
{
    public Transform jugador;
    UnityEngine.AI.NavMeshAgent enemigo;
    private bool dentro = false;
    // Start is called before the first frame update
    void Start()
    {
        enemigo = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Character")
        {
            dentro = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Character")
        {
            dentro =false;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if(!dentro)
        {
            enemigo.destination = jugador.position;
        }
        if (dentro)
        {
            enemigo.destination = this.transform.position;
        }
    }
}
