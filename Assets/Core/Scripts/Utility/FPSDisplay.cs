using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    private float deltaTime = 0.0f;

    void Update()
    {
        // Calculate delta time
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        // Only update the FPS text once per second
        if (Time.time % 2.0f <= Time.unscaledDeltaTime)
        {
            float msec = deltaTime * 1000.0f;
            float fps = 1f / deltaTime;
            string text = string.Format("FPS: {1:0.}", msec, fps);
            fpsText.text = text;
        }
    }
}