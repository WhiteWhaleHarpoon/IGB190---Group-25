using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitSpawnEffect : MonoBehaviour
{
    public float effectDuration;
    public float destroyDelay = 0.5f;
    public GameObject endSpawnEffect;

    private void OnEnable()
    {
        Invoke("EndSpawnEffect", effectDuration + destroyDelay);
    }

    public void EndSpawnEffect ()
    {
        ObjectPooler.DestroyPooled(gameObject);
        if (endSpawnEffect != null)
        {
            GameObject obj = ObjectPooler.InstantiatePooled(endSpawnEffect, transform.position + Vector3.up, Quaternion.identity);
            DestroyAt destroyAt = obj.GetOrAddComponent<DestroyAt>();
            destroyAt.Run(Time.time + 3.0f);
        }
    }
}
