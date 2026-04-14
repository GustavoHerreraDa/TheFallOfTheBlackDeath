using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif 

/// <summary>
/// Handles barra de vida for the current project workflow.
/// </summary>
public class BarraDeVida : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/UI/Linear Progress Bar")]
    /// <summary>
    /// Adds the health bar.
    /// </summary>
    public static void AddHealthBar()
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("GameObject/UI/Linear Progress Bar"));
        obj.transform.SetParent(Selection.activeGameObject.transform, false);
    }
#endif
    public int minimum;
    public int maxium;
    public int current;
    public Image mask;
    public Image fill;
    public Color color;
    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        
    }

    // Update is called once per frame
    /// <summary>
    /// Updates the component each frame while it is active.
    /// </summary>
    void Update()
    {
        
    }

    /// <summary>
    /// Gets the current fill.
    /// </summary>
    void GetCurrentFill()
    {
        float maximumOffset = maxium - minimum;
        float currentOffSet = current - minimum;
        float fillAmount = currentOffSet / maximumOffset;
        mask.fillAmount = fillAmount;
        fill.color = color;
    }
}
