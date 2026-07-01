using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Pro Blackboard System: Maneja el estado global del juego en memoria.
/// </summary>
public class GlobalState : MonoBehaviour
{
    public static GlobalState Instance;

    // --- Estructura de Datos Avanzada ---
    [Serializable]
    public class BlackboardData
    {
        public List<string> flags = new List<string>();
        public List<StringIntPair> intVariables = new List<StringIntPair>();
    }

    [Serializable]
    public struct StringIntPair
    {
        public string key;
        public int value;
    }

    private HashSet<string> _flags = new HashSet<string>();
    private Dictionary<string, int> _intVariables = new Dictionary<string, int>();

    // --- Eventos (Observer Pattern) ---
    public static event Action<string, bool> OnFlagChanged;
    public static event Action<string, int> OnVariableChanged;

    [Header("Debug & Editor")]
    [Tooltip("Si está activo, el estado se borrará cada vez que pulses Play en el editor.")]
    public bool resetOnPlay = false;

    private string SavePath => Path.Combine(Application.persistentDataPath, "global_state.json");

    private void Awake()
    {
#if UNITY_EDITOR
        if (resetOnPlay)
        {
            ClearPersistentFlags();
            Debug.Log("[GlobalState] Estado reseteado automáticamente (Reset On Play activo).");
        }
#endif

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- Gestión de Flags (Booleanos) ---
    public bool HasFlag(string flag) => _flags.Contains(flag);
    public bool HasFlag(GlobalFlag flagSO) => flagSO != null && HasFlag(flagSO.Id);

    public void AddFlag(string flag)
    {
        if (_flags.Add(flag))
        {
            OnFlagChanged?.Invoke(flag, true);
        }
    }

    public void AddFlag(GlobalFlag flagSO)
    {
        if (flagSO != null) AddFlag(flagSO.Id);
    }

    public void RemoveFlag(string flag)
    {
        if (_flags.Remove(flag))
        {
            OnFlagChanged?.Invoke(flag, false);
        }
    }

    public void RemoveFlag(GlobalFlag flagSO)
    {
        if (flagSO != null) RemoveFlag(flagSO.Id);
    }

    // --- Gestión de Variables (Enteros) ---
    public int GetInt(string key, int defaultValue = 0)
    {
        return _intVariables.TryGetValue(key, out int value) ? value : defaultValue;
    }

    public int GetInt(GlobalVariable varSO, int defaultValue = 0)
    {
        return varSO != null ? GetInt(varSO.Id, defaultValue) : defaultValue;
    }

    public void SetInt(string key, int value)
    {
        if (!_intVariables.ContainsKey(key) || _intVariables[key] != value)
        {
            _intVariables[key] = value;
            OnVariableChanged?.Invoke(key, value);
        }
    }

    public void SetInt(GlobalVariable varSO, int value)
    {
        if (varSO != null) SetInt(varSO.Id, value);
    }

    // --- Persistencia manual (JSON) ---
    public void SaveGameToDisk()
    {
        BlackboardData data = new BlackboardData();
        data.flags = new List<string>(_flags);
        
        foreach (var kvp in _intVariables)
        {
            data.intVariables.Add(new StringIntPair { key = kvp.Key, value = kvp.Value });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[GlobalState] Estado guardado manualmente en: {SavePath}");
    }

    public void LoadGameFromDisk()
    {
        if (!File.Exists(SavePath)) return;

        try
        {
            string json = File.ReadAllText(SavePath);
            BlackboardData data = JsonUtility.FromJson<BlackboardData>(json);

            _flags = data != null && data.flags != null
                ? new HashSet<string>(data.flags)
                : new HashSet<string>();
            _intVariables.Clear();
            if (data != null && data.intVariables != null)
            {
                foreach (var pair in data.intVariables)
                {
                    _intVariables[pair.key] = pair.value;
                }
            }
            Debug.Log("[GlobalState] Estado cargado manualmente desde disco.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[GlobalState] Error al cargar estado: {e.Message}");
        }
    }

    public void ClearPersistentFlags()
    {
        _flags.Clear();
        _intVariables.Clear();
        if (File.Exists(SavePath)) File.Delete(SavePath);
        // No llamamos a SaveState aquí para evitar recrear el archivo vacío inmediatamente si no es necesario,
        // o podemos llamarlo para asegurar que el archivo esté limpio pero exista.
        Debug.Log("[GlobalState] Datos persistentes borrados correctamente.");
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(GlobalState))]
public class GlobalStateEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GlobalState gs = (GlobalState)target;

        GUILayout.Space(10);
        GUI.color = Color.red;
        if (GUILayout.Button("BORRAR TODO EL PROGRESO (Flags e Items)"))
        {
            if (UnityEditor.EditorUtility.DisplayDialog("Borrar Estado Global", 
                    "¿Estás seguro de que quieres borrar todos los flags y variables persistentes? Esto no se puede deshacer.", "Sí", "No"))
            {
                gs.ClearPersistentFlags();
            }
        }
        GUI.color = Color.white;
    }
}
#endif
