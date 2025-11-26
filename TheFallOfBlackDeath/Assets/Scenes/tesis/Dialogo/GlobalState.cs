using System.Collections.Generic;
using UnityEngine;

public class GlobalState : MonoBehaviour
{
    public static GlobalState Instance;

    private HashSet<string> flags = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public bool HasFlag(string flag)
    {
        return flags.Contains(flag);
    }

    public void AddFlag(string flag)
    {
        flags.Add(flag);
    }

    public void RemoveFlag(string flag)
    {
        flags.Remove(flag);
    }
}
