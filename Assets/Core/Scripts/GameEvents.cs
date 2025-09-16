using UnityEngine;
using UnityEngine.Events;

public class GameEvents
{
    public UnityEvent<EventMessageInfo> OnEventMessageReceived = new UnityEvent<EventMessageInfo>();
    public class EventMessageInfo
    {
        public string messageText;
        public EventMessageInfo (string messageText)
        {
            this.messageText = messageText;
        }
    }

    public UnityEvent<OnAbilityCastStartedInfo> OnAbilityCastStarted = new UnityEvent<OnAbilityCastStartedInfo>();
    public class OnAbilityCastStartedInfo
    {
        public Unit castingUnit;
        public Ability ability;
        public Unit targetUnit;
        public Vector3 targetPosition;
        public OnAbilityCastStartedInfo(Unit castingUnit, Ability ability, Unit targetUnit, Vector3 targetPosition)
        {
            this.castingUnit = castingUnit;
            this.ability = ability;
            this.targetUnit = targetUnit;
            this.targetPosition = targetPosition;
        }
    }

    public UnityEvent<OnAbilityCastFinishedInfo> OnAbilityCastFinished = new UnityEvent<OnAbilityCastFinishedInfo>();
    public class OnAbilityCastFinishedInfo
    {
        public Unit castingUnit;
        public Ability ability;
        public Unit targetUnit;
        public Vector3 targetPosition;
        public OnAbilityCastFinishedInfo(Unit castingUnit, Ability ability, Unit targetUnit, Vector3 targetPosition)
        {
            this.castingUnit = castingUnit;
            this.ability = ability;
            this.targetUnit = targetUnit;
            this.targetPosition = targetPosition;
        }
    }

    public UnityEvent<OnUnitGainsBuffInfo> OnUnitGainsBuff = new UnityEvent<OnUnitGainsBuffInfo>();
    public class OnUnitGainsBuffInfo
    {
        public Unit unitGainingBuff;
        public Unit buffingUnit;
        public string buffName;
        public OnUnitGainsBuffInfo(Unit unitGainingBuff, Unit buffingUnit, string buffName)
        {
            this.unitGainingBuff = unitGainingBuff;
            this.buffingUnit = buffingUnit;
            this.buffName = buffName;
        }
    }

    public UnityEvent<OnUnitLosesBuffInfo> OnUnitLosesBuff = new UnityEvent<OnUnitLosesBuffInfo>();
    public class OnUnitLosesBuffInfo
    {
        public Unit unitLosingBuff;
        public Unit unitRemovingBuff;
        public string buffName;
        public OnUnitLosesBuffInfo (Unit unitLosingBuff, Unit unitRemovingBuff, string buffName)
        {
            this.unitLosingBuff = unitLosingBuff;
            this.unitRemovingBuff = unitRemovingBuff;
            this.buffName = buffName;
        }
    }

    public UnityEvent<OnUnitRefreshesBuffInfo> OnUnitRefreshesBuff = new UnityEvent<OnUnitRefreshesBuffInfo>();
    public class OnUnitRefreshesBuffInfo
    {
        public Unit unitWithBuff;
        public Unit unitRefreshingBuff;
        public string buffName;
        public OnUnitRefreshesBuffInfo (Unit unitWithBuff, Unit unitRefreshingBuff, string buffName)
        {
            this.unitWithBuff = unitWithBuff;
            this.unitRefreshingBuff = unitRefreshingBuff;
            this.buffName = buffName;
        }
    }

    public UnityEvent<OnUnitStunnedInfo> OnUnitStunned = new UnityEvent<OnUnitStunnedInfo>();
    public class OnUnitStunnedInfo
    {
        public Unit stunnedUnit;
        public Unit stunningUnit;
        public IVisualCodeHandler stunningSource;
        public float stunDuration;
        public OnUnitStunnedInfo (Unit stunnedUnit, Unit stunningUnit,  IVisualCodeHandler stunningSource, float stunDuration)
        {
            this.stunnedUnit = stunnedUnit;
            this.stunningUnit = stunningUnit;
            this.stunningSource = stunningSource;
            this.stunDuration = stunDuration;
        }
    }

