using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GPUProceduralTorus : MonoBehaviour {

    [SerializeField]
    Shader shader;

    [SerializeField]
    int count;

    [SerializeField]
    [Range(0, 1000)]
    int maxSegmentNum;

    [SerializeField]
    [Range(0, 20)]
    int numOfSides;

    Material _mat;
    Material mat
    {
        get
        {
            if(_mat == null)
            {
                _mat = new Material(shader);
            }

            return _mat;
        }
    }

    ComputeBuffer indexBuffer;
    ComputeBuffer vertexBuffer;
    ComputeBuffer segmentBuffer;
    TorusVertex[] vertices;
    Segment[] segments;

    Vector3[] points;
    int[] indices;

    [SerializeField]
    float radius;

    CurlNoiseGenerator curlNoise;
    Vector3 velocity;

    [SerializeField]
    [Range(0, 3f)]
    float aa;

    [SerializeField]
    [Range(0, 1f)]
    float bb;

    [SerializeField]
    ComputeShader cs;

    int totalVertexNum;

    [SerializeField]
    bool OnGPU;

    // Use this for initialization
    void Start ()
    {
        totalVertexNum = count * (numOfSides * maxSegmentNum + numOfSides);

        Debug.Log("totalVertexNum : " + totalVertexNum);

        velocity = new Vector3(0, 0f, 0f);
        curlNoise = new CurlNoiseGenerator();

        indices = new int[3 * 2 * numOfSides * maxSegmentNum];
        vertices = new TorusVertex[totalVertexNum];
        segments = new Segment[maxSegmentNum];



        if (OnGPU)
        {
            for (int i = 0; i < maxSegmentNum; i++)
            {
                var seg = new Segment();
                seg.pos = new Vector3(i, i, 0);
                seg.prev_pos = Vector3.zero;
                segments[i] = seg;
            }

            for (int i = 0; i < totalVertexNum; i++)
            {
                var tv = new TorusVertex();
                tv.pos = Vector3.zero;
                tv.prev_pos = new Vector3(0, -1f, 0);// Vector3.up;
                vertices[i] = tv;
                //Vector3 pos = Random.insideUnitCircle * 5;
                //pos = new Vector3(pos.x * 3f, 0, pos.y);

                //SetVertexPoint(i, pos);
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 pos;// = Random.insideUnitCircle * 5;
                pos = new Vector3(0, 0, 0);

                SetVertexPoint(i, pos);
            }
        }
        

        //for (int i = 0; i < points.Length; i++)
        //{
        //    Debug.Log(string.Format("{0}_{1}", i, points[i]));
        //}

        SetIndices();
        InitBuffer();


    }

    private void SetIndices()
    {
        int lastVertexIndex = 0;
        int v_count = 0;
        for (int i = 0; i <= maxSegmentNum; i++)
        {
            v_count = (i + 1) * numOfSides;

            if (i > 0)
            {
                for (var currentRingVertexIndex = v_count - numOfSides; currentRingVertexIndex < v_count; currentRingVertexIndex++)
                {
                    var p00 = (lastVertexIndex + 1) >= v_count - numOfSides ? v_count - (numOfSides * 2) : (lastVertexIndex + 1);
                    var p01 = (lastVertexIndex);
                    var p02 = (currentRingVertexIndex);

                    var ii = lastVertexIndex * 6;// (i - 1) * 6 * (lastVertexIndex + 1);

                    //Debug.Log("index : " + index);
                    //Debug.Log("ii : " + ii);
                    indices[ii + 0] = p00; // Triangle A
                    indices[ii + 1] = p01;
                    indices[ii + 2] = p02;

                    var p10 = currentRingVertexIndex;
                    var p11 = (currentRingVertexIndex + 1) >= v_count ? v_count - numOfSides : currentRingVertexIndex + 1;
                    var p12 = lastVertexIndex + 1 >= v_count - numOfSides ? v_count - (numOfSides * 2) : (lastVertexIndex + 1);

                    indices[ii + 3] = p10; // Triangle B
                    indices[ii + 4] = p11;
                    indices[ii + 5] = p12;

                    lastVertexIndex++;
                }
            }
        }

        //for (int i = 0; i < indices.Length; i++)
        //{
        //    Debug.Log(string.Format("{0}_{1}", i, indices[i]));
        //}
    }

    Vector3 GetNormal(Vector3 prev_position, Vector3 position, Vector3 prev_normal)
    {
        var direction = (prev_position - position).normalized;

        if (direction.sqrMagnitude == 0)
            direction = Vector3.up;

        var lookAt = Quaternion.LookRotation(direction, prev_normal);

        Debug.Log("direction : " + direction);
        Debug.Log("prev_normal : " + prev_normal);
        Debug.Log("lookAt : " + lookAt);
        var n = lookAt * Vector3.up;

        Debug.Log("n : " + n);
        return n;
    }


    void SetVertexPoint(int index, Vector3 initPos)
    {
        //Debug.Log("points length: " + points.Length);
        //Debug.Log("indices length: " + indices.Length);

        Vector3 prev_position = initPos + new Vector3(0, -1, 0);

        Vector3 prev_normal = Vector3.up;


        for (int i = 0; i <= maxSegmentNum; i++)
        {

            var noise = curlNoise.Noise3D((initPos + prev_position) * aa) * bb;

            //var nnoise = new Vector3(noise.x, noise.y, 0) + velocity;
            var nnoise = noise + velocity;

            //var position = new Vector3(i, Mathf.Sin(i * 0.1f) * 10f, 0);
            var position = new Vector3(0, i, 0);// prev_position + nnoise;// new Vector3(0, i, 0);

            var direction = (prev_position - position).normalized;
            if (direction.sqrMagnitude == 0)
                direction = Vector3.down;

            //if (i == 0)
            //    prev_position = Vector3.zero;

            var nor = GetNormal(prev_position, position, prev_normal);

            Debug.Log("nor : " + nor);
            //Debug.Log("position : " + prev_position);
            for (int j = 0; j < numOfSides; j++)
            {
                TorusVertex tv = new TorusVertex();

                var rr = Quaternion.AngleAxis(j * (360f / numOfSides), new Vector3(0,-1,0));


                Debug.Log("angle : " + j * (360f / numOfSides));
                Vector3 pos = RotateAroundPoint(position + nor * radius, position, rr);

                tv.pos = pos;
                tv.normal = (pos - position).normalized;
                tv.uv.x = (float)(j) / (float)(numOfSides - 1);

                vertices[(index * (numOfSides * maxSegmentNum + numOfSides)) + (i * numOfSides) + j] = tv;
            }

            prev_position = position;
            prev_normal = nor;
        }
    }

    Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion angle)
    {
        return angle * (point - pivot) + pivot;
    }

    private void InitBuffer()
    {
        vertexBuffer = new ComputeBuffer(vertices.Length, Marshal.SizeOf(typeof(TorusVertex)));
        vertexBuffer.SetData(vertices);

        indexBuffer = new ComputeBuffer(indices.Length, sizeof(int));
        indexBuffer.SetData(indices);

        segmentBuffer = new ComputeBuffer(segments.Length, Marshal.SizeOf(typeof(Segment)));
        segmentBuffer.SetData(segments);
    }

    private void OnRenderObject()
    {


        if (OnGPU)
        {
            cs.SetFloat("_MaxSegment", maxSegmentNum);
            cs.SetFloat("_NumOfSlide", numOfSides);
            cs.SetFloat("_Radius", radius);
            //segmentBuffer.GetData(segments);

            //for (int i = 0; i < segments.Length; i++)
            //{
            //    Debug.Log("segment pos : " + segments[i].pos);
            //}

            cs.SetBuffer(0, "_VertexBuffer", vertexBuffer);
            cs.SetBuffer(0, "_SegmentBuffer", segmentBuffer);
            cs.Dispatch(0, totalVertexNum / 8 + (totalVertexNum % 8), 1, 1);

            cs.SetBuffer(1, "_SegmentBuffer", segmentBuffer);
            cs.Dispatch(1, maxSegmentNum / 8 + (maxSegmentNum % 8), 1, 1);
            //vertexBuffer.GetData(vertices);

            //for (int i = 0; i < vertices.Length; i++)
            //{
            //    Debug.Log("vertex pos : " + vertices[i].pos);
            //}
        }

        mat.SetPass(0);
        mat.SetBuffer("_IndexBuffer", indexBuffer);
        mat.SetBuffer("_VertexBuffer", vertexBuffer);
        mat.SetFloat("_T", t);
        mat.SetInt("_NumVertexOfPerTorus", vertices.Length / count);
        mat.SetInt("_NumIndexOfPerTorus", indices.Length);

        //var col = color.Evaluate((float)j / count) * 1f;
        mat.SetColor("_Color", color);

        Graphics.DrawProcedural(MeshTopology.Triangles, indices.Length, count);
    }

    [SerializeField]
    [Range(0,1f)]
    float t;

    [SerializeField]
    Color color;
    // Update is called once per frame
    void Update () {

        //var lookAt = QuaternionLookRotation(new Vector3(0, 1, 0), new Vector3(0, 1, 0));

        //var forward = new Vector3(0, 1, 0);
        //var up = new Vector3(0, 1, 0);
        //Vector3 vector = Vector3.Normalize(forward);
        //Vector3 vector2 = Vector3.Normalize(Vector3.Cross(up, vector));
        //Vector3 vector3 = Vector3.Cross(vector, vector2);

        //Debug.Log("lookAt : " + lookAt);
        //Debug.Log("lookAt UP: " + lookAt * new Quaternion(0,1,0,0));
        //Debug.Log("vector : " + vector);
        //Debug.Log("vector2 : " + vector2);
        //Debug.Log("vector3 : " + vector3);

    }

    private static Quaternion QuaternionLookRotation(Vector3 forward, Vector3 up)
    {
        forward.Normalize();

        Vector3 vector = Vector3.Normalize(forward);
        Vector3 vector2 = Vector3.Normalize(Vector3.Cross(up, vector));
        Vector3 vector3 = Vector3.Cross(vector, vector2);
        var m00 = vector2.x;
        var m01 = vector2.y;
        var m02 = vector2.z;
        var m10 = vector3.x;
        var m11 = vector3.y;
        var m12 = vector3.z;
        var m20 = vector.x;
        var m21 = vector.y;
        var m22 = vector.z;


        float num8 = (m00 + m11) + m22;
        var quaternion = new Quaternion();
        if (num8 > 0f)
        {
            var num = (float)Math.Sqrt(num8 + 1f);
            quaternion.w = num * 0.5f;
            num = 0.5f / num;
            quaternion.x = (m12 - m21) * num;
            quaternion.y = (m20 - m02) * num;
            quaternion.z = (m01 - m10) * num;
            return quaternion;
        }
        if ((m00 >= m11) && (m00 >= m22))
        {
            var num7 = (float)Math.Sqrt(((1f + m00) - m11) - m22);
            var num4 = 0.5f / num7;
            quaternion.x = 0.5f * num7;
            quaternion.y = (m01 + m10) * num4;
            quaternion.z = (m02 + m20) * num4;
            quaternion.w = (m12 - m21) * num4;
            return quaternion;
        }
        if (m11 > m22)
        {
            var num6 = (float)Math.Sqrt(((1f + m11) - m00) - m22);
            var num3 = 0.5f / num6;
            quaternion.x = (m10 + m01) * num3;
            quaternion.y = 0.5f * num6;
            quaternion.z = (m21 + m12) * num3;
            quaternion.w = (m20 - m02) * num3;
            return quaternion;
        }
        var num5 = (float)Math.Sqrt(((1f + m22) - m00) - m11);
        var num2 = 0.5f / num5;
        quaternion.x = (m20 + m02) * num2;
        quaternion.y = (m21 + m12) * num2;
        quaternion.z = 0.5f * num5;
        quaternion.w = (m01 - m10) * num2;
        return quaternion;
    }

    struct TorusVertex
    {
        public Vector3 pos;
        public Vector3 prev_pos;
        public Vector3 normal;
        public Vector3 prev_normal;
        public Vector2 uv;
    };

    struct Segment
    {
        public Vector3 pos;
        public Vector3 prev_pos;
    };
}
