using TMPro;
using UnityEngine;

namespace InventoryNew
{
    public class InventoryStatsUI : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private TMP_Text defeatedEnemiesText;
        [SerializeField] private TMP_Text collectedItemsText;
        [SerializeField] private TMP_Text favoriteBodyPartText;
        [SerializeField] private TMP_Text uniqueItemsText;

        private void OnEnable()
        {
            RefreshStatsUI();
        }

        public void RefreshStatsUI()
        {
            global::ProgressionStats.StatsManager statsManager = global::ProgressionStats.StatsManager.Instance;
            if (statsManager == null)
            {
                SetSafeDefaults();
                return;
            }

            if (defeatedEnemiesText != null)
            {
                defeatedEnemiesText.text = statsManager.DefeatedEnemyIds.Count.ToString();
            }

            if (collectedItemsText != null)
            {
                collectedItemsText.text = statsManager.CollectedItemsCount.ToString();
            }

            if (favoriteBodyPartText != null)
            {
                BodyPart favoritePart = statsManager.GetMostAttackedBodyPart();
                favoriteBodyPartText.text = favoritePart == BodyPart.None ? "-" : favoritePart.ToString();
            }

            if (uniqueItemsText != null)
            {
                uniqueItemsText.text = statsManager.GetUniqueCollectedItems().Count.ToString();
            }
        }

        private void SetSafeDefaults()
        {
            if (defeatedEnemiesText != null) defeatedEnemiesText.text = "0";
            if (collectedItemsText != null) collectedItemsText.text = "0";
            if (favoriteBodyPartText != null) favoriteBodyPartText.text = "-";
            if (uniqueItemsText != null) uniqueItemsText.text = "0";
        }
    }
}