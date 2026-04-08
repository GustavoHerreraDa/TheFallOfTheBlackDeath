using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private void Start()
    {
        ShowUI(false);
    }

    public void ShowUI(bool show)
    {
        dialoguePanel.SetActive(show);
    }

    public void DisplayLine(DialogueLine line)
    {
        nameText.text = line.speakerName;
        currentSentence = line.sentence;
        onTypingFinished = null;

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(currentSentence));
    }

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

    public void EnableKeyboardNavigation()
    {
        currentButtons = choicesPanel.GetComponentsInChildren<Button>();

        if (currentButtons == null || currentButtons.Length == 0)
            return;

        selectedIndex = 0;
        HighlightButton(selectedIndex);
    }

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
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = (selectedIndex + 1) % currentButtons.Length;
            HighlightButton(selectedIndex);
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentButtons[selectedIndex] != null)
                currentButtons[selectedIndex].onClick.Invoke();
        }
    }

    public void ShowChoices(DialogueChoice[] choices)
    {
        dialoguePanel.SetActive(false);


        foreach (Transform child in choicesPanel.transform)
            Destroy(child.gameObject);

        choicesPanel.SetActive(true);

        foreach (DialogueChoice choice in choices)
        {
            // VERIFICACIÓN DE FLAGS
            if (!string.IsNullOrEmpty(choice.requiredFlag) && !GlobalState.Instance.HasFlag(choice.requiredFlag))
                continue; // Salta esta opción si no tiene el flag requerido

            if (!string.IsNullOrEmpty(choice.forbiddenFlag) && GlobalState.Instance.HasFlag(choice.forbiddenFlag))
                continue; // Salta esta opción si tiene el flag prohibido

            GameObject btnObj = Instantiate(choiceButtonPrefab, choicesPanel.transform);
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = choice.playerText;

            btnObj.GetComponent<Button>().onClick.AddListener(() => {
                DialogueManager.Instance.SelectChoice(choice);
            });
        }

        EnableKeyboardNavigation();
    }

    public void HideChoices()
    {
        choicesPanel.SetActive(false);
        dialoguePanel.SetActive(true);
        currentButtons = null; 
    }

    public void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = currentSentence;
        isTyping = false;
        onTypingFinished?.Invoke();
    }
}
