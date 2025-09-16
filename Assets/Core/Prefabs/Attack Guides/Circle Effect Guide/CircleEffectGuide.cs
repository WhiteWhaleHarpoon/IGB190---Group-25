using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleEffectGuide : MonoBehaviour
{
    [SerializeField] private MeshRenderer guideRenderer;
    private float spawnedAt;
    private Vector3 targetScale;
    private bool isSetup = false;

    //public float radius;
    public float duration = 8;

    private Color color;

    public void Awake()
    {
        if (!isSetup)
        {
            Setup(transform.localScale.x / 2.0f, duration, new Color(1.0f, 0.6f, 0.6f));
        }
    }

    public void Setup(float radius, float duration)
    {
        Setup(radius, duration, new Color(1.0f, 0.6f, 0.6f));
        
    }

    public void Setup(float radius, float duration, Color color)
    {
        isSetup = true;
        float size = radius * 2f;
        targetScale = new Vector3(size, 1, size);
        transform.localScale = targetScale;
        this.color = color;
        guideRenderer.material.color = color;
        spawnedAt = Time.time;
        guideRenderer.enabled = false;
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        if (!guideRenderer.enabled) guideRenderer.enabled = true;
        transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, (Time.time - spawnedAt) * 3);

        guideRenderer.material.color = Color.Lerp(new Color(color.r, color.g, color.b, 0), color, duration - (Time.time - spawnedAt)   );
        
    }

    public static void Spawn(Vector3 position, float radius, float duration)
    {
        Instantiate(GameManager.assets.circleEffectGuide, position, Quaternion.identity).Setup(radius, duration);
    }
}
