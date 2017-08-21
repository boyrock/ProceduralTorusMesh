using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PathMoveCamera : MonoBehaviour {

    Vector3[] pathPoints;
    Camera camera;

	// Use this for initialization
	void Start () {
        camera = this.GetComponent<Camera>();
	}

    float t;
    float tt;
    [SerializeField]
    float speed;

    Vector3 prev_position;

	// Update is called once per frame
	void Update () {

        if (pathPoints == null)
            return;

        if (pathPoints.Length == 0)
            return;

        var prev_position = camera.transform.localPosition;

        t += Time.deltaTime * speed;
        //tt += Time.deltaTime * speed;

        if (t >= pathPoints.Length - 2)
        {
            t = 0;
        }

        SetPosition(t);
        SetRotation(t);
    }

    void SetPosition(float t)
    {
        if (t < 1.0f)
            return;

        prev_position = camera.transform.localPosition;

        var position = pathPoints[(int)t - 1];
        var next_position = pathPoints[(int)t];
        var next_next_position = pathPoints[(int)t + 1];

        var diff = t - ((int)t);
        camera.transform.localPosition = Vector3.Lerp(position, next_position, diff);
    }

    Vector3 prev_normal;
    Vector3 normal;

    void SetRotation(float t)
    {
        var position = pathPoints[(int)t];
        var next_position = pathPoints[(int)t + 1];
        var next_next_position = pathPoints[(int)t + 2];

        prev_normal = normal;
        var lookAt = Quaternion.LookRotation(next_position - position, prev_normal);
        normal = lookAt * new Vector3(0, 1.0f, 0);

        var rotation = Quaternion.LookRotation(next_position - position, normal);
        var next_rotation = Quaternion.LookRotation(next_next_position - next_position, normal);

        var diff = t - ((int)t);
        camera.transform.localRotation = Quaternion.Slerp(rotation, next_rotation, diff);
    }

    public void SetPath(Vector3[] points)
    {
        pathPoints = points;
    }
}
