using UnityEngine;

[CreateAssetMenu(fileName = "New Global Variable", menuName = "Blackboard/Global Variable")]
public class GlobalVariable : ScriptableObject
{
    [Tooltip("El ID único de la variable. Si se deja vacío, se usará el nombre del archivo.")]
    public string variableId;

    public string Id => string.IsNullOrEmpty(variableId) ? name : variableId;

    public int GetValue(int defaultValue = 0)
    {
        return GlobalState.Instance != null ? GlobalState.Instance.GetInt(Id, defaultValue) : defaultValue;
    }

    public void SetValue(int value)
    {
        if (GlobalState.Instance != null) GlobalState.Instance.SetInt(Id, value);
    }

    public void Add(int amount)
    {
        if (GlobalState.Instance != null)
        {
            int current = GetValue();
            SetValue(current + amount);
        }
    }
}
