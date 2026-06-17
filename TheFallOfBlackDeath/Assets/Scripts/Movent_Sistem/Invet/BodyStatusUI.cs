using UnityEngine;
using TMPro;
using System.Linq;
using InventoryNew;

/// <summary>
/// Supports inventory and interaction flow by handling body status ui.
/// </summary>
public class BodyStatusUI : MonoBehaviour
{
    public TextMeshProUGUI headTxt;
    public TextMeshProUGUI torsoTxt;
    public TextMeshProUGUI leftArmTxt;
    public TextMeshProUGUI rightArmTxt;
    public TextMeshProUGUI leftLegTxt;
    public TextMeshProUGUI rightLegTxt;
    
    public PartyMemberSelectorUI memberSelector;
    

    private GameManager gm;
    private PlayerFighter fighter;

    /// <summary>
    /// Initializes cached references and runtime state before the component starts running.
    /// </summary>
    void Awake()
    {
    }

    /// <summary>
    /// Registers runtime listeners when the component becomes active.
    /// </summary>
    private void OnEnable()
    {
        PartyManager.OnPartyChanged += Refresh;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerStatsUpdated += Refresh;
        }

        if (memberSelector != null)
        {
            memberSelector.OnMemberSelected += SetFighter;
            if (memberSelector.CurrentSelected != null)
            {
                SetFighter(memberSelector.CurrentSelected);
            }
        }

        Refresh();
    }

    private void OnDisable()
    {
        PartyManager.OnPartyChanged -= Refresh;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerStatsUpdated -= Refresh;
        }

        if (memberSelector != null)
        {
            memberSelector.OnMemberSelected -= SetFighter;
        }
    }

    /// <summary>
    /// Crea un método public void SetFighter(PlayerFighter newFighter) que reciba al personaje, lo asigne a la variable privada existente fighter, y luego ejecute Refresh().
    /// </summary>
    public void SetFighter(PlayerFighter newFighter)
    {
        fighter = newFighter;
        Refresh();
    }

    /// <summary>
    /// Refreshes the value.
    /// </summary>
    public void Refresh()
    {
        if (GameManager.Instance == null)
            return;

        if (fighter == null)
        {
            fighter = GameManager.Instance.character1;
        }

        if (fighter == null)
            return;

        // Actualizamos cada texto individualmente
        UpdatePartUI(BodyPart.Head, headTxt, "Head");
        UpdatePartUI(BodyPart.Torso, torsoTxt, "Torso");
        UpdatePartUI(BodyPart.LeftArm, leftArmTxt, "L-Arm");
        UpdatePartUI(BodyPart.RightArm, rightArmTxt, "R-Arm");
        UpdatePartUI(BodyPart.LeftLeg, leftLegTxt, "L-Leg");
        UpdatePartUI(BodyPart.RightLeg, rightLegTxt, "R-Leg");
    }

    private void UpdatePartUI(BodyPart part, TextMeshProUGUI textUI, string label)
    {
        if (textUI == null) return;

        Fighter.BodyPartData data = fighter.GetBodyPart(part);
        if (data == null) return;

        if (data.IsDestroyed && data.HasActiveProsthetic)
        {
            // Mostrar salud de la prótesis con un indicador [P]
            textUI.text = $"{label}: [P] {Mathf.Round(data.prostheticCurrentHealth)}";
            textUI.color = Color.cyan; // Color distintivo para prótesis
        }
        else
        {
            textUI.text = $"{label}: {Mathf.Round(data.currentHealth)}/{Mathf.Round(data.maxHealth)}";
            textUI.color = data.IsDestroyed ? Color.red : Color.white;
        }
    }



}
