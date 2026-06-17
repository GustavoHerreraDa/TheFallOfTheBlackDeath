using System.Collections.Generic;

using UnityEngine;
//TP2 AUGUSTO NANINI/FACUNDO FERREIRO

public struct DamageReceivedEventData
{
    public readonly Fighter attacker;
    public readonly Fighter receiver;
    public readonly Skill sourceSkill;
    public readonly BodyPart bodyPart;
    public readonly float requestedAmount;
    public readonly float appliedAmount;
    public readonly float previousHealth;
    public readonly float currentHealth;
    public readonly bool affectedBodyPart;
    public readonly bool destroyedBodyPart;
    public readonly DamageResult damageResult;

    public bool IsDamage => appliedAmount < 0f;

    public DamageReceivedEventData(
        Fighter attacker,
        Fighter receiver,
        Skill sourceSkill,
        BodyPart bodyPart,
        float requestedAmount,
        float appliedAmount,
        float previousHealth,
        float currentHealth,
        bool affectedBodyPart,
        bool destroyedBodyPart,
        DamageResult damageResult = default(DamageResult))
    {
        this.attacker = attacker;
        this.receiver = receiver;
        this.sourceSkill = sourceSkill;
        this.bodyPart = bodyPart;
        this.requestedAmount = requestedAmount;
        this.appliedAmount = appliedAmount;
        this.previousHealth = previousHealth;
        this.currentHealth = currentHealth;
        this.affectedBodyPart = affectedBodyPart;
        this.destroyedBodyPart = destroyedBodyPart;
        this.damageResult = damageResult;
    }
}

/// <summary>
/// Defines the shared combatant model used by players and enemies, including stats, body-part damage, status conditions, and turn behavior.
/// </summary>
public abstract class Fighter : MonoBehaviour
{
    [System.Serializable]
    /// <summary>
    /// Stores the health, hit point, and destruction penalties associated with an individual body part.
    /// </summary>
    public class BodyPartData
    {
        public BodyPart part;
        public Transform hitPoint;
        public float baseMaxHealth = 100f;
        public float currentHealth;
        
        // Propiedad maxHealth que se sincroniza con el dueño del Fighter (si existe)
        private Fighter _owner;
        public float maxHealth 
        {
            get 
            {
                if (_owner != null) return GetMaxHealth(_owner);
                // Si no hay dueño asignado, intentamos usar el baseMaxHealth pero logueamos advertencia si es inesperado
                return baseMaxHealth;
            }
        }
        
        
      
        public PartStatus currentStatus = PartStatus.None;
        [Header("Penalizaciones al destruirse")]
        public List<StatusMod> destructionPenalties = new List<StatusMod>();

        [Header("Prótesis")]
        public float prostheticCurrentHealth = 0f;   // 0 si no hay prótesis activa
        [Tooltip("Renderer del mesh de prótesis de esta parte. Asignar en el Inspector del prefab del personaje. Debe estar desactivado (enabled=false) por defecto.")]
        public Renderer prostheticRenderer;
        public bool HasActiveProsthetic => prostheticCurrentHealth > 0f;
        public bool IsEffectivelyFunctional => !IsDestroyed || HasActiveProsthetic;

        /// <summary>
        /// Initializes a new instance of the <see cref="BodyPartData"/> class.
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="health">The health.</param>
        public BodyPartData(BodyPart part, float health)
        {
            this.part = part;
            this.baseMaxHealth = health;
            this.currentHealth = health;
            this.currentStatus = PartStatus.None;
        }

        // Método para vincular al dueño y recalcular maxHealth si es necesario
        public void SetOwner(Fighter owner)
        {
            _owner = owner;
        }

        public bool IsDestroyed => currentHealth <= 0;

        // Propiedad para obtener el maxHealth real (Base + Equipo)
        public float GetMaxHealth(Fighter owner)
        {
            if (owner is PlayerFighter player && player.equipmentHandler != null)
            {
                InventoryNew.EquipmentSlot slot = MapPartToSlot(this.part);
                float bonus = player.equipmentHandler.GetModifierForSlot(slot, InventoryNew.StatType.MaxHealth);
                return baseMaxHealth + bonus;
            }
            return baseMaxHealth;
        }

        public InventoryNew.EquipmentSlot MapPartToSlot(BodyPart part)
        {
            switch (part)
            {
                case BodyPart.Head: return InventoryNew.EquipmentSlot.Head;
                case BodyPart.Torso: return InventoryNew.EquipmentSlot.Torso;
                case BodyPart.LeftArm: return InventoryNew.EquipmentSlot.LeftArm;
                case BodyPart.RightArm: return InventoryNew.EquipmentSlot.RightArm;
                case BodyPart.LeftLeg: return InventoryNew.EquipmentSlot.LeftLeg;
                case BodyPart.RightLeg: return InventoryNew.EquipmentSlot.RightLeg;
                default: return InventoryNew.EquipmentSlot.Accessory;
            }
        }

