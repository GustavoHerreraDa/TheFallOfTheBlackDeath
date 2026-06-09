using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool; // Requiere Unity 2021 o superior

public class MeshTrail : MonoBehaviour
{
    [Header("Configuración del Rastro")]
    public float activeTime = 2f;
    public float meshRefreshRate = 0.1f;
    public float meshDestroyDelay = 3f;
    public Transform positionToSpawn;

    [Header("Configuración del Shader / Material")]
    public Material mat;
    public string shaderVariableReference = "_Alpha";
    public float shaderVariableRate = 0.1f;
    public float shaderVariableRefreshRate = 0.05f;

    [Header("Optimización (Pool)")]
    public int defaultPoolSize = 10;
    public int maxPoolSize = 30;

    private bool isTrailActive;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private IObjectPool<TrailObject> trailPool;

    // Clase auxiliar interna para no tener que usar GetComponent cada vez que sacamos un objeto
    private class TrailObject
    {
        public GameObject gameObject;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public Mesh mesh;
        public Material materialInstance;
        public Coroutine fadeCoroutine;
        public Coroutine releaseCoroutine;
    }

    void Start()
    {
        if (positionToSpawn == null) positionToSpawn = this.transform;

        // Inicializa el sistema de Pooling
        trailPool = new ObjectPool<TrailObject>(
            createFunc: CreateTrailObject,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnedToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: false,
            defaultCapacity: defaultPoolSize,
            maxSize: maxPoolSize
        );
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));
        }
    }

    // ==========================================
    // LÓGICA DEL OBJECT POOL
    // ==========================================

    private TrailObject CreateTrailObject()
    {
        GameObject gObj = new GameObject("TrailMesh_Pooled");
        TrailObject to = new TrailObject();

        to.gameObject = gObj;
        to.meshFilter = gObj.AddComponent<MeshFilter>();
        to.meshRenderer = gObj.AddComponent<MeshRenderer>();

        // OPTIMIZACIÓN: Creamos el Mesh y el Material SOLO UNA VEZ.
        to.mesh = new Mesh();
        to.meshFilter.mesh = to.mesh;

        to.materialInstance = new Material(mat);
        to.meshRenderer.material = to.materialInstance;

        return to;
    }

    private void OnTakeFromPool(TrailObject to)
    {
        to.gameObject.SetActive(true);
        // Reseteamos la opacidad al 100% cada vez que reciclamos el objeto
        to.materialInstance.SetFloat(shaderVariableReference, 1f);
    }

    private void OnReturnedToPool(TrailObject to)
    {
        to.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(TrailObject to)
    {
        // Limpiamos la memoria correctamente si el Pool excede su tamaño máximo
        Destroy(to.mesh);
        Destroy(to.materialInstance);
        Destroy(to.gameObject);
    }

    // ==========================================
    // LÓGICA DEL RASTRO
    // ==========================================

    IEnumerator ActivateTrail(float timeActive)
    {
        if (skinnedMeshRenderers == null)
        {
            skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        while (timeActive > 0)
        {
            timeActive -= meshRefreshRate;

            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                // Obtenemos un clon inactivo del Pool en vez de instanciar uno nuevo
                TrailObject trailObj = trailPool.Get();
                trailObj.gameObject.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                // Sobrescribe la pose en la malla ya existente
                skinnedMeshRenderers[i].BakeMesh(trailObj.mesh);

                // Detenemos rutinas previas si el objeto fue reciclado rápido
                if (trailObj.fadeCoroutine != null) StopCoroutine(trailObj.fadeCoroutine);
                if (trailObj.releaseCoroutine != null) StopCoroutine(trailObj.releaseCoroutine);

                // Iniciamos el fade out y el temporizador para devolverlo al Pool
                trailObj.fadeCoroutine = StartCoroutine(AnimateMaterialFloat(trailObj, 0f, shaderVariableRate, shaderVariableRefreshRate));
                trailObj.releaseCoroutine = StartCoroutine(ReleaseAfterDelay(trailObj, meshDestroyDelay));
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }

        isTrailActive = false;
    }

    IEnumerator AnimateMaterialFloat(TrailObject trailObj, float goal, float rate, float refreshRate)
    {
        float valueToAnimate = trailObj.materialInstance.GetFloat(shaderVariableReference);

        while (valueToAnimate > goal)
        {
            valueToAnimate -= rate;
            trailObj.materialInstance.SetFloat(shaderVariableReference, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }

    IEnumerator ReleaseAfterDelay(TrailObject trailObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        // En lugar de hacer Destroy(), simplemente lo apagamos para reutilizarlo
        trailPool.Release(trailObj);
    }
}