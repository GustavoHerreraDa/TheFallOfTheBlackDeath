using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//TP2 GUSTAVO HERRERA/FACUNDO FERREIRO
public enum KeyType
{
    BronzeKey,
    SilverKey,
    GoldKey,
    Portal
}

public class Gate : MonoBehaviour
{
    private Animator animator;
    public bool IsNeedKey;
    public bool HasKey;
    public Collider collider;
    public bool isOpen;
    public KeyType gateType;
    public AudioSource puertasonido;
    public AudioSource puertaCerradaSonido;
    public delegate void GateOpenedEventHandler();
    public static event GateOpenedEventHandler GateOpened;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OpenGate()
    {
        var HasKey = true;

        if (IsNeedKey)
        {
            HasKey = InventoryManager.instance.HasItemInIventory(GetKey(), 1);

            animator.SetBool("IsOpen", HasKey);

            if (collider != null)
                collider.enabled = !HasKey;
            isOpen = HasKey;
        }

        animator.SetBool("IsOpen", HasKey);

        if (HasKey)
        {
            GateOpened?.Invoke();
            PlayGateSound();
        }
        else PlayGateCloseSound();


    }

    private void PlayGateSound()
    {
        if (puertasonido != null)
            puertasonido.Play();
    }

    private void PlayGateCloseSound()
    {
        if (puertaCerradaSonido != null)
            puertaCerradaSonido.Play();
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
            case KeyType.Portal:
                return 11;
            default:
                return 7;
        }
    }

    private void OnDestroy()
    {
        GateOpened = null;
    }
}