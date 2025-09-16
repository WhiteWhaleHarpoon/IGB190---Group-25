using UnityEngine;

public class DestroyAt : MonoBehaviour
{
    private float time;

    void Update()
    {
        if (Time.time > time) 
        {
            ObjectPooler.DestroyPooled(gameObject);
        }
    }

    public void Run (float time)
    {
        this.time = time;
    }
}
