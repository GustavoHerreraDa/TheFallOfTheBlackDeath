using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIGlitchImage : MonoBehaviour
{
    public Image image;

    Material runtimeMat;

    void Awake()
    {
        runtimeMat = Instantiate(image.material);
        image.material = runtimeMat;
    }

    public void PlayGlitch(float duration = 0.2f, float strength = 1f)
    {
        StopAllCoroutines();
        StartCoroutine(GlitchRoutine(duration, strength));
    }

    IEnumerator GlitchRoutine(float duration, float strength)
    {
        float t = 0f;

        runtimeMat.SetFloat("_GlitchStrength", strength);

        while (t < duration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        runtimeMat.SetFloat("_GlitchStrength", 0f);
    }
}