        public BodyPart MapSlotToBodyPart(InventoryNew.EquipmentSlot slot)
        {
            switch (slot)
            {
                case InventoryNew.EquipmentSlot.Head: return BodyPart.Head;
                case InventoryNew.EquipmentSlot.Torso: return BodyPart.Torso;
                case InventoryNew.EquipmentSlot.LeftArm: return BodyPart.LeftArm;
                case InventoryNew.EquipmentSlot.RightArm: return BodyPart.RightArm;
                case InventoryNew.EquipmentSlot.LeftLeg: return BodyPart.LeftLeg;
                case InventoryNew.EquipmentSlot.RightLeg: return BodyPart.RightLeg;
                default: return BodyPart.None;
            }
        }
    }
    public List<BodyPartData> bodyParts;

    [Header("Visual Effects")]
    [SerializeField] 
    private GameObject partDestroyedVFX;

    [Header("Damage Glitch")]
    [Tooltip("Material swapped onto the damaged body part during the hit reaction. Drag Glitch.mat here.")]
    public Material damageGlitchMaterial;

    public event System.Action<BodyPart> OnBodyPartDestroyedEvent;
    public event System.Action<DamageReceivedEventData> OnDamageReceived;
    public event System.Action<DamageResult> OnDamageResolved;

    public Team team;
    public string idName;
    public StatusPanel statusPanel;
    public Animator animator;
    public CombatManager combatManager;
    public AudioSource audioSource;
    public delegate void HealthModificationDelegate(float amount);
    public HealthModificationDelegate healthModificationDelegate;
    public List<StatusMod> statusMods;
    public int brokenLegCount
    {
        get
        {
            if (bodyParts == null) return 0;
            int count = 0;
            foreach (var part in bodyParts)
            {
                if ((part.part == BodyPart.LeftLeg || part.part == BodyPart.RightLeg)
                    && part.IsDestroyed
                    && !part.HasActiveProsthetic) // NUEVO: prótesis activa = no cuenta como rota
                    count++;
            }
            return count;
        }
    }
    public bool legBroken => brokenLegCount > 0;
    public bool oneLegBroken => brokenLegCount == 1;
    public bool bothLegsBroken => brokenLegCount >= 2;
    public Stats stats;
    public Stats modedStats;
    public Skill[] skills;
    public StatusCondition statusCondition;
    private List<BodyPartStatusCondition> bodyPartStatusConditions;
    private static readonly Renderer[] EmptyRendererCache = new Renderer[0];
    private readonly Dictionary<BodyPart, Renderer[]> renderersByBodyPart = new Dictionary<BodyPart, Renderer[]>();
    private readonly Dictionary<Renderer, Material[]> damageGlitchSlotsByRenderer = new Dictionary<Renderer, Material[]>();
    private bool renderersByBodyPartCacheReady;

    public Transform uiAnchor;
    public Transform scannerAnchor;

    [SerializeField]
    public Transform CameraPivot;

    [SerializeField]
    public Transform DamagePivot;

    /// <summary>
    /// Gets the body part.
    /// </summary>
    /// <param name="part">The part.</param>
    /// <returns>The resulting value.</returns>
    public BodyPartData GetBodyPart(BodyPart part)
    {
        return bodyParts.Find(p => p.part == part);
    }

    /// <summary>Convierte BodyPart a EquipmentSlot. Inverso del MapPartToSlot de BodyPartData.</summary>
    public static InventoryNew.EquipmentSlot BodyPartToEquipmentSlot(BodyPart part)
    {
        switch (part)
        {
            case BodyPart.Head:     return InventoryNew.EquipmentSlot.Head;
            case BodyPart.Torso:    return InventoryNew.EquipmentSlot.Torso;
            case BodyPart.LeftArm:  return InventoryNew.EquipmentSlot.LeftArm;
            case BodyPart.RightArm: return InventoryNew.EquipmentSlot.RightArm;
            case BodyPart.LeftLeg:  return InventoryNew.EquipmentSlot.LeftLeg;
            case BodyPart.RightLeg: return InventoryNew.EquipmentSlot.RightLeg;
            default:                return InventoryNew.EquipmentSlot.Accessory;
        }
    }

