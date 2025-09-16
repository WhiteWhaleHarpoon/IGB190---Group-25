using FastFeedback;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnableOutlineEffect : FastFeedbackEffect
{
    public Outline outline;
    private EnableOutlineWithColor settings;

    private List<EnableOutlineWithColor> activeOutlines = new List<EnableOutlineWithColor>();

    public Dictionary<EnableOutlineWithColor, float> timeAdded = new Dictionary<EnableOutlineWithColor, float>();

    public override void Apply(FeedbackItem item, GameObject target = null, GameObject origin = null)
    {
        base.Apply(item, target, origin);
        EnableOutlineWithColor o = (EnableOutlineWithColor)item;
        //activeOutlines.Add(o);
        //activeOutlines = (List<EnableOutlineWithColor>)activeOutlines.OrderByDescending(o => (o.priority * 100000 + o.createdAt));
        //activeOutlines = (List<EnableOutlineWithColor>)activeOutlines.OrderByDescending(o => o.priority);

        if (!activeOutlines.Contains(o))
        {
            bool added = false;
            for (int i = 0; i < activeOutlines.Count; i++)
            {
                if (o.priority >= activeOutlines[i].priority)
                {
                    activeOutlines.Insert(i, o);
                    added = true;
                    break;
                }
            }
            if (!added) activeOutlines.Add(o);
            
        }
        timeAdded[o] = Time.time;




        if (outline == null)
        {
            outline = gameObject.GetComponent<Outline>();
            if (outline == null) outline = gameObject.AddComponent<Outline>();
        }
        RefreshVisuals();
    }

    void Update()
    {
        List<EnableOutlineWithColor> toDelete = new List<EnableOutlineWithColor>();
        foreach (EnableOutlineWithColor activeOutline in activeOutlines)
        {
            if (activeOutline.durationSettings == EnableOutlineWithColor.DurationSettings.ForXSeconds && Time.time > timeAdded[activeOutline] + activeOutline.durationTime)
            {
                toDelete.Add(activeOutline);
            }
        }
        foreach (EnableOutlineWithColor activeOutline in toDelete)
        {
            activeOutlines.Remove(activeOutline);
            timeAdded.Remove(activeOutline);
        }
        RefreshVisuals();

    }

    private void RefreshVisuals ()
    {
        if (activeOutlines.Count == 0)
        {
            outline.OutlineColor = new Color(0, 0, 0, 0);
            outline.OutlineWidth = 0;
        }
        else
        {
            EnableOutlineWithColor active = activeOutlines[0];
            float strength = 1.0f;
            if (active.outlineType == EnableOutlineWithColor.OutlineType.FlashEveryXSeconds)
            {
                strength = ((Time.time - timeAdded[active]) / active.outlineFlashTime) % 1.0f;
                strength = FastFeedbackSettings.Current.easeInAndOut.Evaluate(strength);
            }
            Color c = active.outlineColor;
            c.a = 0;
            outline.OutlineColor = Color.Lerp(c, active.outlineColor, strength);
            outline.OutlineWidth = active.outlineThickness;
        }
    }

    public void RemoveOutlinesWithTag(string tag)
    {
        for (int i = activeOutlines.Count - 1; i >= 0; i--)
        {
            if (activeOutlines[i].effectTag == tag)
            {
                timeAdded.Remove(activeOutlines[i]);
                activeOutlines.RemoveAt(i);
            }
        }
    }

    public void RemoveOutlinesWithPriority(int priority)
    {
        for (int i = activeOutlines.Count - 1; i >= 0; i--)
        {
            if (activeOutlines[i].priority == priority)
            {
                timeAdded.Remove(activeOutlines[i]);
                activeOutlines.RemoveAt(i);
            }
        }
    }

    public void RemoveOutlinesLessThanPriority(int priority)
    {
        for (int i = activeOutlines.Count - 1; i >= 0; i--)
        {
            if (activeOutlines[i].priority < priority)
            {
                timeAdded.Remove(activeOutlines[i]);
                activeOutlines.RemoveAt(i);
            }
        }
    }

    public void RemoveOutlinesGreaterThanPriority(int priority)
    {
        for (int i = activeOutlines.Count - 1; i >= 0; i--)
        {
            if (activeOutlines[i].priority > priority)
            {
                timeAdded.Remove(activeOutlines[i]);
                activeOutlines.RemoveAt(i);
            }
        }
    }

    public void RemoveAllOutlines ()
    {
        timeAdded = new Dictionary<EnableOutlineWithColor, float>();
        activeOutlines = new List<EnableOutlineWithColor>();
    }
}
