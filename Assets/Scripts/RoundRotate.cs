using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundRotate : MonoBehaviour {

    public GameObject Target;
    public float speed;
    public float radius;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {


        float x = Mathf.Cos(Time.time * speed) * radius;
        float z = Mathf.Sin(Time.time * speed) * radius;
        this.transform.localPosition = new Vector3(x, 0, z);
        this.transform.LookAt(Target.transform);// = Quaternion.LookRotation(Target.transform.position);
	}
}
