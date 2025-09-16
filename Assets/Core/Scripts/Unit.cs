using MyUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Unit : Interactable
{
    public string unitName;
    [HideInInspector] public float health = 100;
    [HideInInspector] public float resource = 0;
    
    [Header("[Base Attributes]")]
    [SerializeField] protected float baseDamage = 10;
    [SerializeField] protected float baseAttacksPerSecond = 1.0f;
    [SerializeField] protected float baseMaxHealth = 100;
    [SerializeField] protected float baseMaxResource = 100;
    [SerializeField] protected float baseMovementSpeed = 3.0f;
    [SerializeField] protected float baseArmor = 100;
    [SerializeField] protected float baseCriticalStrikeChance = 0.2f;
    [SerializeField] protected float baseCriticalStrikeDamage = 2.0f;
    [SerializeField] protected float baseHealthRegen = 0.0f;
    [SerializeField] protected float baseResourceRegen = 0.0f;
    public float baseAttackRange = 2.0f;
    
    
    // Controls all of the unit stats. Use this to request the current value of a stat
    // or apply a modifier.
    public Stats stats = new Stats();

    [Header("[Castable Abilities]")]
    public List<Ability> abilities = new List<Ability>();
    public Dictionary<Ability, float> abilitiesLastCastAt = new Dictionary<Ability, float>();
    [HideInInspector] public Ability lastAbilityCast;
    private Dictionary<Ability, StatModifier> abilityDamageModifiers = new Dictionary<Ability, StatModifier>();
    private Dictionary<Ability, StatModifier> abilityCooldownModifiers = new Dictionary<Ability, StatModifier>();
    private Dictionary<Ability, StatModifier> abilityCostModifiers = new Dictionary<Ability, StatModifier>();

    [Header("[Unit Visuals]")]
    public Transform handPoint;
    public GameFeedback onDeathFeedback;
    public GameFeedback onHitFeedback;

    // Cached Values for Targeting, Attacking and Casting
    public Unit target;
    [NonSerialized] public Vector3 targetPosition;
    [NonSerialized] public Ability abilityBeingCast;
    [NonSerialized] public float finishCastAt;
    [NonSerialized] public float canMoveAt;
    [NonSerialized] public float canCastAt;
    [NonSerialized] public float stunnedUntil;
    protected Vector3 attackDirection;

    // Cache references to important components for easy access later.
    protected NavMeshAgent agentNavigation;
    protected Animator animator;
    protected bool unitIsActive = false;
    protected bool hasAnimations = true;

    // Constants to prevent magic numbers in the code. Makes it easier to edit later.
    [HideInInspector] public bool isDead;
    protected const float MOVEMENT_DELAY_AFTER_ATTACKING = 0.0f;
    protected const float UNIT_TURNING_SPEED = 100.0f;
    protected const float TIME_BEFORE_CORPSE_DESTROYED = 5.0f;
    protected const float UNIT_DEACTIVATION_DISTANCE = 20;
    protected const string DEATH_TRIGGER = "Die";
    public static string[] animations = new string[] { 
        "None", "One Hand Slash", "One Hand Stab", "Two Hand Slash", "Cheer",
        "Shout", "Pickup", "Magic Channel", "Magic Area Attack", "Punch",
        "Bow Shoot", "Jump", "Magic Front Attack", "Roll", "Custom1", "Custom2",
        "Custom3", "Custom4", "Custom5"
    };

    public enum Faction { Player, Enemy, Other };
    [SerializeField] private Faction faction = Faction.Enemy;

    // Variables controlling how a unit is 'spun'.
    private float spinUntil, spinAngle, spinSpeed;

    /// <summary>
    /// Perform all initial setup.
    /// </summary>
    protected virtual void Start()
    {
        CacheComponents();
        SetupStats();
        SetupAbilities();
        SetupAnimations();
    }

    public virtual bool IsStunned ()
    {
        return (Time.time < stunnedUntil);
    }

    public virtual void Stun (float duration, Unit stunningUnit, IVisualCodeHandler stunningSource)
    {
        if (isDead) return;
        stunnedUntil = Time.time + duration;
        animator.CrossFadeInFixedTime("Stunned", 0.1f);
        GameManager.events.OnUnitStunned.Invoke(new GameEvents.OnUnitStunnedInfo(
                this,
                stunningUnit,
                stunningSource,
                duration
            ));
    }

    /// <summary>
    /// Apply the damage formula to this unit.
    /// </summary>
    protected virtual float ApplyDamageFormula(float amount, bool isCritical,
        Unit damagingUnit, IVisualCodeHandler damageSource)
    {
        // Apply damage modifiers (e.g. a -50% damage taken buff).
        amount *= GetBaseDamageTakenModifier();

        // Armor currently doesn't do anything? Add logic here.

        // Return the modified amount.
        return amount;
    }

    /// <summary>
    /// Perform all frame-by-frame unit updates.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        if (GameManager.player.isDead) return;
        if (!(unitIsActive = IsUnitActive())) return;
        ApplyStatBuffs();
        UpdateAnimations();
        ManageAbilityCasting();
        FaceTowardsAttackTarget();
        ApplyHealthRegeneration(Time.deltaTime);
        ApplyResourceRegeneration(Time.deltaTime);
    }

    /// <summary>
    /// Perform all initial stat setup for the unit.
    /// </summary>
    protected virtual void SetupStats()
    {
        stats.TrackStat(Stat.Damage, "Damage", baseDamage);
        stats.TrackStat(Stat.MaxHealth, "Max Health", baseMaxHealth);
        stats.TrackStat(Stat.MaxResource, "Max Resource", baseMaxResource);
        stats.TrackStat(Stat.MovementSpeed, "Movement Speed", 1);
        stats.TrackStat(Stat.Armor, "Armor", baseArmor);
        stats.TrackStat(Stat.AttacksPerSecond, "Attacks Per Second", baseAttacksPerSecond);
        stats.TrackStat(Stat.ResourceCostReduction, "Resource Cost Reduction", 0);
        stats.TrackStat(Stat.CooldownReduction, "Cooldown Reduction", 1);
        stats.TrackStat(Stat.CriticalStrikeChance, "Critical Strike Chance", baseCriticalStrikeChance);
        stats.TrackStat(Stat.CriticalStrikeDamage, "Critical Strike Damage", baseCriticalStrikeDamage);
        stats.TrackStat(Stat.ResourceGeneration, "Resource Generation", 1);
        stats.TrackStat(Stat.DamageTaken, "Damage Taken Modifier", 1);
        health = stats.GetValue(Stat.MaxHealth);
    }

    /// <summary>
    /// Cache components for easier access later.
    /// </summary>
    private void CacheComponents()
    {
        agentNavigation = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// Performs all animation setup. Not all units will use the default animations,
    /// and if they don't, we don't want to try and play those animations (e.g. not
    /// all units may be able to walk).
    /// </summary>
    private void SetupAnimations()
    {
        hasAnimations = false;
        if (animator == null)
        {
            hasAnimations = false;
        }
        else
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
                if (param.name == "Speed") hasAnimations = true;
        }
    }

    /// <summary>
    /// Returns true if the unit can move, otherwise false.
    /// </summary>
    public virtual bool CanMove ()
    {
        return (Time.time >= canMoveAt);
    }

    /// <summary>
    /// Returns true if the unit currently has a cast in progress, otherwise false.
    /// </summary>
    public bool CastInProgress()
    {
        return (abilityBeingCast != null);
    }

    /// <summary>
    /// Set the target of this unit to the specified unit.
    /// </summary>
    protected void SetTarget (Unit unit)
    {
        target = unit;
    }

    /// <summary>
    /// Returns true if the unit is currently moving, otherwise false.
    /// </summary>
    public bool IsMoving ()
    {
        return (agentNavigation.velocity.magnitude > 0);
    }

    /// <summary>
    /// Setup the abilities for this unit. This creates a unique copy so that
    /// it the unique instance on this unit can be modified without modifying the
    /// base copy.
    /// </summary>
    protected virtual void SetupAbilities ()
    {
        for (int i = 0; i < abilities.Count; i++)
        {
            abilities[i] = abilities[i].ShallowCopy();
            abilities[i].SetOwner(this);
            GameManager.logicEngine.AddEngine(abilities[i].engine);
        }
    }

    /// <summary>
    /// Stop the unit from moving.
    /// </summary>
    public void StopMoving ()
    {
        agentNavigation.SetDestination(transform.position);
    }

    /// <summary>
    /// Get the "Cast Point" of this unit.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCastPoint ()
    {
        if (handPoint != null)
            return handPoint.transform.position; 
        else
            return transform.position + new Vector3(0, 1, 0);
    }

    /// <summary>
    /// Play a specific animation on the unit.
    /// </summary>
    public void PlayAnimation (string animation, float totalAnimationTime = 1.0f)
    {
        if (!hasAnimations) return;
        totalAnimationTime = Mathf.Clamp(totalAnimationTime, 0.1f, 5.0f);

        if (animation != animations[0])
        {
            float speedModifier = Mathf.Max(1.0f, 1f / totalAnimationTime);
            animator.SetFloat("AbilityCastSpeed", speedModifier);
            animator.CrossFadeInFixedTime(animation, 0.3f / speedModifier);
        }
    }

    /// <summary>
    /// Smoothly face towards the attack target if the unit is attacking.
    /// </summary>
    private void FaceTowardsAttackTarget ()
    {
        // Smoothly face towards the last attack direction if stationary.
        if (Time.time < canMoveAt && attackDirection != Vector3.zero)
        {
            Quaternion look = Quaternion.LookRotation(attackDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation,
                look, Time.deltaTime * UNIT_TURNING_SPEED);
        }
    }

    /// <summary>
    /// Apply any per-frame unit changes needed due to unit buffs.
    /// </summary>
    private void ApplyStatBuffs ()
    {
        if (IsStunned())
        {
            agentNavigation.speed = 0;
        } 
        else
        {
            agentNavigation.speed = baseMovementSpeed * stats.GetValue(Stat.MovementSpeed);
        }    
    }

    /// <summary>
    /// Apply health regeneration to the unit for the specified time period.
    /// </summary>
    private void ApplyHealthRegeneration(float duration)
    {
        if (baseHealthRegen != 0) AddHealth(baseHealthRegen * duration);
    }

    /// <summary>
    /// Apply resource regeneration to the unit for the specified time period.
    /// </summary>
    private void ApplyResourceRegeneration(float duration)
    {
        if (baseResourceRegen != 0) AddResource(baseResourceRegen * duration);
    }

    /// <summary>
    /// Returns true if the unit is currently active, otherwise false.
    /// Units deactivate when they are far away from the player, to prevent
    /// unnecessary game logic from being run.
    /// </summary>
    private bool IsUnitActive ()
    {
        return (Vector3.Distance(GameManager.player.transform.position, 
            transform.position) < UNIT_DEACTIVATION_DISTANCE);
    }

    /// <summary>
    /// Do all required animation updates.
    /// </summary>
    private void UpdateAnimations ()
    {
        if (!hasAnimations) return;
        animator.SetFloat("Speed", agentNavigation.velocity.magnitude / baseMovementSpeed);
        animator.SetBool("Stunned", IsStunned());
    }

    /// <summary>
    /// Manage the casting of abilities for this unit.
    /// </summary>
    private void ManageAbilityCasting()
    {
        if (IsStunned())
        {
            if (IsCasting()) abilityBeingCast.CancelCast(this);
            return;
        }
        if (IsCasting() && Time.time > finishCastAt)
            abilityBeingCast.FinishCast(this, target, targetPosition);
        else if (!IsCasting())
            TryToCastAbilities();
    }

    /// <summary>
    /// Start a unit spinning.
    /// </summary>
    /// <param name="spinSpeed">The rotation speed per second (in degrees).</param>
    /// <param name="duration">The total time to spin for.</param>
    public void StartSpin(float spinSpeed, float duration)
    {
        this.spinUntil = Time.time + duration;
        this.spinSpeed = spinSpeed;
        this.spinAngle = animator.transform.rotation.eulerAngles.y;
    }

    /// <summary>
    /// Handle spinning in late update so that all other position and rotation
    /// changes have already been handled.
    /// </summary>
    private void LateUpdate()
    {
        Spin();
    }

    /// <summary>
    /// Handle all of the spinning logic for the unit.
    /// </summary>
    private void Spin ()
    {
        if (Time.time < spinUntil)
        {
            spinAngle += spinSpeed * Time.deltaTime;
            animator.transform.rotation = Quaternion.Euler(0, spinAngle, 0);
        }
        else
        {
            animator.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    /// <summary>
    /// This function is called every frame and should include logic for casting abilities.
    /// </summary>
    protected virtual void TryToCastAbilities ()
    {

    }
    
    /// <summary>
    /// Return the attack point of the unit. The attack point is exactly half way
    /// between the unit and the maximum range of the unit. This allows you to do an effect
    /// directly circling the enemies within a set distance of the main attack point.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetAttackPoint ()
    {
        return transform.position + transform.forward * baseAttackRange / 2.0f;
    }

    /// <summary>
    /// Returns true if the unit can cast the given ability, otherwise false.
    /// </summary>
    private bool CanCastAbility (Ability ability, Unit target, Vector3 targetPosition)
    {
        if (IsCasting()) // Unit cannot already be casting.
            return false;
        if (Time.time < canCastAt) // Unit cannot be prevented from casting.
            return false;
        if (!ability.IsValidToCast(this, target, targetPosition)) // Ability requirements must be met.
            return false;
        return true;
    }

    public void ReduceAbilityCooldown (Ability ability, float amount)
    {
        foreach (var a in abilitiesLastCastAt.Keys)
        {
            if (ability.abilityName == a.abilityName)
            {
                ability = a;
                break;
            }
        }

        if (abilitiesLastCastAt.ContainsKey(ability))
        {
            abilitiesLastCastAt[ability] -= amount;
        } 
        else
        {
            abilitiesLastCastAt.Add(ability, -999);
        }
    }

    public virtual void CastAbility (Ability ability, Unit target, Vector3 targetPosition)
    {
        if (!CanCastAbility(ability, target, targetPosition)) return;

        abilitiesLastCastAt[ability] = Time.time;
        if (!ability.canMoveWhileCasting) StopMoving();

        if (ability.requiresLineOfSight)
            targetPosition = Utilities.GetClosestPointInLOS(transform.position, targetPosition);

        ability.StartCast(this, target, targetPosition);

        // Calculate the attack direction for this ability.
        attackDirection = (targetPosition - transform.position);
        attackDirection.y = 0;
        attackDirection.Normalize();
    }

    /// <summary>
    /// Cast the ability without checking any requirements. E.g. An Item may use this method
    /// to cast an ability automatically without the usual fanfare and checks.
    /// </summary>
    public virtual void CastAbilityWithoutCheckingRequirements (Ability ability, 
        Unit target, Vector3 targetPosition)
    {
        ability.StartCast(this, target, targetPosition);
    }

    

    /// <summary>
    /// Return the damage value, modified by unit stats (e.g. armor and DR).
    /// </summary>
    protected virtual float GetArmorDamageTakenModifier()
    {
        return 1.0f - (GameManager.armorValues.armorDamageReductionCurve.Evaluate(
            stats.GetValue(Stat.Armor) / GameManager.armorValues.maxArmor));
    }

    /// <summary>
    /// Apply the base damage taken modifier (e.g. an 'increase damage taken by 100%' debuff).
    /// </summary>
    protected virtual float GetBaseDamageTakenModifier ()
    {
        return stats.GetValue(Stat.DamageTaken);
    }

    /// <summary>
    /// Remove the specified amount of health from the unit, killing it if needed.
    /// </summary>
    public virtual void TakeDamage(float amount, bool isCritical, 
        Unit damagingUnit, IVisualCodeHandler damageSource)
    {
        if (isDead) return;

        amount = ApplyDamageFormula(amount, isCritical, damagingUnit, damageSource);
        if (amount < 0) return;

        // Remove the health.
        RemoveHealth(amount, damagingUnit, damageSource, isCritical);

        // Apply the "on hit" feedback for this unit.
        onHitFeedback?.ActivateFeedback(gameObject, null, transform.position);

        // Trigger OnUnitDamaged event.
        GameManager.events.OnUnitDamaged.Invoke(new GameEvents.OnUnitDamagedInfo(this, amount, damagingUnit, damageSource, isCritical));
    } 

    /// <summary>
    /// Kill the unit, destroying the unit logic but keeping the model 
    /// around to play the death animation.
    /// </summary>
    public virtual void Kill(Unit killingUnit, IVisualCodeHandler killingSource, bool isCritical = false)
    {
        // Do not kill the unit if it is already dead.
        if (isDead) return;

        // If unit isn't dead, perform required death actions.
        isDead = true;
        GameManager.events.OnUnitKilled.Invoke(new GameEvents.OnUnitKilledInfo(this, killingUnit, killingSource, isCritical));
        onDeathFeedback?.ActivateFeedback(gameObject, null, gameObject.transform.position);
        if (hasAnimations)
        {
            animator.SetTrigger(DEATH_TRIGGER);
        }
        StopMoving();
    }

    /// <summary>
    /// // Add the specified amount of health to the unit.
    /// </summary>
    public virtual void AddHealth(float amount, Unit healingUnit = null,
        IVisualCodeHandler healingSource = null)
    {
        health = Mathf.Min(health + amount, stats.GetValue(Stat.MaxHealth));
    }

    /// <summary>
    /// Remove the specified amount of health from the unit. 
    /// </summary>
    public virtual void RemoveHealth(float amount, Unit damagingUnit = null,
        IVisualCodeHandler damageSource = null, bool isCritical = false)
    {
        health -= amount;
        if (health <= 0)
        {
            health = 0;
            Kill(damagingUnit, damageSource, isCritical);
        }
    }

    /// <summary>
    /// Add the specified amount of resource to the unit.
    /// </summary>
    public virtual void AddResource(float amount)
    {
        amount *= stats.GetValue(Stat.ResourceGeneration);
        resource = Mathf.Min(resource + amount, stats.GetValue(Stat.MaxResource));
    }

    /// <summary>
    /// Remove the specified amount of resource from the unit.
    /// </summary>
    public virtual void RemoveResource(float amount)
    {
        resource = Mathf.Max(resource - amount, 0);
    }

    /// <summary>
    /// Filter the given unit list to only return allies of this unit.
    /// </summary>
    public virtual List<Unit> GetAllies (List<Unit> units)
    {
        List<Unit> allies = new List<Unit>();
        foreach (Unit unit in units)
        {
            if (GetFaction() == unit.GetFaction())
            {
                allies.Add(unit);
            }
        }
        return allies;
    }

    /// <summary>
    /// Filter the given unit list to only return enemies of this unit.
    /// </summary>
    public virtual List<Unit> GetEnemies(List<Unit> units)
    {
        List<Unit> enemies = new List<Unit>();
        foreach (Unit unit in units)
        {
            if (GetFaction() != unit.GetFaction())
            {
                enemies.Add(unit);
            }
        }
        return enemies;
    }

    /// <summary>
    /// Return true if the specified unit is an ally of this unit, otherwise return false.
    /// </summary>
    public bool IsAlly (Unit unit)
    {
        return (GetFaction() == unit.GetFaction());
    }

    /// <summary>
    /// Return true if the specified unit is an enemy of this unit, otherwise return false.
    /// </summary>
    public bool IsEnemy (Unit unit)
    {
        return (GetFaction() != unit.GetFaction());
    }

    /// <summary>
    /// Return true if the attack should be a critical, otherwise retuern false.
    /// </summary>
    private bool CheckForCritical ()
    {
        return (Random.value < stats.GetValue(Stat.CriticalStrikeChance));
    }

    /// <summary>
    /// Have this unit damage another unit for the specified amount.
    /// This method will handle the possibility of attacker modifiers, critical
    /// strike chance etc.
    /// </summary>
    public void DamageOtherUnit (Unit unit, float weaponDamagePercent, IVisualCodeHandler source)
    {
        float amount = Mathf.Round(stats.GetValue(Stat.Damage) * weaponDamagePercent);
        if (source != null && source.GetData() is Ability ability)
            amount *= GetAbilityDamageModifier(ability);

        if (CheckForCritical())
        {
            amount *= stats.GetValue(Stat.CriticalStrikeDamage);
            unit.TakeDamage(amount, true, this, source);
        }
        else
        {
            unit.TakeDamage(amount, false, this, source);
        }
    }

    /// <summary>
    /// Have this unit damage the given list of units for the specified amount.
    /// This method will handle the possibility of attacker modifiers, critical
    /// strike chance etc.
    /// </summary>
    public void DamageOtherUnits (List<Unit> units, float weaponDamagePercent, IVisualCodeHandler source)
    {
        float amount = Mathf.Round(stats.GetValue(Stat.Damage) * weaponDamagePercent);
        if (source != null && source.GetData() is Ability ability)
            amount *= GetAbilityDamageModifier(ability);

        bool isCrit = false;
        if (CheckForCritical())
        {
            amount *= stats.GetValue(Stat.CriticalStrikeDamage);
            isCrit = true;
        }
        foreach (Unit unit in units)
        {
            unit.TakeDamage(amount, isCrit, this, source);
        }
    }

    /// <summary>
    /// Teleport the player to the given location.
    /// </summary>
    public void Teleport (Vector3 newPosition)
    {
        agentNavigation.Warp(Utilities.GetValidNavMeshPosition(newPosition));
        StopMoving();
    }
    
    /// <summary> 
    /// Return the faction of the given unit.
    /// </summary>
    public virtual Faction GetFaction()
    {
        return faction;
    }

    public virtual void SetFaction (Faction faction)
    {
        this.faction = faction;
    }

    /// <summary>
    /// Returns true if the point is in range of the unit, otherwise false.
    /// </summary>
    public override bool InRange(Vector3 point)
    {
        return Vector3.Distance(transform.position, point) < baseAttackRange;
    }

    /// <summary>
    /// Move this unit towards the target location over time.
    /// </summary>
    public void MoveOverTime (Vector3 targetLocation, float duration)
    {
        StartCoroutine(MoveOverTimeCoroutine(targetLocation, duration));
    }

    /// <summary>
    /// Coroutine to hand moving the unit in a straight line over time.
    /// </summary>
    private IEnumerator MoveOverTimeCoroutine (Vector3 targetLocation, float duration)
    { 
        Vector3 startLocation = transform.position;
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            StopMoving();
            agentNavigation.speed = 0;
            transform.LookAt(targetPosition);
            float frac = (Time.time - startTime) / duration;
            transform.position = Vector3.Lerp(startLocation, targetLocation, frac);
            yield return null;
        }
        StopMoving();
        agentNavigation.SetDestination(transform.position + 
            (targetLocation - startLocation).normalized * 0.01f);
    }

    /// <summary>
    /// Return true if the unit is currently casting, otherwise false.
    /// </summary>
    /// <returns></returns>
    public bool IsCasting ()
    {
        return (abilityBeingCast != null);
    }

    // Should fix this up to reduce the code repetition.
    public void AddAbility(Ability ability)
    {
        Ability abilityToAdd = ability.ShallowCopy();
        abilityToAdd.SetOwner(this);
        GameManager.logicEngine.AddEngine(abilityToAdd.engine);
        abilities.Add(abilityToAdd);
        OnAbilitiesUpdated();
    }

    public void LockAbility(Ability ability)
    {
        // TODO: Add this logic.
    }

    public void UnlockAbility(Ability ability)
    {
        // TODO: Add this logic.
    }

    // Should fix this up to reduce the code repetition.
    public void RemoveAbility (Ability abilityTemplate)
    {
        foreach (var a in abilities)
        {
            if (abilityTemplate.abilityName == a.abilityName)
            {
                abilityTemplate = a;
                break;
            }
        }
        if (abilityTemplate != null)
        {
            GameManager.logicEngine.RemoveEngine(abilityTemplate.GetEngine());
            abilities.Remove(abilityTemplate);
            OnAbilitiesUpdated();
        }
    }

    // Should fix this up to reduce the code repetition.
    public void ReplaceAbility (Ability abilityToReplaceTemplate, Ability newAbility)
    {
        foreach (var a in abilities)
        {
            if (abilityToReplaceTemplate.abilityName == a.abilityName)
            {
                abilityToReplaceTemplate = a;
                break;
            }
        }
        if (abilityToReplaceTemplate != null)
        {
            int index = abilities.IndexOf(abilityToReplaceTemplate);
            if (index >= 0)
            {
                GameManager.logicEngine.RemoveEngine(abilityToReplaceTemplate.GetEngine());
                Ability abilityToAdd = newAbility.ShallowCopy();
                abilityToAdd.SetOwner(this);
                GameManager.logicEngine.AddEngine(abilityToAdd.engine);
                abilities[index] = abilityToAdd;
                OnAbilitiesUpdated();
            }
        }
    }

    /// <summary>
    /// Return the name of this unit.
    /// </summary>
    public override string ToString()
    {
        return name;
    }

    public virtual void OnAbilitiesUpdated ()
    {

    }

    public void AddTimedAbilityDamageModifier(Ability ability, float modifier, float duration, string buffName = "Buff", int maxStacks = 99)
    {
        bool isRefreshing = HasBuff(buffName);
        if (!abilityDamageModifiers.ContainsKey(ability.template))
        {
            abilityDamageModifiers[ability.template] = new StatModifier($"{ability.abilityName}_DamageMod", 1.0f);
        }
        abilityDamageModifiers[ability.template].AddTimedPercentageModifier(modifier, duration, buffName, maxStacks);
        GameManager.events.OnUnitGainsBuff.Invoke(new GameEvents.OnUnitGainsBuffInfo(this, ability.GetOwner(), buffName));
    }

    public void AddAbilityDamageModifier(Ability ability, float modifier, string buffName = "Buff", int maxStacks = 99)
    {
        if (!abilityDamageModifiers.ContainsKey(ability.template))
        {
            abilityDamageModifiers[ability.template] = new StatModifier($"{ability.abilityName}_DamageMod", 1.0f);
        }
        abilityDamageModifiers[ability.template].AddPercentageModifier(modifier, buffName, maxStacks);
        GameManager.events.OnUnitGainsBuff.Invoke(new GameEvents.OnUnitGainsBuffInfo(this, ability.GetOwner(), buffName));
    }

    public void AddTimedAbilityCooldownModifier(Ability ability, float modifier, float duration, string buffName = "Buff", int maxStacks = 99)
    {
        if (!abilityCooldownModifiers.ContainsKey(ability.template))
        {
            abilityCooldownModifiers[ability.template] = new StatModifier($"{ability.abilityName}_CooldownMod", 1.0f);
        }
        abilityCooldownModifiers[ability.template].AddTimedPercentageModifier(modifier, duration, buffName, maxStacks);
        GameManager.events.OnUnitGainsBuff.Invoke(new GameEvents.OnUnitGainsBuffInfo(this, ability.GetOwner(), buffName));
    }

    public void AddAbilityCooldownModifier(Ability ability, float modifier, string buffName = "Buff", int maxStacks = 99)
    {
        if (!abilityCooldownModifiers.ContainsKey(ability.template))
        {
            abilityCooldownModifiers[ability.template] = new StatModifier($"{ability.abilityName}_CooldownMod", 1.0f);
        }
        abilityCooldownModifiers[ability.template].AddPercentageModifier(modifier, buffName, maxStacks);
        GameManager.events.OnUnitGainsBuff.Invoke(new GameEvents.OnUnitGainsBuffInfo(this, ability.GetOwner(), buffName));
    }

    public void AddTimedAbilityCostModifier(Ability ability, float modifier, float duration, string buffName = "Buff", int maxStacks = 99)
    {
        if (!abilityCostModifiers.ContainsKey(ability.template))
        {
            abilityCostModifiers[ability.template] = new StatModifier($"{ability.abilityName}_CooldownMod", 1.0f);
        }
        abilityCostModifiers[ability.template].AddTimedPercentageModifier(modifier, duration, buffName, maxStacks);
        GameManager.events.OnUnitGainsBuff.Invoke(new GameEvents.OnUnitGainsBuffInfo(this, ability.GetOwner(), buffName));
    }

    public void AddAbilityCostModifier(Ability ability, float modifier, string buffName = "Buff", int maxStacks = 99)
    {
        if (!abilityCostModifiers.ContainsKey(ability.template))
        {
            abilityCostModifiers[ability.template] = new StatModifier($"{ability.abilityName}_CooldownMod", 1.0f);
        }
        abilityCostModifiers[ability.template].AddPercentageModifier(modifier, buffName, maxStacks);
        GameManager.events.OnUnitGainsBuff.Invoke(new GameEvents.OnUnitGainsBuffInfo(this, ability.GetOwner(), buffName));
    }

    public float GetAbilityDamageModifier(Ability ability)
    {
        if (abilityDamageModifiers.ContainsKey(ability.template))
        {
            return abilityDamageModifiers[ability.template].GetValue();
        }
        else
        {
            return 1.0f;
        }
    }

    public float GetAbilityCooldownModifier(Ability ability)
    {
        if (abilityCooldownModifiers.ContainsKey(ability.template))
        {
            return abilityCooldownModifiers[ability.template].GetValue();
        }
        else
        {
            return 1.0f;
        }
    }

    public float GetAbilityCostModifier(Ability ability)
    {
        if (abilityCostModifiers.ContainsKey(ability.template))
        {
            return abilityCostModifiers[ability.template].GetValue();
        }
        else
        {
            return 1.0f;
        }
    }

    public void RemoveAbilityBuffModifiers(Ability ability, string buffName)
    {
        if (abilityCooldownModifiers.ContainsKey(ability.template))
        {
            abilityCooldownModifiers[ability.template].RemoveModifiersWithLabel(buffName);
        }
        if (abilityCostModifiers.ContainsKey(ability.template))
        {
            abilityCostModifiers[ability.template].RemoveModifiersWithLabel(buffName);
        }
        if (abilityDamageModifiers.ContainsKey(ability.template))
        {
            abilityDamageModifiers[ability.template].RemoveModifiersWithLabel(buffName);
        }
    }

    public bool HasBuff (string buffName)
    {
        return stats.HasBuffWithLabel(buffName);
    }

    public float GetCurrentHealthPercent()
    {
        return health / stats.GetValue(Stat.MaxHealth);
    }

    public float GetCurrentResourcePercent()
    {
        return resource / stats.GetValue(Stat.MaxResource);
    }
}
