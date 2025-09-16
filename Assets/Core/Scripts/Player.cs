using MyUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class Player : Unit
{
    [HideInInspector] public float currentGold = 0;
    [HideInInspector] public int currentLevel = 1;
    [HideInInspector] public float currentExperience = 0;
    [HideInInspector] public float experienceToNextLevel = 100;
    [HideInInspector] public Ability leftClickAbility;
    [HideInInspector] public bool rightClickAlsoMoves = false;
    [DoNotSerialize] public Inventory inventory;
    [DoNotSerialize] public Inventory equipment;
    [DoNotSerialize] public Inventory sellSlot;

    public GameFeedback levelUpFeedback;
    public string resourceName = "Resource";
    public Material resourceMaterial;

    [Header("[Player Scaling]")]
    public float bonusDamagePerLevel;
    public float bonusHealthPerLevel;
    public float bonusMovementSpeedPerLevel;
    public float bonusResourcePerLevel;
    public float bonusArmorPerLevel;
    public float bonusCriticalChancePerLevel;
    public float bonusCriticalDamagePerLevel;
    public float bonusHealthRegenPerLevel;
    public float bonusResourceRegenPerLevel;

    //  Constants related to the player.
    public const int MAX_INVENTORY_SIZE = 28;
    private const int WEAPON = 0;
    private const int AMULET = 1;
    private const int ARMOR = 2;
    private const int BOOTS = 3;
    private const int RING1 = 4;
    private const int RING2 = 5;
    private const int LEFT = 0;
    private const int RIGHT = 1;
    private const int TOTAL_EQUIPMENT_SLOTS = 6;
    private const int SELL_INVENTORY_SIZE = 1;
    private const float MAX_DAMAGE_REDUCTION_MOD = 0.2f;
    private const string RESPAWN_ANIMATION = "Idle";
    private const float PLAYER_ROTATION_SPEED = 5;
    private static Color OUTLINE_COLOR = new Color(0.5f, 0.5f, 1.0f, 0.03f);

    /// <summary>
    /// Perform initial setup.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        SetOutline(OUTLINE_COLOR);
        CacheLeftClickAbility();
        UpdateExperienceRequiredForLevel();
        SetupPlayerInventory();
        SetupEquipment();
        SetupSellSlot();
    }

    /// <summary>
    /// Handle all frame-by-frame updates.
    /// </summary>
    protected override void Update()
    {
        base.Update();
        if (!isDead)
        {
            UpdateTargetPosition();
            UpdateTarget();
            HandleMovement();
            HandleRotation();
        }
    }

    /// <summary>
    /// Set up the main inventory for the player. By default, the inventory is empty
    /// and has a fixed size.
    /// </summary>
    private void SetupPlayerInventory() {
        inventory = new Inventory(MAX_INVENTORY_SIZE);
        inventory.onItemAdded.AddListener(OnItemPickedUp);
    }

    /// <summary>
    /// Set up the equipment for the player. By default, the player has no
    /// equipment, but the player can add items of the correct type to each slot
    /// as they aquire them.
    /// </summary>
    private void SetupEquipment ()
    {
        equipment = new Inventory(TOTAL_EQUIPMENT_SLOTS);
        equipment.onItemAdded.AddListener(OnItemEquipped);
        equipment.onItemRemoved.AddListener(OnItemUnequipped);
    }

    /// <summary>
    /// Set up the sell slot for the player. Adding an item to the sell slot
    /// will auto sell it and give the gold to the player.
    /// </summary>
    private void SetupSellSlot ()
    {
        sellSlot = new Inventory(SELL_INVENTORY_SIZE);
        sellSlot.onItemAdded.AddListener((item) => {
            SellItem(item);
            sellSlot.RemoveItem(item);
        });
    }

    /// <summary>
    /// Logic for the player damage taken method.
    /// </summary>
    public override void TakeDamage(float amount, bool isCritical, Unit damagingUnit, IVisualCodeHandler damageSource)
    {
        base.TakeDamage(amount, isCritical, damagingUnit, damageSource);
    }

    /// <summary>
    /// Sell the given item, receiving the cost of the item modified by a global sell factor.
    /// </summary>
    private void SellItem (Item item)
    {
        AddGold(Mathf.Round(item.itemCost * GameManager.inventoryValues.sellItemReturnRate));
    }

    /// <summary>
    ///  Handles all pickup events for items.
    /// </summary>
    private void OnItemPickedUp(Item item)
    {
        // Do not auto-equip the first item, so we can ensure that the player knows how to do this.
        /*
        //if (equipment.GetFilledSlots() == 0)
        //    return;

        // Auto-equip items if the player is not wearing an item in that slot.
        if (item.itemType == Item.ItemType.Weapon && equipment.IsEmpty(WEAPON)) {
            inventory.RemoveItem(item);
            equipment.AddItemAtID(item, WEAPON);
        }
        else if (item.itemType == Item.ItemType.Amulet && equipment.IsEmpty(AMULET))
        {
            inventory.RemoveItem(item);
            equipment.AddItemAtID(item, AMULET);
        }
        else if (item.itemType == Item.ItemType.Armor && equipment.IsEmpty(ARMOR))
        {
            inventory.RemoveItem(item);
            equipment.AddItemAtID(item, ARMOR);
        }
        else if (item.itemType == Item.ItemType.Boots && equipment.IsEmpty(BOOTS))
        {
            inventory.RemoveItem(item);
            equipment.AddItemAtID(item, BOOTS);
        }
        else if (item.itemType == Item.ItemType.Ring && equipment.IsEmpty(RING1))
        {
            inventory.RemoveItem(item);
            equipment.AddItemAtID(item, RING1);
        }
        else if (item.itemType == Item.ItemType.Ring && equipment.IsEmpty(RING2))
        {
            inventory.RemoveItem(item);
            equipment.AddItemAtID(item, RING2);
        }
        */
    }

    /// <summary>
    /// Returns true if the point is in range of the player, otherwise false.
    /// </summary>
    public override bool InRange(Vector3 point)
    {
        return Vector3.Distance(transform.position, point) < baseAttackRange;
    }

    /// <summary>
    /// Returns true if this player can move, otherwise false.
    /// </summary>
    public override bool CanMove ()
    {
        
        if (Time.time < canMoveAt) 
            return false;
        if (Input.GetKey(GameManager.settings.keybindings[Settings.FORCE_HOLD_KEYBIND_ID]))
            return false;
        if (abilityBeingCast != null && !abilityBeingCast.canMoveWhileCasting) 
            return false;
        if (IsStunned())
            return false;

        return true;
    }

    /// <summary>
    /// Set the left-click keybind for the player (if one exists).
    /// </summary>
    private void CacheLeftClickAbility ()
    {
        leftClickAbility = null;
        for (int i = 0; i < GameManager.settings.keybindings.Length; i++)
        {
            if (GameManager.settings.keybindings[i] == KeyCode.Mouse0 && abilities.Count > i)
            {
                leftClickAbility = abilities[i];
                return;
            }
        }
    }

    /// <summary>
    /// Handle all movemnet for the player.
    /// </summary>
    private void HandleMovement ()
    {
        // If the player cannot move, 
        if (!CanMove())
        {
            StopMoving();
            return;
        }

        // Recently selected interactable, move towards it.
        if (GameManager.selectedInteractable != null && Time.time < GameManager.selectedInteractableAt + 2.5f)
        {
            targetPosition = GameManager.selectedInteractable.transform.position;
            agentNavigation.SetDestination(GameManager.selectedInteractable.transform.position);
        }

        // If the player is holding the force move keybind, the player must move.
        else if (Input.GetKey(GameManager.settings.keybindings[Settings.FORCE_MOVE_KEYBIND_ID]))
        {
            agentNavigation.SetDestination(targetPosition);
        }

        // If right-click force move is enabled, move the player.
        else if (rightClickAlsoMoves && Input.GetMouseButton(RIGHT))
        {
            agentNavigation.SetDestination(targetPosition);
        }

        // If left click is not an ability keybind, move as normal.
        else if (leftClickAbility == null && Input.GetMouseButton(LEFT))
        {
            agentNavigation.SetDestination(targetPosition);
        }

        // No monster selected, move towards target location.
        else if (Input.GetMouseButton(LEFT) && GameManager.hoveredMonster == null)
        {
            agentNavigation.SetDestination(targetPosition);
        }

        // Move into range of the target.
        else if (Input.GetMouseButton(LEFT) && GameManager.hoveredMonster != null)
        {
            float range = leftClickAbility.GetAbilityRange(this);
            if (Vector3.Distance(transform.position, targetPosition) > range)
                agentNavigation.SetDestination(targetPosition);
            else
                StopMoving();
        }

        // If no other movement commands are given, stop moving.
        else
        {
            StopMoving();
        }
    }

    /// <summary>
    /// Handle updates to the target position of the player.
    /// </summary>
    private void UpdateTargetPosition()
    {
        if (GameManager.selectedInteractable != null)
            targetPosition = GameManager.selectedInteractable.transform.position;
        if (!IsCasting() || abilityBeingCast.canUpdateTargetWhileCasting)
            targetPosition = Utilities.GetValidNavMeshPosition(Utilities.GetMouseWorldPosition());
    }

    /// <summary>
    /// Update the target if the current target isn't locked in.
    /// </summary>
    private void UpdateTarget()
    {
        if (!CastInProgress() || abilityBeingCast.canUpdateTargetWhileCasting)
        {
            SetTarget(GameManager.hoveredMonster);
        }
    }

    /// <summary>
    /// Rotate the player towards the target
    /// </summary>
    private void HandleRotation()
    {
        if (!IsStunned() && (Input.GetMouseButton(LEFT) || IsMoving() || IsCasting()))
        {
            Vector3 directionToTarget = targetPosition - transform.position;
            directionToTarget.y = 0;
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, 
                    targetRotation, PLAYER_ROTATION_SPEED * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// Try to cast the abilities on the player if the required conditions are met.
    /// (e.g. keybind is pressed, player has resources etc).
    /// </summary>
    protected override void TryToCastAbilities()
    {
        base.TryToCastAbilities();
        for (int i = 0; i < abilities.Count; i++)
        {
            if (abilities[i] != null && TryingToCastAbility(abilities[i], GameManager.settings.keybindings[i]))
            {
                Vector3 pos = abilities[i].GetClosestPositionInRange(this, targetPosition);
                CastAbility(abilities[i], target, pos);
            }
        }
    }

    /// <summary>
    /// Return true if the player is trying to cast the ability, otherwise false.
    /// </summary>
    private bool TryingToCastAbility (Ability ability, KeyCode keybind)
    {
        if (!Input.GetKey(keybind)) return false;
        if (ability == leftClickAbility && GameManager.hoveredMonster == null && 
            !Input.GetKey(GameManager.settings.keybindings[Settings.FORCE_HOLD_KEYBIND_ID])) return false;
        return true;
    }
    
    /// <summary>
    /// Kill this unit.
    /// </summary>
    public override void Kill(Unit killingUnit, IVisualCodeHandler killingSource, bool isCritical)
    {
        if (isDead) return;
        base.Kill(killingUnit, killingSource, isCritical);
        GameManager.events.OnPlayerKilled.Invoke(new GameEvents.OnPlayerKilledInfo(this, killingUnit, killingSource, isCritical));
    }

    /// <summary>
    /// Adds the specified amount of gold to the player.
    /// </summary>
    public virtual void AddGold(float amount)
    {
        currentGold += amount;
        GameManager.events.OnGoldRemoved.Invoke(amount);
    }

    /// <summary>
    /// Removes the specified amount of gold from the player.
    /// </summary>
    public virtual void RemoveGold(float amount)
    {
        currentGold = Mathf.Max(0.0f, currentGold - amount);
        GameManager.events.OnGoldRemoved.Invoke(amount);
    }

    public virtual void SetGold(float amount)
    {
        currentGold = Mathf.Max(0.0f, amount);
        GameManager.events.OnGoldRemoved.Invoke(amount);
    }

    /// <summary>
    /// Add the specified amount of experience to the player.
    /// </summary>
    public virtual void AddExperience(float amount)
    {
        currentExperience += amount;
        GameManager.events.OnPlayerExperienceGained.Invoke(this);
        while (currentExperience >= experienceToNextLevel)
        {
            AddLevels(1);
            currentExperience -= experienceToNextLevel;
        } 
    }

    /// <summary>
    /// Remove the specified amount of experience to the player.
    /// </summary>
    public virtual void RemoveExperience(float amount)
    {
        currentExperience = Mathf.Min(0, currentExperience - amount);
    }

    /// <summary>
    /// Set the player's current experience to the specified amount.
    /// This will never cause the player to lose levels, as 0 = start of level.
    /// </summary>
    public virtual void SetExperience(float amount)
    {
        currentExperience = amount;
        GameManager.events.OnPlayerExperienceGained.Invoke(this);
        while (currentExperience >= experienceToNextLevel)
        {
            AddLevels(1);
            currentExperience -= experienceToNextLevel;
        }
    }

    /// <summary>
    /// Add the specified amount of levels to the player.
    /// </summary>
    public virtual void AddLevels(int levelsToAdd)
    {
        for (int i = 0; i < levelsToAdd; i++)
        {
            currentLevel++;
            UpdateExperienceRequiredForLevel();
            GameManager.events.OnPlayerLevelUp.Invoke(this);
            levelUpFeedback.ActivateFeedback(gameObject, null, transform.position);

            // Apply stat scaling.
            stats[Stat.MovementSpeed].ModifyBaseValue(bonusMovementSpeedPerLevel);
            stats[Stat.MaxHealth].ModifyBaseValue(bonusHealthPerLevel);
            stats[Stat.Damage].ModifyBaseValue(bonusDamagePerLevel);
            stats[Stat.MaxResource].ModifyBaseValue(bonusResourcePerLevel);
            stats[Stat.Armor].ModifyBaseValue(bonusArmorPerLevel);
            stats[Stat.CriticalStrikeChance].ModifyBaseValue(bonusCriticalChancePerLevel);
            stats[Stat.CriticalStrikeDamage].ModifyBaseValue(bonusCriticalDamagePerLevel);
            baseHealthRegen += bonusHealthRegenPerLevel;
            baseResourceRegen += bonusResourceRegenPerLevel;
        }
    }

    /// <summary>
    /// Remove the specified amount of levels from the player.
    /// </summary>
    public virtual void RemoveLevels(int levelsToRemove)
    {
        currentExperience = 0;
        currentLevel -= levelsToRemove;
        UpdateExperienceRequiredForLevel();
    }

    /// <summary>
    /// Set the player's level to the specified level.
    /// </summary>
    public virtual void SetLevel(int newLevel)
    {
        if (newLevel > currentLevel)
            AddLevels(newLevel - currentLevel);
        else
            currentLevel = newLevel;

        UpdateExperienceRequiredForLevel();
        GameManager.events.OnPlayerLevelUp.Invoke(this);
    }

    /// <summary>
    /// Updates the amount of experience required for a level.
    /// </summary>
    private void UpdateExperienceRequiredForLevel ()
    {
        experienceToNextLevel = GameManager.playerExperienceValues.startingXPPerLevel;
        experienceToNextLevel += GameManager.playerExperienceValues.additionalMaxXPPerLevel * currentLevel;
    }

    /// <summary>
    /// When an item is equipped, add its stats to the player.
    /// </summary>
    public void OnItemEquipped(Item item)
    {
        GameManager.events.OnItemEquipped.Invoke(item);
        GameManager.logicEngine.AddEngine(item.engine);
        foreach (Item.RolledStatValue rolledStatValue in item.rolledStatValues)
        {
            if (rolledStatValue.isPercent)
            {
                stats[rolledStatValue.stat].AddPercentageModifier(
                    rolledStatValue.amount, item.GetInstanceID().ToString());
            }
            else
            {
                stats[rolledStatValue.stat].AddValueModifier(
                    rolledStatValue.amount, item.GetInstanceID().ToString());
            }
        }
    }

    /// <summary>
    /// When an item is unequipped, remove its stats from the player.
    /// </summary>
    public void OnItemUnequipped(Item item)
    {
        GameManager.events.OnItemUnequipped.Invoke(item);
        stats.RemoveBuffWithLabel(item.GetInstanceID().ToString());
        GameManager.logicEngine.RemoveEngine(item.engine);
    }

    /// <summary>
    /// Return the faction of the player (the 'Player' faction).
    /// </summary>
    public override Faction GetFaction ()
    {
        return Faction.Player;
    }

    /// <summary>
    /// Revive the player, reseting any temporary buffs and setting them back to maximum health.
    /// </summary>
    public void Revive ()
    {
        if (!isDead) return;

        isDead = false;

        // Remove all timed modifiers on the unit.
        stats.RemoveAllTimedModifiers();

        // Ideally this would be damage reduction instead, but the damage formula isn't done until a later week.
        // (And players *realllllly* need this for the initial balancing activity).
        stats.Get(Stat.MaxHealth).AddTimedValueModifier(100000, 2, "Revive Health", 1);

        // Set the unit to full health and play the idle animation.
        health = stats.GetValue(Stat.MaxHealth);
        animator.Play(RESPAWN_ANIMATION);
    }

    public override void OnAbilitiesUpdated()
    {
        base.OnAbilitiesUpdated();
        GameManager.ui.PlayerWindow.RedrawChacterHUD();
    }

    public bool HasItemEquipped (Item item)
    {
        for (int i = 0; i < equipment.GetSlots(); i++)
        {
            Item slotItem = equipment.GetItemAtID(i);
            if (slotItem != null && slotItem.itemName == item.itemName)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasItemInInventory(Item item)
    {
        for (int i = 0; i < inventory.GetSlots(); i++)
        {
            Item slotItem = inventory.GetItemAtID(i);
            if (slotItem != null && slotItem.itemName == item.itemName)
            {
                return true;
            }
        }
        return false;
    }
}
