using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Supports branching dialogue flow by handling dialogue event.
/// </summary>
public class DialogueEvent : MonoBehaviour
{
    /// <summary>
    /// Defines the named values used by dialogue end action.
    /// </summary>
    public enum DialogueEndAction
    {
        None,
        StartBattle,
        Disappear,
        RecruitCharacter,
        GiveItem
    }

    [Header("Configuraciï¿½n de evento")]
    public DialogueEndAction onDialogueEnd = DialogueEndAction.None;
    public string battleSceneName = "BattleScene";

    public float fadeDuration = 1f;
    public CanvasGroup fadeCanvas;
    public float npcDisappearDelay = 0.5f;

    [Header("Disolver NPC (opcional)")]
    public Renderer npcRenderer;
    public float dissolveSpeed = 1.5f;
    public string dissolveProperty = "_DissolveAmount";

    private bool eventTriggered = false;
    private Material npcMaterialInstance;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    private void Start()
    {
       /* if (npcRenderer != null)
            npcMaterialInstance = npcRenderer.material;
       */
    }

    /// <summary>
    /// Executes the trigger event workflow.
    /// </summary>
    public void TriggerEvent(DialogueEndAction actionOverride = DialogueEndAction.None)
    {
        if (eventTriggered) return;
        eventTriggered = true;

        DialogueEndAction actionToExecute = actionOverride != DialogueEndAction.None ? actionOverride : onDialogueEnd;

        switch (actionToExecute)
        {
            case DialogueEndAction.StartBattle:
                StartCoroutine(FadeAndLoadScene());
                break;

            case DialogueEndAction.Disappear:
                StartCoroutine(FadeAndDisappearSafe());
                break;

            case DialogueEndAction.RecruitCharacter:
                Recruit();
                break;
        }
    }

    /// <summary>
    /// Executes the fade and load scene workflow.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    IEnumerator FadeAndLoadScene()
    {
        yield return StartCoroutine(Fade(1));
        SceneManager.LoadScene(battleSceneName);
        GameManager.Instance.lastPos = GameManager.Instance.character.transform.position;
        Cursor.lockState = CursorLockMode.None;

    }

    /// <summary>
    /// Executes the fade and disappear safe workflow.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    IEnumerator FadeAndDisappearSafe()
    {
        
        GameObject fadeRunner = new GameObject("FadeRunner");
        DialogueEvent tempEvent = fadeRunner.AddComponent<DialogueEvent>();

        
        tempEvent.fadeCanvas = fadeCanvas;
        tempEvent.fadeDuration = fadeDuration;

      
        StartCoroutine(Fade(1));

        
        yield return new WaitForSeconds(npcDisappearDelay);

        if (npcMaterialInstance != null)
            yield return StartCoroutine(DissolveNPC());

  
        yield return new WaitForSeconds(fadeDuration);

       
        Destroy(gameObject);

        
        yield return tempEvent.StartCoroutine(tempEvent.Fade(0));
        Destroy(fadeRunner);
    }

    /// <summary>
    /// Executes the fade workflow.
    /// </summary>
    /// <param name="targetAlpha">The target alpha.</param>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvas == null)
            yield break;

        float startAlpha = fadeCanvas.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            fadeCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        fadeCanvas.alpha = targetAlpha;
    }

    /// <summary>
    /// Executes the dissolve npc workflow.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
    IEnumerator DissolveNPC()
    {
        float dissolveValue = 0f;

        while (dissolveValue < 1f)
        {
            dissolveValue += Time.deltaTime * dissolveSpeed;
            npcMaterialInstance.SetFloat(dissolveProperty, dissolveValue);
            yield return null;
        }
    }

    /// <summary>
    /// Executes the recruit workflow.
    /// </summary>
    private void Recruit()
    {
        Debug.Log("NPC Recruitado: " + name);

    }
}
