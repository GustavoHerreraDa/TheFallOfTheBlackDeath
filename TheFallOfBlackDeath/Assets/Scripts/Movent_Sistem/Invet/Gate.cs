using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InventoryManager;

public enum KeyType
{
    BronzeKey,
    SilverKey,
    GoldKey
}
public class Gate : MonoBehaviour
{
    // Start is called before the first frame update
    public static Gate instance;

    private Animator animator;
    public bool IsNeedKey;
    public Collider collider;
    public bool isOpen;
    public KeyType gateType;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (Gate.instance == null)
        {
            Gate.instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            animator.SetBool("IsOpen", isOpen);
            if (collider != null)
                collider.enabled = !isOpen;
        }
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OpenGate()
    {
        var hasKey = true;

        if (IsNeedKey)
        {
            hasKey = InventoryManager.instance.HasItemInIventory(GetKey(), 1);

            animator.SetBool("IsOpen", hasKey);
            collider.enabled = !hasKey;
            isOpen = hasKey;
        }
    }


    public int GetKey()
    {
        switch (gateType)
        {
            case KeyType.BronzeKey:
                return 7;
            case KeyType.SilverKey:
                return 9;
            case KeyType.GoldKey:
                return 10;
            default:
                return 7;
        }
    }
}
