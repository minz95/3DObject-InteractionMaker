using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {

    void OnMouseDown()
    {
        Renderer rend = GetComponent<Renderer>();
        rend.material.color = Color.blue;
        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
        Transform tr = gameObject.transform;
        for (int i = 0; i < vertices.Length; ++i)
        {
            vertices[i] = tr.TransformPoint(vertices[i]);
        }
        Vector3[] verts = removeDuplicates(vertices);
        drawSpheres(verts);
    }

    Vector3[] removeDuplicates(Vector3[] dupArray)
    {

        Vector3[] newArray = new Vector3[8];  //change 8 to a variable dependent on shape
        bool isDup = false;
        int newArrayIndex = 0;
        for (int i = 0; i < dupArray.Length; i++)
        {
            for (int j = 0; j < newArray.Length; j++)
            {
                if (dupArray[i] == newArray[j])
                {
                    isDup = true;
                }
            }
            if (!isDup)
            {
                newArray[newArrayIndex] = dupArray[i];
                newArrayIndex++;
                isDup = false;
            }
        }
        return newArray;
    }

    void drawSpheres(Vector3[] verts)
    {
        GameObject[] Spheres = new GameObject[verts.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            Spheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Spheres[i].transform.position = verts[i];
            Spheres[i].transform.localScale -= new Vector3(0.8F, 0.8F, 0.8F);
        }
    }

    // Use this for initialization
    void Start () {
        MeshFilter meshFilter = (MeshFilter)gameObject.GetComponent("MeshFilter");
        //meshFilter.transform.position = new Vector3((float)1.381, (float)0.943, (float)1.993);
        Mesh theMesh = meshFilter.mesh;
        Vector3[] vertices = theMesh.vertices;
        //float threshold = 0.1f;
        /*
        List<GameObject> spheres = new List<GameObject>();

        GameObject cubic = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //sphere.transform.position = new Vector3(0, 0, 0);
        MeshFilter c_mesh = (MeshFilter)cubic.GetComponent("MeshFilter");
        c_mesh.transform.position = new Vector3(0, 0, 0);
        Vector3[] c_vertices = c_mesh.mesh.vertices;
        Vector3[] n_vertices = new Vector3[(int)((float)vertices.Length / 2.0)];
        //GameObject[] spheres = new GameObject[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)// Vector3 vert in vertices)
        {
            //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //sphere.transform.position = vert;
            if (i / 2 == 0)
                c_mesh.mesh.vertices[i] = vertices[i];
            else {
                vertices[i].x *= (float)0.5;
                vertices[i].y *= (float)0.5;
                vertices[i].z *= (float)0.5;
                n_vertices[i] = vertices[i];
            }
            //Debug.Log(vert);
        }

        meshFilter.mesh.vertices = n_vertices;
        */
    }
	
	// Update is called once per frame
	void Update () {
        /*
        MeshFilter meshFilter = (MeshFilter)gameObject.GetComponent("MeshFilter");
        Mesh theMesh = meshFilter.mesh;
        Vector3[] vertices = theMesh.vertices;
        //float threshold = 0.1f;
        List<GameObject> spheres = new List<GameObject>();

        //GameObject[] spheres = new GameObject[vertices.Length];
        foreach(Vector3 vert in vertices)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = vert;
            spheres.Add(sphere);
        }
        */
        /*
        for (int i = 0; i < theMesh.vertices.Length; i++)
        {
            Debug.Log(theMesh.vertices[i]);
        }
        */
    }
}
