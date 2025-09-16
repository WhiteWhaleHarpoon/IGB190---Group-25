using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleCustomAbilityLogic : MonoBehaviour
{
    private void Start()
    {
        GameManager.events.OnAbilityCastFinished.AddListener(OnAbilityCastFinished);
    }

    private void OnAbilityCastFinished (GameEvents.OnAbilityCastFinishedInfo castInfo)
    {
        if (castInfo.ability.abilityName == "Healing Potion")
        {
            Debug.Log("This triggered");
        } 
    }
}
