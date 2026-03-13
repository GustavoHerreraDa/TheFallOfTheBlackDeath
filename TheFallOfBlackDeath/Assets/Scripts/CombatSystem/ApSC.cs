using UnityEngine;

/// <summary>
/// Apply Status Condition Skill (con Daño inicial)
/// </summary>
public class ApSC : Skill
{
    [Header("Status Condition Settings")]
    public float damageAmount = 0f; // Cantidad de daño que se generará al aplicar la condición
    private StatusCondition condition;

    protected override void OnRun(Fighter receiver)
    {
        if (this.condition == null)
        {
            this.condition = this.GetComponentInChildren<StatusCondition>();

            if (this.condition.gameObject == this.gameObject)
            {
                throw new System.InvalidOperationException(
                    "The StatusCondition should be a child of the skill object because it needs to be cloned"
                );
            }
        }

        // Regla: No puede tener 2 estados al mismo tiempo
        if (receiver.GetCurrentStatusCondition() != null)
        {
            this.messages.Enqueue($"{receiver.idName} ya tiene una condición de estado activa!");
            
            // Sonido de error de UI (opcional)
            if (AudioManager.Instance != null && AudioManager.Instance.uiClickSound != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.uiClickSound, 0.5f, false);
                
            return;
        }

        // 1. Clonamos la status condition
        GameObject go = Instantiate(this.condition.gameObject);
        go.transform.SetParent(receiver.transform);

        // 2. Asignamos el cambio de estado al receptor
        StatusCondition clonedCondition = go.GetComponent<StatusCondition>();
        clonedCondition.SetReceiver(receiver);
        receiver.statusCondition = clonedCondition;

        // 3. Generamos el daño al receptor (Si la habilidad hace daño al impactar)
        if (damageAmount > 0)
        {
            receiver.ModifyHealth(-damageAmount);
            this.messages.Enqueue($"Hit for {(int)damageAmount} to {receiver.idName}");

            // --- INTEGRACIÓN DE GAME FEEL ---
            Vector3 textPos = receiver.transform.position + Vector3.up * 2f;
            // Usamos un color distintivo (ej: Púrpura/Verde tóxico) para diferenciarlo de un ataque normal
            FloatingTextManager.Instance.ShowText($"-{(int)damageAmount}", textPos, new Color(0.6f, 0.2f, 0.8f)); 
            
            CameraManager.Instance.TriggerShake(0.4f); // Temblor moderado
            
            // Sonido de impacto propio de la habilidad o uno genérico
            AudioClip hitSound = this.customImpactSound != null ? this.customImpactSound : AudioManager.Instance.hitNormalSound;
            if (AudioManager.Instance != null && hitSound != null)
                AudioManager.Instance.PlaySFX(hitSound, 0.8f);
        }

        this.messages.Enqueue(clonedCondition.GetReceptionMessage());
    }
}