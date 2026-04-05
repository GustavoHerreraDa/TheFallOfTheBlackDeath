using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TP2 AUGUSTO NANINI / FACUNDO FERREIRO
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

    void Awake()
    {
        if(sampleButton != null) sampleButton.SetActive(false);
        Hide();
    }

    public void Show(PlayerFighter playerFighter, Fighter targetFighter, Skill currentSkill)
    {
        gameObject.SetActive(true);
        player = playerFighter;
        target = targetFighter;
        skill = currentSkill;

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

    private void OnBodyPartClick(BodyPart part)
    {
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

    public void Hide()
    {
        if (target != null)
            target.OnBodyPartDestroyedEvent -= OnBodyPartDestroyed;

        gameObject.SetActive(false);
    }
    
    
}