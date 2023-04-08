using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Start is called before the first frame update
    private CombatManager combatManager;
    public Camera camera;

    public int currentCameraIndex;
    public int FighterIndex;
    public GameObject gameObjectFighter;
    private void Awake()
    {
        combatManager = FindObjectOfType<CombatManager>();
    }
    void Start()
    {
        currentCameraIndex = combatManager.FighterIndex;
    }

    // Update is called once per frame
    void Update()
    {
        FighterIndex = combatManager.FighterIndex;

        if (currentCameraIndex != combatManager.FighterIndex)
        {
            currentCameraIndex = combatManager.FighterIndex;
            ChangeCameraPositionToCurrentFighter();
        }
    }

    private void ChangeCameraPositionToCurrentFighter()
    {

        var currentFighter = combatManager.fighters[FighterIndex];

        gameObjectFighter = currentFighter.gameObject;
        camera.transform.position = currentFighter.CameraPivot.position;
        camera.transform.LookAt(currentFighter.transform);
    }
}
