using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TP2 AUGUSTO NANINI / FACUNDO FERREIRO
public class BodyPartPanel : MonoBehaviour
{
    [Header("UI")]
    public GameObject sampleButton;   // botón base (prefab oculto en el editor)
    public Transform buttonContainer; 

    private PlayerFighter player;
    private Fighter target;
    private Skill skill;

    private List<Button> buttons = new List<Button>();
    private List<BodyPart> parts = new List<BodyPart>();

    private float baseHeight;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        baseHeight = rectTransform.rect.height;

    
        sampleButton.SetActive(false);
        Hide();
    }

    public void Show(PlayerFighter playerFighter, Fighter targetFighter, Skill currentSkill)
    {
        gameObject.SetActive(true);

        player = playerFighter;
        target = targetFighter;
        skill = currentSkill;

        // Limpieza previa
        foreach (var btn in buttons) Destroy(btn.gameObject);
        buttons.Clear();
        parts.Clear();

        // Generar los botones de partes del cuerpo
        int index = 0;
        foreach (var partData in target.bodyParts)
        {
            if (partData.IsDestroyed) continue; // no mostrar partes destruidas

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
        Text label = btnGO.GetComponentInChildren<Text>();
        if (label != null)
            label.text = part.ToString();

        btn.onClick.AddListener(() => OnBodyPartClick(part));
        return btn;
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

       
        if (buttons.Count == 0)
            Hide();
    }

    public void Hide()
    {
        if (target != null)
            target.OnBodyPartDestroyedEvent -= OnBodyPartDestroyed;

        gameObject.SetActive(false);
    }
}
