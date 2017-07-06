using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurlFlow : MonoBehaviour
{
    [SerializeField]
    float radius;

    [SerializeField]
    ProceduralTorus torusPrefabs;

    [SerializeField]
    Gradient color;

    [SerializeField]
    int count;

    // Use this for initialization
    void Start()
    {
        for (int j = 0; j < count; j++)
        {
            Vector3 pos = Random.insideUnitCircle * radius;
            pos = new Vector3(pos.x * 3f, 0, pos.y);
            var torus = Instantiate<ProceduralTorus>(torusPrefabs);
            torus.SetColor(color.Evaluate((float)j / count) * 1f);
            torus.transform.SetParent(this.transform);
            torus.transform.localPosition = pos;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
