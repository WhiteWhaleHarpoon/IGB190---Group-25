using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCurve : MonoBehaviour
{
    BezierCurve curve;
    float time;

    public Vector3 startOffset = new Vector3(0, 8, 0);
    public Vector3 stopOffset = new Vector3(0, 1, 0);

    void OnEnable()
    {
        Vector3 start = transform.position + startOffset;
        curve = new BezierCurve(start, transform.position + stopOffset);
        time = Time.time;
        transform.position = start;
        GetComponent<TrailRenderer>().Clear();
    }

    void Update()
    {
        float normalized = (Time.time - time) / 2.0f;
        transform.position = curve.Evaluate(normalized);
    }
}
