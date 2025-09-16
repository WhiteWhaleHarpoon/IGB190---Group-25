using MyUtilities;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "Ability", menuName = "Data/Ability", order = 1)]
public class Ability : ScriptableObject, IVisualCodeHandler
{
    public enum TargetMode { None, UnitInMelee, UnitAtRanged, PointInMelee, PointAtRanged }

    [SerializeReference]
    public LogicEngine engine = new LogicEngine();

    // Ability descriptors.
    public string abilityName;
    public string abilityFlavourText;
    public Sprite abilityIcon;
    public TargetMode targetMode = TargetMode.PointAtRanged;
    [TextArea(2, 6)] public string abilityDescription;
    private Ability _template;
    public Ability template { 
    get
        {
            if (_template == null) 
                _template = this;
            return _template;
        }
    }

    // Ability values.
    public string abilityAnimation = "None";
    public float range = 8;
    public float castTime = 0;
    public float abilityCooldown = 1.0f;
    public float abilityCost = 0;
    public float movementLock = 1;
    public float animationActivationPosition = 0.5f;

    // Boolean ability properties.
    public bool canMoveWhileCasting = false;
    public bool hasSpecificCastTime = false;
    public bool cooldownIsAtackSpeed = false;
    public bool abilityGeneratesResource = false;
    public bool requiresLineOfSight = true;
    public bool canUpdateTargetWhileCasting = true;
    public bool castAtClosestPointInRange;
    private Unit owner;

    // VFX.
    public AudioClip castCompleteSound = null;
    public float castCompleteSoundVolume = 1.0f;
    [DoNotSerialize] public bool needsReminderFlash = false;
    [DoNotSerialize] public bool isUnlocked = true;

    // Effect modifiers.
    public StatModifier cooldownModifier = new StatModifier("Cooldown", 1.0f);
    public StatModifier costModifier = new StatModifier("Cost", 1.0f);
    public StatModifier damageModifier = new StatModifier("Damage", 1.0f);

    // Constants.
    public const float MONSTER_SOUND_VOLUME_MODIFIER = 0.1f;
    public const float MOVEMENT_DELAY_LEFT_CLICK = 0.7f;
    public const float MOVEMENT_DELAY_AFTER_CAST = 0.35f;

    public string abilityTag = "";

    /// <summary>
    /// Returns true if this ability requires a unit target, otherwise false.
    /// </summary>
    public bool AbilityTargetsUnit ()
    {
        return (targetMode == TargetMode.UnitAtRanged || 
            targetMode == TargetMode.UnitInMelee);
    }

    /// <summary>
    /// Returns true if this ability targets a location, otherwise false.
    /// </summary>
    public bool AbilityTargetsLocation ()
    {
        return (targetMode == TargetMode.PointInMelee ||
            targetMode == TargetMode.PointAtRanged);
    }

    /// <summary>
    /// Returns true if this is a melee ability, otherwise false.
    /// </summary>
    public bool AbilityRequiresMelee ()
    {
        return (targetMode == TargetMode.UnitInMelee ||
            targetMode == TargetMode.PointInMelee);
    }

    /// <summary>
    /// Returns true if this is a ranged ability, otherwise false.
    /// </summary>
    public bool AbilityRequiresRange ()
    {
        return (targetMode != TargetMode.UnitInMelee && targetMode != TargetMode.PointInMelee);
    }

    /// <summary>
    /// Returns a human readable string containing a tooltip description for the ability.
    /// </summary>
    public string GetTooltip (Unit unit = null)
    {
        // Determines the current resource cost modifier for the ability.
        float mod = 1.0f;
        if (unit != null && abilityGeneratesResource)
            mod = unit.stats.GetValue(Stat.ResourceGeneration);
        else if (unit != null && !abilityGeneratesResource)
            mod = GetCostModifier(unit);

        // Build the tooltip.
        string tooltip = abilityDescription;
        tooltip = Regex.Replace(tooltip, @"\d+(?:\.\d+)?%?", "<color=yellow>$&</color>");
        if (abilityGeneratesResource && abilityCost > 0)
            tooltip += $"\n\n<color=green>Generates {Mathf.Round(abilityCost * mod)} Resource</color>";
        else if (!abilityGeneratesResource && abilityCost > 0)
            tooltip += $"\n\n<color=#FF5555>Costs {Mathf.Round(abilityCost * mod)} Resource</color>";
        return tooltip;
    }

