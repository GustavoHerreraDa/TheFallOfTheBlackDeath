using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Supports exploration and world-state flow by handling sanity system.
/// </summary>
public class SanitySystem : MonoBehaviour
{
    [Header("Sanity Stats")]
    public float corduraMax = 100f;
    public float corduraActual;
    public float perdidaDeCordura = 2f;
    public float sanityThreshold = 20f;
    public float desperationThreshold = 50f;

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

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        corduraActual = corduraMax;
        StartDecreaseSanity();
    }

    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    void Update()
    {
        UpdateUI();
        UpdateVignette();
        UpdateLowSanitySound();
    }

    /// <summary>
    /// Executes the start decrease sanity workflow.
    /// </summary>
    public void StartDecreaseSanity()
    {
        if (corduraRoutine != null)
            StopCoroutine(corduraRoutine);

        corduraRoutine = StartCoroutine(BajarCordura());
    }

    /// <summary>
    /// Executes the start increase sanity workflow.
    /// </summary>
    public void StartIncreaseSanity()
    {
        if (corduraRoutine != null)
            StopCoroutine(corduraRoutine);

        corduraRoutine = StartCoroutine(AumentarCordura());
    }

    /// <summary>
    /// Executes the stop sanity changes workflow.
    /// </summary>
    public void StopSanityChanges()
    {
        if (corduraRoutine != null)
            StopCoroutine(corduraRoutine);
    }

    /// <summary>
    /// Executes the bajar cordura workflow.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
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

    /// <summary>
    /// Executes the aumentar cordura workflow.
    /// </summary>
    /// <returns>An enumerator that drives the coroutine sequence.</returns>
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


    /// <summary>
    /// Updates the ui.
    /// </summary>
    void UpdateUI()
    {
        if (textCordura != null)
            textCordura.text = "Sanity: " + Mathf.RoundToInt(corduraActual);
    }

    /// <summary>
    /// Updates the vignette.
    /// </summary>
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

    /// <summary>
    /// Updates the low sanity sound.
    /// </summary>
    void UpdateLowSanitySound()
    {
        if (lowSanityAudio == null) return;

        if (corduraActual <= sanityThreshold && !isLowSanityPlaying)
        {
            lowSanityAudio.Play();
            isLowSanityPlaying = true;
        }
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="isLowSanityPlaying">The is low sanity playing.</param>
        /// <returns>The resulting value.</returns>
        else if (corduraActual > sanityThreshold && isLowSanityPlaying)
        {
            lowSanityAudio.Stop();
            isLowSanityPlaying = false;
        }
    }

    // Decrease sanity instantly by a given amount and clamp within [0, corduraMax]
    /// <summary>
    /// Executes the decrease sanity instantly workflow.
    /// </summary>
    /// <param name="amount">The amount.</param>
    public void DecreaseSanityInstantly(float amount)
    {
        if (amount <= 0f) return; // no-op for non-positive inputs
        corduraActual = Mathf.Clamp(corduraActual - amount, 0f, corduraMax);
    }

    // Helper to check if current sanity is in desperation zone
    /// <summary>
    /// Determines whether the component is in desperation.
    /// </summary>
    /// <returns>True when the requested condition is met; otherwise, false.</returns>
    public bool IsInDesperation()
    {
        return corduraActual <= desperationThreshold;
    }
}
