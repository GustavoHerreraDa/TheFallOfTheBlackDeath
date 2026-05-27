using UnityEngine;
using TMPro;
using System.Linq;

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
        Refresh();
    }

    private void OnDisable()
    {
        PartyManager.OnPartyChanged -= Refresh;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerStatsUpdated -= Refresh;
        }
    }

    /// <summary>
    /// Refreshes the value.
    /// </summary>
    public void Refresh()
    {
        if (GameManager.Instance == null)
            return;

        fighter = GameManager.Instance.character1;

        if (fighter == null)
            return;

        //obtiene cada parte del cuerpo como una lista de tuplas
        var data = GameManager.Instance.BodyPartsIntegrity(fighter).ToList();

        if (data.Count < 6) return;

        Debug.Log("paertes = " + data.Count);

        for (int i = 0; i < data.Count; i++)
        {
            Debug.Log($"part {i}: {data[i].current}/{data[i].max}");
        }
        //se actualiza
        headTxt.text = $"Head: {data[0].current}/{data[0].max}";
        torsoTxt.text = $"Torso: {data[1].current}/{data[1].max}";
        leftArmTxt.text = $"L-Arm: {data[2].current}/{data[2].max}";
        rightArmTxt.text = $"R-Arm: {data[3].current}/{data[3].max}";
        leftLegTxt.text = $"L-Leg: {data[4].current}/{data[4].max}";
        rightLegTxt.text = $"R-Leg: {data[5].current}/{data[5].max}";
    }



}
