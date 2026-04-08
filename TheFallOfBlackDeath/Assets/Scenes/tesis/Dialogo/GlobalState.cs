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
        // Persistir flags importantes como el reclutamiento
        if (flag.StartsWith("Reclutado_"))
        {
            PlayerPrefs.SetInt("Flag_" + flag, 1);
            PlayerPrefs.Save();
        }
    }

    public void RemoveFlag(string flag)
    {
        flags.Remove(flag);
        if (flag.StartsWith("Reclutado_"))
        {
            PlayerPrefs.DeleteKey("Flag_" + flag);
            PlayerPrefs.Save();
        }
    }

    public void ClearPersistentFlags()
    {
        // Una forma de resetear todo (útil para pruebas)
        PlayerPrefs.DeleteAll(); 
        flags.Clear();
    }
}
