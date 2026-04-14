using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    private string currentSentence;

    [Header("Posibles Respuestas")]
    public GameObject choicesPanel;
    public GameObject choiceButtonPrefab;
    private int selectedIndex = 0;
    private Button[] currentButtons;
    public System.Action onTypingFinished;
    private bool isTyping;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
        ShowUI(false);
    }

    /// <summary>
    /// Shows the ui.
    /// </summary>
    /// <param name="show">The show.</param>
    public void ShowUI(bool show)
    {
        dialoguePanel.SetActive(show);
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
    /// Executes the enable keyboard navigation workflow.
    /// </summary>
    public void EnableKeyboardNavigation()
    {
        currentButtons = choicesPanel.GetComponentsInChildren<Button>();

        if (currentButtons == null || currentButtons.Length == 0)
            return;

        selectedIndex = 0;
        HighlightButton(selectedIndex);
    }

    /// <summary>
    /// Executes the highlight button workflow.
    /// </summary>
    /// <param name="index">The index.</param>
    private void HighlightButton(int index)
    {
        if (currentButtons == null || currentButtons.Length == 0)
            return;

        for (int i = 0; i < currentButtons.Length; i++)
        {
            if (currentButtons[i] == null) continue;

            ColorBlock cb = currentButtons[i].colors;
            cb.normalColor = (i == index) ? Color.yellow : Color.white;
            currentButtons[i].colors = cb;
        }
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    private void Update()
    {
        if (!choicesPanel.activeSelf)
            return;

        if (currentButtons == null || currentButtons.Length == 0)
            return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex = (selectedIndex - 1 + currentButtons.Length) % currentButtons.Length;
            HighlightButton(selectedIndex);
        }
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="Input.GetKeyDown(KeyCode.DownArrow)">The input.get key down(key code.down arrow).</param>
        /// <returns>The resulting value.</returns>
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = (selectedIndex + 1) % currentButtons.Length;
            HighlightButton(selectedIndex);
        }
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="Input.GetKeyDown(KeyCode.Return)">The input.get key down(key code.return).</param>
        /// <returns>The resulting value.</returns>
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentButtons[selectedIndex] != null)
                currentButtons[selectedIndex].onClick.Invoke();
        }
    }

    /// <summary>
    /// Shows the choices.
    /// </summary>
    /// <param name="choices">The choices.</param>
    public void ShowChoices(DialogueChoice[] choices)
    {
        dialoguePanel.SetActive(false);


        foreach (Transform child in choicesPanel.transform)
            Destroy(child.gameObject);

        choicesPanel.SetActive(true);

        foreach (DialogueChoice choice in choices)
        {
            // VERIFICACIÃ“N DE FLAGS
            if (!string.IsNullOrEmpty(choice.requiredFlag) && !GlobalState.Instance.HasFlag(choice.requiredFlag))
                continue; // Salta esta opciÃ³n si no tiene el flag requerido

            if (!string.IsNullOrEmpty(choice.forbiddenFlag) && GlobalState.Instance.HasFlag(choice.forbiddenFlag))
                continue; // Salta esta opciÃ³n si tiene el flag prohibido

            GameObject btnObj = Instantiate(choiceButtonPrefab, choicesPanel.transform);
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = choice.playerText;

            btnObj.GetComponent<Button>().onClick.AddListener(() => {
                DialogueManager.Instance.SelectChoice(choice);
            });
        }

        EnableKeyboardNavigation();
    }

    /// <summary>
    /// Hides the choices.
    /// </summary>
    public void HideChoices()
    {
        choicesPanel.SetActive(false);
        dialoguePanel.SetActive(true);
        currentButtons = null; 
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
