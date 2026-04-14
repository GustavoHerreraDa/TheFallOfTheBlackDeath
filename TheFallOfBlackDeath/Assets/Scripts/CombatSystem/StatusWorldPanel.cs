using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
//TP2 GUSTAVO TORRES
/// <summary>
/// Supports the combat system by handling status world panel.
/// </summary>
public class StatusWorldPanel : MonoBehaviour
{
    private Camera mainCamera;

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    void Start()
    {
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Applies late-frame adjustments after the main update loop has completed.
    /// </summary>
    public void LateUpdate()
    {
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }
}
