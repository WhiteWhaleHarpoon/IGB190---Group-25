using MyUtilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static Player player;
    public static Unit hoveredUnit;
    public static Monster hoveredMonster;
    public static Interactable selectedInteractable;
    public static float selectedInteractableAt;
    public static Interactable hoveredInteractable;
    public static string characterToSpawn = "";

    // Singleton reference to the game manager.
    private static GameManager _instance;
    public static GameManager instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<GameManager>();
            return _instance;
        }
    }

    // Reference to the game assets.
    private VisualCodeManager _logicEngine;
    public static VisualCodeManager logicEngine
    {
        get
        {
            if (instance._logicEngine == null)
                instance._logicEngine = FindFirstObjectByType<VisualCodeManager>();
            return instance._logicEngine;
        }
    }

    // Reference to the game assets.
    [SerializeField] private GameAssets _assets;
    public static GameAssets assets
    {
        get
        {
            return instance._assets;
        }
    }

    // Reference to the game settings.
    [SerializeField] private Settings _settings;
    public static Settings settings
    {
        get {
            if (instance == null)
            {
                if (File.Exists("Settings.json"))
                {
                    return JsonUtility.FromJson<Settings>(File.ReadAllText("Settings.json"));
                }
                else
                {
                    return new Settings();
                }
            }
            if (instance._settings == null)
            {
                if (File.Exists("Settings.json"))
                {
                    instance._settings = JsonUtility.FromJson<Settings>(File.ReadAllText("Settings.json"));
                }
                else
                {
                    instance._settings = new Settings();
                }
            }
            if (instance._settings == null) Debug.Log("Settings were unable to be loaded correctly.");
            return instance._settings;
        }
    }

    // Reference to the UI windows.
    private UIManager _ui;
    public static UIManager ui
    {
        get
        {
            if (instance._ui == null)
                instance._ui = new UIManager();
            return instance._ui;
        }
    }

    // Reference to the game events.
    private GameEvents _events;
    public static GameEvents events
    {
        get
        {
            if (instance._events == null)
                instance._events = new GameEvents();
            return instance._events;
        }
    }

    // Reference to the quest manager.
    private QuestManager _quests;
    public static QuestManager quests
    {
        get
        {
            if (instance._quests == null)
                instance._quests = new QuestManager();
            return instance._quests;
        }
    }

    // Reference to the music manager.
    private MusicManager _music;
    public static MusicManager music
    {
        get
        {
            if (instance._music == null)
                instance._music = FindFirstObjectByType<MusicManager>();
            return instance._music;
        }
    }

    // Reference to the monster spawn manager.
    private static MonsterSpawnManager _spawner;
    public static MonsterSpawnManager spawner
    {
        get
        {
            if (_spawner == null)
                _spawner = FindFirstObjectByType<MonsterSpawnManager>();
            return _spawner;
        }
    }

    // Reference to the item manager.
    private static ItemManager _items;
    public static ItemManager items
    {
        get
        {
            if (_items == null)
                _items = new ItemManager();
            return _items;
        }
    }

    [Header("Game Settings")]

    [SerializeField] private MonsterValues _monsterValues;
    public static MonsterValues monsterValues => instance._monsterValues;
    [System.Serializable] public class MonsterValues
    {
        public float baseGoldDropAmountMinimum = 10;
        public float baseGoldDropAmountMaximum = 20;
        [Range(0.0f, 1.0f)] public float goldDropChance = 0.4f;
        [Range(0.0f, 1.0f)] public float unempoweredMonsterCommonDropChance = 0.02f;
        [Range(0.0f, 1.0f)] public float unempoweredMonsterRareDropChance = 0.005f;
        [Range(0.0f, 1.0f)] public float unempoweredMonsterLegendaryDropChance = 0;
    }

    [SerializeField] private EmpoweredMonsterValues _empoweredMonsterValues;
    public static EmpoweredMonsterValues empoweredMonsterValues => instance._empoweredMonsterValues;
    [System.Serializable] public class EmpoweredMonsterValues
    {
        public float empoweredMonsterHealthModifier = 4.0f;
        public float empoweredMonsterDamageModifier = 1.5f;
        public float empoweredMonsterAttackSpeedModifier = 1.5f;
        public float empoweredMonsterXPModifier = 5.0f;
        public float empoweredMonsterGoldModifier = 3.0f;
        [Range(0.0f, 1.0f)] public float empoweredMonsterCommonDropChance = 1f;
        [Range(0.0f, 1.0f)] public float empoweredMonsterRareDropChance = 0.2f;
        [Range(0.0f, 1.0f)] public float empoweredMonsterLegendaryDropChance = 0;
    }

    [SerializeField] private PlayerExperienceValues _playerExperienceValues;
    public static PlayerExperienceValues playerExperienceValues => instance._playerExperienceValues;
    [System.Serializable] public class PlayerExperienceValues
    {
        public float baseMonsterXP = 10;
        public float startingXPPerLevel = 100;
        public float additionalMaxXPPerLevel = 100;
    }

    [SerializeField] private MonsterScalingValues _monsterScalingValues;
    public static MonsterScalingValues monsterScalingValues => instance._monsterScalingValues;
    [System.Serializable] public class MonsterScalingValues
    {
        [Range(0.0f, 1.0f)] public float increasedHealthPerPlayerLevel = 0.2f;
        [Range(0.0f, 1.0f)] public float increasedDamagePerPlayerLevel = 0.2f;
    }

    [SerializeField] private HealthGlobeValues _healthGlobeValues;
    public static HealthGlobeValues healthGlobeValues => instance._healthGlobeValues;
    [System.Serializable] public class HealthGlobeValues
    {
        [Range(0.0f, 1.0f)] public float baseHealthGlobeChance = 0.5f;
        [Range(0.0f, 1.0f)] public float reducedChancePerExistingGlobe = 0.15f;
        public float spawnRadius = 3;
        public float lifetime = 5;
        public float healthGlobeHealthRestore = 100;
    }

    [SerializeField] private InventoryValues _inventoryValues;
    public static InventoryValues inventoryValues => instance._inventoryValues;
    [System.Serializable] public class InventoryValues
    {
        public float sellItemReturnRate = 0.5f;
    }

    [SerializeField] private ArmorValues _armorValues;
    public static ArmorValues armorValues => instance._armorValues;
    [System.Serializable]
    public class ArmorValues
    {
        public float maxArmor = 1000;
        public AnimationCurve armorDamageReductionCurve;
    }


    

    /// <summary>
    /// When the GameManager is destroyed, save the settings to a file for future use.
    /// </summary>
    private void OnDestroy()
    {
        File.WriteAllText("Settings.json", JsonUtility.ToJson(settings));
    }

    public void OnEnable()
    {
        if (CharacterSelectManager.selectedCharacter != "")
        {
            Player[] players = GameObject.FindObjectsByType<Player>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Player player in players)
            {
                player.gameObject.SetActive(player.unitName == CharacterSelectManager.selectedCharacter);
            }
        }
        player = GameObject.FindFirstObjectByType<Player>();
        if (player == null) 
            Debug.Log("No player character has been added. If you loaded from the character select screen, " +
                "make sure that the character name on the select screen matches the name of the character in the game scene.");
        ui.Setup();
    }


    private void Start()
    {
        Ability[] abilities = Resources.LoadAll<Ability>("Abilities");
        foreach (Ability ability in abilities)
        {
            ability.engine.Setup();
        }
    }

    private void Update()
    {
        HandleUserInput();
        hoveredMonster = GetHoveredMonster();
    }

    private Monster GetHoveredMonster ()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, assets.monsterMask))
            return hit.collider.GetComponent<Monster>();
        return null;
    }

    public void WinGame ()
    {
        GameManager.events.OnGameWon.Invoke();
        StartCoroutine(GoToEpilogue());
    }

    private IEnumerator GoToEpilogue()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        op.allowSceneActivation = false;
        yield return new WaitForSeconds(7.0f);
        op.allowSceneActivation = true;
    }

    private void HandleUserInput ()
    {
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.C))
        {
            ui.CharacterWindow.Show();
            if (settings.showShopWhenCharacterWindowOpens)
                ui.ShopWindow.Show();
            else
                ui.ShopWindow.Hide();
        }
    }

    public static void SendEventMessage (string message)
    {
        events.OnEventMessageReceived.Invoke(new GameEvents.EventMessageInfo(message));
    }
}
