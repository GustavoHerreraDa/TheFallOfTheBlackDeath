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

    [SerializeField]
    private float cameraSpeed;
    private void Awake()
    {
        combatManager = FindObjectOfType<CombatManager>();
    }
    void Start()
    {
        currentCameraIndex = combatManager.fighterIndex;
    }

    // Update is called once per frame
    void Update()
    {
        FighterIndex = combatManager.fighterIndex;

        if (currentCameraIndex != combatManager.fighterIndex)
        {
            currentCameraIndex = combatManager.fighterIndex;
            ChangeCameraPositionToCurrentFighter();

        }
    }


    //private void ChangeCameraPositionToCurrentFighter()
    //{
    //    var currentFighter = combatManager.fighters[FighterIndex];

    //    // Utiliza Lerp para interpolar suavemente entre la posición actual de la cámara y la nueva posición
    //    //Vector3 targetDirection = lookTarget.position - camera.transform.position;
    //    //Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

    //    StartCoroutine(MoveCameraSmoothly(camera.transform.position, currentFighter.CameraPivot.position, camera.transform.rotation, currentFighter.CameraPivot.rotation, cameraSpeed));
    //}

    private void ChangeCameraPositionToCurrentFighter()
    {

        var currentFighter = combatManager.fighters[FighterIndex];

        //gameObjectFighter = currentFighter.gameObject;
        //camera.transform.position = currentFighter.CameraPivot.position;
        //camera.transform.LookAt(currentFighter.transform);
        StartCoroutine(MoveCameraSmoothly(camera.transform.position, currentFighter.CameraPivot.position, camera.transform.rotation, currentFighter.CameraPivot.rotation, cameraSpeed));
    }


    IEnumerator MoveCameraSmoothly(Vector3 startPos, Vector3 endPos, Quaternion startRot, Quaternion endRot, float speed)
    {
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * speed;
            camera.transform.position = Vector3.Lerp(startPos, endPos, t);
            camera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
        //Debug.Log("Final del traslado Smooth ");

        //camera.transform.rotation = Quaternion.RotateTowards(camera.transform.rotation, targetRotation, 45);


    }
}
