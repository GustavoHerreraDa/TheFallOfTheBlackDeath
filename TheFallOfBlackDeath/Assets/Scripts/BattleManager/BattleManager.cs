using UnityEngine;

public class RandomEncounterController : MonoBehaviour
{
    public float encounterRate = 0.1f;
    public float timeBetweenChecks = 1.0f;
    public GameObject battlePrefab; // Referencia al prefab del combate

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

        isEncounterActive = true;

        // Instanciar el prefab del combate en la posición deseada
        GameObject battleInstance = Instantiate(battlePrefab, transform.position, Quaternion.identity);

        // Configurar los datos del combate en el prefab
        // Puedes tener un script en el prefab para recibir esta información.
        //battleInstance.GetComponent<CombatManager>().SetupBattle(/* Aquí pasas los datos necesarios */);
    }

    public void EndEncounter()
    {
        // Resumir el control del jugador u otras acciones después del combate

        isEncounterActive = false;
    }
}