    public static BodyPart EquipmentSlotToBodyPart(InventoryNew.EquipmentSlot slot)
    {
        switch (slot)
        {
            case InventoryNew.EquipmentSlot.Head:     return BodyPart.Head;
            case InventoryNew.EquipmentSlot.Torso:    return BodyPart.Torso;
            case InventoryNew.EquipmentSlot.LeftArm:  return BodyPart.LeftArm;
            case InventoryNew.EquipmentSlot.RightArm: return BodyPart.RightArm;
            case InventoryNew.EquipmentSlot.LeftLeg:  return BodyPart.LeftLeg;
            case InventoryNew.EquipmentSlot.RightLeg: return BodyPart.RightLeg;
            default:                                   return BodyPart.None;
        }
    }

    public bool isAlive
    {
        // Verifica que stats no sea nulo antes de acceder a health
        get => this.stats != null && this.stats.health > 0;
    }

    /// <summary>
    /// Initializes the component once the scene dependencies are ready.
    /// </summary>
    protected virtual void Start()
    {
        if (this.statusPanel != null)
            this.statusPanel.SetStats(this.idName, this.stats);

        this.skills = this.GetComponentsInChildren<Skill>();
        this.modedStats = stats;
        this.statusMods = new List<StatusMod>();
        this.bodyPartStatusConditions = new List<BodyPartStatusCondition>();
        EnsureBodyPartRendererCache();

    }

    /// <summary>
    /// Builds visual lookup data as soon as the fighter becomes active.
    /// </summary>
    protected virtual void OnEnable()
    {
        renderersByBodyPartCacheReady = false;
        EnsureBodyPartRendererCache();
    }

    private void EnsureBodyPartRendererCache()
    {
        if (renderersByBodyPartCacheReady) return;

        BuildBodyPartRendererCache();
    }

    private void BuildBodyPartRendererCache()
    {
        renderersByBodyPart.Clear();
        renderersByBodyPartCacheReady = true;

        if (bodyParts == null || bodyParts.Count == 0) return;

        var partKeys = new List<BodyPart>(bodyParts.Count);
        var partNames = new List<string>(bodyParts.Count);
        var rendererBuckets = new List<List<Renderer>>(bodyParts.Count);

        foreach (var partData in bodyParts)
        {
            if (partData == null || partData.part == BodyPart.None || renderersByBodyPart.ContainsKey(partData.part))
                continue;

            renderersByBodyPart[partData.part] = EmptyRendererCache;
            partKeys.Add(partData.part);
            partNames.Add(partData.part.ToString());
            rendererBuckets.Add(new List<Renderer>());
        }

        if (partKeys.Count == 0) return;

        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        for (int rendererIndex = 0; rendererIndex < allRenderers.Length; rendererIndex++)
        {
            Renderer renderer = allRenderers[rendererIndex];
            if (renderer == null) continue;

            string rendererName = renderer.name;
            for (int partIndex = 0; partIndex < partKeys.Count; partIndex++)
            {
                if (RendererNameMatchesBodyPart(rendererName, partNames[partIndex]))
                    rendererBuckets[partIndex].Add(renderer);
            }
        }

        for (int partIndex = 0; partIndex < partKeys.Count; partIndex++)
        {
            List<Renderer> renderers = rendererBuckets[partIndex];
            renderersByBodyPart[partKeys[partIndex]] = renderers.Count > 0
                ? renderers.ToArray()
                : EmptyRendererCache;
        }
    }

    private static bool RendererNameMatchesBodyPart(string rendererName, string partName)
    {
        return System.StringComparer.OrdinalIgnoreCase.Equals(rendererName, partName) ||
               rendererName.IndexOf(partName, System.StringComparison.Ordinal) >= 0;
    }

    private Renderer[] GetCachedBodyPartRenderers(BodyPart part)
    {
        EnsureBodyPartRendererCache();

        if (renderersByBodyPart.TryGetValue(part, out Renderer[] renderers))
            return renderers;

        return EmptyRendererCache;
    }

    /// <summary>
    /// Executes the auto configure skill targeting workflow.
    /// </summary>
    /// <param name="skill">The skill.</param>
    protected void AutoConfigureSkillTargeting(Skill skill)
    {
        skill.SetEmitter(this);

        switch (skill.targeting)
        {
            case SkillTargeting.AUTO:
                skill.AddReceiver(this);
                break;
            case SkillTargeting.ALL_ALLIES:
                Fighter[] allies = this.combatManager.GetAllyTeam();
                foreach (var receiver in allies)
                {
                    skill.AddReceiver(receiver);
                }
                break;
            case SkillTargeting.ALL_OPPONENTS:
                {
                    Fighter[] enemies = this.combatManager.GetOpposingTeam();

                    foreach (var receiver in enemies)
                    {
                        skill.AddReceiver(receiver);
                    }
                    break;
                }

            case SkillTargeting.SINGLE_ALLY:
            case SkillTargeting.SINGLE_OPPONENT:
                throw new System.InvalidOperationException("Unimplemented! This skill needs manual targeting.");
        }
    }

