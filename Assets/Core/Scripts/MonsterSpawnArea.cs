using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// A monster spawn trigger area is used to calculate where monsters should
/// be spawned in the level. Monsters will spawn within the box collider on the object,
/// and be moved to the closest valid point in the game world.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class MonsterSpawnArea : MonoBehaviour
{
    // The total number of empowered monsters that should spawn in this area. Set this to
    // zero if you do not want an empowered monster to spawn.
    public int empoweredMonsters = 1;

    // The spawn density controls the ratio of monsters spawned in the level in each 1x1 area.
    // (i.e. larger spawn boxes already spawn more units by default). Only adjust this if you
    // want a room to be more or less dense with monsters.
    public float spawnDensity = 1f;

    [Range(1, 10)] public int maximumSpawnLevel = 1;

    // Cache the bounary values to speed up future lookups.
    private float minX, maxX, minY, maxY, minZ, maxZ;
    private BoxCollider boxCollider;

    /// <summary>
    /// Return a random point inside the box collider on this object.
    /// </summary>
    public Vector3 GetRandomVectorInCollider ()
    {
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
            minX = -0.5f * boxCollider.size.x;
            maxX = 0.5f * boxCollider.size.x;
            minY = -0.5f * boxCollider.size.y;
            maxY = 0.5f * boxCollider.size.y;
            minZ = -0.5f * boxCollider.size.z;
            maxZ = 0.5f * boxCollider.size.z;
        }
        Vector3 randomPointInLocalSpace = new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            Random.Range(minZ, maxZ)
        );
        return boxCollider.transform.TransformPoint(randomPointInLocalSpace);
    }

    public float GetSpawnAreaSize ()
    {
        return transform.localScale.x * transform.localScale.z;
    }

    /// <summary>
    /// Return an abolute monster density for the room, based on its size and the desired density.
    /// </summary>
    public float CalculateBoxSpawnDensity ()
    {
        return GetSpawnAreaSize() * spawnDensity;
    }
}
