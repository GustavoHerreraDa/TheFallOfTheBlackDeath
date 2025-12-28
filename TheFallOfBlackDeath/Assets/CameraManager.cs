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

    [Header("Hit Camera Effect")]
    [SerializeField] private float hitZoomFOV = 40f;
    [SerializeField] private float hitZoomSpeed = 12f;
    [SerializeField] private float hitRecoverSpeed = 8f;
    [SerializeField] private float hitMoveAmount = 0.3f;

    private float defaultFOV;
    private Coroutine hitCoroutine;

    [SerializeField]
    private float cameraSpeed;
    private void Awake()

    

    {
        combatManager = FindObjectOfType<CombatManager>();
    }
    void Start()
    {
        currentCameraIndex = combatManager.fighterIndex;
        defaultFOV = camera.fieldOfView;
    }

    // Update is called once per frame
    /*void Update()
    {
        FighterIndex = combatManager.fighterIndex;

        if (currentCameraIndex != combatManager.fighterIndex)
        {
            currentCameraIndex = combatManager.fighterIndex;
            ChangeCameraPositionToCurrentFighter();

        }
    }*/

    private void Update()
    {
        FighterIndex = combatManager.fighterIndex;

        if (currentCameraIndex != combatManager.fighterIndex)
        {
            currentCameraIndex = combatManager.fighterIndex;

            if (currentCameraIndex >= 0 && currentCameraIndex < combatManager.fighters.Length)
            {
                ChangeCameraPositionToCurrentFighter();
            }
        }
    }

    public void PlayHitCameraEffect(Transform attacker, Transform defender)
    {
        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

        hitCoroutine = StartCoroutine(HitCameraEffect(attacker, defender));
    }

    IEnumerator HitCameraEffect(Transform attacker, Transform defender)
    {
        Vector3 originalPos = camera.transform.position;

        // Punto medio entre atacante y defensor
        Vector3 hitPoint = (attacker.position + defender.position) * 0.5f;
        Vector3 dirToHit = (hitPoint - camera.transform.position).normalized;
        Vector3 zoomPos = camera.transform.position + dirToHit * hitMoveAmount;

        float t = 0f;

        // ZOOM IN
        while (t < 1f)
        {
            t += Time.deltaTime * hitZoomSpeed;
            camera.fieldOfView = Mathf.Lerp(defaultFOV, hitZoomFOV, t);
            camera.transform.position = Vector3.Lerp(originalPos, zoomPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.05f); // micro pausa de impacto

        t = 0f;

        // RECOVER
        while (t < 1f)
        {
            t += Time.deltaTime * hitRecoverSpeed;
            camera.fieldOfView = Mathf.Lerp(hitZoomFOV, defaultFOV, t);
            camera.transform.position = Vector3.Lerp(zoomPos, originalPos, t);
            yield return null;
        }

        camera.fieldOfView = defaultFOV;
        camera.transform.position = originalPos;
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