    /// <summary>
    /// Return the total cooldown time of this ability for the given caster.
    /// </summary>
    public float GetTotalCooldown (Unit caster)
    {
        if (cooldownIsAtackSpeed)
            return 1.0f / caster.stats.GetValue(Stat.AttacksPerSecond);
        else
            return abilityCooldown * Mathf.Max(0.1f, 1 - (caster.stats.GetValue(Stat.CooldownReduction) - 1)) * caster.GetAbilityCooldownModifier(this);
    }

    /// <summary>
    /// Return the remaining cooldown time of this ability for the given caster.
    /// </summary>
    public float GetRemainingCooldown (Unit caster)
    {
        float lastCastAt = caster.abilitiesLastCastAt.GetValueOrDefault(this, -1000);
        float remainingCooldown = GetTotalCooldown(caster) - (Time.time - lastCastAt);
        return Mathf.Max(0, remainingCooldown);
    }

    /// <summary>
    /// Return true if the caster has the resources for this ability, otherwise false.
    /// </summary>
    public bool HasResources (Unit caster)
    {
        if (abilityGeneratesResource) 
            return true;
        else
            return (caster.resource >= GetAbilityCost(caster));
    }

    /// <summary>
    /// Returns the current resource cost modifier of the ability.
    /// </summary>
    private float GetCostModifier (Unit caster)
    {
        return (1.0f - caster.stats.GetValue(Stat.ResourceCostReduction)) * caster.GetAbilityCostModifier(this);
    }

    /// <summary>
    /// Returns the total number of resources required to cast the ability (or 0 if
    /// the ability generates resources).
    /// </summary>
    public float GetAbilityCost (Unit caster)
    {
        if (abilityGeneratesResource) return 0;
        if (caster == null) return abilityCost;
        return abilityCost * GetCostModifier(caster);
    }

    /// <summary>
    /// Returns true if the specified target is valid for the ability, otherwise false.
    /// </summary>
    private bool TargetIsValid (Unit caster, Unit target)
    {
        if (!AbilityTargetsUnit()) return true;
        return (target != null && caster.GetFaction() != target.GetFaction());
    }

    /// <summary>
    /// Returns true if the ability is in range of the target, otherwise false.
    /// </summary>
    private bool AbilityInRange (Unit caster, Unit target, Vector3 targetPosition)
    {
        if (AbilityTargetsUnit()) return TargetUnitInRange(caster, target);
        if (AbilityTargetsLocation()) return TargetLocationInRange(caster, targetPosition);
        return true;
    }

    /// <summary>
    /// Returns true if the specified unit is in range of the caster, otherwise false.
    /// </summary>
    private bool TargetUnitInRange (Unit caster, Unit target)
    {
        float distance = Vector3.Distance(caster.transform.position, target.transform.position);
        return (distance <= GetAbilityRange(caster));
    }

    /// <summary>
    /// Returns true if the specified position is in range of the caster, otherwise false.
    /// </summary>
    private bool TargetLocationInRange (Unit caster, Vector3 target)
    {
        float distance = Vector3.Distance(caster.transform.position, target);
        return (distance <= GetAbilityRange(caster));
    }

    /// <summary>
    /// Returns the range of this ability for the specified unit.
    /// </summary>
    public float GetAbilityRange(Unit caster)
    {
        if (AbilityRequiresMelee()) return caster.baseAttackRange;
        if (AbilityRequiresRange()) return range;
        return 0;
    }

