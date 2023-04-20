
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillPanel : MonoBehaviour
{
    public GameObject[] skillButtons;
    public Text[] skillButtonLabels;
    public GameObject skillPanel;

    private void Awake()
    {
        this.Hide();
        
        foreach (var btn in this.skillButtons)
        {
            btn.SetActive(false);
        }

    }
    public void ConfigureButtons(int index, string skillName)
    {
        this.skillButtons[index].SetActive(true);
        this.skillButtonLabels[index].text = skillName;
    }

    public void Show()
    {
        this.skillPanel.SetActive(true);
    }

    public void Hide()
    {
        this.skillPanel.SetActive(false);
    }
}
