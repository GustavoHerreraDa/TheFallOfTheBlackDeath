
using UnityEngine;
using UnityEngine.UI;

public class BodyPartPanel : MonoBehaviour
{
    public Button headButton;
    public Button torsoButton;
    public Button armsButton;
    public Button legsButton;

    private Fighter currentTarget;
    private Skill currentSkill;
    private PlayerFighter player;

    void Awake()
    {
        Hide();
    }

    public void Show(PlayerFighter playerFighter, Fighter target, Skill skill)
    {
        gameObject.SetActive(true);

        player = playerFighter;
        currentTarget = target;
        currentSkill = skill;

        headButton.onClick.RemoveAllListeners();
        torsoButton.onClick.RemoveAllListeners();
        armsButton.onClick.RemoveAllListeners();
        legsButton.onClick.RemoveAllListeners();

        headButton.onClick.AddListener(() => SelectPart(BodyPart.Head));
        torsoButton.onClick.AddListener(() => SelectPart(BodyPart.Torso));
        armsButton.onClick.AddListener(() => SelectPart(BodyPart.Arms));
        legsButton.onClick.AddListener(() => SelectPart(BodyPart.Legs));
    }

    private void SelectPart(BodyPart part)
    {
        currentSkill.BodyPartTarget = part;
        currentSkill.AddReceiver(currentTarget);

        // Aplicar daño directo a esa parte
        if (currentSkill is HealthModSkill healthSkill)
        {
            float amount = healthSkill.GetModification(currentTarget);
            currentTarget.ModifyBodyPartHealth(part, amount);
        }

        // Ejecutar efectos visuales/animaciones de skill
        player.combatManager.OnFighterSkill(currentSkill);

        Hide();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
