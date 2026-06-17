using UnityEngine;

/// <summary>
/// Clase de utilidad para testear la destrucción de partes del cuerpo en el PlayerFighter.
/// </summary>
public class FighterDebugTools : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode modifierKey = KeyCode.LeftShift;
    public bool onlyInEditor = true;

    [Header("Instructions")]
    [TextArea(3, 10)]
    public string instructions = "Mientras mantienes Shift:\n" +
                                "1: Destruir Cabeza\n" +
                                "2: Destruir Torso\n" +
                                "3: Destruir Brazo Izquierdo\n" +
                                "4: Destruir Brazo Derecho\n" +
                                "5: Destruir Pierna Izquierda\n" +
                                "6: Destruir Pierna Derecha\n" +
                                "0: Restaurar todo (Heal)";

    private void Update()
    {
        if (onlyInEditor && !Application.isEditor) return;

        if (Input.GetKey(modifierKey))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) DestroyPart(BodyPart.Head);
            if (Input.GetKeyDown(KeyCode.Alpha2)) DestroyPart(BodyPart.Torso);
            if (Input.GetKeyDown(KeyCode.Alpha3)) DestroyPart(BodyPart.LeftArm);
            if (Input.GetKeyDown(KeyCode.Alpha4)) DestroyPart(BodyPart.RightArm);
            if (Input.GetKeyDown(KeyCode.Alpha5)) DestroyPart(BodyPart.LeftLeg);
            if (Input.GetKeyDown(KeyCode.Alpha6)) DestroyPart(BodyPart.RightLeg);
            if (Input.GetKeyDown(KeyCode.Alpha0)) RestoreAll();
        }
    }

    private void DestroyPart(BodyPart part)
    {
        PlayerFighter player = FindFirstObjectByType<PlayerFighter>();
        if (player == null) return;

        Fighter.BodyPartData partData = player.GetBodyPart(part);
        if (partData == null) return;

        Debug.Log($"[DebugTools] Destruyendo {part} de {player.idName}");
        
        // Aplicar daño letal a la parte específica para disparar OnBodyPartDestroyed
        // Usamos un valor muy alto para asegurar que llegue a 0
        player.ModifyBodyPartHealth(part, -9999f, player, null);
    }

    private void RestoreAll()
    {
        PlayerFighter player = FindFirstObjectByType<PlayerFighter>();
        if (player == null) return;

        Debug.Log($"[DebugTools] Restaurando todas las partes y HP de {player.idName}");

        foreach (var part in player.bodyParts)
        {
            part.currentHealth = part.maxHealth;
            // Si estaba destruida, reseteamos el flag y resincronizamos visuales
        }
        
        player.ModifyHealth(player.stats.maxHealth);
        player.SyncBodyPartVisuals();
    }
}
