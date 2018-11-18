using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPhysics : MonoBehaviour {
    Rigidbody rigid;
    float thrust = (float)-30;
    public bool selected = false;

    void OnMouseDown()
    {
        if (selected == false)
        {
            rigid.AddRelativeForce(Vector3.left * thrust);
            selected = true;
        }
        else
        {
            rigid.AddRelativeForce(Vector3.right * thrust);
            selected = false;
        }
    }

    // Use this for initialization
    void Start () {
        rigid = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        //rigid.AddRelativeTorque(Vector3.down * thrust);
	}
}
