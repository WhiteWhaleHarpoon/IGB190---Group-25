using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class AttributesItem : MonoBehaviour
{
    public TextMeshProUGUI description;
    public TextMeshProUGUI value;
    private Stat attachedStat;
    private string formatting;

    public void Setup (Stat stat, string formatting)
    {
        attachedStat = stat;
        this.formatting = formatting;
        description.text = stat.Label();
        value.text = GameManager.player.stats.GetValue(stat).ToString(formatting, CultureInfo.InvariantCulture);
    }

    void Update()
    {
        value.text = GameManager.player.stats.GetValue(attachedStat).ToString(formatting, CultureInfo.InvariantCulture);
    }
}
