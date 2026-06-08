using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    [Header("Configuración del Rastro")]
    public float activeTime = 2f;                // Tiempo total que durará el efecto activo
    public float meshRefreshRate = 0.1f;          // Cada cuántos segundos se genera una nueva copia de la malla
    public float meshDestroyDelay = 3f;          // Tiempo antes de destruir por completo el objeto copiado
    public Transform positionToSpawn;            // El transform del personaje (origen de la posición y rotación)

    [Header("Configuración del Shader / Material")]
    public Material mat;                         // Material con el shader de brillo/transparencia
    public string shaderVariableReference = "_Alpha"; // Nombre de la propiedad de transparencia en tu Shader Graph
    public float shaderVariableRate = 0.1f;      // Cantidad que disminuye la opacidad en cada paso
    public float shaderVariableRefreshRate = 0.05f; // Frecuencia con la que se actualiza el desvanecimiento

    private bool isTrailActive;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;

    void Update()
    {
        // Detecta la pulsación de la barra espaciadora para iniciar el rastro
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));
        }
    }

    IEnumerator ActivateTrail(float timeActive)
    {
        while (timeActive > 0)
        {
            timeActive -= meshRefreshRate;

            // Obtiene todos los componentes SkinnedMeshRenderer de los hijos si no se han guardado antes
            if (skinnedMeshRenderers == null)
            {
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            }

            // Recorre cada malla del personaje para clonarla
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                GameObject gObj = new GameObject();
                gObj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
                MeshFilter mf = gObj.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                // Toma una "instantánea" de la pose actual de la animación
                skinnedMeshRenderers[i].BakeMesh(mesh);

                mf.mesh = mesh;
                mr.material = mat;

                // Inicia el desvanecimiento del material de forma independiente
                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVariableRate, shaderVariableRefreshRate));

                // Destruye la malla clonada después de un tiempo
                Destroy(gObj, meshDestroyDelay);
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }

        isTrailActive = false;
    }

    IEnumerator AnimateMaterialFloat(Material mat, float goal, float rate, float refreshRate)
    {
        float valueToAnimate = mat.GetFloat(shaderVariableReference);

        // Reduce gradualmente el valor alfa (opacidad) hasta llegar a 0
        while (valueToAnimate > goal)
        {
            valueToAnimate -= rate;
            mat.SetFloat(shaderVariableReference, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }
}