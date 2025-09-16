using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class BezierCurve
{
    private int numberOfControlPoints = 2;
    private Vector3[] controlPoints;

    public BezierCurve (Vector3 start, Vector3 end)
    {
        controlPoints = new Vector3[numberOfControlPoints + 2];
        controlPoints[0] = start;
        controlPoints[numberOfControlPoints + 1] = end;

        for (int i = 1; i <= numberOfControlPoints; i++)
        {
            float t = (float)i / (numberOfControlPoints + 1);
            controlPoints[i] = Vector3.Lerp(start, end, t);
            controlPoints[i] += Random.insideUnitSphere * 10f; // Adjust randomness as needed

            if (controlPoints[i].y < 0) controlPoints[i].y = Random.Range(0.5f, 4.0f);
            if (controlPoints[i].y > 5) controlPoints[i].y = Random.Range(0.5f, 4.0f);
        }
    }

    public Vector3 Evaluate(float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;

        Vector3 point = oneMinusT * oneMinusT * oneMinusT * controlPoints[0];
        point += 3f * oneMinusT * oneMinusT * t * controlPoints[1];
        point += 3f * oneMinusT * t * t * controlPoints[2];
        point += t * t * t * controlPoints[3];

        return point;
    }

    /*
    // Get a point on the bezier curve at a normalized time between 0 and 1
    public Vector3 Evaluate(float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;

        Vector3 point = oneMinusT * oneMinusT * oneMinusT * oneMinusT * controlPoints[0];
        point += 5f * oneMinusT * oneMinusT * oneMinusT * t * controlPoints[1];
        point += 10f * oneMinusT * oneMinusT * t * t * controlPoints[2];
        point += 10f * oneMinusT * t * t * t * controlPoints[3];
        point += t * t * t * t * controlPoints[4];

        return point;
    }
    */
}