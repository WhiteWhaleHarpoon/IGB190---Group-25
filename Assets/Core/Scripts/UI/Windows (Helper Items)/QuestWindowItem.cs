using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestWindowItem : MonoBehaviour
{
    public TextMeshProUGUI questHeader;
    public Image questHeaderIcon;

    [SerializeField] private RectTransform _questRequirementContainer;
    [SerializeField] private QuestWindowItemRequirement _template;

    public TextMeshProUGUI[] questItemSlots;

    [SerializeField] private RectTransform _questReward;
    [SerializeField] private TextMeshProUGUI _questRewardText;

    private Quest _quest;

    public void Show (Quest quest)
    {
        _quest = quest;
        Redraw();

        /*
        gameObject.SetActive(true);
        questHeader.text = quest.Description;
        Quest.QuestItem[] questItems = quest.GetItems();
        for (int i = 0; i < questItemSlots.Length; i++)
        {
            if (i < questItems.Length)
            {
                questItemSlots[i].gameObject.SetActive(true);

                string progress = "";
                if (questItems[i].MaxProgress > 1)
                {
                    progress = $"[{questItems[i].CurrentProgress}/{questItems[i].MaxProgress}]";
                }
                questItemSlots[i].text = $" - {questItems[i].Description} {progress}";
            }
            else
            {
                questItemSlots[i].gameObject.SetActive(false);
            }
        }
        if (quest.Reward.Length > 0)
        {
            questReward.gameObject.SetActive(true);
            questReward.text = $" - <color=yellow>Reward</color>: {quest.Reward}";
        }
        else
        {
            questReward.gameObject.SetActive(false);
        }
        */
    }

    public void Redraw ()
    {
        gameObject.SetActive(true);
        questHeader.text = _quest.Description;

        // Destroy all non-template children.
        while (_questRequirementContainer.childCount > 1) 
        {
            Destroy(_questRequirementContainer.GetChild(_questRequirementContainer.childCount - 1).gameObject);
        }

        // Populate all of the quest requirements.
        Quest.QuestItem[] questItems = _quest.GetItems();
        for (int i = 0; i < questItems.Length; i++)
        {
            var item = Instantiate(_template, _questRequirementContainer);

            // Craft the full quest item requirement label.
            string progress = "";
            if (questItems[i].MaxProgress > 1)
            {
                progress = $"[{questItems[i].CurrentProgress}/{questItems[i].MaxProgress}]";
            }
            item.itemText.text = $"{questItems[i].Description} {progress}";
            item.gameObject.SetActive(true);
        }

        // Populate the reward label if the quest has a reward.
        if (_quest.Reward.Length > 0)
        {
            _questReward.gameObject.SetActive(true);
            _questRewardText.text = $"<color=yellow>Reward</color>: {_quest.Reward}";
        }
    }

    public void Hide ()
    {
        gameObject.SetActive(false);
    }
}
