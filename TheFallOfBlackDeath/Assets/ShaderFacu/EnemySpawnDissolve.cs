using System.Collections;
using UnityEngine;

public class EnemySpawnDissolve : MonoBehaviour
{
    [Header("Configuración del Efecto")]
    [Tooltip("Arrastra aquí las partes del enemigo (brazos, piernas, torso). Si lo dejas vacío, el script las buscará automáticamente.")]
    public Renderer[] enemyParts;
    
    [Tooltip("Altura en Y donde el enemigo es completamente invisible.")]
    public float startHeight = 2f; 
    
    [Tooltip("Altura en Y donde el enemigo ya está completamente materializado.")]
    public float endHeight = -2f;  
    
    [Tooltip("Duración de la animación en segundos.")]
    public float duration = 2.5f;

    // ID de la propiedad del shader para mayor rendimiento
    private int cutoffHeightID;
    private MaterialPropertyBlock propBlock;

    void Awake()
    {
        // Si no asignaste los renderers a mano en el inspector, los busca automáticamente en los hijos
        if (enemyParts == null || enemyParts.Length == 0)
        {
            enemyParts = GetComponentsInChildren<Renderer>();
        }

        // Convertimos el string exacto de tu material a ID 
        cutoffHeightID = Shader.PropertyToID("_CutoffHeight");
        propBlock = new MaterialPropertyBlock();
    }

    // Puedes llamar a esta función desde otro script, desde un evento de animación o un Timeline
    public void StartMaterialization()
    {
        StartCoroutine(MaterializeRoutine());
    }

    private IEnumerator MaterializeRoutine()
    {
        float elapsedTime = 0f;

        // Asegurarnos de que inicie en el valor correcto inmediatamente
        UpdateCutoffHeight(startHeight);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            
            // Suavizamos el tiempo para que empiece rápido y termine lento (opcional pero se ve mejor)
            float t = elapsedTime / duration;
            float smoothStep = Mathf.SmoothStep(0f, 1f, t); 
            
            // Calculamos la altura actual
            float currentHeight = Mathf.Lerp(startHeight, endHeight, smoothStep);

            // Aplicamos la altura a todos los renderers
            UpdateCutoffHeight(currentHeight);

            yield return null; // Esperamos al siguiente frame
        }

        // Nos aseguramos de clavar el valor final al terminar el bucle
        UpdateCutoffHeight(endHeight);
    }

    // Función auxiliar para aplicar el valor a todos los renderers sin crear copias de materiales
    private void UpdateCutoffHeight(float height)
    {
        foreach (Renderer rend in enemyParts)
        {
            if (rend != null)
            {
                rend.GetPropertyBlock(propBlock);
                propBlock.SetFloat(cutoffHeightID, height);
                rend.SetPropertyBlock(propBlock);
            }
        }
    }
}