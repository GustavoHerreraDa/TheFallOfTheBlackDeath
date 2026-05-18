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
        
        [Header("UI Feedback (Optional)")]
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private TextMeshProUGUI promptText;

        private bool playerInRange = false;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(pickupId))
            {
                pickupId = System.Guid.NewGuid().ToString();
            }
        }

        private void Start()
        {
            // Comprobar si ya fue recogido
            if (GameManager.Instance != null && GameManager.Instance.IsPickupCollected(pickupId))
            {
                Destroy(gameObject);
                return;
            }

            if (interactionPrompt != null) interactionPrompt.SetActive(false);
            
            // Opcional: Podríamos intentar auto-detectar el nombre si no hay mensaje personalizado
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
                
                // Registrar recogida en el GameManager
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RegisterPickupCollected(pickupId);
                }
                
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
                // Si tienes un sistema de UI de interacción global, podrías mostrar el mensaje aquí
                // Por ejemplo, buscando un componente en el jugador o un singleton de UI
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
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(show);
                if (show && promptText != null && itemData != null)
                {
                    promptText.text = pickupMessage + itemData.itemName;
                }
            }
            else
            {
                // Fallback al log si no hay UI conectada
                if (show && itemData != null)
                {
                    Debug.Log($"{pickupMessage} {itemData.itemName}");
                }
            }
        }
    }
}
