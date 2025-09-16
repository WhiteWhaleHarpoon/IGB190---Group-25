using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ArcEffectGuide : MonoBehaviour
{
    [SerializeField] private MeshRenderer guideRenderer;
    private float spawnedAt;
    private Vector3 targetScale;

    public void Setup(float arc, float radius, float duration)
    {
        float size = radius * 2f;
        targetScale = new Vector3(size, 1, size);
        guideRenderer.material.color = new Color(1, 0.4f, 0.3f);
        guideRenderer.transform.localRotation = Quaternion.Euler(90, -0.5f * (180 - arc), 0);
        guideRenderer.material.SetFloat("_Angle", arc);
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, Time.time - spawnedAt * 3);
    }

    public static void Spawn (float arc, Unit unit, float radius, float duration)
    {
        Instantiate(GameManager.assets.arcEffectGuide, unit.transform.position, 
            unit.transform.rotation).Setup(arc, radius, duration);
    }
}
