// Diseño central: Singleton para gestión de party en escena delegando persistencia y registro al GameManager.
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    public static event System.Action OnPartyChanged;

    [Header("Party en escena")]
    public List<GameObject> partyObjects;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (partyObjects == null || GameManager.Instance == null) return;

        foreach (var obj in partyObjects)
        {
            if (obj != null)
            {
                PlayerFighter pf = obj.GetComponent<PlayerFighter>();
                if (pf != null && GameManager.Instance.IsRecruited(pf.figherIndex))
                {
                    GameManager.Instance.RegisterPartyMember(pf);
                }
            }
        }
    }

    public void Recruit(int index)
    {
        if (partyObjects == null || index < 0 || index >= partyObjects.Count || GameManager.Instance == null) return;

        GameObject obj = partyObjects[index];
        if (obj != null)
        {
            PlayerFighter pf = obj.GetComponent<PlayerFighter>();
            if (pf != null)
            {
                GameManager.Instance.RegisterPartyMember(pf);
                OnPartyChanged?.Invoke();
            }
        }
    }

    public void Remove(int index)
    {
        if (partyObjects == null || index < 0 || index >= partyObjects.Count || GameManager.Instance == null) return;

        GameObject obj = partyObjects[index];
        if (obj != null)
        {
            PlayerFighter pf = obj.GetComponent<PlayerFighter>();
            if (pf != null)
            {
                GameManager.Instance.UnregisterPartyMember(pf);
                OnPartyChanged?.Invoke();
            }
        }
    }
}
