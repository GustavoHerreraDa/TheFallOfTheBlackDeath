using TMPro;
using UnityEngine;
using System.Linq;
public class BodyStatusUI : MonoBehaviour
{
    public TextMeshProUGUI headText;
    public TextMeshProUGUI torsoText;
    public TextMeshProUGUI leftArmText;
    public TextMeshProUGUI rightArmText;
    public TextMeshProUGUI leftLegText;
    public TextMeshProUGUI rightLegText;

    public GameManager gameManager;
    public PlayerFighter player;

    private void Awake()
    {
       
        if (gameManager == null)
            gameManager = GameManager.Instance;

        
        if (player == null)
            player = FindObjectOfType<PlayerFighter>();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (gameManager == null || player == null)
            return;

        var values = gameManager.BodyPartsIntegrity(player).ToList();

        headText.text = $"Head:     {(values[0] * 100f):0}%";
        torsoText.text = $"Torso:    {(values[1] * 100f):0}%";
        leftArmText.text = $"L-Arm:    {(values[2] * 100f):0}%";
        rightArmText.text = $"R-Arm:    {(values[3] * 100f):0}%";
        leftLegText.text = $"L-Leg:    {(values[4] * 100f):0}%";
        rightLegText.text = $"R-Leg:    {(values[5] * 100f):0}%";
    }
}