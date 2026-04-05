using UnityEngine;
using UnityEngine.EventSystems;
using TMPro; // Needed to update the button text

public class PartHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Renderer targetRenderer;
    private Material highlightMaterial;
    [SerializeField]
    private Material[] originalMaterials; 

    // --- NEW VARIABLES FOR DAMAGE PREVIEW ---
    private Skill currentSkill;
    private Fighter targetFighter;
    private BodyPart targetPart;
    private TextMeshProUGUI buttonLabel;
    private string originalText;

    // Updated Init method to receive the necessary data
    public void Init(Renderer rend, Material highMat, Skill skill, Fighter target, BodyPart part, TextMeshProUGUI label)
    {
        targetRenderer = rend;
        highlightMaterial = highMat;
        
        currentSkill = skill;
        targetFighter = target;
        targetPart = part;
        buttonLabel = label;
        originalText = label.text; // Store the original text (e.g., "RightArm")

        if (rend != null)
        {
            Material[] mats = rend.materials;
            originalMaterials = new Material[mats.Length];
            mats.CopyTo(originalMaterials, 0);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.uiHoverSound, 0.5f, false);
        
        // 1. Visual Highlight (Your existing code)
        if (targetRenderer != null && highlightMaterial != null)
        {
            Material[] highlightSheet = new Material[targetRenderer.materials.Length];
            for (int i = 0; i < highlightSheet.Length; i++) {
                highlightSheet[i] = highlightMaterial;
            }
            targetRenderer.materials = highlightSheet;
        }

            // 2. --- NEW: DAMAGE PREVIEW CALCULATION ---
            if (currentSkill != null && currentSkill is HealthModSkill healthSkill && targetFighter != null)
            {
                // Temporarily set the skill's target part so GetAdjustedMissChance calculates correctly
                BodyPart previousTarget = healthSkill.BodyPartTarget;
                healthSkill.BodyPartTarget = targetPart;

                // Calculate potential damage
                float estimatedDamage = healthSkill.GetEstimatedDamage(targetFighter, targetPart);
                
                // Synergy Check
                bool hasSynergy = healthSkill.CanTriggerSynergy(targetFighter, targetPart);
                string damageColor = hasSynergy ? "#ffff00" : "#ff3333";
                string synergyText = hasSynergy ? " <color=#ffff00>[COMBO!]</color>" : "";

                // Format the text to look juicy (e.g., "RightArm <color=red>[-45]</color>")
                buttonLabel.text = $"{originalText}{synergyText} <color={damageColor}>[-{(int)estimatedDamage}]</color>";

                // Restore the previous target just in case
                healthSkill.BodyPartTarget = previousTarget;
            }
    }

    public void OnPointerExit(PointerEventData eventData) => ResetToOriginal();

    public void OnPointerClick(PointerEventData eventData) => ResetToOriginal();

    public void ResetToOriginal()
    {
        // 1. Reset Visuals
        if (targetRenderer != null && originalMaterials != null) {
            targetRenderer.materials = originalMaterials;
        }

        // 2. Reset Text
        if (buttonLabel != null && originalText != null)
        {
            buttonLabel.text = originalText;
        }
    }

    private void OnDisable() => ResetToOriginal();
}