using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CursorManager : MonoBehaviour
{
    
    [Header("Settings")]
    public bool alwaysVisible = false; 
    
    public static CursorManager Instance { get; private set; }

    public Texture2D[] cursorFrames;
    public float frameRate = 0.1f;
    public Vector2 hotSpot = Vector2.zero;

    private int currentFrame;
    private HashSet<object> requesters = new HashSet<object>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // No se especifica Don'tDestroyOnLoad, se asume que existe en la escena necesaria
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(AnimateCursor());
        EvaluateCursorState();
    }

    public void RequestCursor(object requester)
    {
        if (requester == null) return;
        requesters.Add(requester);
        EvaluateCursorState();
    }

    public void ReleaseCursor(object requester)
    {
        if (requester == null) return;
        requesters.Remove(requester);
        EvaluateCursorState();
    }

    private void EvaluateCursorState()
    {
        
        bool shouldShow = alwaysVisible || requesters.Count > 0;
        Cursor.visible = shouldShow;
        Cursor.lockState = shouldShow ? CursorLockMode.None : CursorLockMode.Locked;
        
        Debug.Log($"[CursorManager] Evaluate: visible={Cursor.visible}, lockState={Cursor.lockState}, requesters={requesters.Count}");
    }

    IEnumerator AnimateCursor()
    {
        while (true)
        {
            if (cursorFrames != null && cursorFrames.Length > 0)
            {
                Cursor.SetCursor(cursorFrames[currentFrame], hotSpot, CursorMode.Auto);
                currentFrame = (currentFrame + 1) % cursorFrames.Length;
            }
            yield return new WaitForSeconds(frameRate);
        }
    }
}