using UnityEngine;

[CreateAssetMenu(fileName = "New Global Flag", menuName = "Blackboard/Global Flag")]
public class GlobalFlag : ScriptableObject
{
    [Tooltip("El ID único del flag. Si se deja vacío, se usará el nombre del archivo.")]
    public string flagId;

    public string Id => string.IsNullOrEmpty(flagId) ? name : flagId;

    /// <summary>
    /// Comprueba si este flag está activo en el GlobalState.
    /// </summary>
    public bool IsActive()
    {
        return GlobalState.Instance != null && GlobalState.Instance.HasFlag(Id);
    }

    /// <summary>
    /// Activa este flag.
    /// </summary>
    public void Set()
    {
        if (GlobalState.Instance != null) GlobalState.Instance.AddFlag(Id);
    }

    /// <summary>
    /// Desactiva este flag.
    /// </summary>
    public void Clear()
    {
        if (GlobalState.Instance != null) GlobalState.Instance.RemoveFlag(Id);
    }
}