    /// <summary>
    /// Gets the skill targets.
    /// </summary>
    /// <param name="skill">The skill.</param>
    /// <returns>The resulting collection.</returns>
    protected Fighter[] GetSkillTargets(Skill skill)
    {
        switch (skill.targeting)
        {
            case SkillTargeting.AUTO:
            case SkillTargeting.ALL_ALLIES:
            case SkillTargeting.ALL_OPPONENTS:
            {
                Fighter[] enemies = this.combatManager.GetOpposingTeam();
                foreach (var receiver in enemies)
                {
                    skill.AddReceiver(receiver); 
                }
                break;
            }
            case SkillTargeting.SINGLE_ALLY:
                return this.combatManager.GetAllyTeam();
            case SkillTargeting.SINGLE_OPPONENT:
                return this.combatManager.GetOpposingTeam();
        }

        // Esto no deberia ejecutarse nunca pero hay que ponerlo para hacer al compilador feliz.
        throw new System.InvalidOperationException("Fighter::GetSkillTargets. Unreachable!");
    }

    /// <summary>
    /// Executes the die workflow.
    /// </summary>
    protected void Die()
    {
        if (team == Team.ENEMIES && global::ProgressionStats.StatsManager.Instance != null)
        {
            global::ProgressionStats.StatsManager.Instance.RegisterEnemyDefeat(idName);
        }

        this.statusPanel.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// Executes the modify health workflow.
    /// </summary>
    /// <param name="amount">The amount.</param>
    public void ModifyHealth(float amount)
    {
        ModifyHealth(amount, null, null, BodyPart.None);
    }

    public void ModifyHealth(float amount, Fighter attacker, Skill sourceSkill, BodyPart bodyPart = BodyPart.None)
    {
        DamageResult result = DamageResult.FromLegacyAmount(attacker, this, sourceSkill, bodyPart, amount);
        ApplyHealthModification(amount, attacker, sourceSkill, bodyPart, result);
    }

    public void ModifyHealth(DamageResult result)
    {
        result.receiver = this;

        if (result.isMiss)
        {
            PlayMissAnimation();
            float current = this.stats != null ? this.stats.health : 0f;
            OnDamageResolved?.Invoke(result.WithApplication(0f, current, current, false, false));
            return;
        }

        ApplyHealthModification(result.finalAmount, result.attacker, result.sourceSkill, result.targetPart, result);
    }

    private void ApplyHealthModification(
        float amount,
        Fighter attacker,
        Skill sourceSkill,
        BodyPart bodyPart,
        DamageResult damageResult)
    {
        float previousHealth = this.stats.health;

        this.stats.health = Mathf.Clamp(this.stats.health + amount, 0f, this.stats.maxHealth);
        this.stats.health = Mathf.Round(this.stats.health);
        float modifiedAmount = this.stats.health - previousHealth;

  
        if (healthModificationDelegate != null)
        {
            healthModificationDelegate(modifiedAmount);
        }

        this.statusPanel.SetHealth(this.stats.health, this.stats.maxHealth);
        
        if (amount == 0f)
        {
            PlayMissAnimation();
        }
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="0f">The 0f.</param>
        /// <returns>The resulting value.</returns>
        else if (amount > 0f)
        {
            if (animator != null)
                animator.Play("Heal");
        }
        else
        {
            if (animator != null)
                animator.Play("Damages");

            if (bodyPart != BodyPart.None && global::ProgressionStats.StatsManager.Instance != null)
            {
                global::ProgressionStats.StatsManager.Instance.RegisterBodyPartAttack(bodyPart);
            }
            
            if (this.GetComponent<PlayerFighter>() != null) 
            {
                if (CameraManager.Instance != null)
                    CameraManager.Instance.TriggerDamageGlitch();
            }
        }

        if (this.isAlive == false)
        {
            if (AudioManager.Instance != null && audioSource != null)
            {
                AudioManager.Instance.PlaySFX(audioSource.clip, audioSource.volume, false);
            }
            else if (audioSource != null)
            {
                audioSource.Play();
            }
            if (animator != null)
                animator.Play("Death");
            Invoke("Die", 2f);
        }

        DamageResult appliedResult = damageResult.WithApplication(
            modifiedAmount,
            previousHealth,
            this.stats.health,
            false,
            false);
        OnDamageResolved?.Invoke(appliedResult);

        if (modifiedAmount < 0f)
        {
            OnDamageReceived?.Invoke(new DamageReceivedEventData(
                attacker,
                this,
                sourceSkill,
                bodyPart,
                amount,
                modifiedAmount,
                previousHealth,
                this.stats.health,
                false,
                false,
                appliedResult));
        }
    }

    /// <summary>
    /// Executes the modify body part health workflow.
    /// </summary>
    /// <param name="part">The part.</param>
    /// <param name="amount">The amount.</param>
    public void ModifyBodyPartHealth(BodyPart part, float amount)
    {
        ModifyBodyPartHealth(part, amount, null, null);
    }

    public void ModifyBodyPartHealth(BodyPart part, float amount, Fighter attacker, Skill sourceSkill)
    {
        DamageResult result = DamageResult.FromLegacyAmount(attacker, this, sourceSkill, part, amount);
        ApplyBodyPartHealthModification(part, amount, attacker, sourceSkill, result);
    }

    public void ModifyBodyPartHealth(BodyPart part, DamageResult result, Fighter attacker, Skill sourceSkill)
    {
        result.attacker = attacker;
        result.receiver = this;
        result.sourceSkill = sourceSkill;
        result.targetPart = part;

        BodyPartData target = bodyParts != null ? bodyParts.Find(p => p.part == part) : null;
        if (result.isMiss)
        {
            PlayMissAnimation();
            float current = target != null ? target.currentHealth : 0f;
            OnDamageResolved?.Invoke(result.WithApplication(0f, current, current, true, false));
            return;
        }

        ApplyBodyPartHealthModification(part, result.finalAmount, attacker, sourceSkill, result);
    }

    private void ApplyBodyPartHealthModification(
        BodyPart part,
        float amount,
        Fighter attacker,
        Skill sourceSkill,
        DamageResult damageResult)
    {
        BodyPartData target = bodyParts.Find(p => p.part == part);
        if (target == null) return;

        if (damageResult.hasStatusChange)
            target.currentStatus = damageResult.resultingStatus;
        
        
        if (amount < 0 && !target.IsDestroyed)
        {
            StartCoroutine(DamageGlowEffect(part, 1.2f));
        }


        float prev = target.currentHealth;
        
        // Si la parte está destruida pero tiene prótesis activa, el daño va a la prótesis
        if (target.IsDestroyed && target.HasActiveProsthetic)
        {
            DamageProsthetic(part, Mathf.Abs(amount));
            // Disparar el evento OnDamageResolved para que DamageFeedbackListener muestre el número
            OnDamageResolved?.Invoke(damageResult.WithApplication(amount, target.prostheticCurrentHealth + Mathf.Abs(amount), target.prostheticCurrentHealth, true, target.prostheticCurrentHealth <= 0f));
            return;
        }

        target.currentHealth = Mathf.Clamp(target.currentHealth + amount, 0, target.GetMaxHealth(this));
        float modifiedAmount = target.currentHealth - prev;
        bool destroyedBodyPart = prev > 0 && target.IsDestroyed;

        Debug.Log($"{part} recibiÃ³ {amount}. Salud actual: {target.currentHealth} / {target.GetMaxHealth(this)}");

        PlayDamageAnimation(part);
        
        if (this.GetComponent<PlayerFighter>() != null) 
        {
            if (CameraManager.Instance != null)
                CameraManager.Instance.TriggerDamageGlitch();
        }
        if (destroyedBodyPart)
        {
            OnBodyPartDestroyed(target);
        }

        if (target.currentHealth == 0)
        {
            Vector3 textPos = transform.position + Vector3.up * 3f;
            FloatingTextManager.Instance.ShowText($"{part} destroyed!", textPos, Color.magenta);
        }

        if (modifiedAmount < 0f)
        {
            DamageResult appliedResult = damageResult.WithApplication(
                modifiedAmount,
                prev,
                target.currentHealth,
                true,
                destroyedBodyPart);

            OnDamageResolved?.Invoke(appliedResult);

            OnDamageReceived?.Invoke(new DamageReceivedEventData(
                attacker,
                this,
                sourceSkill,
                part,
                amount,
                modifiedAmount,
                prev,
                target.currentHealth,
                true,
                destroyedBodyPart,
                appliedResult));
        }
        else
        {
            OnDamageResolved?.Invoke(damageResult.WithApplication(
                modifiedAmount,
                prev,
                target.currentHealth,
                true,
                destroyedBodyPart));
        }
    }
    
    /// <summary>
    /// Gets the current stats.
    /// </summary>
    /// <returns>The resulting value.</returns>
    public virtual Stats GetCurrentStats()
    {
        Stats modedStats = this.stats.Clone();

        foreach (var mod in this.statusMods)
        {
            modedStats = mod.Apply(modedStats);
        }

        return modedStats;
    }


    /// <summary>
    /// Gets the current status condition.
    /// </summary>
    /// <returns>The resulting value.</returns>
    public StatusCondition GetCurrentStatusCondition()
    {
        if (this.statusCondition != null && this.statusCondition.hasExpired)
        {
            Destroy(this.statusCondition.gameObject);
            this.statusCondition = null;
        }

        return this.statusCondition;
    }

    /// <summary>
    /// Adds the body part status condition.
    /// </summary>
    /// <param name="condition">The condition.</param>
    public void AddBodyPartStatusCondition(BodyPartStatusCondition condition)
    {
        if (condition == null)
            return;

        if (this.bodyPartStatusConditions == null)
            this.bodyPartStatusConditions = new List<BodyPartStatusCondition>();

        this.bodyPartStatusConditions.Add(condition);
    }

    /// <summary>
    /// Gets the current body part status condition.
    /// </summary>
    /// <param name="conditionType">The condition type.</param>
    /// <param name="part">The part.</param>
    /// <returns>The resulting value.</returns>
    public BodyPartStatusCondition GetCurrentBodyPartStatusCondition(System.Type conditionType, BodyPart part)
    {
        foreach (BodyPartStatusCondition condition in this.GetCurrentBodyPartStatusConditions())
        {
            if (condition != null && condition.Matches(conditionType, part))
                return condition;
        }

        return null;
    }

    /// <summary>
    /// Gets the current body part status conditions.
    /// </summary>
    /// <returns>The resulting collection.</returns>
    public List<BodyPartStatusCondition> GetCurrentBodyPartStatusConditions()
    {
        if (this.bodyPartStatusConditions == null)
            this.bodyPartStatusConditions = new List<BodyPartStatusCondition>();

        for (int i = this.bodyPartStatusConditions.Count - 1; i >= 0; i--)
        {
            BodyPartStatusCondition condition = this.bodyPartStatusConditions[i];
            bool expired = condition == null || condition.hasExpired;

            if (!expired)
            {
                BodyPartData partData = this.GetBodyPart(condition.TargetPart);
                expired = partData == null || partData.IsDestroyed;
            }

            if (expired)
            {
                if (condition != null)
                    Destroy(condition.gameObject);

                this.bodyPartStatusConditions.RemoveAt(i);
            }
        }

        return this.bodyPartStatusConditions;
    }

    /// <summary>
    /// Sets the mod stats.
    /// </summary>
    /// <param name="stats">The stats.</param>
    public void SetModStats(Stats stats)
    {
        modedStats = stats;
    }

    /// <summary>
    /// Executes the on body part destroyed workflow.
    /// </summary>
    /// <param name="part">The part.</param>
    private void OnBodyPartDestroyed(BodyPartData part)
    {
        // 1. Notificar evento (siempre)
        OnBodyPartDestroyedEvent?.Invoke(part.part);

        // 2. Verificar prótesis PRIMERO — antes del VFX y antes de ocultar la malla
        var playerFighter = GetComponent<PlayerFighter>();
        InventoryNew.ProstheticData activeProsthetic = null;
        if (playerFighter?.equipmentHandler != null)
        {
            InventoryNew.EquipmentSlot slot = BodyPartToEquipmentSlot(part.part);
            activeProsthetic = playerFighter.equipmentHandler.GetEquippedItem(slot) as InventoryNew.ProstheticData;
        }

        if (activeProsthetic != null)
        {
            // La prótesis absorbe el golpe: inicializar HP, mostrar mesh, NO disparar VFX de destrucción
            part.prostheticCurrentHealth = activeProsthetic.prostheticMaxHealth;
            HideProstheticMesh(part.part);
            ShowProstheticMesh(part.part);
            playerFighter?.SaveBodyPartState(part.part);
            return;
        }

        // 3. Sin prótesis: flujo normal — ahora sí instanciar VFX
        if (partDestroyedVFX != null)
        {
            Transform spawnLocation = GetHitPoint(part.part);
            Instantiate(partDestroyedVFX, spawnLocation.position, spawnLocation.rotation, spawnLocation);
        }

        HidePartMesh(part.part);

        if (playerFighter != null)
            playerFighter.SaveBodyPartState(part.part);

        CameraManager.Instance.TriggerHitStop(1);
        CameraManager.Instance.TriggerShake(0.5f);
        CameraManager.Instance.TriggerDamageGlitch();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.armorBreakSound, 1f);

        foreach (StatusMod penalty in part.destructionPenalties)
        {
            if (penalty != null)
            {
                this.statusMods.Add(penalty);
                Debug.Log($"{idName} sufrió una penalización de {penalty.amount} en {penalty.type} por perder {part.part}");
            }
        }

        switch (part.part)
        {
            case BodyPart.Head:
            case BodyPart.Torso:
                Debug.Log($"{part.part} destruido → muerte instantánea");
                ModifyHealth(-stats.health);
                break;
            case BodyPart.LeftLeg:
            case BodyPart.RightLeg:
                Debug.Log("Pierna destruida → el jugador no podrá correr");
                break;
        }
    }

