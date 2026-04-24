using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Toggles simple world-space body part health labels for enemies during combat.
/// </summary>
public class CombatScannerSystem : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.F;
    public Vector3 worldOffset = new Vector3(0f, 2.6f, 0f);
    public Vector3 anchorOffset = Vector3.zero;
    public float fontSize = 3f;
    public Color textColor = new Color(0.55f, 1f, 0.88f);
    public TMP_FontAsset fontAsset;

    private readonly Dictionary<Fighter, TextMeshPro> labels = new Dictionary<Fighter, TextMeshPro>();
    private readonly StringBuilder builder = new StringBuilder(256);

    private CombatManager combatManager;
    private Camera mainCamera;
    private bool scannerEnabled;

    private void Awake()
    {
        combatManager = GetComponent<CombatManager>();
    }

    private void Update()
    {
        if (combatManager == null)
            return;

        mainCamera = mainCamera != null ? mainCamera : Camera.main;

        bool canUseScanner = CanCurrentFighterUseScanner();

        if (!canUseScanner)
        {
            scannerEnabled = false;
            HideAll();
            return;
        }

        if (Input.GetKeyDown(toggleKey))
        {
            scannerEnabled = !scannerEnabled;
        }

        if (!scannerEnabled)
        {
            HideAll();
            return;
        }

        RefreshLabels();
    }

    private bool CanCurrentFighterUseScanner()
    {
        if (!combatManager.isCombatActive)
            return false;

        Fighter currentFighter = combatManager.CurrentFighter;
        if (currentFighter == null || currentFighter.team != Team.PLAYERS || !currentFighter.isAlive)
            return false;

        PlayerFighter playerFighter = currentFighter as PlayerFighter;
        if (playerFighter == null)
            return false;

        return playerFighter.hasCombatScanner;
    }

    private void RefreshLabels()
    {
        Fighter[] enemyTeam = combatManager.enemyTeam;
        if (enemyTeam == null)
            return;

        foreach (Fighter enemy in enemyTeam)
        {
            if (enemy == null || !enemy.isAlive)
            {
                HideLabel(enemy);
                continue;
            }

            TextMeshPro label = GetOrCreateLabel(enemy);
            label.text = BuildEnemyText(enemy);
            label.gameObject.SetActive(true);
            label.transform.position = GetLabelPosition(enemy);

            if (mainCamera != null)
            {
                Vector3 direction = label.transform.position - mainCamera.transform.position;
                label.transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        List<Fighter> trackedFighters = new List<Fighter>(labels.Keys);
        foreach (Fighter tracked in trackedFighters)
        {
            if (tracked == null)
                continue;

            bool stillTracked = false;
            foreach (Fighter enemy in enemyTeam)
            {
                if (enemy == tracked)
                {
                    stillTracked = true;
                    break;
                }
            }

            if (!stillTracked)
                HideLabel(tracked);
        }
    }

    private string BuildEnemyText(Fighter enemy)
    {
        builder.Clear();
        builder.AppendLine(enemy.idName);

        if (enemy.bodyParts == null)
            return builder.ToString();

        foreach (Fighter.BodyPartData partData in enemy.bodyParts)
        {
            if (partData == null)
                continue;

            builder.Append(partData.part);
            builder.Append(": ");
            builder.Append(Mathf.RoundToInt(partData.currentHealth));
            builder.Append('/');
            builder.Append(Mathf.RoundToInt(partData.maxHealth));

            if (partData.IsDestroyed)
                builder.Append(" X");

            builder.AppendLine();
        }

        return builder.ToString();
    }

    private TextMeshPro GetOrCreateLabel(Fighter enemy)
    {
        if (labels.TryGetValue(enemy, out TextMeshPro existing) && existing != null)
            return existing;

        GameObject labelObject = new GameObject("CombatScannerText");
        labelObject.transform.SetParent(enemy.transform, true);

        TextMeshPro textMesh = labelObject.AddComponent<TextMeshPro>();
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontSize = fontSize;
        textMesh.color = textColor;
        textMesh.outlineWidth = 0.2f;
        textMesh.enableWordWrapping = false;
        textMesh.text = string.Empty;
        if (fontAsset != null)
            textMesh.font = fontAsset;

        labels[enemy] = textMesh;
        return textMesh;
    }

    private Vector3 GetLabelPosition(Fighter enemy)
    {
        if (enemy != null && enemy.scannerAnchor != null)
            return enemy.scannerAnchor.position + anchorOffset;

        return enemy.transform.position + worldOffset;
    }

    private void HideAll()
    {
        foreach (TextMeshPro label in labels.Values)
        {
            if (label != null)
                label.gameObject.SetActive(false);
        }
    }

    private void HideLabel(Fighter enemy)
    {
        if (enemy == null)
            return;

        if (labels.TryGetValue(enemy, out TextMeshPro label) && label != null)
            label.gameObject.SetActive(false);
    }
}
