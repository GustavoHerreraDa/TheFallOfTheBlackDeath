using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using InventoryNew;

/// <summary>
/// Supports branching dialogue flow by handling dialogue ui.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;


    [Header("Efecto de voz")]
    public AudioSource audioSource;
    public AudioClip voiceBip;
    [Range(0f, 1f)] public float bipPitchVariation = 0.2f;
    public float typingSpeed = 0.03f;

    private Coroutine typingCoroutine;
    public bool IsTyping => isTyping;
    public bool IsShowingChoices => choicesPanel != null && choicesPanel.activeSelf;
    private string currentSentence;

    [Header("Posibles Respuestas")]
    public GameObject choicesPanel;
    public GameObject choiceButtonPrefab;
    private Button[] currentButtons;
    public System.Action onTypingFinished;
    private bool isTyping;
    private bool hadStoredCursorState;
    private bool previousCursorVisible;
    private CursorLockMode previousCursorLockMode;
    private CanvasGroup[] childCanvasGroups;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
        childCanvasGroups = GetComponentsInChildren<CanvasGroup>(true);
        RefreshInvisibleRaycastBlockers();
        ShowUI(false);
    }

    /// <summary>
    /// Shows the ui.
    /// </summary>
    /// <param name="show">The show.</param>
    public void ShowUI(bool show)
    {
        dialoguePanel.SetActive(show);
        RefreshInvisibleRaycastBlockers();
    }

    /// <summary>
    /// Executes the display line workflow.
    /// </summary>
    /// <param name="line">The line.</param>
    public void DisplayLine(DialogueLine line)
    {
        nameText.text = line.speakerName;
        currentSentence = line.sentence;
        onTypingFinished = null;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(currentSentence));
    }

    /// <summary>
    /// Executes the type text workflow.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text += text[i];

            if (char.IsLetterOrDigit(text[i]) && voiceBip != null)
            {
                audioSource.pitch = 1f + Random.Range(-bipPitchVariation, bipPitchVariation);
                audioSource.PlayOneShot(voiceBip);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        onTypingFinished?.Invoke();
    }

    /// <summary>
    /// Stores the cursor state and enables mouse input while dialogue choices are visible.
    /// </summary>
    private void EnableMouseChoiceInput()
    {
        if (!hadStoredCursorState)
        {
            previousCursorVisible = Cursor.visible;
            previousCursorLockMode = Cursor.lockState;
            hadStoredCursorState = true;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Restores the cursor state that was active before the choice list was shown.
    /// </summary>
    private void RestoreCursorState()
    {
        if (!hadStoredCursorState)
            return;

        Cursor.visible = previousCursorVisible;
        Cursor.lockState = previousCursorLockMode;
        hadStoredCursorState = false;
    }

    /// <summary>
    /// Configures a dialogue choice button to be selected with the mouse.
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="choice">The choice.</param>
    private void ConfigureChoiceButton(Button button, DialogueChoice choice)
    {
        if (button == null)
            return;

        button.navigation = new Navigation { mode = Navigation.Mode.None };
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => DialogueManager.Instance.SelectChoice(choice));
    }

    /// <summary>
    /// Shows the choices.
    /// </summary>
    /// <param name="choices">The choices.</param>
    public void ShowChoices(List<DialogueChoice> choices)
    {
        dialoguePanel.SetActive(false);

        foreach (Transform child in choicesPanel.transform)
            Destroy(child.gameObject);

        choicesPanel.SetActive(true);
        EnableMouseChoiceInput();
        RefreshInvisibleRaycastBlockers();

        foreach (DialogueChoice choice in choices)
        {
            if (!DialogueManager.Instance.IsChoiceVisible(choice, out bool hasCost))
                continue;

            GameObject btnObj = Instantiate(choiceButtonPrefab, choicesPanel.transform);
            var label = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            // Si no tiene el costo y se muestra como bloqueado, mostrar label de "falta ítem"
            if (!hasCost)
            {
                string missing = string.IsNullOrEmpty(choice.missingCostLabel)
                    ? $"[Falta: {choice.costItemSO?.itemName ?? "ítem"} x{choice.costItemAmount}]"
                    : choice.missingCostLabel;
                label.text = $"{choice.playerText}\n<size=70%><color=#FF6B6B>{missing}</color></size>";

                // Deshabilitar el botón visualmente
                var btn = btnObj.GetComponent<Button>();
                if (btn != null) btn.interactable = false;
            }
            else
            {
                label.text = choice.playerText;
                ConfigureChoiceButton(btnObj.GetComponent<Button>(), choice);
            }
        }

        currentButtons = choicesPanel.GetComponentsInChildren<Button>();

        if (currentButtons != null && currentButtons.Length > 0 && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    /// <summary>
    /// Hides the choices.
    /// </summary>
    public void HideChoices()
    {
        choicesPanel.SetActive(false);
        dialoguePanel.SetActive(true);
        currentButtons = null;
        RestoreCursorState();
        RefreshInvisibleRaycastBlockers();
    }

    /// <summary>
    /// Disables raycast blocking on invisible canvas groups so hidden overlays do not eat mouse clicks.
    /// </summary>
    private void RefreshInvisibleRaycastBlockers()
    {
        if (childCanvasGroups == null || childCanvasGroups.Length == 0)
            childCanvasGroups = GetComponentsInChildren<CanvasGroup>(true);

        foreach (CanvasGroup canvasGroup in childCanvasGroups)
        {
            if (canvasGroup == null)
                continue;

            if (canvasGroup.alpha <= 0.001f)
            {
                canvasGroup.blocksRaycasts = false;
            }
        }
    }

    /// <summary>
    /// Executes the skip typing workflow.
    /// </summary>
    public void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = currentSentence;
        isTyping = false;
        onTypingFinished?.Invoke();
    }
}
