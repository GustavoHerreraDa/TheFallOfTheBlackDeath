using System.Collections.Generic;
using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;
    
    [Header("Configuración")]
    public GameObject floatingTextPrefab;
    public Transform container; // Un objeto vacío dentro del Canvas para mantener el orden
    public int initialPoolSize = 20;

    // Usamos Stack en lugar de List para acceso O(1)
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
        // Instanciamos dentro del contenedor para no ensuciar la jerarquía
        GameObject obj = Instantiate(floatingTextPrefab, container);
        FloatingText txt = obj.GetComponent<FloatingText>();
        
        // Configuramos el objeto para que esté apagado y en la pila
        obj.SetActive(false);
        textPool.Push(txt); 
        return txt;
    }

    // Método mejorado con opción de "isCritical"
    public void ShowText(string message, Vector3 position, Color color, bool isCritical = false)
    {
        FloatingText txt;

        // Si la pila está vacía, creamos uno nuevo al vuelo
        if (textPool.Count == 0)
        {
            txt = CreateNewText();
            // Lo sacamos inmediatamente de la pila porque lo vamos a usar
            textPool.Pop(); 
        }
        else
        {
            txt = textPool.Pop();
        }

        // Activamos y configuramos
        txt.gameObject.SetActive(true);
        txt.transform.position = position;
        txt.Initialize(message, color, isCritical);
    }

    // Este método es llamado por el texto cuando termina su animación
    public void ReturnToPool(FloatingText txt)
    {
        txt.gameObject.SetActive(false);
        textPool.Push(txt); // Lo devolvemos a la cima de la pila
    }
}