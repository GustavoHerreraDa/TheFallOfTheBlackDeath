using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Pro Blackboard System: Maneja el estado global del juego (flags, variables, persistencia).
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

    private string SavePath => Path.Combine(Application.persistentDataPath, "global_state.json");

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadState();
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
            SaveState();
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
            SaveState();
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
            SaveState();
        }
    }

    public void SetInt(GlobalVariable varSO, int value)
    {
        if (varSO != null) SetInt(varSO.Id, value);
    }

    // --- Persistencia Pro (JSON) ---
    public void SaveState()
    {
        BlackboardData data = new BlackboardData();
        data.flags = new List<string>(_flags);
        
        foreach (var kvp in _intVariables)
        {
            data.intVariables.Add(new StringIntPair { key = kvp.Key, value = kvp.Value });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        // Debug.Log($"[GlobalState] Estado guardado en: {SavePath}");
    }

    public void LoadState()
    {
        if (!File.Exists(SavePath)) return;

        try
        {
            string json = File.ReadAllText(SavePath);
            BlackboardData data = JsonUtility.FromJson<BlackboardData>(json);

            _flags = new HashSet<string>(data.flags);
            _intVariables.Clear();
            foreach (var pair in data.intVariables)
            {
                _intVariables[pair.key] = pair.value;
            }
            // Debug.Log("[GlobalState] Estado cargado correctamente.");
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
        SaveState();
    }
}