    /// <summary>
    /// Return true if the caster can cast the ability with the given parameters, otherwise false.
    /// </summary>
    public bool IsValidToCast (Unit caster, Unit target, Vector3 targetPosition)
    {
        if (!isUnlocked) return false;
        if (caster.IsStunned()) return false;
        if (!HasResources(caster)) return false;
        else if (!TargetIsValid(caster, target)) return false;
        else if (!AbilityInRange(caster, target, targetPosition)) return false;
        else if (GetRemainingCooldown(caster) > 0) return false;
        return true;
    }

    /// <summary>
    /// Have the given unit start casting this ability with the given targets.
    /// </summary>
    public void StartCast (Unit caster, Unit target, Vector3 targetPosition)
    {
        needsReminderFlash = false;
        ClampTargetLocation(caster, target, ref targetPosition);       
        ApplyResourceChange(caster);
        PlayAbilityAnimation(caster);
        ApplyCastingRestrictionsToCaster(caster);
        SendCastBeginEvents(caster, target, targetPosition);
    }

    /// <summary>
    /// Have the given unit finish casting this ability with the given targets.
    /// </summary>
    public void FinishCast (Unit caster, Unit target, Vector3 targetPosition)
    {
        ClampTargetLocation(caster, target, ref targetPosition);
        PlayCastCompleteFeedback();
        SendCastCompleteEvents(caster, target, targetPosition);
        caster.abilityBeingCast = null;
    }

    public void CancelCast (Unit caster)
    {
        caster.abilityBeingCast = null;
    }

    /// <summary>
    /// Returns an updated position, clamping the target to fit all required conditions.
    /// </summary>
    private void ClampTargetLocation (Unit caster, Unit target, ref Vector3 targetPosition)
    {
        // Only players can cast if out of range and have the target location be shifted.
        if (caster is Player && !AbilityInRange(caster, target, targetPosition))
            targetPosition = ClampPositionIntoRange(caster, targetPosition);

        // If a ranged ability requires line of sight, move the target in line of sight.
        if (AbilityTargetsLocation() && AbilityRequiresRange() && requiresLineOfSight)
            targetPosition = ClampTargetToLOS(caster, targetPosition);
    }

    /// <summary>
    /// Returns an updated position, casting the ability at the closest point in range.
    /// This is used by the player, if the chosen location is out of range.
    /// </summary>
    public Vector3 ClampPositionIntoRange(Unit caster, Vector3 targetPosition)
    {
        Vector3 distance = targetPosition - caster.transform.position;
        distance = caster.transform.position + distance.normalized * GetAbilityRange(caster);
        return distance;
    }

    /// <summary>
    /// Returns an updated target position, adjusted so that the target is in sight of the unit.
    /// </summary>
    private Vector3 ClampTargetToLOS(Unit caster, Vector3 targetPosition)
    {
        return Utilities.GetClosestPointInLOS(caster.transform.position + 
            Vector3.up * 0.1f, targetPosition + Vector3.up * 0.1f);
    }

    /// <summary>
    /// Apply any relevant casting restrictions to the player, preventing movement
    /// if needed, and defining the final cast time of the ability.
    /// </summary>
    private void ApplyCastingRestrictionsToCaster (Unit caster)
    {
        caster.abilityBeingCast = this;

        // If the ability cast time is derived by an ability-specific value.
        if (hasSpecificCastTime)
        {
            caster.finishCastAt = Time.time + castTime;
            if (castTime > 0)
                caster.canMoveAt = caster.finishCastAt + MOVEMENT_DELAY_AFTER_CAST;
        }

        // If the ability cast time is derived from the animation.
        else
        {
            float animationTime = GetAnimationTime(caster);
            caster.finishCastAt = Time.time + animationTime * animationActivationPosition;
            if (this == GameManager.player.leftClickAbility)
                caster.canMoveAt = Time.time + MOVEMENT_DELAY_LEFT_CLICK;
            else
                caster.canMoveAt = Time.time + MOVEMENT_DELAY_AFTER_CAST;
        }
    }

