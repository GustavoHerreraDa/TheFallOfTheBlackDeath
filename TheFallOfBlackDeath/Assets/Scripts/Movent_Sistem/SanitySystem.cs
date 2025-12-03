using System.Collections;
using UnityEngine;
using TMPro;

public class SanitySystem : MonoBehaviour
{
    [Header("Sanity Stats")]
    public float corduraMax = 100f;
    public float corduraActual;
    public float perdidaDeCordura = 2f;
    public float sanityThreshold = 20f;

    [Header("References")]
    public TextMeshProUGUI textCordura;
    public Material vignetteMaterial;
    public AudioSource lowSanityAudio;

    [Header("Vignette Settings")]
    public float minVigp = 0f;
    public float maxVigp = 1f;
    public float minVigi = 0f;
    public float maxVigi = 1f;

    private bool isLowSanityPlaying = false;
    private Coroutine corduraRoutine;

    public bool IsBelowThreshold => corduraActual <= sanityThreshold;

    void Start()
    {
        corduraActual = corduraMax;
        StartDecreaseSanity();
    }

    void Update()
    {
        UpdateUI();
        UpdateVignette();
        UpdateLowSanitySound();
    }

    public void StartDecreaseSanity()
    {
        if (corduraRoutine != null)
            StopCoroutine(corduraRoutine);

        corduraRoutine = StartCoroutine(BajarCordura());
    }

    public void StartIncreaseSanity()
    {
        if (corduraRoutine != null)
            StopCoroutine(corduraRoutine);

        corduraRoutine = StartCoroutine(AumentarCordura());
    }

    public void StopSanityChanges()
    {
        if (corduraRoutine != null)
            StopCoroutine(corduraRoutine);
    }

    IEnumerator BajarCordura()
    {
        while (true)
        {
            if (corduraActual > 0)
            {
                float perdida = (corduraMax * (perdidaDeCordura / 2000f)) * Time.deltaTime;
                corduraActual -= perdida;
                corduraActual = Mathf.Clamp(corduraActual, 0, corduraMax);
            }
            yield return null;
        }
    }

    IEnumerator AumentarCordura()
    {
        while (true)
        {
            if (corduraActual < corduraMax)
            {
                float ganancia = (corduraMax * (perdidaDeCordura / 100f)) * Time.deltaTime;
                corduraActual += ganancia;
                corduraActual = Mathf.Clamp(corduraActual, 0, corduraMax);
            }
            yield return null;
        }
    }


    void UpdateUI()
    {
        if (textCordura != null)
            textCordura.text = "Sanity: " + Mathf.RoundToInt(corduraActual);
    }

    void UpdateVignette()
    {
        if (vignetteMaterial == null) return;

        float normalized = Mathf.InverseLerp(corduraMax, sanityThreshold, corduraActual);
        normalized = Mathf.Clamp01(normalized);

        float vigpValue = Mathf.Lerp(minVigp, maxVigp, normalized);
        float vigiValue = Mathf.Lerp(minVigi, maxVigi, normalized);

        vignetteMaterial.SetFloat("_vigp", vigpValue);
        vignetteMaterial.SetFloat("_vigi", vigiValue);
    }

    void UpdateLowSanitySound()
    {
        if (lowSanityAudio == null) return;

        if (corduraActual <= sanityThreshold && !isLowSanityPlaying)
        {
            lowSanityAudio.Play();
            isLowSanityPlaying = true;
        }
        else if (corduraActual > sanityThreshold && isLowSanityPlaying)
        {
            lowSanityAudio.Stop();
            isLowSanityPlaying = false;
        }
    }
}
