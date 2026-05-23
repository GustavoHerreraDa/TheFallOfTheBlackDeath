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
        bool destroyedBodyPart)
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

        private InventoryNew.EquipmentSlot MapPartToSlot(BodyPart part)
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
                if ((part.part == BodyPart.LeftLeg || part.part == BodyPart.RightLeg) && part.IsDestroyed)
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
            animator.Play("Miss");
        }
        /// <summary>
        /// Executes the if workflow.
        /// </summary>
        /// <param name="0f">The 0f.</param>
        /// <returns>The resulting value.</returns>
        else if (amount > 0f)
        {
            animator.Play("Heal");
        }
        else
        {
            animator.Play("Damages");
            
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
            animator.Play("Death");
            Invoke("Die", 2f);
        }

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
                false));
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
        BodyPartData target = bodyParts.Find(p => p.part == part);
        if (target == null) return;
        
        
        if (amount < 0 && !target.IsDestroyed)
        {
            StartCoroutine(DamageGlowEffect(part, 1.2f));
        }


        float prev = target.currentHealth;
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
        //  Notificar al evento
        OnBodyPartDestroyedEvent?.Invoke(part.part);
        
        if (partDestroyedVFX != null)
        {
            Transform spawnLocation = GetHitPoint(part.part);
            GameObject vfx = Instantiate(partDestroyedVFX, spawnLocation.position, spawnLocation.rotation, spawnLocation);
        }
        
        //  Ocultar la malla
        HidePartMesh(part.part);

        
        var playerFighter = GetComponent<PlayerFighter>();
        if (playerFighter != null)
        {
            playerFighter.SaveBodyPartState(part.part);
        }
        
        CameraManager.Instance.TriggerHitStop(1); // Hit stop 
        CameraManager.Instance.TriggerShake(1.4f);    // Camera Shake
        CameraManager.Instance.TriggerDamageGlitch(); // AberraciÃ³n cromÃ¡tica
        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.armorBreakSound, 1f);
        
        
        foreach (StatusMod penalty in part.destructionPenalties)
        {
            if (penalty != null)
            {
                this.statusMods.Add(penalty);
                Debug.Log($"{idName} sufriÃ³ una penalizaciÃ³n de {penalty.amount} en {penalty.type} por perder {part.part}");
            }
        }

        // 4. la logica de reglas "Duras" (Cosas que no son solo restas de stats)
        switch (part.part)
        {
            case BodyPart.Head:
            case BodyPart.Torso:
                Debug.Log($"{part.part} destruido â†’ muerte instantÃ¡nea");
                ModifyHealth(-stats.health);
                break;
            case BodyPart.LeftLeg:
            case BodyPart.RightLeg:
                Debug.Log("Pierna destruida â†’ el jugador no podrÃ¡ correr");
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
                HidePartMesh(partData.part);
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
