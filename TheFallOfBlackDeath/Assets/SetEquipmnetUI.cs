using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CanvasType
{
    Object,
    Equipment
}
public class SetEquipmnetUI : MonoBehaviour
{
    // Start is called before the first frame update
    public CanvasType canvasType;

    void Awake()
    {
        Debug.Log("Actualizo canvas equipmento o objet");
        if (canvasType == CanvasType.Equipment)
            InventoryManager.instance.equipmentUI = this.transform;
        if (canvasType == CanvasType.Object)
            InventoryManager.instance.objetsUI = this.transform;


    }

    // Update is called once per frame
    void Update()
    {

    }
}
