using UnityEngine;

/// <summary>
/// Controls the logic for an interactable object.
/// </summary>
public class Interactable : MonoBehaviour
{
    [HideInInspector] public Outline outline;
    [HideInInspector] public Color color;
    [HideInInspector] public Color colorFade;

    private const float DEFAULT_INTERACTION_DISTANCE = 2.0f;

    /// <summary>
    /// Determines if this object is the currently hovered interactable.
    /// </summary>
    private bool IsHoveredInteractable()
    {
        return GameManager.hoveredInteractable == this;
    }

    /// <summary>
    /// Determines if this object is the currently selected interactable.
    /// </summary>
    private bool IsSelectedInteractable()
    {
        return GameManager.selectedInteractable == this;
    }

    /// <summary>
    /// Updates the outline properties based on the interaction state.
    /// </summary>
    private void UpdateOutline()
    {
        if (outline == null) return;

        outline.OutlineColor = IsHoveredInteractable() || IsSelectedInteractable() ? color : colorFade;
        outline.UpdateMaterialProperties();
        outline.enabled = false;
        outline.enabled = true;
    }

    /// <summary>
    /// Updates the interactable object every frame, handling outline and interaction logic.
    /// </summary>
    protected virtual void Update()
    {
        UpdateOutline();

        if (IsSelectedInteractable())
        {
            TryToInteract();
        }
    }

    /// <summary>
    /// Sets the outline color and size of the interactable object.
    /// </summary>
    public virtual void SetOutline(Color color, float size = 2.0f, float fadeAlpha = 0.03f)
    {
        if (!GameManager.settings.showUnitOutlines) return;
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineVisible;
        }

        this.color = color;
        this.colorFade = new Color(color.r, color.g, color.b, fadeAlpha);
        outline.OutlineColor = color;
        outline.OutlineWidth = size;
        UpdateOutline();
    }

    /// <summary>
    /// Attempts to interact with the object, completing the interaction if in range.
    /// </summary>
    public virtual void TryToInteract()
    {
        if (GameManager.selectedInteractable != this)
        {
            GameManager.selectedInteractable = this;
            GameManager.selectedInteractableAt = Time.time;
        }

        if (InRange(GameManager.player.transform.position))
        {
            OnInteraction();
            GameManager.selectedInteractable = null;
        }
    }

    /// <summary>
    /// Checks if the interactable is within the interaction range of a given point.
    /// </summary>
    public virtual bool InRange(Vector3 point)
    {
        return Vector3.Distance(point, transform.position) < GetInteractionDistance();
    }

    /// <summary>
    /// Returns the interaction distance for the object.
    /// </summary>
    public float GetInteractionDistance()
    {
        return DEFAULT_INTERACTION_DISTANCE;
    }

    /// <summary>
    /// Override to implement the specific interaction logic for the object.
    /// </summary>
    public virtual void OnInteraction()
    {
        // Implement interaction logic in derived classes.
    }
}