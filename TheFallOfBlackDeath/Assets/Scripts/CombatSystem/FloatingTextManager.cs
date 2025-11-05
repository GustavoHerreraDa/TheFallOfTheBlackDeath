using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;
    public GameObject floatingTextPrefab;
    public Canvas worldCanvas;

    void Awake()
    {
        Instance = this;
    }

    public void ShowText(string message, Vector3 position, Color color)
    {
        if (!floatingTextPrefab) return;

        GameObject textObj = Instantiate(floatingTextPrefab, worldCanvas.transform);
        textObj.transform.position = position;

        FloatingText floatingText = textObj.GetComponent<FloatingText>();
        floatingText.SetText(message, color);
    }
}