    /// <summary>
    /// Return the time required to play the animation. By default, 
    /// all animations are normalised to 1 second.
    /// </summary>
    private float GetAnimationTime (Unit caster)
    {
        float animationTime = 1.0f;
        if (cooldownIsAtackSpeed)
        {
            float attacksPerSecond = caster.stats.GetValue(Stat.AttacksPerSecond);
            animationTime = Mathf.Min(animationTime, 1.0f / attacksPerSecond);
        }
        return animationTime;
    }

    /// <summary>
    /// Play the ability animation on the caster.
    /// </summary>
    private void PlayAbilityAnimation (Unit caster)
    {
        caster.PlayAnimation(abilityAnimation, GetAnimationTime(caster));
    }

    /// <summary>
    /// Apply the ability resource cost/gain to the caster.
    /// </summary>
    private void ApplyResourceChange (Unit caster)
    {
        if (abilityGeneratesResource)
            caster.AddResource(abilityCost);
        else
            caster.RemoveResource(GetAbilityCost(caster));
    }

    /// <summary>
    /// Send all events for a started ability cast.
    /// </summary>
    private void SendCastBeginEvents (Unit caster, Unit target, Vector3 targetPosition)
    {
        Dictionary<string, object> presets = new Dictionary<string, object>
            {
                { LogicEngine.PRESET_CASTING_UNIT, caster },
                { LogicEngine.PRESET_ABILITY_CAST, this },
                { LogicEngine.PRESET_TARGET_UNIT, target },
                { LogicEngine.PRESET_TARGET_POSITION, targetPosition }
            };
        engine.TriggerEvent(presets, "WhenUnitBeginsCastingThisAbility");
        GameManager.events.OnAbilityCastStarted.Invoke(new GameEvents.OnAbilityCastStartedInfo(caster, this, target, targetPosition));
    }

    /// <summary>
    /// Send all events for a completed ability cast.
    /// </summary>
    private void SendCastCompleteEvents (Unit caster, Unit target, Vector3 targetPosition)
    {
        Dictionary<string, object> presets = new Dictionary<string, object>
            {
                { LogicEngine.PRESET_CASTING_UNIT, caster },
                { LogicEngine.PRESET_ABILITY_CAST, this },
                { LogicEngine.PRESET_TARGET_UNIT, target },
                { LogicEngine.PRESET_TARGET_POSITION, targetPosition }
            };
        engine.TriggerEvent(presets, "WhenUnitFinishesCastingThisAbility");
        GameManager.events.OnAbilityCastFinished.Invoke(new GameEvents.OnAbilityCastFinishedInfo(caster, this, target, targetPosition));
    }

    /// <summary>
    /// Play all feedback on completion of the cast. 
    /// </summary>
    private void PlayCastCompleteFeedback ()
    {
        if (castCompleteSound != null)
        {
            if (GetOwner() is Player)
                GameManager.music.PlaySound(castCompleteSound, castCompleteSoundVolume);
            else
                GameManager.music.PlaySound(castCompleteSound, castCompleteSoundVolume * 
                    MONSTER_SOUND_VOLUME_MODIFIER);
        }
    }

    /// <summary>
    /// Return a human-readable name for this ability.
    /// </summary>
    public override string ToString()
    {
        return abilityName;
    }

    /// <summary>
    /// Create a deep copy of the ability.
    /// </summary>
    public Ability Copy ()
    {
        Ability copy = Instantiate(this);
        copy.abilityName = abilityName + " (Copy)";
        copy.engine = engine.Copy();
        return copy;
    }

    /// <summary>
    /// Do a ` copy of the ability, not copying over the ability scripting.
    /// This reduces copying and allows you to 
    /// </summary>
    public Ability ShallowCopy ()
    {
        Ability ability = Instantiate(this);
        ability.engine = engine.ShallowCopy(ability);
        ability._template = template;
        return ability;
    }

