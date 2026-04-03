using System.Collections.Generic;

using UnityEngine;
//TP2 AUGUSTO NANINI/FACUNDO FERREIRO

public abstract class Fighter : MonoBehaviour
{
    [System.Serializable]
    public class BodyPartData
    {
        public BodyPart part;
        public Transform hitPoint;
        public float maxHealth = 100f;
        public float currentHealth;
      
        public PartStatus currentStatus = PartStatus.None;
        [Header("Penalizaciones al destruirse")]
        public List<StatusMod> destructionPenalties = new List<StatusMod>();

        public BodyPartData(BodyPart part, float health)
        {
            this.part = part;
            this.maxHealth = health;
            this.currentHealth = health;
            this.currentStatus = PartStatus.None;
        }

        public bool IsDestroyed => currentHealth <= 0;
    }
    public List<BodyPartData> bodyParts;

    [Header("Visual Effects")]
    [SerializeField] 
    private GameObject partDestroyedVFX;

    public event System.Action<BodyPart> OnBodyPartDestroyedEvent;

    public Team team;
    public string idName;
    public StatusPanel statusPanel;
    public Animator animator;
    public CombatManager combatManager;
    public AudioSource audioSource;
    public delegate void HealthModificationDelegate(float amount);
    public HealthModificationDelegate healthModificationDelegate;
    public List<StatusMod> statusMods;
    public bool legBroken => bodyParts != null && bodyParts.Exists(p =>
        (p.part == BodyPart.LeftLeg || p.part == BodyPart.RightLeg) && p.IsDestroyed);
    public Stats stats;
    public Stats modedStats;
    public Skill[] skills;
    public StatusCondition statusCondition;

    [SerializeField]
    public Transform CameraPivot;

    [SerializeField]
    public Transform DamagePivot;

    public BodyPartData GetBodyPart(BodyPart part)
    {
        return bodyParts.Find(p => p.part == part);
    }

    public bool isAlive
    {
        get => this.stats.health > 0;
    }

    protected virtual void Start()
    {
        if (this.statusPanel != null)
            this.statusPanel.SetStats(this.idName, this.stats);

        this.skills = this.GetComponentsInChildren<Skill>();
        this.modedStats = stats;
        this.statusMods = new List<StatusMod>();



    }

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

    protected void Die()
    {
        this.statusPanel.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void ModifyHealth(float amount)
    {
        float previousHealth = this.stats.health;

        this.stats.health = Mathf.Clamp(this.stats.health + amount, 0f, this.stats.maxHealth);
        this.stats.health = Mathf.Round(this.stats.health);

  
        if (healthModificationDelegate != null)
        {
            float modifiedAmount = this.stats.health - previousHealth;
            healthModificationDelegate(modifiedAmount);
        }

        this.statusPanel.SetHealth(this.stats.health, this.stats.maxHealth);
        
        if (amount == 0f)
        {
            animator.Play("Miss");
        }
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
            audioSource.Play();
            animator.Play("Death");
            Invoke("Die", 2f);
        }
    }

    public void ModifyBodyPartHealth(BodyPart part, float amount)
    {
        BodyPartData target = bodyParts.Find(p => p.part == part);
        if (target == null) return;
        
        
        if (amount < 0 && !target.IsDestroyed)
        {
            StartCoroutine(DamageFlickerEffect(part, 1.2f)); // 1.2s de parpadeo
        }


        float prev = target.currentHealth;
        target.currentHealth = Mathf.Clamp(target.currentHealth + amount, 0, target.maxHealth);

        Debug.Log($"{part} recibió {amount}. Salud actual: {target.currentHealth}");

        PlayDamageAnimation(part);
        
        if (this.GetComponent<PlayerFighter>() != null) 
        {
            if (CameraManager.Instance != null)
                CameraManager.Instance.TriggerDamageGlitch();
        }
        if (prev > 0 && target.IsDestroyed)
        {
            OnBodyPartDestroyed(target);
        }

        if (target.currentHealth == 0)
        {
            Vector3 textPos = transform.position + Vector3.up * 3f;
            FloatingTextManager.Instance.ShowText($"{part} destroyed!", textPos, Color.magenta);
        }
    }
    
    public Stats GetCurrentStats()
    {
        Stats modedStats = this.stats.Clone();

        foreach (var mod in this.statusMods)
        {
            modedStats = mod.Apply(modedStats);
        }

        return modedStats;
    }


    public StatusCondition GetCurrentStatusCondition()
    {
        if (this.statusCondition != null && this.statusCondition.hasExpired)
        {
            Destroy(this.statusCondition.gameObject);
            this.statusCondition = null;
        }

        return this.statusCondition;
    }

    public void SetModStats(Stats stats)
    {
        modedStats = stats;
    }

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
        
        CameraManager.Instance.TriggerHitStop(0.25f); // Hit stop 
        CameraManager.Instance.TriggerShake(1.4f);    // Camera Shake
        CameraManager.Instance.TriggerDamageGlitch(); // Aberración cromática
        
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

        // 4. Lógica de reglas "Duras" (Cosas que no son solo restas de stats)
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
    
    public void SyncBodyPartVisuals()
    {
        if (bodyParts == null) return;
        foreach (var partData in bodyParts)
        {
            if (partData.IsDestroyed)
                HidePartMesh(partData.part);
        }
    }

    protected void HidePartMesh(BodyPart part)
    {
        string partName = part.ToString();
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer r in allRenderers)
        {
            if (r.name.Equals(partName, System.StringComparison.OrdinalIgnoreCase) || r.name.Contains(partName))
            {
                // CAMBIO IMPORTANTE:
                // No desactives el objeto (r.gameObject.SetActive(false))
                // Solo desactiva el componente que lo dibuja.
                r.enabled = false; 
                
                Debug.Log($"Malla de {partName} ocultada (Renderer desactivado).");
            }
        }
    }
    public abstract void InitTurn();
    
    private System.Collections.IEnumerator DamageFlickerEffect(BodyPart part, float duration)
    {
        string partName = part.ToString();
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        List<Renderer> partRenderers = new List<Renderer>();

        // Filtrar renderers de la parte específica (mismo criterio que HidePartMesh)
        foreach (Renderer r in allRenderers)
        {
            if (r.name.Equals(partName, System.StringComparison.OrdinalIgnoreCase) || r.name.Contains(partName))
                partRenderers.Add(r);
        }

        float elapsed = 0;
        while (elapsed < duration)
        {
            // Si la parte se destruyó durante el efecto, detenemos el parpadeo
            if (GetBodyPart(part).IsDestroyed) break;

            foreach (var r in partRenderers) r.enabled = !r.enabled;
        
            float interval = 0.1f;
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        // Asegurar que queden encendidos si NO están destruidos
        if (!GetBodyPart(part).IsDestroyed)
        {
            foreach (var r in partRenderers) r.enabled = true;
        }
    }
    
}