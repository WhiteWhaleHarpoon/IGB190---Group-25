using UnityEngine;

/// <summary>
/// The CharacterWindow class serves as a container for several other windows.
/// E.g. Inventory, Shop, Stats, Equipment. Showing this window will show all
/// of them.
/// </summary>
public class CharacterWindow : UIWindow, IPausing, ICloseable
{
    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
        GameManager.ui.TooltipWindow.Hide();
    }

    protected override void Update()
    {
        base.Update();
        if  (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.C))
        {
            Hide();
        }
    }
}
