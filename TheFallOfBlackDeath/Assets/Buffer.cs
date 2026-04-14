using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles buffer for the current project workflow.
/// </summary>
public class Buffer : MonoBehaviour
{
    // Start is called before the first frame update
    public Fighter fighter;
    public GameObject playerRenderer;
    public Material baseMaterial;
    public Material buffMaterial;
    private Color color;
    public Material[] materiales;
    Renderer rendererFighter;
    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        fighter = gameObject.GetComponent<Fighter>();
        materiales = new Material[2];
        rendererFighter = playerRenderer.GetComponent<Renderer>();
    }

    // Update is called once per frame
    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    void Update()
    {
        if (rendererFighter == null)
            return;

        if (fighter.statusMods.Count > 0)
        {
            //fighter.statusMods
            switch (fighter.statusMods[0].type)
            {
                case StatusModType.ATTACK_MOD:
                    color = new Color(256, 0, 0);
                    buffMaterial.SetColor("_Fresnel_Color", color);
                    break;
                case StatusModType.DEFFENSE_MOD:
                    color = new Color(0, 256, 0);
                    buffMaterial.SetColor("_Fresnel_Color", color);
                    break;
            }
            materiales[0] = baseMaterial;
            materiales[1] = buffMaterial;
            rendererFighter.materials = materiales;
        }
        if (fighter.statusMods.Count == 0)
        {
            materiales[0] = baseMaterial;
            materiales[1] = null;
            rendererFighter.materials = materiales;
        }
    }
}
