using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

public class SafeZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Charecter"))
        {
            print ("Entraste a la zona segura");
            GameManager.Instance.SetGameState(GameManager.GameStates.SAFE_ZONE);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Charecter"))
        {
            print ("Saliste de la zona segura");
            GameManager.Instance.SetGameState(GameManager.GameStates.TOWN_STATE);
        }
    }
}
