using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Supports exploration and world-state flow by handling safe zone trigger.
/// </summary>
public class SafeZoneTrigger : MonoBehaviour
{
    /// <summary>
    /// Responds to the corresponding Unity trigger callback for this component.
    /// </summary>
    /// <param name="other">The other.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Charecter"))
        {
            print ("Entraste a la zona segura");
            GameManager.Instance.SetGameState(GameManager.GameStates.SAFE_ZONE);
        }
    }

    /// <summary>
    /// Responds to the corresponding Unity trigger callback for this component.
    /// </summary>
    /// <param name="other">The other.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Charecter"))
        {
            print ("Saliste de la zona segura");
            GameManager.Instance.SetGameState(GameManager.GameStates.TOWN_STATE);
        }
    }
}
