using UnityEngine;
//TP2 GUSTAVO TORRES
/// <summary>
/// Supports the combat system by handling status mod skill.
/// </summary>
public class StatusModSkill : Skill
{
    [Header("Status mod skill")]
    public string message;
    protected StatusMod mod;
    

    /// <summary>
    /// Executes the on run workflow.
    /// </summary>
    /// <param name="receiver">The receiver.</param>
    protected override void OnRun(Fighter receiver)
    {
        if (this.mod == null)
        {
            this.mod = this.GetComponent<StatusMod>();
        }


        this.messages.Enqueue(this.message.Replace("{receiver}", receiver.idName));

        receiver.statusMods.Add(this.mod);
        receiver.animator.Play("Buff");

        // Notificar al sistema de feedback para mostrar texto flotante
        receiver.RaiseStatModApplied(this.mod.type, this.mod.amount);
 
    }

}