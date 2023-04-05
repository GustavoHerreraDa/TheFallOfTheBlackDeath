using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    [Header("Base Skill")]
    //Nombre de la habilidad
    public string skillName;
    //Duracion de la animacion
    public float animationDuration;
    //Si es autoinfligida o no
    public bool selfInflicted;
    //PreFab para la animacion
    public GameObject effectPrfb;
    //Quien la recibe y quien emite la habilidad
    protected Fighter emitter;
    protected Fighter receiver;

    private void Animate()
    {
        var go = Instantiate(this.effectPrfb, this.receiver.transform.position, Quaternion.identity);
        Destroy(go, this.animationDuration);
    }

    public void Run()
    {
        if (this.selfInflicted)
        {
            this.receiver = this.emitter;
        }

        this.Animate();

        this.OnRun();
    }

    public void SetEmitterAndReceiver(Fighter _emitter, Fighter _receiver)
    {
        this.emitter = _emitter;
        this.receiver = _receiver;
    }

    protected abstract void OnRun();
}