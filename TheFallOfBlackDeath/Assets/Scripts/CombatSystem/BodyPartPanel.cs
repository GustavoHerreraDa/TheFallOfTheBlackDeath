using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TP2 AUGUSTO NANINI / FACUNDO FERREIRO
/// <summary>
/// Supports the combat system by handling body part panel.
/// </summary>
public class BodyPartPanel : MonoBehaviour
{
    [Header("UI")]
    public GameObject sampleButton; 
    public Transform buttonContainer; 

    [Header("Visual Effects")]
    public Material globalHighlightMaterial; 

    private PlayerFighter player;
    private Fighter target;
    private Skill skill;

    private List<Button> buttons = new List<Button>();
    private List<BodyPart> parts = new List<BodyPart>();

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    void Awake()
    {
        if(sampleButton != null) sampleButton.SetActive(false);
        Hide();
    }

    /// <summary>
    /// Shows the value.
    /// </summary>
    /// <param name="playerFighter">The player fighter.</param>
    /// <param name="targetFighter">The target fighter.</param>
    /// <param name="currentSkill">The current skill.</param>
    public void Show(PlayerFighter playerFighter, Fighter targetFighter, Skill currentSkill)
    {
        gameObject.SetActive(true);
        player = playerFighter;
        target = targetFighter;
        skill = currentSkill;

        if (playerFighter.uiAnchor != null)
        {
            Vector3 targetPosition = playerFighter.uiAnchor.position;
            targetPosition.y = this.transform.position.y;
            this.transform.position = targetPosition;
            this.transform.rotation = playerFighter.uiAnchor.rotation;
        }

        // 1. Limpieza de botones anteriores para evitar duplicados
        foreach (var btn in buttons) if(btn != null) Destroy(btn.gameObject);
        buttons.Clear();
        parts.Clear();

        // 2. Generar botones para cada parte del cuerpo activa
        int index = 0;
        foreach (var partData in target.bodyParts)
        {
            if (partData.IsDestroyed) continue; 

            Button btn = CreateButton(partData.part, index);
            buttons.Add(btn);
            parts.Add(partData.part);
            index++;
        }

        target.OnBodyPartDestroyedEvent += OnBodyPartDestroyed;
    }

    /// <summary>
    /// Creates the button.
    /// </summary>
    /// <param name="part">The part.</param>
    /// <param name="index">The index.</param>
    /// <returns>The resulting value.</returns>
    private Button CreateButton(BodyPart part, int index)
    {
        GameObject btnGO = Instantiate(sampleButton, buttonContainer);
        btnGO.transform.localScale = Vector3.one;
        btnGO.SetActive(true);

        Button btn = btnGO.GetComponent<Button>();
        TextMeshProUGUI label = btnGO.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
        {
            label.text = part.ToString();
            
            // Highlight synergy in label
            if (skill != null && skill.CanTriggerSynergy(target, part))
            {
                label.text = $"<b>{part}</b> <color=#ffff00>[SYNERGY!]</color>";
            }
        }

        // 🔍 Buscar el Renderer REAL de esa parte
        Renderer partRenderer = FindPartRenderer(part);

        if (partRenderer != null)
        {
            // 🔒 El handler es el ÚNICO responsable del highlight
            PartHoverHandler hover = btnGO.AddComponent<PartHoverHandler>();
            
            // --- UPDATED INIT CALL ---
            // Pass the skill, target, part, and label for the damage preview
            hover.Init(partRenderer, globalHighlightMaterial, skill, target, part, label);
        }
        else
        {
            Debug.LogWarning($"[BodyPartPanel] Renderer no encontrado para {part}");
        }

        // Click = seleccionar parte y atacar
        btn.onClick.AddListener(() => OnBodyPartClick(part));

        return btn;
    }
    
    /// <summary>
    /// Finds the part renderer.
    /// </summary>
    /// <param name="part">The part.</param>
    /// <returns>The resulting value.</returns>
    private Renderer FindPartRenderer(BodyPart part)
    {
        string partName = part.ToString();
        
        // Buscamos en todos los hijos del enemigo el Renderer que coincida con el nombre
        foreach (Renderer r in target.GetComponentsInChildren<Renderer>())
        {
            // Compara nombres ignorando mayúsculas/minúsculas o si contiene la palabra (ej: Head_GEO)
            if (r.name.Equals(partName, System.StringComparison.OrdinalIgnoreCase) || r.name.Contains(partName)) 
                return r;
        }
        return null;
    }

    /// <summary>
    /// Executes the on body part click workflow.
    /// </summary>
    /// <param name="part">The part.</param>
    private void OnBodyPartClick(BodyPart part)
    {
        // Regresar a Idle o dejar que la animación de ataque tome el control
        if (player != null && player.animator != null)
        {
            player.animator.Play("Idle");
        }

        // LIMPIEZA TOTAL: Antes de cerrar, forzamos a todas las mallas a volver a su color
        foreach (var btn in buttons)
        {
            if (btn == null) continue;
            PartHoverHandler handler = btn.GetComponent<PartHoverHandler>();
            if (handler != null) handler.ResetToOriginal();
        }

        // Ejecutar ataque
        skill.BodyPartTarget = part;
        skill.AddReceiver(target);
        player.combatManager.OnFighterSkill(skill);

        Hide();
    }

    /// <summary>
    /// Executes the on body part destroyed workflow.
    /// </summary>
    /// <param name="destroyedPart">The destroyed part.</param>
    private void OnBodyPartDestroyed(BodyPart destroyedPart)
    {
        int idx = parts.IndexOf(destroyedPart);
        if (idx >= 0 && idx < buttons.Count)
        {
            Destroy(buttons[idx].gameObject);
            buttons.RemoveAt(idx);
            parts.RemoveAt(idx);
        }

        if (buttons.Count == 0) Hide();
    }

    /// <summary>
    /// Hides the value.
    /// </summary>
    public void Hide()
    {
        if (target != null)
            target.OnBodyPartDestroyedEvent -= OnBodyPartDestroyed;

        gameObject.SetActive(false);
    }
    
    
}
