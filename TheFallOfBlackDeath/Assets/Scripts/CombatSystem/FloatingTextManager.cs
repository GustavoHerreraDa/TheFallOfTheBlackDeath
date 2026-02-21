using System.Collections.Generic;
using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;
    
    [Header("Configuración")]
    public GameObject floatingTextPrefab;
    public Transform container;
    public int initialPoolSize = 20;

    
    private Stack<FloatingText> textPool = new Stack<FloatingText>();

    void Awake()
    {
        Instance = this;
        InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewText();
        }
    }

    private FloatingText CreateNewText()
    {
        
        GameObject obj = Instantiate(floatingTextPrefab, container);
        FloatingText txt = obj.GetComponent<FloatingText>();
        
        
        obj.SetActive(false);
        textPool.Push(txt); 
        return txt;
    }

    
    public void ShowText(string message, Vector3 position, Color color, bool isCritical = false)
    {
        FloatingText txt;

       
        if (textPool.Count == 0)
        {
            txt = CreateNewText();
            
            textPool.Pop(); 
        }
        else
        {
            txt = textPool.Pop();
        }

        
        txt.gameObject.SetActive(true);
        txt.transform.position = position;
        txt.Initialize(message, color, isCritical);
    }

    
    public void ReturnToPool(FloatingText txt)
    {
        txt.gameObject.SetActive(false);
        textPool.Push(txt);
    }
}