    /// <summary>
    /// Gets the hit point.
    /// </summary>
    /// <param name="part">The part.</param>
    /// <returns>The resulting value.</returns>
    public Transform GetHitPoint(BodyPart part)
    {
        if (part == BodyPart.None)
            return DamagePivot;

        foreach (var bp in bodyParts)
        {
            if (bp.part == part && bp.hitPoint != null)
                return bp.hitPoint;
        }

        return DamagePivot;
    }

    private void PlayMissAnimation()
    {
        if (animator != null)
            animator.Play("Miss");
    }

    /// <summary>
    /// Executes the play damage animation workflow.
    /// </summary>
    /// <param name="part">The part.</param>
    protected void PlayDamageAnimation(BodyPart part)
    {
        if (animator == null) return;

        switch (part)
        {
            case BodyPart.Head:
                animator.Play("Head");
                break;

            case BodyPart.Torso:
                animator.Play("Damages");
                break;

            case BodyPart.LeftArm:
                animator.Play("Aleft");
                break;

            case BodyPart.RightArm:
                animator.Play("Arigth");
                break;

            case BodyPart.LeftLeg:
                animator.Play("Lleft");
                break;

            case BodyPart.RightLeg:
                animator.Play("Lrigth");
                break;

            default:
                animator.Play("Damages");
                break;
        }
    }
    
