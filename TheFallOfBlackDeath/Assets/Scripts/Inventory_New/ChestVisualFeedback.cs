using UnityEngine;
using System.Collections;

namespace InventoryNew
{
    /// <summary>
    /// Componente para manejar la retroalimentación visual de cofres.
    /// Proporciona animaciones para abrir la tapa del cofre y mantener
    /// la persistencia visual al cambiar de escenas.
    /// </summary>
    public class ChestVisualFeedback : MonoBehaviour
    {
        [Header("Chest References")]
        [SerializeField] private Transform chestLid;

        [Header("Animation Settings")]
        [SerializeField] private Vector3 openRotation = new Vector3(90f, 0f, 0f);
        [SerializeField] private float animationDuration = 0.5f;

        private Coroutine openAnimationCoroutine;

        private void OnValidate()
        {
            if (chestLid == null && transform.childCount > 0)
            {
                chestLid = transform.GetChild(0);
            }
        }

        /// <summary>
        /// Abre el cofre con una animación suave usando Lerp.
        /// </summary>
        public void OpenChestAnimated()
        {
            // Detener la corrutina anterior si está en ejecución
            if (openAnimationCoroutine != null)
            {
                StopCoroutine(openAnimationCoroutine);
            }

            openAnimationCoroutine = StartCoroutine(OpenChestAnimatedCoroutine());
        }

        /// <summary>
        /// Abre el cofre instantáneamente sin animación.
        /// Útil para mantener la persistencia visual al cambiar de escena.
        /// </summary>
        public void SetChestOpenInstantly()
        {
            if (chestLid == null)
            {
                Debug.LogWarning("[ChestVisualFeedback] No hay referencia a la tapa del cofre (chestLid).");
                return;
            }

            chestLid.localEulerAngles = openRotation;
        }

        /// <summary>
        /// Corrutina que anima la apertura del cofre.
        /// </summary>
        private IEnumerator OpenChestAnimatedCoroutine()
        {
            if (chestLid == null)
            {
                Debug.LogWarning("[ChestVisualFeedback] No hay referencia a la tapa del cofre (chestLid).");
                yield break;
            }

            Vector3 initialRotation = chestLid.localEulerAngles;
            float elapsedTime = 0f;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / animationDuration);

                chestLid.localEulerAngles = Vector3.Lerp(initialRotation, openRotation, t);

                yield return null;
            }

            // Asegurar que la rotación final es exacta
            chestLid.localEulerAngles = openRotation;
        }
    }
}