    /// <summary>
    /// Return the logic engine associated with this ability.
    /// The logic engine controls the visual scripting logic for the ability.
    /// </summary>
    public LogicEngine GetEngine() 
    {
        return engine;
    }

    /// <summary>
    /// Returns the Unit owner of the ability.
    /// </summary>
    public Unit GetOwner()
    {
        return owner;
    }

    /// <summary>
    /// Sets the owner of this ability, allowing it to be used for later calculations.
    /// </summary>
    public void SetOwner(Unit owner)
    {
        this.owner = owner;
    }

    /// <summary>
    /// Return the data (e.g. prefab or scriptable object) associated with the scripting engine.
    /// </summary>
    public Object GetData()
    {
        return this;
    }

    /// <summary>
    /// Adds a cooldown percentage modifier to this specific ability.
    /// </summary>
    public void AddCooldownModifier(float modifier, string buff, int maxStacks = 99)
    {
        cooldownModifier.AddPercentageModifier(modifier, buff, maxStacks);
    }

    /// <summary>
    /// Add a cost modifier to this specific ability.
    /// </summary>
    public void AddCostModifier(float modifier, string buff, int maxStacks = 99)
    {
        costModifier.AddPercentageModifier(modifier, buff, maxStacks);
    }

    /// <summary>
    /// Add a cost modifier to this specific ability.
    /// </summary>
    public void AddDamageModifier(float modifier, string buff, int maxStacks = 99)
    {
        damageModifier.AddPercentageModifier(modifier, buff, maxStacks);
    }

    /// <summary>
    /// Adds a cooldown percentage modifier to this specific ability.
    /// </summary>
    public void AddTimedCooldownModifier(float modifier, float duration, string buff, int maxStacks = 99)
    {
        cooldownModifier.AddTimedPercentageModifier(modifier, duration, buff, maxStacks);
    }

    /// <summary>
    /// Add a cost modifier to this specific ability.
    /// </summary>
    public void AddTimedCostModifier(float modifier, float duration, string buff, int maxStacks = 99)
    {
        costModifier.AddTimedPercentageModifier(modifier, duration, buff, maxStacks);
    }

    /// <summary>
    /// Add a damage modifier to this specific ability.
    /// </summary>
    public void AddTimedDamageModifier(float modifier, float duration, string buff, int maxStacks = 99)
    {
        damageModifier.AddTimedPercentageModifier(modifier, duration, buff, maxStacks);
    }

    /// <summary>
    /// Remove all modifiers on this ability with the given label.
    /// </summary>
    private void RemoveModifiersWithLabel(string label)
    {
        cooldownModifier.RemoveModifiersWithLabel(label);
        costModifier.RemoveModifiersWithLabel(label);
    }

    /// <summary>
    /// Lock the ability, prevent it from being shown and cast.
    /// </summary>
    public void Lock()
    {
        isUnlocked = false;
        needsReminderFlash = true;
    }

    /// <summary>
    /// Unlock the ability, prevent it from being shown and cast.
    /// </summary>
    public void Unlock()
    {
        isUnlocked = true;
    }

    public Vector3 GetClosestPositionInRange (Unit caster, Vector3 target)
    {
        float distance = Vector3.Distance(caster.transform.position, target);

        if (AbilityRequiresMelee())
        {
            return target;
        }
        if (GetAbilityRange(caster) > distance)
        {
            return target;
        }
        else
        {
            Vector3 newPos = caster.transform.position + (target - caster.transform.position).normalized * GetAbilityRange(caster);
            return newPos;
        }
    }

    public static bool AbilitiesShareTemplate(Ability ability1, Ability ability2)
    {
        if (ability1.template != null) ability1 = ability1.template;
        if (ability2.template != null) ability2 = ability2.template;
        return ability1 == ability2;
    }

    public string GetTag()
    {
        return abilityTag;
    }
}
