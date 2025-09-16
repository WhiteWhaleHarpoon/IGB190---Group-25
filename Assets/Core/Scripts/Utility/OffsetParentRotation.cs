using UnityEngine;

/// <summary>
/// Prevent this object from changing its rotation when the parent rotation changes.
/// </summary>

public class OffsetParentRotation : MonoBehaviour
{
    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}
