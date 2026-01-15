using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//TP2 AUGUSTO NANINI/FACUNDO FERREIRO
public class EnemiesPanel : MonoBehaviour
{
    public GameObject sampleButton;
    public GameObject botonReturn;

    private PlayerFighter targetFighter;
    private List<Fighter> targets;

    private List<EnemyButtonUI> buttons;

    private float baseHeight;
    private RectTransform rectTransform;

    void Awake()
    {
        this.targets = new List<Fighter>();
        this.buttons = new List<EnemyButtonUI>();

        this.rectTransform = this.GetComponent<RectTransform>();
        this.baseHeight = this.rectTransform.rect.height;

        
        EnemyButtonUI btn = this.InsertNewButton(this.sampleButton, 0);
        btn.Hide();

        this.Hide();
    }

    public void OnTargetButtonClick(int index)
    {
        Fighter target = this.targets[index];
        this.targetFighter.SetTargetAndAttack(target);

        // Iteramos sobre todos los botones para limpiar el highlight de todos los enemigos
        foreach (var btn in this.buttons)
        {
            if (btn != null)
            {
                // Usamos un método dentro de EnemyButtonUI para que cada enemigo 
                // restaure TODAS sus piezas (Head, Torso, etc.)
                btn.ResetHighlight(); 
            }
        }
    }
    public void Show(PlayerFighter playerFighter, Fighter[] targets)
    {
        this.gameObject.SetActive(true);
        botonReturn.SetActive(true);
        this.targetFighter = playerFighter;

        int btnIndex = 0;

        foreach (var target in targets)
        {
            EnemyButtonUI btn = this.ActivateNextButton(btnIndex);
            btn.SetText(target.idName);
            btn.SetTarget(target);

            this.targets.Add(target);

            btnIndex++;
        }


        this.rectTransform.sizeDelta = new Vector2(
            this.rectTransform.rect.width,
            this.baseHeight * targets.Length
        );
    }

    public void Hide()
    {
        this.sampleButton.SetActive(false);
        this.botonReturn.SetActive(false);
        foreach (var btn in this.buttons)
        {
            btn.Hide();
        }

        this.targets.Clear();
    }

    private EnemyButtonUI ActivateNextButton(int index)
    {
        foreach (var btn in this.buttons)
        {
            if (btn.index == index)
            {
                btn.Show();
                btn.target = this.targets.Count > index ? this.targets[index] : null;
                return btn;
            }
        }

        
        GameObject btnGO = Instantiate(this.sampleButton);
        btnGO.transform.SetParent(this.transform);
        btnGO.transform.localScale = Vector3.one;

        
        EnemyButtonUI but = this.InsertNewButton(btnGO, index);

        
        if (this.targets.Count > index)
            but.target = this.targets[index];

        but.Show();
        return but;
    }


    private EnemyButtonUI InsertNewButton(GameObject btnGO, int index)
    {
       
        EnemyButtonUI btn = btnGO.GetComponent<EnemyButtonUI>();

        if (btn == null)
            btn = btnGO.AddComponent<EnemyButtonUI>();

        btn.index = index;
        btn.button = btnGO.GetComponent<Button>();
        btn.label = btnGO.GetComponentInChildren<TextMeshProUGUI>();

        
        btn.button.onClick.AddListener(() => { this.OnTargetButtonClick(btn.index); });

        this.buttons.Add(btn);
        return btn;
    }

    public void Show()
    {
        this.sampleButton.SetActive(true);
        botonReturn.SetActive(true);
    }

}