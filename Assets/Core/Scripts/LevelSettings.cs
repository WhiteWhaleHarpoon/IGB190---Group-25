using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSettings
{
    public bool preventLootDrops = false;
    public bool preventCommonItemDrops = false;
    public bool preventRareItemDrops = false;
    public bool preventLegendaryItemDrops = false;

    public float playerLootDropModifier = 1.0f;
    public float playerCommonItemDropModifier = 1.0f;
    public float playerRareItemDropModifier = 1.0f;
    public float playerLegendaryItemDropModifier = 1.0f;
    public float playerExperienceModifier = 1.0f;
    public float playerItemDropRateModifier = 1.0f;
    public float monsterDamageModifier = 1.0f;
    public float monsterHealthModifier = 1.0f;
    public float healthGlobeSpawnModifier = 1.0f;
    public float monsterSpawnRateModifier = 1.0f;
    public float goldDropRateModifier = 1.0f;
    public float goldDropAmountModifier = 1.0f;
}
