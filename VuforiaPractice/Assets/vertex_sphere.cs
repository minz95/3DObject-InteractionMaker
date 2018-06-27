using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vertex_sphere : MonoBehaviour {
    public bool selected = false;
    void OnMouseDown()
    {
        Renderer rend = GetComponent<Renderer>();
        rend.material.color = Color.red;
        if (selected == false) selected = true;
        else selected = false;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
