using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomFog : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject obj = Instantiate(GameManager.assets.fog, transform.position, transform.rotation, transform);
        obj.transform.localScale = Vector3.one;
        int size = Mathf.RoundToInt(transform.localScale.x * transform.localScale.z);
        var system = obj.GetComponent<ParticleSystem>().emission;
        system.rateOverTime = size / 40.0f;

        obj.GetComponent<ParticleSystem>().Clear();
        obj.GetComponent<ParticleSystem>().Stop();
        obj.GetComponent<ParticleSystem>().Play();
    }
}
