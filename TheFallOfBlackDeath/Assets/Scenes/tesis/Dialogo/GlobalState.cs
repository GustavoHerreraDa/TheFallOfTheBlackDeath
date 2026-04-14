using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Supports branching dialogue flow by handling global state.
/// </summary>
public class GlobalState : MonoBehaviour
{
    public static GlobalState Instance;

    private HashSet<string> flags = new HashSet<string>();

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    /// <summary>
    /// Determines whether the component has flag.
    /// </summary>
    /// <param name="flag">The flag.</param>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    public bool HasFlag(string flag)
    {
        return flags.Contains(flag);
    }

    /// <summary>
    /// Adds the flag.
    /// </summary>
    /// <param name="flag">The flag.</param>
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

    /// <summary>
    /// Removes the flag.
    /// </summary>
    /// <param name="flag">The flag.</param>
    public void RemoveFlag(string flag)
    {
        flags.Remove(flag);
        if (flag.StartsWith("Reclutado_"))
        {
            PlayerPrefs.DeleteKey("Flag_" + flag);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Executes the clear persistent flags workflow.
    /// </summary>
    public void ClearPersistentFlags()
    {
        // Una forma de resetear todo (Ãºtil para pruebas)
        PlayerPrefs.DeleteAll(); 
        flags.Clear();
    }
}
