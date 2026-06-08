using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProgressionStats
{
    public class StatsManager : MonoBehaviour
    {
        public static StatsManager Instance { get; private set; }

        private readonly List<string> defeatedEnemyIds = new List<string>();
        private readonly HashSet<string> uniqueCollectedItemIds = new HashSet<string>();
        private readonly Dictionary<BodyPart, int> attackedBodyParts = new Dictionary<BodyPart, int>();

        public IReadOnlyList<string> DefeatedEnemyIds => defeatedEnemyIds;
        public int CollectedItemsCount { get; private set; }
        public IReadOnlyDictionary<BodyPart, int> AttackedBodyParts => attackedBodyParts;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeBodyPartsDictionary();
        }

        private void InitializeBodyPartsDictionary()
        {
            foreach (BodyPart bodyPart in System.Enum.GetValues(typeof(BodyPart)))
            {
                if (bodyPart == BodyPart.None) continue;

                if (!attackedBodyParts.ContainsKey(bodyPart))
                {
                    attackedBodyParts[bodyPart] = 0;
                }
            }
        }

        public void RegisterEnemyDefeat(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId)) return;
            defeatedEnemyIds.Add(enemyId);
        }

        public void RegisterItemCollected(string itemId, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0) return;

            CollectedItemsCount += amount;
            uniqueCollectedItemIds.Add(itemId);
        }

        public void RegisterBodyPartAttack(BodyPart bodyPart)
        {
            if (bodyPart == BodyPart.None) return;

            if (!attackedBodyParts.ContainsKey(bodyPart))
            {
                attackedBodyParts[bodyPart] = 0;
            }

            attackedBodyParts[bodyPart]++;
        }

        public BodyPart GetMostAttackedBodyPart()
        {
            if (attackedBodyParts.Count == 0) return BodyPart.None;

            KeyValuePair<BodyPart, int> result = attackedBodyParts.OrderByDescending(entry => entry.Value).First();
            return result.Value > 0 ? result.Key : BodyPart.None;
        }

        public List<string> GetUniqueCollectedItems()
        {
            return uniqueCollectedItemIds.ToList();
        }
    }
}