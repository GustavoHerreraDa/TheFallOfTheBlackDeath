using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public float encounterRate = 0.1f; // Tasa de encuentro (10%)
    public float timeBetweenChecks = 1.0f; // Tiempo entre cada comprobación de encuentro
    public GameObject battleScene; // Referencia a la escena de combate

    private bool isEncounterActive = false;
    private float timeSinceLastCheck = 0.0f;

    private void Update()
    {
        timeSinceLastCheck += Time.deltaTime;

        if (!isEncounterActive && timeSinceLastCheck >= timeBetweenChecks)
        {
            timeSinceLastCheck = 0.0f;
            float randomValue = Random.Range(0.0f, 1.0f);
            if (randomValue < encounterRate)
            {
                StartRandomEncounter();
            }
        }
    }

    private void StartRandomEncounter()
    {
        // Pausar el control del jugador u otras acciones necesarias antes del combate
        // Por ejemplo, puedes desactivar el movimiento del personaje principal.

        isEncounterActive = true;
        // Mostrar animación de transición a la escena de combate si es necesario.
        // Aquí deberías cargar la escena de combate y activarla.

        // NOTA: Aquí deberías hacer más cosas, como configurar enemigos, recompensas, etc.
    }

    public void EndEncounter()
    {
        // Resumir el control del jugador u otras acciones después del combate
        // Por ejemplo, activar el movimiento del personaje principal.

        isEncounterActive = false;
    }
}