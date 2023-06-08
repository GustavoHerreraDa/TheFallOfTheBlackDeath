using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif 

public class BarraDeVida : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/UI/Linear Progress Bar")]
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
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GetCurrentFill()
    {
        float maximumOffset = maxium - minimum;
        float currentOffSet = current - minimum;
        float fillAmount = currentOffSet / maximumOffset;
        mask.fillAmount = fillAmount;
        fill.color = color;
    }
}