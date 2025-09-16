using MyUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using Random = UnityEngine.Random;

public partial class VisualCodeScript
{
    [VisualScriptingFunction(
         dropdownDescription = "Random Item of Rarity",
         dynamicDescription = "Random $ Item")]
    [StringArg(argType = ArgType.Value, choicePreset = PresetChoices.Rarities, allowFunction = false, allowPreset = false)]
    public Item RandomItemOfRarity(string rarity)
    {
        return Item.GetRandomItemOfRarity((Item.ItemRarity)Enum.Parse(typeof(Item.ItemRarity), rarity));
    }

    // TODO: CAN BE REMOVED?
    public Item ThisItem()
    {
        return (Item)LogicEngine.current.engineHandler;
    }
}
