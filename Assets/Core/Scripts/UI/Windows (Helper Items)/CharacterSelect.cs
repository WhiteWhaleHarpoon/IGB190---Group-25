using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelect : MonoBehaviour
{
    public string characterName;
    public GameObject characterTitle;
    public GameObject characterHide;
    public GameObject characterDetails;

   
    private void OnMouseEnter()
    {
        CharacterSelectManager.hoveredCharacter = characterName;
    }

    private void OnMouseExit()
    {
        if (CharacterSelectManager.hoveredCharacter == characterName)
            CharacterSelectManager.hoveredCharacter = "";
    }

    private void OnMouseDown()
    {
        GameObject.FindFirstObjectByType<CharacterSelectManager>().SetSelectedCharacter(characterName);
    }

    private void Update()
    {
        characterTitle.SetActive(
            CharacterSelectManager.hoveredCharacter == characterName ||
            CharacterSelectManager.selectedCharacter == characterName);

        characterHide.SetActive(
            !(CharacterSelectManager.hoveredCharacter == characterName ||
            CharacterSelectManager.selectedCharacter == characterName));

        characterDetails.SetActive(CharacterSelectManager.selectedCharacter == characterName);
    }
}
