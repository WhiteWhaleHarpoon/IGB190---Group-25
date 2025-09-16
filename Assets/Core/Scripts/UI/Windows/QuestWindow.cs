using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the display of quests in the quest window.
/// </summary>
public class QuestWindow : UIWindow
{
    [SerializeField] private GameObject header;
    [SerializeField] private QuestWindowItem[] questWindowItems;

    [SerializeField] private RectTransform _questContainer;
    [SerializeField] private QuestWindowItem _questTemplate;
    private QuestWindowItem[] _questItems;

    /// <summary>
    /// Sets up the quest window, subscribing to relevant events.
    /// </summary>
    public override void Setup()
    {
        base.Setup();
        SubscribeToEvents();
    }

    /// <summary>
    /// Subscribes to quest-related events to refresh the display when quests are updated.
    /// </summary>
    private void SubscribeToEvents()
    {
        GameManager.events.OnQuestAdded.AddListener(RefreshDisplay);
        GameManager.events.OnQuestCompleted.AddListener(RefreshDisplay);
        GameManager.events.OnQuestUpdated.AddListener(RefreshDisplay);
    }

    /// <summary>
    /// Refreshes the quest display when quests are added, completed, or updated.
    /// </summary>
    private void RefreshDisplay(Quest quest)
    {
        Quest[] activeQuests = GameManager.quests.activeQuests.Values.ToArray();

        // Clear the current list of quests (if it exists).
        if (_questItems != null)
        {
            for (int i = _questItems.Length - 1; i >= 0; i--)
            {
                Destroy(_questItems[i].gameObject);
            }
        }
        _questItems = new QuestWindowItem[activeQuests.Length];

        // Show the header if a quest is active.
        header.SetActive(activeQuests.Length > 0);

        // Populate the new list of quests.
        for (int i = 0; i < _questItems.Length; i++)
        {
            _questItems[i] = Instantiate(_questTemplate, _questContainer);
            _questItems[i].Show(activeQuests[i]);
        }

        // Reset the window to ensure the display is updated correctly
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }
}