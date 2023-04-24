
using UnityEngine;

public class TurnBlockStatusCondition : StatusCondition
{
    [Range(0f, 1f)]
    public float blockChance;

    private bool blocks;

    public override void OnApply()
    {
        this.blocks = false;

        float dice = Random.Range(0f, 1f);
        if (dice <= this.blockChance)
        {
            this.blocks = true;
            this.messages.Enqueue(this.applyMessage.Replace("(receiver)", this.receiver.idName));
        }
    }
        public override bool BlocksTurn() => this.blocks;
}
