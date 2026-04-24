using UnityEngine;
using System.Collections;

public class CursorManager : MonoBehaviour
{
    public Texture2D[] cursorFrames;
    public float frameRate = 0.1f;
    public Vector2 hotSpot = Vector2.zero;

    private int currentFrame;

    void Start()
    {
        StartCoroutine(AnimateCursor());
    }

    IEnumerator AnimateCursor()
    {
        while (true)
        {
            Cursor.SetCursor(cursorFrames[currentFrame], hotSpot, CursorMode.Auto);
            currentFrame = (currentFrame + 1) % cursorFrames.Length;
            yield return new WaitForSeconds(frameRate);
        }
    }
}