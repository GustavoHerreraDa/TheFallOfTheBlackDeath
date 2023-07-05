using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//TP2 GUSTAVO HERRERA/FACUNDO FERREIRO
public enum KeyType
{
    BronzeKey,
    SilverKey,
    GoldKey
}

public class Gate : MonoBehaviour
{
    private Animator animator;
    public bool IsNeedKey;
    public Collider collider;
    public bool isOpen;
    public KeyType gateType;
    public AudioSource puertasonido;

    public delegate void GateOpenedEventHandler();
    public static event GateOpenedEventHandler GateOpened;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OpenGate()
    {
        var hasKey = true;

        if (IsNeedKey)
        {
            hasKey = InventoryManager.instance.HasItemInIventory(GetKey(), 1);

            animator.SetBool("IsOpen", hasKey);

            if (collider != null)
                collider.enabled = !hasKey;
            isOpen = hasKey;
        }

        animator.SetBool("IsOpen", hasKey);

        if (hasKey)
        {
            GateOpened?.Invoke();
        }

        PlayGateSound();
    }

    private void PlayGateSound()
    {
        if (puertasonido != null)
            puertasonido.Play();
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

    private void OnDestroy()
    {
        GateOpened = null;
    }
}