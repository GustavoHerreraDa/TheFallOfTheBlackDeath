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

    private PlayerFighter player;
    private Fighter target;
    private Skill skill;

    private List<Button> buttons = new List<Button>();
    private List<BodyPart> parts = new List<BodyPart>();
    
    public Material globalHighlightMaterial; // Lo asignas una sola vez en el Inspector del Panel

    void Awake()
    {
        sampleButton.SetActive(false);
        Hide();
    }

    public void Show(PlayerFighter playerFighter, Fighter targetFighter, Skill currentSkill)
    {
        gameObject.SetActive(true);
        player = playerFighter;
        target = targetFighter;
        skill = currentSkill;

        // Limpieza de botones anteriores
        foreach (var btn in buttons) if(btn != null) Destroy(btn.gameObject);
        buttons.Clear();
        parts.Clear();

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
            label.text = part.ToString();

        // --- Lógica de Iluminación Específica ---
        Renderer partRenderer = FindPartRenderer(part);
    
        // Añadimos el manejador de Hover al botón de la interfaz
        var hover = btnGO.AddComponent<PartHoverHandler>();
    
        // CAMBIO AQUÍ: Ahora usamos el material del panel, no el del target
        hover.Init(partRenderer, globalHighlightMaterial); 

        btn.onClick.AddListener(() => OnBodyPartClick(part));
        return btn;
    }
    
    // Busca la malla que corresponde a la parte del cuerpo (ej: "Head", "LeftArm")
    private Renderer FindPartRenderer(BodyPart part)
    {
        string partName = part.ToString();
        
        // 1. Intenta encontrar el objeto por nombre exacto dentro del Armature/Modelo
        foreach (Renderer r in target.GetComponentsInChildren<Renderer>())
        {
            if (r.name.Equals(partName, System.StringComparison.OrdinalIgnoreCase)) 
                return r;
        }

        // 2. Si falló, busca por coincidencia parcial (ej: "Head_GEO")
        foreach (Renderer r in target.GetComponentsInChildren<Renderer>())
        {
            if (r.name.Contains(partName)) 
                return r;
        }
        return null;
    }

    private void OnBodyPartClick(BodyPart part)
    {
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