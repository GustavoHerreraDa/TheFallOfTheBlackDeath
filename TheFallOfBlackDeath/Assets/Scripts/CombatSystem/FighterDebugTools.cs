using UnityEngine;

/// <summary>
/// Clase de utilidad para testear la destrucción de partes del cuerpo en el PlayerFighter.
/// </summary>
public class FighterDebugTools : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode modifierKey = KeyCode.LeftShift;
    public bool onlyInEditor = false;

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

    private void Awake()
    {
        Debug.Log("[FighterDebugTools] Componente activo. Usa Shift + 1-6 para destruir partes, 0 para restaurar.");
    }

    private void Update()
    {
        if (onlyInEditor && !Application.isEditor) return;

        if (Input.GetKey(modifierKey))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) DestroyPart(BodyPart.LeftArm);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) DestroyPart(BodyPart.RightArm);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) DestroyPart(BodyPart.LeftLeg);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) DestroyPart(BodyPart.RightLeg);
            else if (Input.GetKeyDown(KeyCode.Alpha0)) RestoreAll();
        }
    }

    private void DestroyPart(BodyPart part)
    {
        PlayerFighter player = null;
        
        // Intentar obtener el personaje principal desde el GameManager primero
        if (GameManager._instance != null)
        {
            player = GameManager._instance.character1;
        }

        // Fallback si no hay referencia en GameManager
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerFighter>();
        }

        if (player == null)
        {
            Debug.LogError("[FighterDebugTools] No se encontró el Main Character (character1) ni ningún PlayerFighter en la escena.");
            return;
        }
        
        Fighter.BodyPartData partData = player.GetBodyPart(part);
        if (partData == null)
        {
            Debug.LogError($"[FighterDebugTools] La parte {part} no existe en {player.idName}");
            return;
        }

        Debug.Log($"[FighterDebugTools] Destruyendo {part} de {player.idName} (Main Character)");
        
        // Aplicar daño letal a la parte específica para disparar OnBodyPartDestroyed
        player.ModifyBodyPartHealth(part, -9999f, player, null);
    }

    private void RestoreAll()
    {
        PlayerFighter player = null;
        
        if (GameManager._instance != null)
        {
            player = GameManager._instance.character1;
        }

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerFighter>();
        }

        if (player == null) return;

        Debug.Log($"[FighterDebugTools] Restaurando todas las partes y HP de {player.idName} (Main Character)");

        foreach (var part in player.bodyParts)
        {
            part.currentHealth = part.maxHealth;
        }
        
        player.ModifyHealth(player.stats.maxHealth);
        player.SyncBodyPartVisuals();
    }
}
