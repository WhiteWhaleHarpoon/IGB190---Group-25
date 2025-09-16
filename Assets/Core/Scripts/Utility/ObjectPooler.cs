using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    // Singleton instance for global access
    public static ObjectPooler Instance { get; private set; }

    // Dictionary to store available objects grouped by prefab
    private readonly Dictionary<GameObject, Queue<GameObject>> availableObjects = new Dictionary<GameObject, Queue<GameObject>>();

    // Dictionary to track objects currently in use, mapped to their prefab
    private readonly Dictionary<GameObject, GameObject> inUseObjects = new Dictionary<GameObject, GameObject>();

    private void Awake()
    {
        // Ensure only one instance of ObjectPooler exists
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // Keep across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    /// <summary>
    /// Check if an object is currently tracked as in use.
    /// </summary>
    public static bool IsTracked(GameObject obj)
    {
        return Instance.inUseObjects.ContainsKey(obj);
    }

    /// <summary>
    /// Instantiate or reuse a pooled object at the specified position and rotation.
    /// </summary>
    public static GameObject InstantiatePooled(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // Get or create the object pool for the prefab
        if (!Instance.availableObjects.TryGetValue(prefab, out Queue<GameObject> objectPool))
        {
            objectPool = new Queue<GameObject>();
            Instance.availableObjects[prefab] = objectPool;
        }

        GameObject obj;

        // Reuse an object from the pool if available, otherwise create a new one
        if (objectPool.Count > 0)
        {
            obj = objectPool.Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, position, rotation);
        }

        // Track the object as in use
        Instance.inUseObjects[obj] = prefab;

        return obj;
    }

    /// <summary>
    /// Return a pooled object back to the pool.
    /// </summary>
    public static void DestroyPooled(GameObject objectToPool)
    {
        // Deactivate the object and detach it from any parent
        objectToPool.SetActive(false);
        objectToPool.transform.SetParent(null);

        // Return the object to its pool if it was tracked as in use
        if (Instance.inUseObjects.TryGetValue(objectToPool, out GameObject prefab))
        {
            Instance.inUseObjects.Remove(objectToPool);

            // Get or create the pool for the prefab
            if (!Instance.availableObjects.TryGetValue(prefab, out Queue<GameObject> objectPool))
            {
                objectPool = new Queue<GameObject>();
                Instance.availableObjects[prefab] = objectPool;
            }

            // Add the object back to the pool
            objectPool.Enqueue(objectToPool);
        }
        else
        {
            Debug.LogWarning($"ObjectPooler: Tried to pool an object {objectToPool.name} that is not being tracked.");
        }
    }
}