using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Controls all data related to a quest in the game. A quest has:
/// 1) A unique description,
/// 2) A reward string (optional),
/// 3) A list of Quest Items controlling the quest requirements.
///
/// When all requirements of a quest are "completed" (i.e., their current progress >= their maximum 
/// progress), the quest will be automatically completed and the OnQuestCompleted event will be fired.
/// </summary>
public class Quest
{
    public string Label { get; }
    public string Description { get; }
    public string Reward { get; private set; } = string.Empty;

    private const string DefaultLabel = "None";
    private readonly Dictionary<string, QuestItem> _questItems = new();

    /// <summary>
    /// Constructor for a quest. A quest needs a unique label and description.
    /// </summary>
    public Quest(string label, string description)
    {
        Label = label;
        Description = description;
    }

    public Quest (string label)
    {
        Label = label;
    }

    /// <summary>
    /// Sets the reward string for the quest.
    /// </summary>
    public void SetReward(string reward)
    {
        Reward = reward;
        HandleQuestUpdate();
    }

    public void AddCompletionRequirement(string description, int maxProgress = 1, int currentProgress = 0)
    {
        _questItems[""] = new QuestItem("", description, maxProgress, currentProgress);
        HandleQuestUpdate();
    }

    /// <summary>
    /// Adds a new completion requirement to the quest.
    /// </summary>
    public void AddCompletionRequirement(string label, string description, int maxProgress, int currentProgress = 0)
    {
        _questItems[label] = new QuestItem(label, description, maxProgress, currentProgress);
        HandleQuestUpdate();
    }

    /// <summary>
    /// Increases the progress of the specified quest requirement.
    /// </summary>
    public void IncrementProgress(string label, int value)
    {
        GetQuestItem(label).IncrementProgress(value);
        HandleQuestUpdate();
    }

    /// <summary>
    /// Increases the progress of the specified quest requirement.
    /// </summary>
    public void IncrementProgress(int value = 1)
    {
        GetQuestItem("").IncrementProgress(value);
        HandleQuestUpdate();
    }

    /// <summary>
    /// Decreases the progress of the specified quest requirement.
    /// </summary>
    public void DecrementProgress(string label, int value)
    {
        GetQuestItem(label).DecrementProgress(value);
        HandleQuestUpdate();
    }

    /// <summary>
    /// Decreases the progress of the specified quest requirement.
    /// </summary>
    public void DecrementProgress(int value = 1)
    {
        GetQuestItem("").DecrementProgress(value);
        HandleQuestUpdate();
    }

    /// <summary>
    /// Sets the progress of the specified quest requirement.
    /// </summary>
    public void SetProgress(string label, int value)
    {
        GetQuestItem(label).SetProgress(value);
        HandleQuestUpdate();
    }

    /// <summary>
    /// Sets the progress of the specified quest requirement.
    /// </summary>
    public void SetProgress(int value)
    {
        GetQuestItem("").SetProgress(value);
        HandleQuestUpdate();
    }

    /// <summary>
    /// Returns an array of all quest items.
    /// </summary>
    public QuestItem[] GetItems()
    {
        return _questItems.Values.ToArrayPooled();
    }

    /// <summary>
    /// Handles the logic for updating a quest. If the quest is complete, it triggers the OnQuestCompleted event,
    /// otherwise, it triggers the OnQuestUpdated event.
    /// </summary>
    private void HandleQuestUpdate()
    {
        if (IsComplete())
        {
            GameManager.quests.RemoveQuest(this);
            GameManager.events.OnQuestCompleted.Invoke(this);
        }
        else
        {
            GameManager.events.OnQuestUpdated.Invoke(this);
        }
    }

    /// <summary>
    /// Checks if all quest requirements are complete.
    /// </summary>
    private bool IsComplete()
    {
        return _questItems.Values.All(item => item.IsComplete());
    }

    /// <summary>
    /// Retrieves the quest item by its label. If the label is "None", it returns the first quest item.
    /// </summary>
    private QuestItem GetQuestItem(string label)
    {
        if (label == DefaultLabel) label = _questItems.Keys.First();
        return _questItems[label];
    }

    /// <summary>
    /// A Quest Item controls all data for a single requirement of a quest.
    /// </summary>
    public class QuestItem
    {
        public string Label { get; }
        public string Description { get; }
        public int MaxProgress { get; }
        public int CurrentProgress { get; private set; }

        /// <summary>
        /// Constructor for a Quest Item. It needs a label, description, and maximum progress count.
        /// </summary>
        public QuestItem(string label, string description, int maxProgress, int currentProgress = 0)
        {
            Label = label;
            Description = description;
            MaxProgress = maxProgress;
            CurrentProgress = currentProgress;
        }

        /// <summary>
        /// Increases the progress of this requirement.
        /// </summary>
        public void IncrementProgress(int value = 1)
        {
            CurrentProgress = Mathf.Min(CurrentProgress + value, MaxProgress);
        }

        /// <summary>
        /// Decreases the progress of this requirement.
        /// </summary>
        public void DecrementProgress(int value = 1)
        {
            CurrentProgress = Mathf.Max(CurrentProgress - value, 0);
        }

        /// <summary>
        /// Sets the progress of this requirement.
        /// </summary>
        public void SetProgress(int value)
        {
            CurrentProgress = value;
        }

        /// <summary>
        /// Checks if the requirement is complete.
        /// </summary>
        public bool IsComplete()
        {
            return CurrentProgress == MaxProgress;
        }
    }
}