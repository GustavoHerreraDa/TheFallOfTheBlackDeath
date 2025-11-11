using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Efecto de voz")]
    public AudioSource audioSource;
    public AudioClip voiceBip; // Sonido corto tipo "bip"
    [Range(0f, 1f)] public float bipPitchVariation = 0.2f;
    public float typingSpeed = 0.03f;

    private Coroutine typingCoroutine;
    private bool isTyping;
    private string currentSentence;

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

            // bip por letra
            if (char.IsLetterOrDigit(text[i]) && voiceBip != null)
            {
                audioSource.pitch = 1f + Random.Range(-bipPitchVariation, bipPitchVariation);
                audioSource.PlayOneShot(voiceBip);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void Update()
    {
        if (dialoguePanel.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentSentence;
                isTyping = false;
            }
            else
            {
                DialogueManager.Instance.NextLine();
            }
        }
    }
}
