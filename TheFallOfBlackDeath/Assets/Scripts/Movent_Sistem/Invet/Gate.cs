using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    // Start is called before the first frame update
    private Animator animator;
    public bool IsNeedKey;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OpenGate()
    {
        //var hasKey = InventoryManager.instance.HasItemInIventory()
        animator.SetBool("IsOpen", true);
    }
}
