using UnityEngine;

public class BleedingCondition : StatusCondition
{
    [Header("Bleeding Settings")]
    public float damagePerTurn = 15f; // Cuánto daño hace por turno

    public override bool OnApply()
    {
        if (receiver == null) return false;

        // 1. Aplicamos el daño directo a la vida
        receiver.ModifyHealth(-damagePerTurn);

        // 2. Mensaje para la consola de combate
        messages.Enqueue($"{receiver.idName} sufre {(int)damagePerTurn} de daño por Sangrado.");

        // 3. --- GAME FEEL (El Jugo) ---
        Vector3 textPos = receiver.transform.position + Vector3.up * 2f;
        
        // Color rojo sangre oscuro
        if (FloatingTextManager.Instance != null)
            FloatingTextManager.Instance.ShowText($"-{(int)damagePerTurn}", textPos, new Color(0.7f, 0.1f, 0.1f));

        // Un temblor muy leve para indicar dolor sin ser un impacto real
        if (CameraManager.Instance != null)
            CameraManager.Instance.TriggerShake(0.3f); 

        // Sonido de daño tipo "carne" suave
        if (AudioManager.Instance != null && AudioManager.Instance.hitNormalSound != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.hitNormalSound, 0.5f);

        return true; // Retorna true para que la clase base instancie el 'effectPrfb' (tus partículas de sangre)
    }

    public override bool BlocksTurn()
    {
        // El sangrado duele, pero NO le saltea el turno al enemigo
        return false; 
    }
}