    /// <summary>
    /// Executes the sync body part visuals workflow.
    /// </summary>
    public void SyncBodyPartVisuals()
    {
        if (bodyParts == null) return;
        foreach (var partData in bodyParts)
        {
            if (partData.IsDestroyed)
            {
                if (partData.HasActiveProsthetic)
                    ShowProstheticMesh(partData.part);
                else
                {
                    HidePartMesh(partData.part);
                    HideProstheticMesh(partData.part); // ← asegurar que el mesh de prótesis no quede visible
                }
            }
            else
            {
                HideProstheticMesh(partData.part); // parte sana: nunca mostrar el mesh de prótesis
            }
        }
    }

    /// <summary>
    /// Hides the part mesh.
    /// </summary>
    /// <param name="part">The part.</param>
    protected void HidePartMesh(BodyPart part)
    {
        Renderer[] partRenderers = GetCachedBodyPartRenderers(part);

        for (int i = 0; i < partRenderers.Length; i++)
        {
            Renderer r = partRenderers[i];
            if (r == null) continue;

            // CAMBIO IMPORTANTE:
            // No desactives el objeto (r.gameObject.SetActive(false))
            // Solo desactiva el componente que lo dibuja.
            r.enabled = false;

            Debug.Log($"Malla de {part} ocultada (Renderer desactivado).");
        }
    }

