using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MostrarDaño : MonoBehaviour
{
    public GameObject dañoVisual;
    void InstanciarDañoVisual()
    {
        GameObject go = Instantiate(dañoVisual, transform.position + Random.onUnitSphere, Quaternion.identity) as GameObject;
        go.GetComponent<Dañocanvas>().Inicializar(Random.Range(1, 10));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
