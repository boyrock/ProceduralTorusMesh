using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GPUProceduralTorus : MonoBehaviour {

    [SerializeField]
    Shader shader;

    [SerializeField]
    [Range(0, 3000)]
    int count;

    [SerializeField]
    [Range(0, 500)]
    int maxSegmentNum;

    [SerializeField]
    [Range(0, 20)]
    int numOfSides;

    [SerializeField]
    Material mat;

    ComputeBuffer indexBuffer;
    ComputeBuffer vertexBuffer;
    ComputeBuffer segmentBuffer;
    ComputeBuffer colorBuf;

    TorusVertex[] vertices;
    Segment[] segments;
    Color[] colors;

    Vector3[] points;
    int[] indices;

    [SerializeField]
    ComputeShader cs;

    public float radius { get; set; }
    public float length { get; set; }
    public float noiseFreq { get; set; }
    public float colorChangeSpeed { get; set; }

    [SerializeField]
    Gradient[] gradientColors;

    int totalVertexNum;
    int totalSegmentNum;

    int initSegment_kernelIdx;
    int updateVertex_kernelIdx;
    int applyNoise_kernelIdx;

    List<Color[]> colorsArr;

    // Use this for initialization
    void Start ()
    {
        length = 0.7f;
        noiseFreq = 0f;
        radius = 0.2f;
        colorChangeSpeed = 0.1f;

        InitKernelIndex();

        totalSegmentNum = (count * (maxSegmentNum + 1));
        totalVertexNum = count * (numOfSides * maxSegmentNum + numOfSides);
        indices = new int[3 * 2 * numOfSides * maxSegmentNum];

        Debug.Log("totalVertexNum : " + totalVertexNum);
        Debug.Log("totalSegmentNum : " + totalSegmentNum);
        Debug.Log("indexNum : " + indices.Length);

        vertices = new TorusVertex[totalVertexNum];
        segments = new Segment[totalSegmentNum];

        for (int i = 0; i < totalSegmentNum; i++)
            segments[i] = new Segment();

        for (int i = 0; i < totalVertexNum; i++)
            vertices[i] = new TorusVertex();

        SetIndices();
        InitBuffer();
        InitSegments();

        colorsArr = new List<Color[]>();
        for (int i = 0; i < gradientColors.Length; i++)
        {
            Gradient gradient = gradientColors[i];

            var colors = new Color[100];
            for (int j = 0; j < 100; j++)
            {
                colors[j] = gradient.Evaluate(j / 100f);
            }

            colorsArr.Add(colors);
        }
    }

    void ChangeColor(float t)
    {
        int fromIndex = (int)t;
        int toIndex = (int)t + 1 >= colorsArr.Count ? 0 : (int)t + 1;
        float tt = t % 1.0f;
        colors = LerpColors(colorsArr[fromIndex], colorsArr[toIndex], tt);
    }
    Color[] LerpColors(Color[] from, Color[] to, float t)
    {
        Color[] colors = new Color[from.Length];

        for (int i = 0; i < from.Length; i++)
        {
            colors[i] = Color.Lerp(from[i], to[i], t);
        }

        return colors;
    }


    private void InitKernelIndex()
    {
        initSegment_kernelIdx = cs.FindKernel("InitSegment");
        updateVertex_kernelIdx = cs.FindKernel("UpdateVertex");
        applyNoise_kernelIdx = cs.FindKernel("ApplyNoise");
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

                    var ii = lastVertexIndex * 6;

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
    }

    private void InitBuffer()
    {
        vertexBuffer = new ComputeBuffer(vertices.Length, Marshal.SizeOf(typeof(TorusVertex)));
        vertexBuffer.SetData(vertices);

        indexBuffer = new ComputeBuffer(indices.Length, sizeof(int));
        indexBuffer.SetData(indices);

        segmentBuffer = new ComputeBuffer(segments.Length, Marshal.SizeOf(typeof(Segment)));
        segmentBuffer.SetData(segments);

        colorBuf = new ComputeBuffer(count, Marshal.SizeOf(typeof(Color)));
    }

    private void InitSegments()
    {
        cs.SetFloat("_MaxSegment", maxSegmentNum);
        cs.SetBuffer(initSegment_kernelIdx, "_SegmentBuffer", segmentBuffer);
        cs.Dispatch(initSegment_kernelIdx, count / 16 + (count % 16), 1, 1);
    }

    private void OnRenderObject()
    {
        var color_t = Mathf.Repeat(Time.time * colorChangeSpeed, gradientColors.Length);
        ChangeColor(color_t);
        colorBuf.SetData(colors);

        cs.SetFloat("_MaxSegment", maxSegmentNum);
        cs.SetFloat("_NumOfSlide", numOfSides);
        cs.SetFloat("_T", length);
        cs.SetFloat("_Time", Time.time);
        cs.SetFloat("_Radius", radius);

        cs.SetBuffer(updateVertex_kernelIdx, "_VertexBuffer", vertexBuffer);
        cs.SetBuffer(updateVertex_kernelIdx, "_SegmentBuffer", segmentBuffer);
        cs.Dispatch(updateVertex_kernelIdx, totalVertexNum / 8 + (totalVertexNum % 8), 1, 1);

        cs.SetFloat("_NoiseFreq", noiseFreq);
        cs.SetBuffer(applyNoise_kernelIdx, "_SegmentBuffer", segmentBuffer);
        cs.Dispatch(applyNoise_kernelIdx, totalSegmentNum / 8 + (totalSegmentNum % 8), 1, 1);

        mat.SetPass(0);
        mat.SetBuffer("_IndexBuffer", indexBuffer);
        mat.SetBuffer("_VertexBuffer", vertexBuffer);
        mat.SetBuffer("_ColorBuffer", colorBuf);
        mat.SetInt("_NumVertexOfPerTorus", vertices.Length / count);
        mat.SetInt("_NumIndexOfPerTorus", indices.Length);

        Graphics.DrawProcedural(MeshTopology.Triangles, indices.Length, count);
    }

    // Update is called once per frame
    void Update () {
    }

    struct TorusVertex
    {
        public Vector3 pos;
        public Vector3 normal;
        public Vector2 uv;
    };

    struct Segment
    {
        public Vector3 initPos;
        public Vector3 pos;
        public Vector3 direction;
        public Vector3 normal;
    };
}