    /// <summary>
    /// Activa los renderers de prótesis para la parte dada.
    /// Convención de nombres en el prefab: el GameObject de la malla de prótesis
    /// debe llamarse "Prosthetic_LeftLeg", "Prosthetic_RightArm", etc. (BodyPart.ToString())
    /// o tener el tag "ProstheticMesh" con un componente ProstheticMeshMarker que indique la part.
    /// </summary>
    protected void ShowProstheticMesh(BodyPart part)
    {
        // Ocultar malla orgánica primero
        HidePartMesh(part);

        // Intentar usar la referencia directa (asignada en Inspector)
        var partData = GetBodyPart(part);
        if (partData?.prostheticRenderer != null)
        {
            partData.prostheticRenderer.enabled = true;
            partData.prostheticRenderer.gameObject.SetActive(true);
            return;
        }

        // Fallback: búsqueda por convención de nombre (si no hay referencia asignada)
        string targetName = "Prosthetic_" + part.ToString();
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == targetName)
            {
                child.gameObject.SetActive(true);
                foreach (var r in child.GetComponentsInChildren<Renderer>(true))
                    r.enabled = true;
            }
        }
    }

    protected void HideProstheticMesh(BodyPart part)
    {
        // Intentar usar la referencia directa
        var partData = GetBodyPart(part);
        if (partData?.prostheticRenderer != null)
        {
            partData.prostheticRenderer.enabled = false;
            return;
        }

        // Fallback: búsqueda por nombre
        string targetName = "Prosthetic_" + part.ToString();
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == targetName)
                child.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Aplica daño a la prótesis de la parte indicada.
    /// <param name="rawDamage">Valor positivo. Se resta de prostheticCurrentHealth.</param>
    /// </summary>
    public void DamageProsthetic(BodyPart part, float rawDamage)
    {
        var partData = GetBodyPart(part);
        if (partData == null || !partData.HasActiveProsthetic) return;

        partData.prostheticCurrentHealth = Mathf.Max(0f, partData.prostheticCurrentHealth - rawDamage);

        if (partData.prostheticCurrentHealth <= 0f)
            OnProstheticDestroyed(partData);
    }

    private void OnProstheticDestroyed(BodyPartData part)
    {
        part.prostheticCurrentHealth = 0f;
        HideProstheticMesh(part.part);

        // Desmontar la prótesis del equipmentHandler sin devolver al inventario (se destruyó)
        var playerFighter = GetComponent<PlayerFighter>();
        if (playerFighter?.equipmentHandler != null)
        {
            InventoryNew.EquipmentSlot slot = BodyPartToEquipmentSlot(part.part);
            playerFighter.equipmentHandler.DestroyProsthetic(slot);
        }

        // Reaplicar penalizaciones originales de la parte
        foreach (StatusMod penalty in part.destructionPenalties)
        {
            if (penalty != null)
                this.statusMods.Add(penalty);
        }

        // Para piernas: reaplicar restricción de movilidad (brokenLegCount ya lo maneja automáticamente)
        // Para Head/Torso sin prótesis: muerte
        switch (part.part)
        {
            case BodyPart.Head:
            case BodyPart.Torso:
                ModifyHealth(-stats.health);
                break;
        }

        CameraManager.Instance?.TriggerShake(0.5f);
        CameraManager.Instance?.TriggerDamageGlitch();
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.armorBreakSound, 1f);
    }

    public abstract void InitTurn();
    
    /// <summary>
    /// Pulses an emission glow on the damaged body part's renderers instead of
    /// toggling their visibility. The glow ramps up quickly, then fades out over
    /// <paramref name="duration"/> seconds. Renderers are never disabled so the
    /// mesh stays visible throughout the hit reaction.
    /// Swaps every material on the damaged body part's renderers to
    /// <see cref="damageGlitchMaterial"/> for <paramref name="duration"/> seconds,
    /// then restores the originals. The swap is per-slot so multi-material
    /// renderers are handled correctly.
    /// </summary>
    private System.Collections.IEnumerator DamageGlowEffect(BodyPart part, float duration)
    {
        if (damageGlitchMaterial == null) yield break;

        Renderer[] partRenderers = GetCachedBodyPartRenderers(part);
        if (partRenderers.Length == 0) yield break;

        // Cache original material arrays and swap in the glitch material
        var originalMaterials = new Material[partRenderers.Length][];
        for (int i = 0; i < partRenderers.Length; i++)
        {
            Renderer partRenderer = partRenderers[i];
            if (partRenderer == null) continue;

            originalMaterials[i] = partRenderer.sharedMaterials;
            Material[] glitchSlots = GetDamageGlitchSlots(partRenderer, originalMaterials[i].Length);

            partRenderer.sharedMaterials = glitchSlots;
        }

        // Hold for duration (or until the part is destroyed)
        BodyPartData damagedPart = GetBodyPart(part);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (damagedPart == null || damagedPart.IsDestroyed) break;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restore original materials
        for (int i = 0; i < partRenderers.Length; i++)
        {
            if (partRenderers[i] != null && originalMaterials[i] != null)
                partRenderers[i].sharedMaterials = originalMaterials[i];
        }
    }

    private Material[] GetDamageGlitchSlots(Renderer renderer, int slotCount)
    {
        if (damageGlitchSlotsByRenderer.TryGetValue(renderer, out Material[] glitchSlots) &&
            glitchSlots != null &&
            glitchSlots.Length == slotCount &&
            (slotCount == 0 || glitchSlots[0] == damageGlitchMaterial))
        {
            return glitchSlots;
        }

        glitchSlots = new Material[slotCount];
        for (int i = 0; i < glitchSlots.Length; i++)
            glitchSlots[i] = damageGlitchMaterial;

        damageGlitchSlotsByRenderer[renderer] = glitchSlots;
        return glitchSlots;
    }
    
}
