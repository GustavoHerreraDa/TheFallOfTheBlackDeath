using UnityEngine;

/// <summary>
/// Se coloca en cualquier panel (como Equipment) para instanciar una copia local 
/// del modelo 3D del personaje seleccionado actualmente.
/// </summary>
public class PanelModelInstantiator : CharacterPreviewUI
{
    [Header("Configuración de Instanciación")]
    [SerializeField] private Transform spawnAnchor; // El objeto Padre vacío dentro del panel donde se ubicará el modelo

    private GameObject currentSpawnedModel;

    protected override void OnFighterChanged(PlayerFighter fighter)
    {
        // Ejecuta la lógica base (como el log en consola si está activo)
        base.OnFighterChanged(fighter);

        if (fighter == null) return;

        // 1. Destruir el modelo del personaje anterior si existe
        LimpiarModeloAnterior();

        // 2. Pedirle el modelo al CharacterDisplayManager
        GameObject originalModel = CharacterDisplayManager.Instance.GetModel(fighter.figherIndex);

        if (originalModel != null)
        {
            // 3. Instanciar la copia bajo nuestro punto de anclaje (spawnAnchor)
            currentSpawnedModel = Instantiate(originalModel, spawnAnchor);

            // 4. Asegurarnos de que sea visible (por si el original está desactivado en el manager)
            currentSpawnedModel.SetActive(true);

            // 5. Resetear las coordenadas locales para que quede perfectamente alineado al anchor
            currentSpawnedModel.transform.localPosition = Vector3.zero;
            currentSpawnedModel.transform.localRotation = Quaternion.identity;
            currentSpawnedModel.transform.localScale = Vector3.one;
            
            // TIP: Si tus modelos tienen componentes de lógica, IA o físicas que no querés 
            // que corran dentro del menú, deberías desactivados acá.
        }
    }

    private void LimpiarModeloAnterior()
    {
        if (currentSpawnedModel != null)
        {
            Destroy(currentSpawnedModel);
            currentSpawnedModel = null;
        }
    }

    private void OnDestroy()
    {
        // Limpieza preventiva si se destruye el panel de golpe
        LimpiarModeloAnterior();
    }
}