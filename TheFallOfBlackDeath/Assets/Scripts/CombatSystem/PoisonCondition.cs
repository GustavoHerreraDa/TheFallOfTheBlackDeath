using UnityEngine;

public class PoisonCondition : StatusCondition
{
    [Header("Poison Settings")]
    public float poisonDamage = 10f; 

    public override bool OnApply()
    {
        if (receiver == null) return false;

        receiver.ModifyHealth(-poisonDamage);

        messages.Enqueue($"{receiver.idName} sufre {(int)poisonDamage} de daño por Veneno.");

        // --- GAME FEEL (El Jugo) ---
        Vector3 textPos = receiver.transform.position + Vector3.up * 2f;
        
        // Color verde tóxico brillante
        if (FloatingTextManager.Instance != null)
            FloatingTextManager.Instance.ShowText($"-{(int)poisonDamage}", textPos, new Color(0.2f, 0.9f, 0.2f));

        if (CameraManager.Instance != null)
            CameraManager.Instance.TriggerShake(0.2f); 

        // Opcional: Si en tu AudioManager agregas un sonido de "ácido" o "burbujas", lo llamas aquí.
        // Por ahora usamos uno genérico de interfaz o impacto suave.
        if (AudioManager.Instance != null && AudioManager.Instance.uiHoverSound != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.uiHoverSound, 0.6f);

        return true; // Retorna true para instanciar el 'effectPrfb' (partículas de veneno)
    }

    public override bool BlocksTurn()
    {
        return false; // El veneno tampoco saltea el turno
    }
}