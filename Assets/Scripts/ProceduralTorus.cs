using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralTorus : MonoBehaviour {

    List<int> triangleList;
    List<Vector3> vertexList;
    List<Vector3> normals;
    List<Vector2> uvList;

    [SerializeField]
    [Range(0,64000)]
    int maxNumVertices;
    [SerializeField]
    [Range(0, 500)]
    int maxNumSegments;
    [SerializeField]
    [Range(0, 100)]
    int numberOfSides;
    [SerializeField]
    [Range(0, 10)]
    float radius;

    int count;
    int lastVertexIndex;
    Vector3 prev_position;
    Vector3 prev_normal;

    MeshFilter filter;

    CurlNoiseGenerator curlNoise;

    Vector3 velocity;


    // Use this for initialization
    void Start ()
    {
        //global_value += 0.0001f;

        //rand = global_value;// Random.Range(-0.1f, 0.1f);

        velocity = new Vector3(0, 0.06f,0f );
        curlNoise = new CurlNoiseGenerator();

        filter = gameObject.GetComponent<MeshFilter>();
        if (filter == null) filter = gameObject.AddComponent<MeshFilter>();

        Generate();
    }

    internal void SetColor(Color color)
    {
        this.GetComponent<Renderer>().material.SetColor("_Color", color);
    }

    private void SetMesh()
    {
        // Get mesh or create one
        var mesh = filter.sharedMesh;
        if (mesh == null)
            mesh = filter.sharedMesh = new Mesh();
        else
            mesh.Clear();

        // Assign vertex data
        mesh.vertices = vertexList.ToArray();
        mesh.uv = uvList.ToArray();
        mesh.triangles = triangleList.ToArray();
        mesh.normals = normals.ToArray();
    }

    void Generate()
    {
        prev_normal = Vector3.up;

        if (vertexList == null)
        {
            vertexList = new List<Vector3>();
            uvList = new List<Vector2>();
            normals = new List<Vector3>();
            triangleList = new List<int>();
        }
        else
        {
            vertexList.Clear();
            uvList.Clear();
            normals.Clear();
            triangleList.Clear();
        }

        count = 0;

        //prev_position = GetPosition(500 - 1);
        prev_position = Vector3.zero;
        var position = GetPosition(0);
        var normal = GetNormal(position);

        Branch(position, normal);

        SetMesh();
    }

    Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion angle)
    {
        return angle * (point - pivot) + pivot;
    }

    Vector3 GetNormal(Vector3 position)
    {
        var direction = (prev_position - position).normalized;

        if (direction.sqrMagnitude == 0)
            direction = Vector3.up;

        var lookAt = Quaternion.LookRotation(direction, prev_normal);

        var n = lookAt * Vector3.up;

        return n;
    }

    [SerializeField]
    [Range(0,3f)]
    float aa;

    [SerializeField]
    [Range(0, 1f)]
    float bb;
    Vector3 GetPosition(float tt)
    {
        int nlongitude = 50;
        int nmeridian = 60;
        float NSEGMENTS = 5000;
        var t = tt * Mathf.PI * 2f * nmeridian / NSEGMENTS;

        float x = 0;
        float y = tt * 0.005f;
        float z = 0;

        //x = Mathf.Cos(t) * (1.0f + Mathf.Cos(nlongitude * t / nmeridian) / 2.0f);
        //y = Mathf.Sin(t) * (1.0f + Mathf.Cos(nlongitude * t / nmeridian) / 2.0f);
        //z = Mathf.Sin(nlongitude * t / nmeridian) / 2.0f;


        //particle[i].force.x += Mathf.PerlinNoise(prev_position.x * aa, prev_position.y * aa, 1.352 + time);
        //particle[i].force.x += Mathf.PerlinNoise(prev_position.x * aa, prev_position.y * aa, 12.814 + time);

        var p = new Vector3(x, y, z);
        //var noise = curlNoise.Noise2D((this.transform.position + prev_position) * aa) * bb;
        var noise = curlNoise.Noise3D((this.transform.position + prev_position) * aa) * bb;

        //var nnoise = new Vector3(noise.x, noise.y, 0) + velocity;
        var nnoise = noise + velocity;

        //noise += new Vector3(rand, 0, 0);
        //float x = t * 0.2f;// Mathf.Cos(t) * (1.0f + Mathf.Cos(nlongitude * t / nmeridian) / 2.0f);
        //float y = Mathf.Sin(t * 1f);
        //float z = Mathf.Cos(t * 1f);

        return (prev_position + new Vector3(nnoise.x, nnoise.y, nnoise.z));
    }

    float rand;

    void Branch(Vector3 position, Vector3 nor)
    {
        var texCoord = Vector2.zero;

        //Debug.Log("radius : " + radius);
        var direction = (prev_position - position).normalized;
        if (direction.sqrMagnitude == 0)
            direction = Vector3.down;

        //Debug.Log("direction : " + direction);

        texCoord.y = (float)count / (float)(maxNumSegments);
        for (int j = 0; j < numberOfSides; j++)
        {
            var rr = Quaternion.AngleAxis(j * (360f / numberOfSides), direction);
            var p = RotateAroundPoint(position + nor * radius, position, rr);
            vertexList.Add(p);
            normals.Add((p - position).normalized);
            texCoord.x = (float)(j) / (float)(numberOfSides - 1);

            uvList.Add(texCoord);
        }

        if (count > 0)
        {
            for (var currentRingVertexIndex = vertexList.Count - numberOfSides; currentRingVertexIndex < vertexList.Count; currentRingVertexIndex++)
            {
                var p00 = (lastVertexIndex + 1) >= vertexList.Count - numberOfSides ? vertexList.Count - (numberOfSides * 2) : (lastVertexIndex + 1);
                var p01 = (lastVertexIndex);
                var p02 = (currentRingVertexIndex);

                triangleList.Add(p00); // Triangle A
                triangleList.Add(p01);
                triangleList.Add(p02);

                var p10 = currentRingVertexIndex;
                var p11 = (currentRingVertexIndex + 1) >= vertexList.Count ? vertexList.Count - numberOfSides : currentRingVertexIndex + 1;
                var p12 = lastVertexIndex + 1 >= vertexList.Count - numberOfSides ? vertexList.Count - (numberOfSides * 2) : (lastVertexIndex + 1);

                triangleList.Add(p10); // Triangle B
                triangleList.Add(p11);
                triangleList.Add(p12);

                lastVertexIndex++;
            }
        }

        count++;

        prev_position = position;

        var newPos = GetPosition(count);

        lastVertexIndex = vertexList.Count - numberOfSides;

        prev_normal = nor;

        Vector3 normal = GetNormal(newPos);

        if (count > maxNumSegments || vertexList.Count + numberOfSides >= maxNumVertices)
            return;

        Branch(newPos, normal); // Next segment
    }

    void OnDisable()
    {
        if (filter.sharedMesh == null) return;
        DestroyImmediate(filter.sharedMesh, true);
    }


    //private void OnDrawGizmos()
    //{
    //    if(normals != null)
    //    {
    //        for (int i = 0; i < normals.Count; i++)
    //        {
    //            Gizmos.color = Color.white;
    //            Gizmos.DrawSphere(vertexList[i], 0.5f);
    //            Gizmos.DrawLine(vertexList[i], vertexList[i] + normals[i] * 4);
    //            //Gizmos.color = Color.red;
    //            //Gizmos.DrawLine(vertexList[i], vertexList[i] + tangents[i] * 20f);
    //        }
    //    }
    //}

    // Update is called once per frame
    void Update () {

        //var lookAt = Quaternion.LookRotation(new Vector3(0, 1, 0), new Vector3(0, 1, 0));

        //Debug.Log("lookAt : " + lookAt);
        //maxNumSegments = (int)(((Mathf.Sin(Time.time * 0.5f) + 1) * 0.5f) * 500);
        //Generate();
    }
}
