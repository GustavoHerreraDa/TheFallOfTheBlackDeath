using UnityEngine;
//TP2 FACUNDO FERREIRO
/// <summary>
/// Apply Status Condition Skill
/// </summary>
public class ApplySCSkill : Skill
{

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


        this.messages.Enqueue(clonedCondition.GetReceptionMessage());
    }
}