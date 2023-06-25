using UnityEngine;

/// <summary>
/// Apply Status Condition Skill
/// </summary>
public class ApplySCSkill : Skill
{
    public float damageAmount = 0f; // Cantidad de daño que se generará al aplicar la condición de estado
    private StatusCondition condition;
    public AudioSource audioSource;

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

        if (receiver.GetCurrentStatusCondition())
        {
            this.messages.Enqueue("The fighter cannot have 2 status conditions!");
            audioSource.Play();
            return;
        }

        // Clonamos la status condition
        GameObject go = Instantiate(this.condition.gameObject);
        go.transform.SetParent(receiver.transform);

        // Asignamos el cambio de estado al receptor
        StatusCondition clonedCondition = go.GetComponent<StatusCondition>();
        clonedCondition.SetReceiver(receiver);
        receiver.statusCondition = clonedCondition;

        // Generamos el daño al receptor
        receiver.ModifyHealth(-damageAmount);
        this.messages.Enqueue("Hit for " + (int)damageAmount + (" to " + receiver.idName));

        this.messages.Enqueue(clonedCondition.GetReceptionMessage());
    }
}