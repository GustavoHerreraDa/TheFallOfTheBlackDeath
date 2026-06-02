using UnityEngine;
using InventoryNew;
using TMPro;

namespace InventoryNew
{
    /// <summary>
    /// Componente para permitir que el jugador recoja objetos del mundo físico
    /// y los añada al NewInventoryManager.
    /// </summary>
    public class NewItemPickup : MonoBehaviour
    {
        [Header("Persistence")]
        public string pickupId;

        [Header("Item Configuration")]
        public NewItemData itemData;
        public int amount = 1;

        [Header("Interaction Settings")]
        [SerializeField] private string pickupMessage = "Presiona E para recoger ";
        [SerializeField] private bool destroyOnPickup = true;
        
        private bool playerInRange = false;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(pickupId))
            {
                pickupId = System.Guid.NewGuid().ToString();
            }
        }

        private void Awake()
        {
            if (string.IsNullOrEmpty(pickupId))
            {
                pickupId = System.Guid.NewGuid().ToString();
            }
        }

        private void Start()
        {
            // Comprobar si ya fue recogido
            if (GameManager.Instance != null && GameManager.Instance.IsPickupCollected(GetPersistenceKey()))
            {
                Destroy(gameObject);
                return;
            }

            if (string.IsNullOrEmpty(pickupMessage) && itemData != null)
            {
                pickupMessage = "Recoger " + itemData.itemName;
            }
        }

        private void Update()
        {
            if (playerInRange && Input.GetKeyDown(KeyCode.E))
            {
                Pickup();
            }
        }

        public void Pickup()
        {
            if (itemData == null)
            {
                Debug.LogWarning($"[NewItemPickup] {gameObject.name} no tiene ItemData asignado.");
                return;
            }

            if (NewInventoryManager.Instance != null)
            {
                NewInventoryManager.Instance.AddItem(itemData, amount);
                Debug.Log($"[NewItemPickup] Recogido: {itemData.itemName} x{amount}");
                
                // ── NOTIFICATION ────────────────────────────────────────────
                if (ItemNotificationManager.Instance != null)
                {
                    ItemNotificationManager.Instance.NotifyPickup(itemData, amount);
                }
                else
                {
                    Debug.LogWarning($"[NewItemPickup] No se pudo notificar pickup de '{itemData.itemName}' porque ItemNotificationManager.Instance es null.");
                }
                // ────────────────────────────────────────────────────────────

                // Registrar recogida en el GameManager
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RegisterPickupCollected(GetPersistenceKey());
                }
                
                // Ocultar el prompt antes de destruir/desactivar
                InteractionPromptUI.Instance?.Hide();

                if (destroyOnPickup)
                {
                    Destroy(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogError("[NewItemPickup] No se encontró NewInventoryManager en la escena.");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Charecter"))
            {
                playerInRange = true;
                ShowInteractionPrompt(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Charecter"))
            {
                playerInRange = false;
                ShowInteractionPrompt(false);
            }
        }

        private void ShowInteractionPrompt(bool show)
        {
            if (show && itemData != null)
            {
                InteractionPromptUI.Instance?.Show($"[ E ]  Recoger {itemData.itemName}");
            }
            else if (!show)
            {
                InteractionPromptUI.Instance?.Hide();
            }
        }

        public string GetPersistenceKey()
        {
            return $"{gameObject.scene.name}:{pickupId}:{GetHierarchyPath(transform)}";
        }

        private string GetHierarchyPath(Transform current)
        {
            string path = current.name;
            while (current.parent != null)
            {
                current = current.parent;
                path = current.name + "/" + path;
            }

            return path;
        }
    }
}