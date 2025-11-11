using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DialogueEvent : MonoBehaviour
{
    public enum DialogueEndAction
    {
        None,
        StartBattle,
        Disappear
    }

    [Header("Configuración de evento")]
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

    private void Start()
    {
        if (npcRenderer != null)
            npcMaterialInstance = npcRenderer.material;
    }

    public void TriggerEvent()
    {
        if (eventTriggered) return;
        eventTriggered = true;

        switch (onDialogueEnd)
        {
            case DialogueEndAction.StartBattle:
                StartCoroutine(FadeAndLoadScene());
                break;

            case DialogueEndAction.Disappear:
                StartCoroutine(FadeAndDisappearSafe());
                break;
        }
    }

    IEnumerator FadeAndLoadScene()
    {
        yield return StartCoroutine(Fade(1));
        SceneManager.LoadScene(battleSceneName);
    }

    IEnumerator FadeAndDisappearSafe()
    {
        // Ejecuta el fade y la disolución desde un objeto temporal que NO se desactiva
        GameObject fadeRunner = new GameObject("FadeRunner");
        DialogueEvent tempEvent = fadeRunner.AddComponent<DialogueEvent>();

        // Copiamos datos necesarios
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
}
