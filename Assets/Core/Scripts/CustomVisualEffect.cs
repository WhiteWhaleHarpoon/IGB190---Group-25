using UnityEngine;

/// <summary>
/// This component is used to identify all visual effects in the game that the 
/// user can "spawn". It also handles the destruction of the effects after the 
/// specified amount of time.
/// </summary>
public class CustomVisualEffect : MonoBehaviour
{
    public string subGroup;
    public bool isTemplate;
    private const string DESTROY_METHOD = "DestroyThis";

    /// <summary>
    /// When the object is disabled, cancel all remaining destroy requests.
    /// </summary>
    private void OnDisable()
    {
        CancelInvoke();
    }

    /// <summary>
    /// Destroy the object after the specified amount of time. If the object is already 
    /// slated for deletion, cancel that deletion and create a new one.
    /// </summary>
    /// <param name="duration"></param>
    public void DestroyAfter (float duration)
    {
        CancelInvoke(DESTROY_METHOD);
        Invoke(DESTROY_METHOD, duration);
    }

    /// <summary>
    /// Destroys the object.
    /// </summary>
    private void DestroyThis()
    {
        ObjectPooler.DestroyPooled(gameObject);
    }

    /// <summary>
    /// Set the ToString be the name of the object (removing all "extrateous" stuff).
    /// </summary>
    public override string ToString()
    {
        return name;
    }
}
