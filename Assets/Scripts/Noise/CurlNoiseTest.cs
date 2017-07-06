using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurlNoiseTest : MonoBehaviour {

    [SerializeField ]
    GameObject targetPrefabs;

    GameObject go;
    [SerializeField]
    [Range(0,2f)]
    float speed;
    CurlNoiseGenerator curlNoiseGen;
    // Use this for initialization
    void Start ()
    {
        curlNoiseGen = new CurlNoiseGenerator();
        go = Instantiate(targetPrefabs);
	}
	
	// Update is called once per frame
	void Update () {

        var x = Mathf.Cos(Time.time * speed);
        var y = Mathf.Sin(Time.time * speed);
        go.transform.localPosition += new Vector3(x, y, 0);
        var curlNoise = curlNoiseGen.Noise3D(go.transform.localPosition * 0.05f) * 0.6f;
        //curlNoise += new Vector3(0.1f, 0, 0);
        go.transform.localPosition += curlNoise * 1f;
        go.transform.localScale = this.transform.localScale;
    }
}
