using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TP2 AUGUSTO NANINI / FACUNDO FERREIRO
public class EnemiesPanel : MonoBehaviour
{
    public GameObject sampleButton;
    public GameObject botonReturn;

    private PlayerFighter targetFighter;
    private readonly List<Fighter> targets = new();
    private readonly List<EnemyButtonUI> buttons = new();

    private float baseHeight;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        baseHeight = rectTransform.rect.height;

        EnemyButtonUI btn = InsertNewButton(sampleButton, 0);
        btn.Hide();

        Hide();
    }

    public void Show(PlayerFighter playerFighter, Fighter[] enemyTargets)
    {
        gameObject.SetActive(true);
        botonReturn.SetActive(true);

        targetFighter = playerFighter;
        targets.Clear();

        if (playerFighter.uiAnchor != null)
        {
            Vector3 targetPosition = playerFighter.uiAnchor.position;
            targetPosition.y = this.transform.position.y;
            this.transform.position = targetPosition;
            this.transform.rotation = playerFighter.uiAnchor.rotation;
        }

        int index = 0;

        foreach (var enemy in enemyTargets)
        {
            EnemyButtonUI btn = ActivateNextButton(index);
            btn.SetText(enemy.idName);
            btn.SetTarget(enemy);

            targets.Add(enemy);
            index++;
        }

        rectTransform.sizeDelta = new Vector2(
            rectTransform.sizeDelta.x,
            baseHeight * enemyTargets.Length
        );
    }

    public void Hide()
    {
        sampleButton.SetActive(false);
        botonReturn.SetActive(false);

        foreach (var btn in buttons)
        {
            if (btn != null) btn.Hide();
        }

        targets.Clear();
    }

    // ================= BUTTON CLICK =================

    public void OnTargetButtonClick(int index)
    {
        if (index < 0 || index >= targets.Count) return;

        Fighter target = targets[index];
        targetFighter.SetTargetAndAttack(target);

        // Limpiar highlight de TODOS los enemigos
        foreach (var btn in buttons)
        {
            if (btn != null) btn.ResetHighlight();
        }
    }

    // ================= INTERNAL =================

    private EnemyButtonUI ActivateNextButton(int index)
    {
        foreach (var btn in buttons)
        {
            if (btn.index == index)
            {
                btn.Show();
                return btn;
            }
        }

        GameObject btnGO = Instantiate(sampleButton, transform);
        btnGO.transform.localScale = Vector3.one;

        EnemyButtonUI btnNew = InsertNewButton(btnGO, index);
        btnNew.Show();
        return btnNew;
    }

    private EnemyButtonUI InsertNewButton(GameObject btnGO, int index)
    {
        EnemyButtonUI btn = btnGO.GetComponent<EnemyButtonUI>();
        if (btn == null) btn = btnGO.AddComponent<EnemyButtonUI>();

        btn.index = index;
        btn.button = btnGO.GetComponent<Button>();
        btn.label = btnGO.GetComponentInChildren<TextMeshProUGUI>();

        btn.button.onClick.AddListener(() => OnTargetButtonClick(btn.index));

        buttons.Add(btn);
        return btn;
    }
    
    public EnemyButtonUI GetButtonFor(Fighter fighter)
    {
        foreach (var btn in buttons)
        {
            if (btn != null && btn.target == fighter)
                return btn;
        }
        return null;
    }

}
