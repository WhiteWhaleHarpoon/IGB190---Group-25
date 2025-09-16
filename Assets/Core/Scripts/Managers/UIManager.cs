using UnityEngine;

/// <summary>
/// The UIManager controls all UI windows in the game.
/// </summary>
public class UIManager
{
    public UIWindow currentActiveWindow;

    private Canvas _dynamicCanvas;
    public Canvas DynamicCanvas
    {
        get
        {
            if (_dynamicCanvas == null)
            {
                _dynamicCanvas = GameManager.instance.GetComponentInChildren<Canvas>();
            }
            return _dynamicCanvas;
        }
    }


    private SelectedEnemyWindow _selectedEnemyWindow;
    public SelectedEnemyWindow SelectedEnemyWindow =>
        _selectedEnemyWindow ??= GameManager.FindFirstObjectByType<SelectedEnemyWindow>(FindObjectsInactive.Include);


    private TooltipWindow _tooltipWindow;
    public TooltipWindow TooltipWindow =>
        _tooltipWindow ??= GameManager.FindFirstObjectByType<TooltipWindow>(FindObjectsInactive.Include);


    private PlayerWindow _playerWindow;
    public PlayerWindow PlayerWindow =>
        _playerWindow ??= GameManager.FindFirstObjectByType<PlayerWindow>(FindObjectsInactive.Include);


    private MessageWindow _messageWindow;
    public MessageWindow MessageWindow =>
        _messageWindow ??= GameManager.FindFirstObjectByType<MessageWindow>(FindObjectsInactive.Include);


    private CharacterWindow _characterWindow;
    public CharacterWindow CharacterWindow =>
        _characterWindow ??= GameManager.FindFirstObjectByType<CharacterWindow>(FindObjectsInactive.Include);


    private InventoryWindow _inventoryWindow;
    public InventoryWindow InventoryWindow =>
        _inventoryWindow ??= GameManager.FindFirstObjectByType<InventoryWindow>(FindObjectsInactive.Include);


    private EquipmentWindow _equipmentWindow;
    public EquipmentWindow EquipmentWindow =>
        _equipmentWindow ??= GameManager.FindFirstObjectByType<EquipmentWindow>(FindObjectsInactive.Include);


    private StatsWindow _statsWindow;
    public StatsWindow StatsWindow =>
        _statsWindow ??= GameManager.FindFirstObjectByType<StatsWindow>(FindObjectsInactive.Include);


    private ShopWindow _shopWindow;
    public ShopWindow ShopWindow =>
        _shopWindow ??= GameManager.FindFirstObjectByType<ShopWindow>(FindObjectsInactive.Include);


    private DeathWindow _deathWindow;
    public DeathWindow DeathWindow =>
        _deathWindow ??= GameManager.FindFirstObjectByType<DeathWindow>(FindObjectsInactive.Include);


    private NotificationWindow _notificationWindow;
    public NotificationWindow NotificationWindow =>
        _notificationWindow ??= GameManager.FindFirstObjectByType<NotificationWindow>(FindObjectsInactive.Include);


    private MainMenuWindow _mainMenuWindow;
    public MainMenuWindow MainMenuWindow =>
        _mainMenuWindow ??= GameManager.FindFirstObjectByType<MainMenuWindow>(FindObjectsInactive.Include);      


    private OptionsWindow _optionsWindow;
    public OptionsWindow OptionsWindow =>
        _optionsWindow ??= GameManager.FindFirstObjectByType<OptionsWindow>(FindObjectsInactive.Include);


    /// <summary>
    /// Sets up all UI windows by calling their respective Setup methods.
    /// </summary>
    public void Setup()
    {
        UIWindow[] windows = GameManager.FindObjectsByType<UIWindow>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (UIWindow window in windows)
        {
            window.Setup();
        }
        UIWindow.SetEscapeWindow(MainMenuWindow);
    }
}