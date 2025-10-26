using MyUtilities;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public partial class VisualCodeScript
{
    [VisualScriptingFunction(
        dropdownDescription = "Flow/Wait",
        dynamicDescription = "Wait for $ second(s)",
        icon = waitIcon)]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void Wait(float duration)
    {
        // No Logic Here - Handled Seperately.
    } 

    [VisualScriptingFunction(
        dropdownDescription = "Flow/If Statement",
        dynamicDescription = "Do actions if $",
        allowsChildren = true,
        icon = conditionIcon)]
    [BoolArg(argType = ArgType.Temp)]
    public void DoActionsIfBool(bool condition)
    {
        // No Logic Here - Handled Seperately.
    }

    [VisualScriptingFunction(
        dropdownDescription = "Flow/While Loop",
        dynamicDescription = "Do actions while $",
        allowsChildren = true,
        icon = loopIcon)]
    [BoolArg(argType = ArgType.Temp)]
    public void DoActionsWhileBool(bool condition)
    {
        // No Logic Here - Handled Seperately.
    }

    [VisualScriptingFunction(
        dropdownDescription = "Flow/For Loop",
        dynamicDescription = "Do actions $ times (Variable Storing Current Iteration: $)",
        allowsChildren = true,
        icon = loopIcon)]
    [NumberArg(argType = ArgType.Temp)]
    [StringArg(argType = ArgType.Value, defaultValue = "Loop ID")]
    public void DoActionsXTimesStoringVariable(float times, string variable)
    {
        // No Logic Here - Handled Seperately.
    }

    [VisualScriptingFunction(
        dropdownDescription = "Flow/For Loop (No Variable)",
        dynamicDescription = "Do actions $ times",
        allowsChildren = true,
        icon = loopIcon)]
    [NumberArg(argType = ArgType.Temp)]
    public void DoActionsXTimes(float times)
    {
        // No Logic Here - Handled Seperately.
    }

    [VisualScriptingFunction(
        dropdownDescription = "Flow/For Each Unit in Unit Group",
        dynamicDescription = "For Each Unit in $ (Variable Storing Current Unit: $)",
        allowsChildren = true,
        icon = loopIcon)]
    [UnitGroupArg(argType = ArgType.Temp, allowValue = false)]
    [StringArg(argType = ArgType.Value, defaultValue = "Unit")]
    public void ForEachUnitInGroup(UnitGroup group, string variable)
    {
        // No Logic Here - Handled Seperately.
    }

    [VisualScriptingFunction(
        dropdownDescription = "Flow/Disable This Script",
        dynamicDescription = "Disable this script",
        icon = cancelIcon)]
    public void DisableScript()
    {
        LogicEngine.current.disabledScripts[this] = float.MaxValue;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Flow/Disable This Script",
        dynamicDescription = "Disable this script for $",
        icon = cancelIcon)]
    [NumberArg(argType = ArgType.Value, defaultValue = 1, suffix = "s")]
    public void DisableScriptForDuration(float duration)
    {
        LogicEngine.current.disabledScripts[this] = Time.time + duration;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Flow/Send Event Message",
        dynamicDescription = "Send event message with label $",
        icon = eventIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Label")]
    public void SendEventMessage(string message)
    {
        GameManager.SendEventMessage(message);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Spin",
        dynamicDescription = "Spin $ by $ for $",
        icon = unitIcon)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 360, suffix = "�/s")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1, suffix = "s")]
    public void SpinUnit(Unit unit, float speed, float duration)
    {
        Error(unit == null, "The unit you want to spin is invalid.");
        unit.StartSpin(speed, duration);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Spawn Monster",
        dynamicDescription = "Spawn $ at $",
        icon = unitIcon)]
    [UnitArg(argType = ArgType.Temp, allowPreset = false, allowFunction = false)]
    [VectorArg(argType = ArgType.Temp)]
    public void SpawnUnit(Unit unit, Vector3 position)
    {
        Error(unit == null, "The unit to spawn is invalid.");
        SpawnUnit2(unit, position, false);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Spawn Unit",
        dynamicDescription = "Spawn $ for $ faction at $",
        icon = unitIcon)]
    [UnitArg(argType = ArgType.Temp, allowPreset = false, allowFunction = false)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.Factions, allowFunction = false, allowPreset = false)]
    [VectorArg(argType = ArgType.Temp)]
    public void SpawnUnit3 (Unit unit, string faction,  Vector3 position)
    {
        Error(unit == null, "The unit to spawn is invalid.");
        UnitSpawnEffect spawnEffect = null;
        if (unit is Monster) spawnEffect = ((Monster)unit).spawnEffect;
        position = Utilities.GetValidNavMeshPosition(position);

        Unit.Faction fact = (Unit.Faction)Enum.Parse(typeof(Unit.Faction), faction);
        if (spawnEffect != null)
        {
            SpawnUnitWithEffect(unit, position, spawnEffect, false, fact);
        }
        else
        {
            Unit u = GameObject.Instantiate(unit, position, Quaternion.identity);
        }
    }

    public void SpawnUnit2(Unit unit, Vector3 position, bool isEmpowered)
    {
        Error(unit == null, "The unit to spawn is invalid.");
        UnitSpawnEffect spawnEffect = null;
        if (unit is Monster) spawnEffect = ((Monster)unit).spawnEffect;
        position = Utilities.GetValidNavMeshPosition(position);
        if (spawnEffect != null)
        {
            SpawnUnitWithEffect(unit, position, spawnEffect, isEmpowered);
        }
        else
        {
            Unit u = GameObject.Instantiate(unit, position, Quaternion.identity);
            if (isEmpowered && u is Monster monster) monster.Empower();
        }
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Spawn Empowered Monster",
        dynamicDescription = "Spawn empowered $ at $",
        icon = unitIcon)]
    [UnitArg(argType = ArgType.Temp, allowPreset = false, allowFunction = false)]
    [VectorArg(argType = ArgType.Temp)]
    public void SpawnEmpoweredUnit(Unit unit, Vector3 position)
    {
        Error(unit == null, "The unit to spawn is invalid.");
        SpawnUnit2(unit, position, true);
    }

    public void SpawnUnitWithEffect(Unit unit, Vector3 position, UnitSpawnEffect spawnEffect, bool isEmpowered = false, Unit.Faction faction = Unit.Faction.Enemy)
    {
        Error(unit == null, "The unit to spawn is invalid.");
        Error(spawnEffect == null, "The effect to spawn is invalid.");
        GameManager.instance.StartCoroutine(SpawnUnitCoroutine(unit, position, spawnEffect, isEmpowered, faction));
    }

    private IEnumerator SpawnUnitCoroutine(Unit unit, Vector3 position, UnitSpawnEffect spawnEffect, bool isEmpowered = false, Unit.Faction faction = Unit.Faction.Enemy)
    {
        float duration = spawnEffect.GetComponent<UnitSpawnEffect>().effectDuration;
        ObjectPooler.InstantiatePooled(spawnEffect.gameObject, position, Quaternion.identity);
        yield return new WaitForSeconds(duration);
        Unit u = GameObject.Instantiate(unit, position, Quaternion.identity);
        if (isEmpowered && u is Monster monster) monster.Empower();
        u.SetFaction(faction);
        IVisualCodeHandler engine = LogicEngine.current.engineHandler;
        Unit spawningUnit = engine.GetOwner();
        GameManager.events.OnUnitSpawned.Invoke(new GameEvents.OnUnitSpawnedInfo(u, spawningUnit));
    }

    public void SpawnUnits(float count, Unit unit, Vector3 position)
    {
        Error(unit == null, "The unit to spawn is invalid.");
        int unitsToSpawn = (int)count;
        if (unit != null && unitsToSpawn > 0)
        {
            GameObject.Instantiate(unit, position, Quaternion.identity);
        }
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Kill",
        dynamicDescription = "Kill $",
        icon = unitIcon)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public void KillUnit(Unit unit)
    {
        Error(unit == null, "The unit to kill is invalid.");
        unit.Kill(LogicEngine.current.engineHandler.GetOwner(), LogicEngine.current.engineHandler);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Add Health",
        dynamicDescription = "Add $ health to $",
        icon = unitIcon)]
    [NumberArg(argType = ArgType.Value, defaultValue = 100)]
    [UnitArg(argType = ArgType.Preset, preset = LogicEngine.PRESET_ABILITY_OWNER, allowValue = false)]
    public void AddHealth(float amount, Unit unit)
    {
        Error(unit == null, "The unit to add health to is invalid.");
        unit.AddHealth(Mathf.Max(0, amount));
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Remove Health",
        dynamicDescription = "Remove $ health from $",
        icon = unitIcon)]
    [NumberArg(argType = ArgType.Value, defaultValue = 100)]
    [UnitArg(argType = ArgType.Preset, preset = LogicEngine.PRESET_ABILITY_OWNER, allowValue = false)]
    public void RemoveHealth(float amount, Unit unit)
    {
        Error(unit == null, "The unit to remove health from is invalid.");
        unit.RemoveHealth(Mathf.Max(0, amount));
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Add Resource",
        dynamicDescription = "Add $ resource to $",
        icon = unitIcon)]
    [NumberArg(argType = ArgType.Value, defaultValue = 100)]
    [UnitArg(argType = ArgType.Preset, preset = LogicEngine.PRESET_ABILITY_OWNER, allowValue = false)]
    public void AddResource(float amount, Unit unit)
    {
        Error(unit == null, "The unit to add resource to is invalid.");
        unit.AddResource(Mathf.Max(0, amount));
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Remove Resource",
        dynamicDescription = "Remove $ resource to $",
        icon = unitIcon)]
    [NumberArg(argType = ArgType.Value, defaultValue = 100)]
    [UnitArg(argType = ArgType.Preset, preset = LogicEngine.PRESET_ABILITY_OWNER, allowValue = false)]
    public void RemoveResource(float amount, Unit unit)
    {
        Error(unit == null, "The unit to remove resource from is invalid.");
        unit.RemoveResource(Mathf.Max(0, amount));
    }

    [VisualScriptingFunction(
        dropdownDescription = "Player/Add Gold",
        dynamicDescription = "Add $ gold to the player",
        icon = unitIcon)]
    [NumberArg(argType = ArgType.Value, defaultValue = 100)]
    public void AddGold(float amount)
    {
        Error(GameManager.player == null, "No player exists.");
        GameManager.player.AddGold(Mathf.Max(0, amount));
    }

    [VisualScriptingFunction(
        dropdownDescription = "Player/Remove Gold",
        dynamicDescription = "Remove $ gold to the player",
        icon = unitIcon)]
    [NumberArg(argType = ArgType.Value, defaultValue = 100)]
    public void RemoveGold(float amount)
    {
        Error(GameManager.player == null, "No player exists.");
        GameManager.player.RemoveGold(Mathf.Max(0, amount));
    }

    [VisualScriptingFunction(
        dropdownDescription = "Player/Add Experience",
        dynamicDescription = "Add $ experience to the player",
        icon = unitIcon)]
    [NumberArg(argType = ArgType.Value, defaultValue = 100)]
    public void AddExperience(float amount)
    {
        Error(GameManager.player == null, "No player exists.");
        GameManager.player.AddExperience(Mathf.Max(0, amount));
    }

    [VisualScriptingFunction(
        dropdownDescription = "Player/Remove Experience",
        dynamicDescription = "Remove $ experience to the player",
        icon = unitIcon)]
    [NumberArg(argType = ArgType.Value, defaultValue = 100)]
    public void RemoveExperience(float amount)
    {
        Error(GameManager.player == null, "No player exists.");
        GameManager.player.RemoveExperience(Mathf.Max(0, amount));
    }

    [VisualScriptingFunction(
        dropdownDescription = "Player/Set Experience",
        dynamicDescription = "Set the current experience of the player to $",
        icon = unitIcon)]
    [NumberArg(argType = ArgType.Temp)]
    public void SetExperience(float amount)
    {
        Error(GameManager.player == null, "No player exists.");
        GameManager.player.SetExperience(Mathf.Max(0, amount));
    }

    [VisualScriptingFunction(
        dropdownDescription = "Player/Add Levels",
        dynamicDescription = "Add $ level(s) to the player",
        icon = unitIcon)]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void AddLevels(float amount)
    {
        Error(GameManager.player == null, "No player exists.");
        GameManager.player.AddLevels((int)Mathf.Max(0, amount));
    }

    [VisualScriptingFunction(
        dropdownDescription = "Player/Remove Levels",
        dynamicDescription = "Remove $ level(s) from the player",
        icon = unitIcon)]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void RemoveLevels(float amount)
    {
        Error(GameManager.player == null, "No player exists.");
        GameManager.player.RemoveLevels((int)Mathf.Max(0, amount));
    }

    [VisualScriptingFunction(
        dropdownDescription = "Player/Set Level",
        dynamicDescription = "Set the player to level $",
        icon = unitIcon)]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void SetLevel(float amount)
    {
        Error(GameManager.player == null, "No player exists.");
        GameManager.player.SetLevel((int)Mathf.Max(1, amount));
    }

    [VisualScriptingFunction(
        dropdownDescription = "Player/Add Item",
        dynamicDescription = "Add $ to the player",
        icon = unitIcon)]
    [ItemArg(argType = ArgType.Temp)]
    public void AddItem(Item item)
    {
        Error(GameManager.player == null, "No player exists.");
        Error(item == null, "The item specified is invalid.");
        GameManager.player.inventory.AddItem(item.RollItem());
    }

    [VisualScriptingFunction(
        dropdownDescription = "Player/Remove Equipment",
        dynamicDescription = "Remove all equipment on the player",
        icon = unitIcon)]
    public void RemoveEquipment()
    {
        Error(GameManager.player == null, "No player exists.");
        for (int i = 0; i < GameManager.player.equipment.GetSlots(); i++)
        {
            GameManager.player.equipment.RemoveItemAtID(i);
        }
    }

    [VisualScriptingFunction(
        dropdownDescription = "Player/Equip Item",
        dynamicDescription = "Have the player equip $",
        icon = unitIcon)]
    [ItemArg(argType = ArgType.Temp)]
    public void EquipItem(Item item)
    {
        Error(item == null, "The specified item is invalid.");
        item = item.RollItem();
        if (item.itemType == Item.ItemType.Helmet)
            GameManager.player.equipment.AddItemAtID(item, 0);

        else if (item.itemType == Item.ItemType.ChestPlate)
            GameManager.player.equipment.AddItemAtID(item, 1);

        else if (item.itemType == Item.ItemType.Trinket)
            GameManager.player.equipment.AddItemAtID(item, 2);

        else if (item.itemType == Item.ItemType.WeaponPiece1)
            GameManager.player.equipment.AddItemAtID(item, 3);

        else if (item.itemType == Item.ItemType.WeaponPiece2)
            GameManager.player.equipment.AddItemAtID(item, 4);

        else if (item.itemType == Item.ItemType.WeaponPiece3)
            GameManager.player.equipment.AddItemAtID(item, 5);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Player/Remove Item",
        dynamicDescription = "Remove $ to the player",
        icon = unitIcon)]
    [ItemArg(argType = ArgType.Temp)]
    public void RemoveItem(Item item)
    {
        Error(GameManager.player == null, "No player exists.");
        Error(item == null, "The specified item is invalid.");
        GameManager.player.inventory.RemoveItem(item);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Play Animation",
        dynamicDescription = "Play $ animation on $",
        icon = unitIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Animation Name")]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public void PlayUnitAnimation(string animation, Unit unit)
    {
        Error(unit == null, "The specified unit does not exist.");
        unit.PlayAnimation(animation);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Audio/Play Sound",
        dynamicDescription = "Play $ at $ volume",
        icon = soundIcon)]
    [AudioClipArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Value, defaultValue = 100, suffix = "%")]
    public void Play2DSound(AudioClip clip, float volume)
    {
        Error(clip == null, "The specified audio clip was invalid.");
        Error(volume < 0, "The specified volume was invalid (less than zero).");
        volume /= 100.0f;
        GameManager.music.PlaySound(clip, volume);
    }

    public void Play3DSound(AudioClip clip, Vector3 position)
    {

    }

    [VisualScriptingFunction(
        dropdownDescription = "Feedback/Shake Screen",
        dynamicDescription = "Shake the screen with a strength of $",
        icon = soundIcon)]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void ShakeScreen(float strength)
    {
        ScreenShakeEffect effect = Camera.main.GetOrAddComponent<ScreenShakeEffect>();
        Error(effect == null, "Error trying to shake the camera.");
        effect.shakeStrength += (strength * 0.15f);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Feedback/Flash Color on Unit",
        dynamicDescription = "Flash $ on $ for $",
        icon = soundIcon)]
    [ColorArg(argType = ArgType.Temp)]
    [UnitArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Value, defaultValue = 1, suffix = "s")]
    public void ColorFlash(Color color, Unit unit, float time)
    {
        Error(unit == null, "The specified unit was invalid.");
        if (unit.GetComponent<FlashTextureEffect>() != null) return;
        unit.AddComponent<FlashTextureEffect>().Setup(color, time);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Audio/Play Music",
        dynamicDescription = "Change Game Music to $",
        icon = soundIcon)]
    [AudioClipArg(argType = ArgType.Temp)]
    public void ChangeGameMusic(AudioClip clip)
    {
        Error(clip == null, "The specified audio clip was invalid.");
        GameManager.music.FadeIntoNewClip(clip);
    }

    

    /*
    public void HaveUnitDamageUnit (Unit damagingUnit, float amount, Unit damagedUnit)
    {
        if (damagingUnit == null)
        {
            Error("The damaging unit was invalid.");
            return;
        }
        if (damagedUnit == null)
        {
            Error("The damaging unit was invalid.");
            return;
        }

        damagingUnit.DamageOtherUnit(damagedUnit, amount / 100.0f, LogicEngine.current.engineHandler);
    }
    */

    public void HaveUnitDamageUnits(Unit damagingUnit, float amount, UnitGroup unitGroup)
    {
        Error(damagingUnit == null, "The damaging unit was invalid.");
        Error(unitGroup == null, "The damaged unit list was invalid.");
        damagingUnit.DamageOtherUnits(unitGroup.GetUnits(), amount / 100.0f, LogicEngine.current.engineHandler);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Stun Unit",
        dynamicDescription = "Stun $ for $",
        icon = unitIcon)]
    [UnitArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Value, defaultValue = 2, suffix = "s")]
    
    public void StunUnit(Unit unit, float duration)
    {
        Error(unit == null, "The specified unit was invalid.");
        IVisualCodeHandler engine = LogicEngine.current.engineHandler;
        unit.Stun(duration, engine.GetOwner(), engine);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Stun Group of Units",
        dynamicDescription = "Stun $ for $",
        icon = unitIcon)]
    [UnitGroupArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 2, suffix = "s")]
    public void StunUnits(UnitGroup unitGroup, float duration)
    {
        Error(unitGroup == null, "The unit list was invalid.");
        IVisualCodeHandler engine = LogicEngine.current.engineHandler;
        Unit stunningUnit = engine.GetOwner();
        foreach (Unit unit in unitGroup.unitList)
            if (unit != null)
                unit.Stun(duration, stunningUnit, engine);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Deal Damage to Unit",
        dynamicDescription = "Deal $ attack damage to $",
        icon = unitIcon)]
    [NumberArg(argType = ArgType.Value, defaultValue = 100, suffix = "%")]
    [UnitArg(argType = ArgType.Temp)]
    public void HaveUnitDamageUnit2(float amount, Unit damagedUnit)
    {
        Error(damagedUnit == null, "The damaged unit was invalid.");
        IVisualCodeHandler engine = LogicEngine.current.engineHandler;
        engine.GetOwner().DamageOtherUnit(damagedUnit, amount / 100.0f, engine);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Deal Damage to Unit Group",
        dynamicDescription = "Deal $ attack damage to $",
        icon = unitIcon)]
    [NumberArg(argType = ArgType.Value, defaultValue = 100, suffix = "%")]
    [UnitGroupArg(argType = ArgType.Temp, allowValue = false)]
    public void HaveUnitDamageUnits2(float amount, UnitGroup unitGroup)
    {
        Error(unitGroup == null, "The damaged unit list was invalid.");
        IVisualCodeHandler engine = LogicEngine.current.engineHandler;
        engine.GetOwner().DamageOtherUnits(unitGroup.GetUnits(), amount / 100.0f, engine);
    }


    public void PlayFeedbackAtPoint(GameFeedback feedback, Vector3 point)
    {
        Error(feedback == null, "The specified feedback was invalid.");
        feedback.ActivateFeedback(null, null, point);
    }

    public void PlayFeedbackOnUnit(GameFeedback feedback, Unit unit)
    {
        Error(feedback == null, "The specified feedback was invalid.");
        feedback.ActivateFeedback(unit.gameObject, unit.gameObject, unit.transform.position);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Feedback/Create Circular Effect Guide",
        dynamicDescription = "Create a circular guide at $ with radius $ for $",
        icon = unitIcon)]
    [VectorArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Value, defaultValue = 2, suffix = "m")]
    [NumberArg(argType = ArgType.Temp, tempLabel = "Duration", suffix = "m")]
    public void CreateCircleGuide(Vector3 location, float radius, float duration)
    {
        CircleEffectGuide.Spawn(location, radius, duration);
    }

    public void CreateLineGuide(Vector3 location1, Vector3 location2, float width, float duration)
    {
        LineEffectGuide.Spawn(location1, location2, width, duration);
    }

    public void CreateLineGuide2(Unit unit, float width, float length, float duration)
    {
        LineEffectGuide.Spawn(unit, width, length, duration);
    }

    public void CreateArcGuide(float arc, Unit unit, float radius, float duration)
    {
        ArcEffectGuide.Spawn(arc, unit, radius, duration);
    }

    public void PlayFeedbackOnUnits(GameFeedback feedback, UnitGroup unitGroup)
    {
        Error(unitGroup == null, "The specified unit list was invalid.");
        foreach (Unit unit in unitGroup)
        {
            if (unit != null)
            {
                feedback.ActivateFeedback(unit.gameObject, unit.gameObject, unit.transform.position);
            }
        }
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Remove Buff",
        dynamicDescription = "Remove buff named $ from $",
        icon = unitIcon)]
    [StringArg(argType = ArgType.Temp, allowPreset = false, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public void RemoveBuff(string buffName, Unit unit)
    {
        Error(unit == null, "The specified unit was invalid.");
        unit.stats.RemoveBuffWithLabel(buffName);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Increase Stat/Increase Stat on Unit by Value",
        dynamicDescription = "Increase $ of $ by $ for $ (Buff Name: $ | Max Stacks: $)",
        icon = unitIcon)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.Buffs, allowPreset = false, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Value, defaultValue = 5, suffix = "s")]
    [StringArg(argType = ArgType.Value, defaultValue = "None")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void IncreaseStatValue(string modifier, Unit unit, float mod, float duration, string buff, float maxStacks)
    {
        Error(unit == null, "The specified unit was invalid.");
        Stat stat = StatExtensions.LabelToStat(modifier);
        if (duration > 0)
            unit.stats[stat].AddTimedValueModifier(Mathf.Max(0, mod), duration, buff, (int)maxStacks);
        else
            unit.stats[stat].AddValueModifier(Mathf.Max(0, mod), buff, (int)maxStacks);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Increase Stat/Increase Stat on Unit by Percent",
        dynamicDescription = "Increase $ of $ by $ for $ (Buff Name: $ | Max Stacks: $)",
        icon = unitIcon)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.Buffs, allowPreset = false, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Temp, tempLabel = "Percent", suffix = "%")]
    [NumberArg(argType = ArgType.Value, defaultValue = 5, suffix = "s")]
    [StringArg(argType = ArgType.Value, defaultValue = "None")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void IncreaseStatPercent(string modifier, Unit unit, float mod, float duration, string buff, float maxStacks)
    {
        Error(unit == null, "The specified unit was invalid.");
        Stat stat = StatExtensions.LabelToStat(modifier);
        if (duration > 0)
            unit.stats[stat].AddTimedPercentageModifier(Mathf.Max(0, mod / 100.0f), duration, buff, (int)maxStacks);
        else
            unit.stats[stat].AddPercentageModifier(Mathf.Max(0, mod / 100.0f), buff, (int)maxStacks);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Decrease Stat/Decrease Stat on Unit by Value",
        dynamicDescription = "Decrease $ of $ by $ for $ (Buff Name: $ | Max Stacks: $)",
        icon = unitIcon)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.Buffs, allowPreset = false, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Value, defaultValue = 5, suffix = "s")]
    [StringArg(argType = ArgType.Value, defaultValue = "None")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void DecreaseStatValue(string modifier, Unit unit, float mod, float duration, string buff, float maxStacks)
    {
        Error(unit == null, "The specified unit was invalid.");
        Stat stat = StatExtensions.LabelToStat(modifier);
        if (duration > 0)
            unit.stats[stat].AddTimedValueModifier(Mathf.Min(-mod, 0), duration, buff, (int)maxStacks);
        else
            unit.stats[stat].AddValueModifier(Mathf.Min(-mod, 0), buff, (int)maxStacks);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Decrease Stat/Decrease Stat on Unit by Percent",
        dynamicDescription = "Decrease $ of $ by $ for $ (Buff Name: $ | Max Stacks: $)",
        icon = unitIcon)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.Buffs, allowPreset = false, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Temp, tempLabel = "Percent", suffix = "%")]
    [NumberArg(argType = ArgType.Value, defaultValue = 5, suffix = "s")]
    [StringArg(argType = ArgType.Value, defaultValue = "None")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void DecreaseStatPercent(string modifier, Unit unit, float mod, float duration, string buff, float maxStacks)
    {
        Error(unit == null, "The specified unit was invalid.");
        Stat stat = StatExtensions.LabelToStat(modifier);
        if (duration > 0)
            unit.stats[stat].AddTimedPercentageModifier(Mathf.Min(0, -mod / 100.0f), duration, buff, (int)maxStacks);
        else
            unit.stats[stat].AddPercentageModifier(Mathf.Min(0, -mod / 100.0f), buff, (int)maxStacks);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Increase Stat/Increase Stat of Unit Group by Value",
        dynamicDescription = "Increase $ of $ by $ for $ (Buff Name: $ | Max Stacks: $)",
        icon = unitIcon)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.Buffs, allowPreset = false, allowFunction = false)]
    [UnitGroupArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Value, defaultValue = 5, suffix = "s")]
    [StringArg(argType = ArgType.Value, defaultValue = "None")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void IncreaseStatValueUnitGroup(string modifier, UnitGroup unitGroup, float mod, float duration, string buff, float maxStacks)
    {
        Error(unitGroup == null, "The specified unit list was invalid.");
        Stat stat = StatExtensions.LabelToStat(modifier);
        foreach (Unit unit in unitGroup)
            IncreaseStatValue(modifier, unit, mod, duration, buff, maxStacks);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Increase Stat/Increase Stat of Unit Group by Percent",
        dynamicDescription = "Increase $ of $ by $ for $ (Buff Name: $ | Max Stacks: $)",
        icon = unitIcon)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.Buffs, allowPreset = false, allowFunction = false)]
    [UnitGroupArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Temp, tempLabel = "Percent", suffix = "%")]
    [NumberArg(argType = ArgType.Value, defaultValue = 5, suffix = "s")]
    [StringArg(argType = ArgType.Value, defaultValue = "None")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void IncreaseStatPercentUnitGroup(string modifier, UnitGroup unitGroup, float mod, float duration, string buff, float maxStacks)
    {
        Error(unitGroup == null, "The specified unit list was invalid.");
        Stat stat = StatExtensions.LabelToStat(modifier);
        foreach (Unit unit in unitGroup)
            IncreaseStatPercent(modifier, unit, mod, duration, buff, maxStacks);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Decrease Stat/Decrease Stat of Unit Group by Value",
        dynamicDescription = "Decrease $ of $ by $ for $ (Buff Name: $ | Max Stacks: $)",
        icon = unitIcon)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.Buffs, allowPreset = false, allowFunction = false)]
    [UnitGroupArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Value, defaultValue = 5, suffix = "s")]
    [StringArg(argType = ArgType.Value, defaultValue = "None")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void DecreaseStatValueUnitGroup(string modifier, UnitGroup unitGroup, float mod, float duration, string buff, float maxStacks)
    {
        Error(unitGroup == null, "The specified unit list was invalid.");
        Stat stat = StatExtensions.LabelToStat(modifier);
        foreach (Unit unit in unitGroup)
            DecreaseStatValue(modifier, unit, mod, duration, buff, maxStacks);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Decrease Stat/Decrease Stat of Unit Group by Percent",
        dynamicDescription = "Decrease $ of $ by $ for $ (Buff Name: $ | Max Stacks: $)",
        icon = unitIcon)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.Buffs, allowPreset = false, allowFunction = false)]
    [UnitGroupArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Temp, tempLabel = "Percent", suffix = "%")]
    [NumberArg(argType = ArgType.Value, defaultValue = 5, suffix = "s")]
    [StringArg(argType = ArgType.Value, defaultValue = "None")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void DecreaseStatPercentUnitGroup(string modifier, UnitGroup unitGroup, float mod, float duration, string buff, float maxStacks)
    {
        Error(unitGroup == null, "The specified unit list was invalid.");
        Stat stat = StatExtensions.LabelToStat(modifier);
        foreach (Unit unit in unitGroup)
            DecreaseStatPercent(modifier, unit, mod, duration, buff, maxStacks);
    }

    public static Projectile lastCreatedProjectile = null;

    [VisualScriptingFunction(
        dropdownDescription = "Projectile/Spawn Projectile",
        dynamicDescription = "Spawn $ at $",
        icon = projectileIcon)]
    [ProjectileArg(argType = ArgType.Temp, allowPreset = false, allowFunction = false)]
    [VectorArg(argType = ArgType.Temp)]
    public void SpawnProjectile(Projectile projectile, Vector3 position)
    {
        Error(projectile == null, "The specified projectile was invalid.");
        Unit unit = LogicEngine.current.GetOwner();
        if (projectile == null) return;
        Quaternion rotation = Quaternion.identity;
        if (unit != null) rotation = unit.transform.rotation;
        GameObject obj = GameObject.Instantiate(projectile.gameObject, position, rotation);
        Projectile p = obj.GetComponent<Projectile>();
        p.Setup(LogicEngine.current.engineHandler);
        lastCreatedProjectile = p;
    }

    [VisualScriptingFunction(
        dropdownDescription = "Projectile/Move Projectile Forwards",
        dynamicDescription = "Move $ forwards at $",
        icon = projectileIcon)]
    [ProjectileArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 3, suffix = "m/s")]
    public void MoveForwardAtSpeed(Projectile projectile, float speed)
    {
        Error(projectile == null, "The specified projectile was invalid.");
        projectile.MoveProjectileForwards(speed);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Projectile/Rotate Projectile",
        dynamicDescription = "Rotate $ by $",
        icon = projectileIcon)]
    [ProjectileArg(argType = ArgType.Preset, preset = LogicEngine.PRESET_PROJECTILE_LAST_CREATED, allowValue = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 30, suffix = "�")]
    public void RotateProjectile(Projectile projectile, float amount)
    {
        Error(projectile == null, "The specified projectile was invalid.");
        projectile.Rotate(amount);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Projectile/Face Projectile Towards Point",
        dynamicDescription = "Face $ towards $",
        icon = projectileIcon)]
    [ProjectileArg(argType = ArgType.Preset, preset = LogicEngine.PRESET_PROJECTILE_LAST_CREATED, allowValue = false)]
    [VectorArg(argType = ArgType.Temp)]
    public void FaceProjectileTowardsPoint(Projectile projectile, Vector3 point)
    {
        Error(projectile == null, "The specified projectile was invalid.");
        projectile.FaceProjectileTowardsPoint(point);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Projectile/Set Lifetime",
        dynamicDescription = "Set max lifetime of $ to $",
        icon = projectileIcon)]
    [ProjectileArg(argType = ArgType.Preset, preset = LogicEngine.PRESET_PROJECTILE_LAST_CREATED, allowValue = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 2, suffix = "s")]
    public void SetProjectileLifetime(Projectile projectile, float lifetime)
    {
        Error(projectile == null, "The specified projectile was invalid.");
        projectile.SetLifetime(lifetime);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Projectile/Destroy Projectile",
        dynamicDescription = "Destroy $",
        icon = projectileIcon)]
    [ProjectileArg(argType = ArgType.Temp, allowValue = false)]
    public void DestroyProjectile(Projectile projectile)
    {
        Error(projectile == null, "The specified projectile was invalid.");
        if (projectile != null)
            projectile.DestroyProjectile();
    }

    [VisualScriptingFunction(
        dropdownDescription = "Projectile/Move Projectile Towards Point",
        dynamicDescription = "Move $ towards $ at $",
        icon = projectileIcon)]
    [ProjectileArg(argType = ArgType.Preset, preset = LogicEngine.PRESET_PROJECTILE_LAST_CREATED, allowValue = false)]
    [VectorArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Value, defaultValue = 3, suffix = "m/s")]
    public void MoveTowardsPointAtSpeed(Projectile projectile, Vector3 point, float speed)
    {
        Error(projectile == null, "The specified projectile was invalid.");
        projectile.MoveProjectileTowardsPoint(point, speed);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Projectile/Move Projectile Towards Point in Arc",
        dynamicDescription = "Move $ towards $ in $ with $ arc",
        icon = projectileIcon)]
    [ProjectileArg(argType = ArgType.Preset, preset = LogicEngine.PRESET_PROJECTILE_LAST_CREATED, allowValue = false)]
    [VectorArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Value, defaultValue = 3, suffix = "s")]
    [NumberArg(argType = ArgType.Value, defaultValue = 2, suffix = "m")]
    public void MoveTowardsPointInArc(Projectile projectile, Vector3 point, float time, float arcHeight)
    {
        Error(projectile == null, "The specified projectile was invalid.");
        projectile.MoveProjectileInArcTowardsPoint(point, time, arcHeight);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Teleport",
        dynamicDescription = "Teleport $ to $",
        icon = unitIcon)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [VectorArg(argType = ArgType.Temp)]
    public void TeleportUnit(Unit unit, Vector3 newPosition)
    {
        Error(unit == null, "The specified unit was invalid.");
        unit.Teleport(newPosition);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Move Over Time",
        dynamicDescription = "Move $ to $ over $",
        icon = unitIcon)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [VectorArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Value, defaultValue = 1, suffix = "s")]
    public void MoveUnitOverTime(Unit unit, Vector3 newPosition, float duration)
    {
        Error(unit == null, "The specified unit was invalid.");
        unit.MoveOverTime(newPosition, duration);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Region/Destroy Region",
        dynamicDescription = "Destroy regions named $",
        icon = regionIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Region Name")]
    public void DestroyRegions(string regionName)
    {
        Error(regionName.Length == 0, "The region name cannot be empty.");
        Region.DestroyAllRegionsWithName(regionName);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Quests/Create Quest",
        dynamicDescription = "Give the player a quest named $",
        icon = questIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Quest Name")]
    public void CreateQuest2(string questName)
    {
        Error(questName.Length == 0, "The quest name cannot be empty.");
        Quest quest = new Quest(questName, questName);
        GameManager.quests.AddQuest(quest);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Quests/Add Quest Requirement",
        dynamicDescription = "Add $ requirement to $ with $ progress increments",
        icon = questIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Requirement")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Quest Name")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void AddQuestRequirement2(string requirement, string questName, float increments)
    {
        Error(requirement.Length == 0, "The requirement name cannot be empty.");
        Quest quest = GameManager.quests.GetQuest(questName);
        Error(quest == null, "The player does not have the specified quest.");
        quest.AddCompletionRequirement(requirement, requirement, (int)increments);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Quests/Add Quest Reward",
        dynamicDescription = "Add reward labeled $ to quest $",
        icon = questIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Reward")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Quest Name")]
    public void AddQuestReward2(string reward, string questName)
    {
        Error(reward.Length == 0, "The reward name cannot be empty.");
        Quest quest = GameManager.quests.GetQuest(questName);
        Error(quest == null, "The player does not have the specified quest.");
        GameManager.quests.GetQuest(questName).SetReward(reward);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Quests/Set Quest Progress",
        dynamicDescription = "Set quest progress of $ to $",
        icon = questIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Quest Name")]
    [NumberArg(argType = ArgType.Temp)]
    public void SetQuestRequirementProgress2(string questName, float progress)
    {
        Quest quest = GameManager.quests.GetQuest(questName);
        Error(quest == null, "The player does not have the specified quest.");
        quest.SetProgress("None", (int)progress);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Quests/Modify Quest Progress",
        dynamicDescription = "Modify quest progress of $ by $",
        icon = questIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Quest Name")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void ModifyQuestRequirementProgress2(string questName, float progress)
    {
        Quest quest = GameManager.quests.GetQuest(questName);
        Error(quest == null, "The player does not have the specified quest.");
        quest.IncrementProgress("None", (int)progress);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Quests/Set Requirement Progress",
        dynamicDescription = "Set quest progress of $ on $ to $",
        icon = questIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Requirement Name")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Quest Name")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void SetSpecificQuestRequirementProgress2(string requirement, string questName, float progress)
    {
        Quest quest = GameManager.quests.GetQuest(questName);
        Error(quest == null, "The player does not have the specified quest.");
        quest.SetProgress(requirement, (int)progress);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Quests/Modify Requirement Progress",
        dynamicDescription = "Modify quest progress of $ on $ by $",
        icon = questIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Requirement Name")]
    [StringArg(argType = ArgType.Temp, tempLabel = "Quest Name")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void ModifySpecificQuestRequirementProgress2(string requirement, string questName, float progress)
    {
        Quest quest = GameManager.quests.GetQuest(questName);
        Error(quest == null, "The player does not have the specified quest.");
        quest.IncrementProgress(requirement, (int)progress);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Pickups/Spawn Item Pickup",
        dynamicDescription = "Spawn an item pickup at $ containing $",
        icon = questIcon)]
    [VectorArg(argType = ArgType.Temp, tempLabel = "Location")]
    [ItemArg(argType = ArgType.Temp)]
    public void SpawnItemDrop(Vector3 location, Item item)
    {
        Error(item == null, "The specified item is invalid.");
        ItemPickup.Spawn(location, item);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Pickups/Spawn Gold Pickup",
        dynamicDescription = "Spawn a gold pickup at $ containing $ gold",
        icon = questIcon)]
    [VectorArg(argType = ArgType.Temp, tempLabel = "Location")]
    [NumberArg(argType = ArgType.Temp)]
    public void SpawnGoldDrop(Vector3 location, float goldAmount)
    {
        GoldPickup.Spawn(location, (int)goldAmount);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Pickups/Spawn Health Pickup",
        dynamicDescription = "Spawn a health pickup at $",
        icon = questIcon)]
    [VectorArg(argType = ArgType.Temp, tempLabel = "Location")]
    public void SpawnHealthDrop(Vector3 location)
    {
        HealthPickup.Spawn(location);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Game State/Win Game",
        dynamicDescription = "Have the player win the game",
        icon = gameIcon)]
    public void WinGame()
    {
        GameManager.instance.WinGame();
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Ability/Enable Ability on Unit",
        dynamicDescription = "Enable $ on $",
        icon = unitIcon)]
    [AbilityArg(argType = ArgType.Temp, allowFunction = false, allowPreset = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public void EnableAbility(Ability ability, Unit unit)
    {
        Error(ability == null, "The specified ability is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        for (int i = 0; i < unit.abilities.Count; i++)
        {
            if (unit.abilities[i].abilityName == ability.abilityName)
            {
                unit.abilities[i].Unlock();
                if (unit == GameManager.player) GameManager.ui.NotificationWindow.DisplayMessage("Ability Unlocked", ability.abilityName, ability.abilityIcon);
            }
        }
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Ability/Disable Ability on Unit",
        dynamicDescription = "Disable $ on $",
        icon = unitIcon)]
    [AbilityArg(argType = ArgType.Temp, allowFunction = false, allowPreset = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public void DisableAbility(Ability ability, Unit unit)
    {
        Error(ability == null, "The specified ability is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        for (int i = 0; i < unit.abilities.Count; i++)
        {
            if (unit.abilities[i].abilityName == ability.abilityName)
            {
                unit.abilities[i].Lock();
            }
        }
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Ability/Reduce Current Ability Cooldown",
        dynamicDescription = "Reduce current cooldown of $ on $ by $",
        icon = unitIcon)]
    [AbilityArg(argType = ArgType.Temp, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 1, suffix = "s")]
    public void ReduceAbilityCooldown(Ability ability, Unit unit, float amount)
    {
        Error(ability == null, "The specified ability is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        unit.ReduceAbilityCooldown(ability, amount);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Ability/Add Ability",
        dynamicDescription = "Add $ to $",
        icon = unitIcon)]
    [AbilityArg(argType = ArgType.Temp, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public void AddAbilityToUnit(Ability ability, Unit unit)
    {
        Error(ability == null, "The specified ability is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        unit.AddAbility(ability);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Ability/Remove Ability",
        dynamicDescription = "Remove $ from $",
        icon = unitIcon)]
    [AbilityArg(argType = ArgType.Temp, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public void RemoveAbilityFromUnit(Ability ability, Unit unit)
    {
        Error(ability == null, "The specified ability is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        unit.RemoveAbility(ability);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Ability/Replace Ability",
        dynamicDescription = "Replace $ with $ on $",
        icon = unitIcon)]
    [AbilityArg(argType = ArgType.Temp, allowFunction = false)]
    [AbilityArg(argType = ArgType.Temp, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public void ReplaceAbilityOnUnit(Ability oldAbility, Ability newAbility, Unit unit)
    {
        Error(oldAbility == null, "The old ability is invalid.");
        Error(newAbility == null, "The new ability is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        unit.ReplaceAbility(oldAbility, newAbility);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Ability/Add Cost Modifier to Ability",
        dynamicDescription = "$ cost of $ on $ by $ (Buff Name: $ | Max Stacks: $)",
        icon = unitIcon)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.IncreaseDecrease, allowFunction = false)]
    [AbilityArg(argType = ArgType.Temp, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 50, suffix = "%")]
    [StringArg(argType = ArgType.Value, defaultValue = "None")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void AddAbilityCostModifierToUnit(string increaseDecrease, Ability ability, Unit unit, float modifier, string buffName = "None", float maxStacks = 99)
    {
        Error(ability == null, "The specified ability is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        if (increaseDecrease == "Decrease") modifier *= -1;
        unit.AddAbilityCostModifier(ability, modifier, buffName, (int)maxStacks);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Ability/Add Cooldown Modifier to Ability",
        dynamicDescription = "$ cooldown of $ on $ by $ (Buff Name: $ | Max Stacks: $)",
        icon = unitIcon)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.IncreaseDecrease, allowFunction = false)]
    [AbilityArg(argType = ArgType.Temp, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 50, suffix = "%")]
    [StringArg(argType = ArgType.Value, defaultValue = "None")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void AddAbilityCooldownModifierToUnit(string increaseDecrease, Ability ability, Unit unit, float modifier, string buffName = "None", float maxStacks = 99)
    {
        Error(ability == null, "The specified ability is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        if (increaseDecrease == "Decrease") modifier *= -1;
        unit.AddAbilityCooldownModifier(ability, modifier, buffName, (int)maxStacks);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Ability/Add Damage Modifier to Ability",
        dynamicDescription = "$ damage of $ on $ by $ (Buff Name: $ | Max Stacks: $)",
        icon = unitIcon)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.IncreaseDecrease, allowFunction = false)]
    [AbilityArg(argType = ArgType.Temp, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 50, suffix = "%")]
    [StringArg(argType = ArgType.Value, defaultValue = "None")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void AddAbilityDamageModifierToUnit(string increaseDecrease, Ability ability, Unit unit, float modifier, string buffName = "None", float maxStacks = 99)
    {
        Error(ability == null, "The specified ability is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        if (increaseDecrease == "Decrease") modifier *= -1;
        unit.AddAbilityDamageModifier(ability, modifier / 100.0f, buffName, (int)maxStacks);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Ability/Add Timed Cost Modifier to Ability",
        dynamicDescription = "$ cost of $ on $ by $ for $ (Buff Name: $ | Max Stacks: $)",
        icon = unitIcon)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.IncreaseDecrease, allowFunction = false)]
    [AbilityArg(argType = ArgType.Temp, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 50, suffix = "%")]
    [NumberArg(argType = ArgType.Value, defaultValue = 5, suffix = "s")]
    [StringArg(argType = ArgType.Value, defaultValue = "None")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void AddTimedAbilityCostModifierToUnit(string increaseDecrease, Ability ability, Unit unit, float modifier, float duration, string buffName = "None", float maxStacks = 99)
    {
        Error(ability == null, "The specified ability is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        if (increaseDecrease == "Decrease") modifier *= -1;
        unit.AddTimedAbilityCostModifier(ability, modifier, duration, buffName, (int)maxStacks);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Ability/Add Timed Cooldown Modifier to Ability",
        dynamicDescription = "$ cooldown of $ on $ by $ for $ (Buff Name: $ | Max Stacks: $)",
        icon = unitIcon)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.IncreaseDecrease, allowFunction = false)]
    [AbilityArg(argType = ArgType.Temp, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 50, suffix = "%")]
    [NumberArg(argType = ArgType.Value, defaultValue = 5, suffix = "s")]
    [StringArg(argType = ArgType.Value, defaultValue = "None")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void AddTimedAbilityCooldownModifierToUnit(string increaseDecrease, Ability ability, Unit unit, float modifier, float duration, string buffName = "None", float maxStacks = 99)
    {
        Error(ability == null, "The specified ability is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        if (increaseDecrease == "Decrease") modifier *= -1;
        unit.AddTimedAbilityCooldownModifier(ability, modifier, duration, buffName, (int)maxStacks);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Ability/Add Timed Damage Modifier to Ability",
        dynamicDescription = "$ damage of $ on $ by $ for $ (Buff Name: $ | Max Stacks: $)",
        icon = unitIcon)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.IncreaseDecrease, allowFunction = false)]
    [AbilityArg(argType = ArgType.Temp, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 50, suffix = "%")]
    [NumberArg(argType = ArgType.Value, defaultValue = 5, suffix = "s")]
    [StringArg(argType = ArgType.Value, defaultValue = "None")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1)]
    public void AddTimedAbilityDamageModifierToUnit(string increaseDecrease, Ability ability, Unit unit, float modifier, float duration, string buffName = "None", float maxStacks = 99)
    {
        Error(ability == null, "The specified ability is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        if (increaseDecrease == "Decrease") modifier *= -1;
        unit.AddTimedAbilityDamageModifier(ability, modifier / 100.0f, duration, buffName, (int)maxStacks);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Unit/Ability/Remove Ability Modifier",
        dynamicDescription = "Remove buffs for $ on $ named $",
        icon = unitIcon)]
    [AbilityArg(argType = ArgType.Temp, allowFunction = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [StringArg(argType = ArgType.Temp)]
    public void RemoveAbilityModifier(Ability ability, Unit unit, string buffName)
    {
        Error(ability == null, "The specified ability is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        unit.RemoveAbilityBuffModifiers(ability, buffName);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Feedback/Play Effect at Location",
        dynamicDescription = "Play $ effect at $ for $ at $ scale",
        icon = effectIcon)]
    [EffectArg(argType = ArgType.Temp, allowPreset = false, allowFunction = false)]
    [VectorArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 2, suffix = "s")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1.0f, suffix = "x")]
    public void SpawnEffectAtLocation(CustomVisualEffect effect, Vector3 position, float duration, float scale)
    {
        Error(effect == null, "The specified effect is invalid.");
        GameObject obj = ObjectPooler.InstantiatePooled(effect.gameObject, position, Quaternion.identity);
        obj.transform.localScale = effect.transform.localScale * scale;
        if (duration > 0) obj.GetComponent<CustomVisualEffect>().DestroyAfter(duration);
    }

    // TODO: USED BY OTHER METHODS.
    private void SpawnEffectOnUnit(CustomVisualEffect effect, Unit unit, float duration, float scale)
    {
        Error(effect == null, "The specified effect is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        GameObject obj = ObjectPooler.InstantiatePooled(effect.gameObject, unit.transform.position, unit.transform.rotation);
        obj.name = effect.name;
        obj.transform.localScale = effect.transform.localScale * scale;
        if (duration > 0) obj.GetComponent<CustomVisualEffect>().DestroyAfter(duration);
        obj.transform.SetParent(unit.transform);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Feedback/Play Effect on Unit",
        dynamicDescription = "$ $ effect on $ for $ at $ scale",
        icon = effectIcon)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.PlayOptions, allowFunction = false, allowPreset = false)]
    [EffectArg(argType = ArgType.Temp)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 2, suffix = "s")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1.0f, suffix = "x")]
    public void SpawnEffectOnUnit(string action, CustomVisualEffect effect, Unit unit, float duration, float scale)
    {
        Error(effect == null, "The specified effect is invalid.");
        Error(unit == null, "The specified unit is invalid.");
        if (action == "Play")
        {
            SpawnEffectOnUnit(effect, unit, duration, scale);
        }
        else if (action == "Stop")
        {
            Transform existing = unit.transform.Find(effect.name);
            if (existing != null)
            {
                ObjectPooler.DestroyPooled(unit.transform.Find(effect.name).gameObject);
            }
        }
        else if (action == "Play or Refresh")
        {
            Transform existing = unit.transform.Find(effect.name);
            if (existing != null)
            {
                existing.GetComponent<CustomVisualEffect>().DestroyAfter(duration);
            }
            else
            {
                SpawnEffectOnUnit(effect, unit, duration, scale);
            }
        }
    }

    [VisualScriptingFunction(
        dropdownDescription = "Feedback/Play Effect on Unit Group",
        dynamicDescription = "$ $ effect on $ for $ at $ scale",
        icon = effectIcon)]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.PlayOptions, allowFunction = false, allowPreset = false)]
    [EffectArg(argType = ArgType.Temp)]
    [UnitGroupArg(argType = ArgType.Temp, allowValue = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 2, suffix = "s")]
    [NumberArg(argType = ArgType.Value, defaultValue = 1.0f, suffix = "x")]
    public void SpawnEffectOnUnitGroup(string action, CustomVisualEffect effect, UnitGroup unitGroup, float duration, float scale)
    {
        Error(effect == null, "The specified effect is invalid.");
        Error(unitGroup == null, "The specified unit group is invalid.");
        foreach (Unit unit in unitGroup)
        {
            SpawnEffectOnUnit(action, effect, unit, duration, scale);
        }
    }

    [VisualScriptingFunction(
        dropdownDescription = "UI Actions/Show Debug Message",
        dynamicDescription = "Show a debug message printing $",
        icon = uiIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Message")]
    public void ShowDebugMessage(string message)
    {
        Debug.Log(message);
    }

    [VisualScriptingFunction(
        dropdownDescription = "UI Actions/Show In-World Status Message",
        dynamicDescription = "Show a status message at $ with color $ printing $",
        icon = uiIcon)]
    [VectorArg(argType = ArgType.Temp)]
    [ColorArg(argType = ArgType.Temp)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Message")]
    public void ShowStatusMessage(Vector3 position, Color color, string message)
    {
        Error(message.Length == 0, "The message was empty.");
        StatusMessageUI.Spawn(position, message, color);
    }

    [VisualScriptingFunction(
        dropdownDescription = "UI Actions/Show Tutorial Message",
        dynamicDescription = "Show a tutorial message printing $ for $ seconds",
        icon = uiIcon)]
    [StringArg(argType = ArgType.Temp)]
    [NumberArg(argType = ArgType.Value, defaultValue = 6)]
    public void ShowTutorialMessage(string message, float duration = 6.0f)
    {
        Error(message.Length == 0, "The message was empty.");
        GameManager.ui.MessageWindow.DisplayMessage(message, duration);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Variables/Set Number Variable",
        dynamicDescription = "Set script number variable named $ to $",
        icon = variableIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 0)]
    public void SetNumberVariable(string name, float value)
    {
        Error(name.Length == 0, "The specified variable name was invalid.");
        LogicEngine.current.SetLocalVariable(name, value);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Variables/Set Unit Group Variable",
        dynamicDescription = "Set Unit Group named $ to $",
        icon = variableIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    [UnitGroupArg(argType = ArgType.Temp, allowValue = false)]
    public void SetUnitGroupVariable(string name, UnitGroup value)
    {
        Error(value == null, "The specified unit group was invalid.");
        LogicEngine.current.SetLocalVariable(name, value);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Variables/Add Unit to Unit Group Variable",
        dynamicDescription = "Add $ to Unit Group Named $",
        icon = variableIcon)]
    [UnitArg(argType = ArgType.Temp)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    public void AddToUnitGroup(Unit unit, string name)
    {
        Error(unit == null, "The specified unit was invalid.");
        if (!LogicEngine.current.localVariables.ContainsKey(name)) return;
        ((UnitGroup)LogicEngine.current.localVariables[name]).AddUnit(unit);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Variables/Remove Unit from Unit Group Variable",
        dynamicDescription = "Remove $ from Unit Group Named $",
        icon = variableIcon)]
    [UnitArg(argType = ArgType.Temp)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    public void RemoveFromUnitGroup(Unit unit, string name)
    {
        Error(unit == null, "The specified unit was invalid.");
        Error(name.Length == 0, "The specified variable name was invalid.");
        if (!LogicEngine.current.localVariables.ContainsKey(name)) return;
        ((UnitGroup)LogicEngine.current.localVariables[name]).RemoveUnit(unit);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Variables/Add Unit to Global Unit Group Variable",
        dynamicDescription = "Add $ to Global Unit Group Named $",
        icon = variableIcon)]
    [UnitArg(argType = ArgType.Temp)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    public void AddToGlobalUnitGroup(Unit unit, string name)
    {
        Error(unit == null, "The specified unit was invalid.");
        Error(name.Length == 0, "The specified variable name was invalid.");
        if (!LogicEngine.globalVariables.ContainsKey(name)) return;
        ((UnitGroup)LogicEngine.globalVariables[name]).AddUnit(unit);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Variables/Remove Unit from Global Unit Group Variable",
        dynamicDescription = "Remove $ from Global Unit Group Named $",
        icon = variableIcon)]
    [UnitArg(argType = ArgType.Temp)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    public void RemoveFromGlobalUnitGroup(Unit unit, string name)
    {
        Error(unit == null, "The specified unit was invalid.");
        Error(name.Length == 0, "The specified variable name was invalid.");
        if (!LogicEngine.globalVariables.ContainsKey(name)) return;
        ((UnitGroup)LogicEngine.globalVariables[name]).RemoveUnit(unit);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Variables/Set Bool Variable",
        dynamicDescription = "Set script bool variable named $ to $",
        icon = variableIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    [BoolArg(argType = ArgType.Value, defaultValue = true)]
    public void SetBoolVariable(string name, bool value)
    {
        Error(name.Length == 0, "The specified variable name was invalid.");
        LogicEngine.current.SetLocalVariable(name, value);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Variables/Set Unit Variable",
        dynamicDescription = "Set script unit variable named $ to $",
        icon = variableIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public void SetUnitVariable(string name, Unit value)
    {
        Error(name.Length == 0, "The specified variable name was invalid.");
        LogicEngine.current.SetLocalVariable(name, value);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Variables/Set Vector Variable",
        dynamicDescription = "Set script vector variable named $ to $",
        icon = variableIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    [VectorArg(argType = ArgType.Temp)]
    public void SetVectorVariable(string name, Vector3 value)
    {
        Error(name.Length == 0, "The specified variable name was invalid.");
        LogicEngine.current.SetLocalVariable(name, value);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Variables/Set String Variable",
        dynamicDescription = "Set script string variable named $ to $",
        icon = variableIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    [StringArg(argType = ArgType.Temp)]
    public void SetStringVariable(string name, string value)
    {
        Error(name.Length == 0, "The specified variable name was invalid.");
        LogicEngine.current.SetLocalVariable(name, value);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Global Variables/Modify Global Number Variable",
        dynamicDescription = "Modify global number variable named $ by $",
        icon = variableIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 0)]
    public void ModifyNumberVariable(string name, float value)
    {
        Error(name.Length == 0, "The specified variable name was invalid.");
        Error(!LogicEngine.current.LocalVariableExists(name), "The specified variable does not exist.");
        float currentValue = LogicEngine.current.GetLocalVariable<float>(name);
        LogicEngine.current.SetLocalVariable(name, currentValue + value);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Global Variables/Modify Global Number Variable",
        dynamicDescription = "Modify global number variable named $ by $",
        icon = variableIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 0)]
    public void ModifyGlobalNumberVariable(string name, float value)
    {
        Error(name.Length == 0, "The specified variable name was invalid.");
        Error(!LogicEngine.GlobalVariableExists(name), "The specified variable does not exist.");
        float currentValue = LogicEngine.GetGlobalVariable<float>(name);
        LogicEngine.SetGlobalVariable(name, currentValue + value);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Global Variables/Set Global Number Variable",
        dynamicDescription = "Set global number variable named $ to $",
        icon = variableIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    [NumberArg(argType = ArgType.Value, defaultValue = 0)]
    public void SetGlobalNumberVariable(string name, float value)
    {
        Error(name.Length == 0, "The specified variable name was invalid.");
        LogicEngine.SetGlobalVariable(name, value);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Global Variables/Set Global Unit Group Variable",
        dynamicDescription = "Set Global Unit Group named $ to $",
        icon = variableIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    [UnitGroupArg(argType = ArgType.Temp, allowValue = false)]
    public void SetGlobalUnitGroupVariable(string name, UnitGroup value)
    {
        Error(name.Length == 0, "The specified variable name was invalid.");
        Error(value == null, "The specified unit group was invalid.");
        LogicEngine.SetGlobalVariable(name, value);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Global Variables/Set Global Bool Variable",
        dynamicDescription = "Set global bool variable named $ to $",
        icon = variableIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    [BoolArg(argType = ArgType.Value, defaultValue = true)]
    public void SetGlobalBoolVariable(string name, bool value)
    {
        Error(name.Length == 0, "The specified variable name was invalid.");
        LogicEngine.SetGlobalVariable(name, value);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Global Variables/Set Global Unit Variable",
        dynamicDescription = "Set global unit variable named $ to $",
        icon = variableIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    [UnitArg(argType = ArgType.Temp, allowValue = false)]
    public void SetGlobalUnitVariable(string name, Unit value)
    {
        Error(name.Length == 0, "The specified variable name was invalid.");
        LogicEngine.SetGlobalVariable(name, value);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Global Variables/Set Global Vector Variable",
        dynamicDescription = "Set global vector variable named $ to $",
        icon = variableIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    [VectorArg(argType = ArgType.Temp)]
    public void SetGlobalVectorVariable(string name, Vector3 value)
    {
        Error(name.Length == 0, "The specified variable name was invalid.");
        LogicEngine.SetGlobalVariable(name, value);
    }

    [VisualScriptingFunction(
        dropdownDescription = "Global Variables/Set Global String Variable",
        dynamicDescription = "Set global string variable named $ to $",
        icon = variableIcon)]
    [StringArg(argType = ArgType.Temp, tempLabel = "Variable Name", allowFunction = false, allowPreset = false)]
    [StringArg(argType = ArgType.Temp)]
    public void SetGlobalStringVariable(string name, string value)
    {
        Error(name.Length == 0, "The specified variable name was invalid.");
        LogicEngine.SetGlobalVariable(name, value);
    }
}