using UnityEngine;
/// <summary>
/// A basic class storing all of the settings in the game. This class should only
/// contain basic types (e.g. int, float, bool, string) or types which can be serialised.
/// 
/// The game settings will be serialised into a JSON file when the game is closed, and
/// loaded from the JSON file when it is opened.
/// </summary>
[System.Serializable]
public class Settings
{
    [Header("Default Quality Settings")]
    public int graphicsQuality = 5;

    [Header("Sound Settings")]
    public float masterVolume = 0.5f;
    public float musicVolume = 0.2f;
    public float effectsVolume = 0.5f;

    [Header("User Interface")]
    public bool showTutorialMessages = true;
    public bool showDamageNumbers = true;
    public bool showFullHealthBars = true;
    public bool showGoldPickupUI = true;
    public bool showTopHealthBarUI = true;
    public bool showGameUI = true;
    public bool showShopWhenCharacterWindowOpens = true;
    public bool showUnitOutlines = false;

    [Header("Keybinds")]
    //public KeyCode generatorAbilityKeybind = KeyCode.Mouse0;
    //public KeyCode spenderAbilityKeybind = KeyCode.Mouse1;
    //public KeyCode abilityOneKeybind = KeyCode.Q;
    //public KeyCode abilityTwoKeybind = KeyCode.W;
    //public KeyCode abilityThreeKeybind = KeyCode.E;
    //public KeyCode abilityFourKeybind = KeyCode.R;
    //public KeyCode forceMoveKeybind = KeyCode.Space;
    //public KeyCode forceHoldKeybind = KeyCode.LeftShift;

    public KeyCode[] keybindings = new KeyCode[8]
    {
        KeyCode.Mouse0,
        KeyCode.Mouse1,
        KeyCode.Q,
        KeyCode.W,
        KeyCode.E,
        KeyCode.R,
        KeyCode.Space,
        KeyCode.LeftShift
    };

    public const int FORCE_MOVE_KEYBIND_ID = 6;
    public const int FORCE_HOLD_KEYBIND_ID = 7;

    public void VerifySettings ()
    {
        if (GameManager.settings == null)
        {
            Debug.Log("Error loading settings.");
        }
    }                              
}