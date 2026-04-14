using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//TP2 GUSTAVO HERRERA/FACUNDO FERREIRO
/// <summary>
/// Defines the named values used by key type.
/// </summary>
public enum KeyType
{
    BronzeKey,
    SilverKey,
    GoldKey,
    Portal
}

/// <summary>
/// Supports inventory and interaction flow by handling gate.
/// </summary>
public class Gate : MonoBehaviour
{
    private Animator animator;
    public bool IsNeedKey;
    public bool HasKey;
    public new Collider collider;
    public bool isOpen;
    public KeyType gateType;
    public AudioSource puertasonido;
    public AudioSource puertaCerradaSonido;
    public delegate void GateOpenedEventHandler();
    public static event GateOpenedEventHandler GateOpened;
    public string gateMessage = "Press E to open gate.";
    public Renderer sensorRenderer;
    public Color lockedColor = Color.red;
    public Color unlockedColor = Color.green;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Executes the open gate workflow.
    /// </summary>
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

    /// <summary>
    /// Executes the play gate sound workflow.
    /// </summary>
    private void PlayGateSound()
    {
        if (puertasonido != null)
            puertasonido.Play();
    }

    /// <summary>
    /// Executes the play gate close sound workflow.
    /// </summary>
    private void PlayGateCloseSound()
    {
        if (puertaCerradaSonido != null)
            puertaCerradaSonido.Play();
    }

    /// <summary>
    /// Gets the key.
    /// </summary>
    /// <returns>The resulting value.</returns>
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

    /// <summary>
    /// Executes the on destroy workflow.
    /// </summary>
    private void OnDestroy()
    {
        GateOpened = null;
    }
    
    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    void Update()
    {
        if (!IsNeedKey || sensorRenderer == null) return;

        bool hasKeyNow = InventoryManager.instance.HasItemInIventory(GetKey(), 1);

        sensorRenderer.material.color = hasKeyNow ? unlockedColor : lockedColor;
    }
}
