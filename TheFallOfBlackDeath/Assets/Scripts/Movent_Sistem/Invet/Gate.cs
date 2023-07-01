using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    // Start is called before the first frame update
    private Animator animator;
    public bool IsNeedKey;
    public Collider collider;
    public bool isOpen;
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
        var hasKey = true;

        if (IsNeedKey)
            hasKey = InventoryManager.instance.HasItemInIventory(7, 1);

        animator.SetBool("IsOpen", hasKey);
    }
}
