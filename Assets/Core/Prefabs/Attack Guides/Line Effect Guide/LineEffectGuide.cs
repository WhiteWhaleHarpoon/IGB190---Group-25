using Unity.VisualScripting;
using UnityEngine;

public class LineEffectGuide : MonoBehaviour
{
    [SerializeField] private MeshRenderer guideRenderer;
    private float spawnedAt;
    private Vector3 targetScale;

    public void Setup(float width, float length, float duration)
    {
        targetScale = new Vector3(width, 1, length);
        guideRenderer.material.color = new Color(1, 0.4f, 0.3f);
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, Time.time - spawnedAt * 3);
    }

    public static void Spawn(Vector3 position1, Vector3 position2, float width, float duration)
    {
        Vector3 look = position2 - position1;
        look.y = 0;
        float length = Vector3.Distance(position1, position2);
        Instantiate(GameManager.assets.lineEffectGuide, position1, Quaternion.LookRotation(look)).Setup(width, length, duration);
    }

    public static void Spawn (Unit unit, float width, float length, float duration)
    {
        Instantiate(GameManager.assets.lineEffectGuide, unit.transform.position, unit.transform.rotation).Setup(width, length, duration);
    }
}