    public UnityEvent<OnUnitKilledInfo> OnUnitKilled = new UnityEvent<OnUnitKilledInfo>();
    public class OnUnitKilledInfo
    {
        public Unit killedUnit;
        public Unit killingUnit;
        public IVisualCodeHandler killingSource;
        public bool isCrit;
        public OnUnitKilledInfo(Unit killedUnit, Unit killingUnit, IVisualCodeHandler killingSource, bool isCrit)
        {
            this.killedUnit = killedUnit;
            this.killingUnit = killingUnit;
            this.killingSource = killingSource;
            this.isCrit = isCrit;
        }
    }

    public UnityEvent<OnPlayerKilledInfo> OnPlayerKilled = new UnityEvent<OnPlayerKilledInfo>();
    public class OnPlayerKilledInfo
    {
        public Unit killedUnit;
        public Unit killingUnit;
        public IVisualCodeHandler killingSource;
        public bool isCrit;
        public OnPlayerKilledInfo(Player player, Unit killingUnit, IVisualCodeHandler killingSource, bool isCrit)
        {
            this.killedUnit = player;
            this.killingUnit = killingUnit;
            this.killingSource = killingSource;
            this.isCrit = isCrit;
        }
    }

    public UnityEvent<OnUnitEnteredRegionInfo> OnPlayerEnteredRegion = new UnityEvent<OnUnitEnteredRegionInfo>();
    public class OnUnitEnteredRegionInfo
    {
        public Unit enteringUnit;
        public Region region;
        public string regionName;
        public OnUnitEnteredRegionInfo(Unit enteringUnit, Region region, string regionName)
        {
            this.enteringUnit = enteringUnit;
            this.region = region;
            this.regionName = regionName;
        }
    }

    public UnityEvent<OnUnitExitedRegionInfo> OnPlayerExitedRegion = new UnityEvent<OnUnitExitedRegionInfo>();
    public class OnUnitExitedRegionInfo
    {
        public Unit exitingUnit;
        public Region region;
        public string regionName;
        public OnUnitExitedRegionInfo(Unit enteringUnit, Region region, string regionName)
        {
            this.exitingUnit = enteringUnit;
            this.region = region;
            this.regionName = regionName;
        }
    }

    public UnityEvent<OnUnitDamagedInfo> OnUnitDamaged = new UnityEvent<OnUnitDamagedInfo>();
    public class OnUnitDamagedInfo
    {
        public Unit damagedUnit;
        public Unit damagingUnit;
        public float damage;
        public IVisualCodeHandler damageSource;
        public bool isCritical;

        public OnUnitDamagedInfo(Unit damagedUnit, float damage, Unit damagingUnit, IVisualCodeHandler damageSource, bool isCritical)
        {
            this.damagedUnit = damagedUnit;
            this.damagingUnit = damagingUnit;
            this.damage = damage;
            this.damageSource = damageSource;
            this.isCritical = isCritical;
        }
    }

    public UnityEvent<OnUnitSpawnedInfo> OnUnitSpawned = new UnityEvent<OnUnitSpawnedInfo>();
    public class  OnUnitSpawnedInfo
    {
        public Unit unitSpawned;
        public Unit owner;
        public OnUnitSpawnedInfo(Unit unitSpawned, Unit owner)
        {
            this.unitSpawned = unitSpawned;
            this.owner = owner;
        }
    }

    public UnityEvent<float> OnGoldAdded = new UnityEvent<float>();
    public UnityEvent<float> OnGoldRemoved = new UnityEvent<float>();
    public UnityEvent<float> OnGoldPickedUp = new UnityEvent<float>();
    public UnityEvent<float> OnHealthPickedUp = new UnityEvent<float>();
    public UnityEvent<Item> OnItemEquipped = new UnityEvent<Item>();
    public UnityEvent<Item> OnItemUnequipped = new UnityEvent<Item>();
    public UnityEvent<Item> OnItemPickedUp = new UnityEvent<Item>();
    public UnityEvent<Item> OnItemSold = new UnityEvent<Item>();
    public UnityEvent<Quest> OnQuestUpdated = new UnityEvent<Quest>();
    public UnityEvent<Quest> OnQuestCompleted = new UnityEvent<Quest>();
    public UnityEvent<Quest> OnQuestAdded = new UnityEvent<Quest>();
    public UnityEvent<Player> OnPlayerExperienceGained = new UnityEvent<Player>();
    public UnityEvent<Player> OnPlayerLevelUp = new UnityEvent<Player>();
    public UnityEvent OnGameWon = new UnityEvent();
}
