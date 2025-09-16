public partial class VisualCodeScript
{
    [VisualScriptingEvent(icon = eventIcon,
        dropdownDescription = "Ability/When a unit begins casting this ability",
        dynamicDescription = "When a unit begins casting this ability")]
    [EventPreset(LogicEngine.PRESET_CASTING_UNIT)]
    [EventPreset(LogicEngine.PRESET_TARGET_POSITION)]
    [EventPreset(LogicEngine.PRESET_TARGET_UNIT)]
    public void WhenUnitBeginsCastingThisAbility() { }


    [VisualScriptingEvent(icon = eventIcon,
        dropdownDescription = "Ability/When a unit finishes casting this ability",
        dynamicDescription = "When a unit finishes casting this ability")]
    [EventPreset(LogicEngine.PRESET_CASTING_UNIT)]
    [EventPreset(LogicEngine.PRESET_TARGET_POSITION)]
    [EventPreset(LogicEngine.PRESET_TARGET_UNIT)]
    public void WhenUnitFinishesCastingThisAbility() { }


    [VisualScriptingEvent(icon = timerIcon,
        dropdownDescription = "Time/On script loaded",
        dynamicDescription = "On script loaded")]
    public void ScriptLoaded() { }


    [VisualScriptingEvent(icon = timerIcon,
        dropdownDescription = "Time/On script unloaded",
        dynamicDescription = "On script unloaded")]
    public void ScriptUnloaded() { }


    [VisualScriptingEvent(icon = timerIcon,
        dropdownDescription = "Time/Do every frame",
        dynamicDescription = "Do actions every frame")]
    public void EveryFrame() { }


    [VisualScriptingEvent(icon = timerIcon,
        dropdownDescription = "Time/Do after X seconds",
        dynamicDescription = "After $ seconds")]
    [NumberArg(argType = ArgType.Value, defaultValue = 5)]
    public void OnOneOffTimerFinished(float delay) { }


    [VisualScriptingEvent(icon = timerIcon,
        dropdownDescription = "Time/Do every X Seconds",
        dynamicDescription = "Every $ seconds")]
    [NumberArg(argType = ArgType.Value, defaultValue = 5)]
    public void OnTimerFinished(float period) { }


    [VisualScriptingEvent(icon = eventIcon,
        dropdownDescription = "Unit/Unit is killed",
        dynamicDescription = "When a unit is killed")]
    [EventPreset(LogicEngine.PRESET_KILLED_UNIT)]
    [EventPreset(LogicEngine.PRESET_KILLING_UNIT)]
    [EventPreset(LogicEngine.PRESET_KILLING_ABILITY)]
    [EventPreset(LogicEngine.PRESET_IS_CRITICAL)]
    public void WhenUnitIsKilled() { }


    [VisualScriptingEvent(icon = eventIcon,
        dropdownDescription = "Unit/Unit is damaged",
        dynamicDescription = "When a unit is damaged")]
    [EventPreset(LogicEngine.PRESET_DAMAGED_UNIT)]
    [EventPreset(LogicEngine.PRESET_DAMAGING_UNIT)]
    [EventPreset(LogicEngine.PRESET_DAMAGING_ABILITY)]
    [EventPreset(LogicEngine.PRESET_DAMAGE_DEALT)]
    [EventPreset(LogicEngine.PRESET_IS_CRITICAL)]
    public void OnUnitDamaged() { }


    [VisualScriptingEvent(icon = eventIcon,
        dropdownDescription = "Unit/Unit is healed",
        dynamicDescription = "When a unit is healed")]
    [EventPreset(LogicEngine.PRESET_HEALED_UNIT)]
    [EventPreset(LogicEngine.PRESET_HEALING_UNIT)]
    public void WhenUnitIsHealed() { }


    [VisualScriptingEvent(icon = eventIcon,
        dropdownDescription = "Unit/Unit gains resource",
        dynamicDescription = "When a unit gains resource")]
    [EventPreset(LogicEngine.PRESET_TRIGGERING_UNIT)]
    [EventPreset(LogicEngine.PRESET_RESOURCES_GAINED)]
    public void WhenUnitGainsResource() { }


    [VisualScriptingEvent(icon = eventIcon,
        dropdownDescription = "Unit/Unit loses resource",
        dynamicDescription = "When a unit loses resource")]
    [EventPreset(LogicEngine.PRESET_TRIGGERING_UNIT)]
    [EventPreset(LogicEngine.PRESET_RESOURCES_LOST)]
    public void WhenUnitLosesResource() { }


    [VisualScriptingEvent(icon = eventIcon,
        dropdownDescription = "Ability/When a unit starts casting specific ability",
        dynamicDescription = "When Unit Starts Casting $")]
    [EventPreset(LogicEngine.PRESET_CASTING_UNIT)]
    [EventPreset(LogicEngine.PRESET_ABILITY_CAST)]
    public void UnitStartCast() { }


    [VisualScriptingEvent(icon = eventIcon,
        dropdownDescription = "Ability/When a unit finishes casting specific ability",
        dynamicDescription = "When Unit Finishes Casting $")]
    [EventPreset(LogicEngine.PRESET_CASTING_UNIT)]
    [EventPreset(LogicEngine.PRESET_ABILITY_CAST)]
    public void UnitFinishCast() { }


    [VisualScriptingEvent(icon = eventIcon,
        dropdownDescription = "Player/Player gains experience",
        dynamicDescription = "When the player gains experience")]
    public void PlayerGainsExperience() { }


    [VisualScriptingEvent(icon = eventIcon,
        dropdownDescription = "Player/Player gains a level",
        dynamicDescription = "When the player gains a level")]
    public void OnPlayerLevelUp() { }


    [VisualScriptingEvent(icon = eventIcon,
        dropdownDescription = "Player/Player sells an item",
        dynamicDescription = "When the player sells an item")]
    [EventPreset(LogicEngine.PRESET_TRIGGERING_ITEM)]
    public void OnItemSold() { }


    [VisualScriptingEvent(icon = eventIcon,
        dropdownDescription = "Player/Player buys an item",
        dynamicDescription = "When the player buys an item")]
    [EventPreset(LogicEngine.PRESET_TRIGGERING_ITEM)]
    public void OnItemBought() { }


    [VisualScriptingEvent(icon = pickupIcon,
        dropdownDescription = "Player/Player equips an item",
        dynamicDescription = "When the player equips an item")]
    [EventPreset(LogicEngine.PRESET_TRIGGERING_ITEM)]
    public void OnItemEquipped() { }


    [VisualScriptingEvent(icon = pickupIcon,
        dropdownDescription = "Player/Player unequips an item",
        dynamicDescription = "When the player unequips an item")]
    [EventPreset(LogicEngine.PRESET_TRIGGERING_ITEM)]
    public void OnItemUnequipped() { }


    [VisualScriptingEvent(icon = pickupIcon,
        dropdownDescription = "Player/Player picks up an item",
        dynamicDescription = "When the player picks up an item")]
    [EventPreset(LogicEngine.PRESET_TRIGGERING_ITEM)]
    public void OnItemPickedUp() { }


    [VisualScriptingEvent(icon = pickupIcon,
        dropdownDescription = "Player/Player picks up gold",
        dynamicDescription = "When the player picks up gold")]
    [EventPreset(LogicEngine.PRESET_GOLD_PICKED_UP)]
    public void OnPickupGold() { }


    [VisualScriptingEvent(icon = pickupIcon,
        dropdownDescription = "Player/Player picks up a health globe",
        dynamicDescription = "When the player picks up a health globe")]
    [EventPreset(LogicEngine.PRESET_HEALTH_PICKED_UP)]
    public void OnPickupHealth() { }


    [VisualScriptingEvent(icon = regionIcon,
        dropdownDescription = "Region/Unit enters region",
        dynamicDescription = "Unit enters region named $")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Region Name")]
    public void UnitEntersRegion(string regionName) { }


    [VisualScriptingEvent(icon = regionIcon,
        dropdownDescription = "Region/Unit leaves region",
        dynamicDescription = "Unit leaves region named $")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Region Name")]
    public void UnitExitsRegion(string regionName) { }


    [VisualScriptingEvent(icon = projectileIcon,
        dropdownDescription = "Projectile/Projectile collides with an enemy",
        dynamicDescription = "Projectile from this object collides with an enemy")]
    [EventPreset(LogicEngine.PRESET_EVENT_PROJECTILE)]
    [EventPreset(LogicEngine.PRESET_CASTING_UNIT)]
    [EventPreset(LogicEngine.PRESET_COLLIDING_UNIT)]
    public void ProjectileMadeByThisCollidesWithUnit() { }


    [VisualScriptingEvent(icon = projectileIcon,
        dropdownDescription = "Projectile/Projectile times out",
        dynamicDescription = "Projectile from this object times out")]
    [EventPreset(LogicEngine.PRESET_EVENT_PROJECTILE)]
    [EventPreset(LogicEngine.PRESET_CASTING_UNIT)]
    public void ProjectileTimesOut() { }


    [VisualScriptingEvent(icon = projectileIcon,
        dropdownDescription = "Projectile/Projectile reaches its goal",
        dynamicDescription = "Projectile from this object reaches its goal")]
    [EventPreset(LogicEngine.PRESET_EVENT_PROJECTILE)]
    [EventPreset(LogicEngine.PRESET_CASTING_UNIT)]
    [EventPreset(LogicEngine.PRESET_GOAL_POSITION)]
    [EventPreset(LogicEngine.PRESET_GOAL_UNIT)]
    public void ProjectileReachesGoal() { }


    [VisualScriptingEvent(icon = projectileIcon,
        dropdownDescription = "Projectile/Projectile collides with terrain",
        dynamicDescription = "Projectile from this object collides with the terrain")]
    [EventPreset(LogicEngine.PRESET_EVENT_PROJECTILE)]
    [EventPreset(LogicEngine.PRESET_CASTING_UNIT)]
    public void ProjectileCollidesWithTerrain() { }


    [VisualScriptingEvent(icon = projectileIcon,
        dropdownDescription = "Quests/On quest completed",
        dynamicDescription = "When the quest named $ is completed")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Quest Name")]
    public void OnQuestCompleted(string questName) { }


    [VisualScriptingEvent(icon = projectileIcon,
        dropdownDescription = "Quests/On quest received",
        dynamicDescription = "When a quest named $ is received")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Quest Name")]
    public void OnQuestReceived(string questName) { }


    [VisualScriptingEvent(icon = inputIcon,
        dropdownDescription = "Input/On Key Down",
        dynamicDescription = "When the $ key is pressed down")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Key")]
    public void OnKeyDown(string key) { }


    [VisualScriptingEvent(icon = inputIcon,
        dropdownDescription = "Input/On Key Up",
        dynamicDescription = "When the $ key is released")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Key")]
    public void OnKeyUp(string key) { }


    [VisualScriptingEvent(icon = inputIcon,
        dropdownDescription = "Input/On Key Held",
        dynamicDescription = "While the $ key is held down")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Key")]
    public void OnKeyHeld(string key) { }

}
