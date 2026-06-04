using UnityEngine;
using UnityEngine.Events;
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
        /// <summary>
        /// Define el comportamiento del objeto después de ser recogido.
        /// </summary>
        public enum PostPickupAction
        {
            Destroy,      // Destruye el objeto
            Deactivate,   // Desactiva el objeto
            KeepVisible   // Mantiene el objeto visible pero sin collider
        }

        [Header("Persistence")]
        public string pickupId;

        [Header("Item Configuration")]
        public NewItemData itemData;
        public int amount = 1;

        [Header("Interaction Settings")]
        [SerializeField] private string pickupMessage = "Presiona E para recoger ";
        [SerializeField] private PostPickupAction postPickupAction = PostPickupAction.Destroy;

        [Header("Events")]
        public UnityEvent onPickup = new UnityEvent();
        public UnityEvent onAlreadyCollected = new UnityEvent();

        private bool playerInRange = false;
        private bool isCollected = false;
        private Collider pickupCollider;

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

            pickupCollider = GetComponent<Collider>();
        }

        private void Start()
        {
            // Comprobar si ya fue recogido
            if (GameManager.Instance != null && GameManager.Instance.IsPickupCollected(GetPersistenceKey()))
            {
                // Marcar como ya recogido
                isCollected = true;

                // Desactivar el collider
                if (pickupCollider != null)
                {
                    pickupCollider.enabled = false;
                }

                // Disparar el evento de ya recogido
                onAlreadyCollected?.Invoke();

                // Ejecutar la acción según el enum
                ExecutePostPickupAction();

                return;
            }

            if (string.IsNullOrEmpty(pickupMessage) && itemData != null)
            {
                pickupMessage = "Recoger " + itemData.itemName;
            }
        }

        private void Update()
        {
            if (playerInRange && !isCollected && Input.GetKeyDown(KeyCode.E))
            {
                Pickup();
            }
        }

        public void Pickup()
        {
            // Evitar loots múltiples
            if (isCollected)
            {
                return;
            }

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
                // ───────────────────────────────────────────────────────────

                // Marcar como recogido
                isCollected = true;

                // Registrar recogida en el GameManager
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RegisterPickupCollected(GetPersistenceKey());
                }
                
                // Ocultar el prompt antes de procesar
                InteractionPromptUI.Instance?.Hide();

                // Desactivar el collider
                if (pickupCollider != null)
                {
                    pickupCollider.enabled = false;
                }

                // Disparar el evento de recogida
                onPickup?.Invoke();

                // Ejecutar la acción según el enum
                ExecutePostPickupAction();
            }
            else
            {
                Debug.LogError("[NewItemPickup] No se encontró NewInventoryManager en la escena.");
            }
        }

        private void ExecutePostPickupAction()
        {
            switch (postPickupAction)
            {
                case PostPickupAction.Destroy:
                    Destroy(gameObject);
                    break;

                case PostPickupAction.Deactivate:
                    gameObject.SetActive(false);
                    break;

                case PostPickupAction.KeepVisible:
                    // El objeto permanece en la escena sin collider (ya está desactivado)
                    break;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Charecter") && !isCollected)
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
