using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Toggles simple world-space body part health labels for enemies during combat.
/// </summary>
public class CombatScannerSystem : MonoBehaviour
{
    public static CombatScannerSystem Instance { get; private set; }

    public Vector3 worldOffset = new Vector3(0f, 2.6f, 0f);
    public Vector3 anchorOffset = Vector3.zero;
    public float fontSize = 3f;
    public Color textColor = new Color(0.55f, 1f, 0.88f);
    public TMP_FontAsset fontAsset;
    public Material scannerMaterial;

    private readonly Dictionary<Fighter, TextMeshPro> labels = new Dictionary<Fighter, TextMeshPro>();
    private readonly StringBuilder builder = new StringBuilder(256);

    private CombatManager combatManager;
    private Camera mainCamera;
    private bool scannerEnabled;

    private void Awake()
    {
        Instance = this;
        combatManager = GetComponent<CombatManager>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (combatManager == null)
            return;

        mainCamera = mainCamera != null ? mainCamera : Camera.main;

        if (!CanCurrentFighterUseScanner())
        {
            scannerEnabled = false;
            HideAll();
            return;
        }

        if (!scannerEnabled)
        {
            HideAll();
            return;
        }

        RefreshLabels();
    }

    /// <summary>
    /// Toggles the scanner state. Called from UI Button.
    /// </summary>
    public void ToggleScanner()
    {
        if (!CanCurrentFighterUseScanner())
        {
            if (scannerEnabled)
            {
                scannerEnabled = false;
                HideAll();
                if (CameraFXManager.Instance != null)
                    CameraFXManager.Instance.SetCombatScanEffect(false);
                if (CameraDirector.Instance != null &&
                    CameraDirector.Instance.CurrentState == CameraState.Scanner)
                    CameraDirector.Instance.ChangeState(CameraDirector.Instance.StateBeforeUi);
            }
            return;
        }

        scannerEnabled = !scannerEnabled;

        if (scannerEnabled)
        {
            if (CameraDirector.Instance != null)
                CameraDirector.Instance.FocusScannerOn(combatManager.enemyTeam);

            if (CameraFXManager.Instance != null)
                CameraFXManager.Instance.SetCombatScanEffect(true);
        }
        else
        {
            HideAll();

            if (CameraFXManager.Instance != null)
                CameraFXManager.Instance.SetCombatScanEffect(false);

            if (CameraDirector.Instance != null &&
                CameraDirector.Instance.CurrentState == CameraState.Scanner)
            {
                CameraDirector.Instance.ChangeState(CameraDirector.Instance.StateBeforeUi);
            }
        }
    }

    /// <summary>
    /// Returns whether the current fighter can use the scanner. 
    /// Useful for setting Button.interactable.
    /// </summary>
    public bool CanUseScannerUI()
    {
        return CanCurrentFighterUseScanner();
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

            // Notify enemy renderers about the scanner state
            SetEnemyScannerVisuals(enemy, true);

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

            float mHp = partData.maxHealth;
            builder.Append(partData.part);
            builder.Append(": ");
            builder.Append(Mathf.RoundToInt(partData.currentHealth));
            builder.Append('/');
            builder.Append(Mathf.RoundToInt(mHp));

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
        foreach (var kvp in labels)
        {
            if (kvp.Value != null)
                kvp.Value.gameObject.SetActive(false);
            
            if (kvp.Key != null)
                SetEnemyScannerVisuals(kvp.Key, false);
        }
    }

    private void HideLabel(Fighter enemy)
    {
        if (enemy == null)
            return;

        if (labels.TryGetValue(enemy, out TextMeshPro label) && label != null)
            label.gameObject.SetActive(false);
        
        SetEnemyScannerVisuals(enemy, false);
    }

    /// <summary>
    /// Activa o desactiva visualmente el escaneo en los renderers del enemigo.
    /// </summary>
    private void SetEnemyScannerVisuals(Fighter enemy, bool active)
    {
        if (enemy == null) return;

        Renderer[] renderers = enemy.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            // Validación: Ignorar objetos de texto flotantes del mundo
            if (r.gameObject.name == "CombatScannerText") continue;
            if (r.GetComponent<TextMeshPro>() != null) continue;

            BodyPartMaterialController controller = r.GetComponent<BodyPartMaterialController>();
            if (active)
            {
                if (controller == null) controller = r.gameObject.AddComponent<BodyPartMaterialController>();
                controller.SetScannerState(true, scannerMaterial);
            }
            else
            {
                if (controller != null) controller.SetScannerState(false, scannerMaterial);
            }
        }
    }
}
