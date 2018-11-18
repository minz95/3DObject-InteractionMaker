using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class chain_behavior : MonoBehaviour {
    public bool selected = false;
    const int sphere_num = 1000;
    int vert_length = 0;
    Transform[] Spheres = new Transform[sphere_num];
    private Rigidbody rb;
    Vector3[] vertices;
    //int[] triangles;
    Renderer rend;
    float thrust = (float)-30;

    void OnMouseDown()
    {
        if (selected == false)
        {
            rb.AddRelativeForce(Vector3.up * thrust);
            selected = true;
        }
        else
        {
            rb.AddRelativeForce(Vector3.down * thrust);
            selected = false;
        }
        //rb.AddRelativeForce(Vector3.right * thrust);

        /*
        if (selected == false)
        {
            //Renderer rend = GetComponent<Renderer>();
            rend.material.color = Color.blue;
            
            //Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
            Transform tr = gameObject.transform;
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] = tr.TransformPoint(vertices[i]);
            }
            List<Vector3> r_verts = new List<Vector3>(vertices);
            
            Vector3[] verts = r_verts.Distinct<Vector3>().ToList().ToArray();
            //removeDuplicates(vertices);
            drawSpheres(verts);
            selected = true;
        }
        else
        {
            for(int i = 0; i < vert_length; i++)
            {
                Destroy(Spheres[i].gameObject);
            }
            selected = false;
        }
        */
    }

    Vector3[] removeDuplicates(Vector3[] dupArray)
    {

        Vector3[] newArray = new Vector3[dupArray.Length];  //change 8 to a variable dependent on shape
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

    public Transform vertex_sphere;
    void drawSpheres(Vector3[] verts)
    {
        //Transform[] Spheres = new Transform[verts.Length];
        vert_length = verts.Length;
        Debug.Log(vert_length);
        for (int i = 0; i < verts.Length; i++)
        {
            Spheres[i] = Instantiate(vertex_sphere, verts[i], Quaternion.identity);
            Spheres[i].GetComponent<Renderer>().sortingOrder = 0;
            //Spheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //Spheres[i].transform.position = verts[i];
            //Spheres[i].transform.localScale -= new Vector3(0.9F, 0.9F, 0.9F);
        }
        
        GameObject button = new GameObject("buttonObject");
        
        button.AddComponent<MeshFilter>();
        button.AddComponent<MeshCollider>();
        button.AddComponent<MeshRenderer>();
        Vector3[] b_vertices = new Vector3[4];
        b_vertices[0] = verts[verts.Length - 4];
        b_vertices[1] = verts[verts.Length - 3];
        b_vertices[2] = verts[verts.Length - 2];
        b_vertices[3] = verts[verts.Length - 1];
        
        Transform tr = gameObject.transform;
        /*
        for (int i = 0; i < 4; i++)
        {
            b_vertices[i] = tr.TransformPoint(b_vertices[i]);
        }
        */
        
        Mesh b_mesh = button.GetComponent<MeshFilter>().mesh;
        
        /*
        b_mesh.vertices = b_vertices;
        b_mesh.uv = new Vector2[3]{new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0)};
        b_mesh.triangles = new int[3] { 0, 1, 2 };
        b_mesh.RecalculateBounds();
        */
        button.transform.position = verts[verts.Length - 4];
        
        button.GetComponent<Renderer>().material.color = Color.green;
        button.GetComponent<Renderer>().sortingOrder = 0;
        //button.AddComponent<Rigidbody2D>();
        //button.GetComponent<Rigidbody2D>().AddForce(Vector2.down);

        Vector3[] n_vertices = new Vector3[verts.Length-4];
        Vector2[] n_uv = new Vector2[verts.Length - 4];

        for(int i = 0; i < verts.Length-4; i++)
        {
            n_vertices[i] = verts[i];
        }

        HashSet<int> vertsToRemove = new HashSet<int>();
        vertsToRemove.Add(verts.Length - 4);
        vertsToRemove.Add(verts.Length - 3);
        vertsToRemove.Add(verts.Length - 2);
        vertsToRemove.Add(verts.Length - 1);

        Mesh n_mesh = GetComponent<MeshFilter>().mesh;
        
        int[] n_triangles = new int[n_mesh.triangles.Length - 3];
        for (int i = 0; i < verts.Length - 4; i++)
        {
            n_vertices[i] = tr.TransformPoint(verts[i]);
            n_uv[i] = n_mesh.uv[i];
            n_triangles[i] = n_mesh.triangles[i];
        }
        
        int[] indices = n_mesh.GetTriangles(0);
        List<int> updatedIndices = new List<int>();
        List<int> b_indices = new List<int>();

        for (int index = 0; index < indices.Length; index += 3)
        {
            // Each triangle utilizes three slots in the index buffer, check to see if any of the
            // triangle indices contain a vertex that should be removed.
            if (vertsToRemove.Contains(indices[index]) ||
                vertsToRemove.Contains(indices[index + 1]) ||
                vertsToRemove.Contains(indices[index + 2]))
            {
                
                // Do nothing, we don't want to save this triangle...
            }
            else
            {
                updatedIndices.Add(indices[index]);
                updatedIndices.Add(indices[index + 1]);
                updatedIndices.Add(indices[index + 2]);
            }
            
            if(vertsToRemove.Contains(indices[index]) &&
                vertsToRemove.Contains(indices[index + 1]) &&
                vertsToRemove.Contains(indices[index + 2]))
            {
                b_indices.Add(indices[index]);
                b_indices.Add(indices[index + 1]);
                b_indices.Add(indices[index + 2]);
            }
        }

        n_mesh.SetTriangles(updatedIndices.ToArray(), 0);
        n_mesh.RecalculateBounds();

        b_mesh.SetVertices(new List<Vector3>(b_vertices));
        b_mesh.SetTriangles(b_indices.Distinct<int>().ToList().ToArray(), 0);
        b_mesh.RecalculateBounds();
        button.GetComponent<Renderer>().material = rend.material;

        /*
        n_mesh.Clear();
        n_mesh.uv = n_uv;
        n_mesh.triangles = n_triangles;
        n_mesh.vertices = n_vertices;

        n_mesh.RecalculateBounds();
        n_mesh.RecalculateNormals();
        */
    }

    public static void AutoWeld(Mesh mesh, float threshold, float bucketStep)
    {
        Vector3[] oldVertices = mesh.vertices;
        Vector3[] newVertices = new Vector3[oldVertices.Length];
        int[] old2new = new int[oldVertices.Length];
        int newSize = 0;

        // Find AABB
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < oldVertices.Length; i++)
        {
            if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
            if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
            if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
            if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
            if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
            if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
        }

        // Make cubic buckets, each with dimensions "bucketStep"
        int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
        int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
        int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
        List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

        // Make new vertices
        for (int i = 0; i < oldVertices.Length; i++)
        {
            // Determine which bucket it belongs to
            int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
            int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
            int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

            // Check to see if it's already been added
            if (buckets[x, y, z] == null)
                buckets[x, y, z] = new List<int>(); // Make buckets lazily

            for (int j = 0; j < buckets[x, y, z].Count; j++)
            {
                Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                if (Vector3.SqrMagnitude(to) < threshold)
                {
                    old2new[i] = buckets[x, y, z][j];
                    goto skip; // Skip to next old vertex if this one is already there
                }
            }

            // Add new vertex
            newVertices[newSize] = oldVertices[i];
            buckets[x, y, z].Add(newSize);
            old2new[i] = newSize;
            newSize++;

            skip:;
        }

        // Make new triangles
        int[] oldTris = mesh.triangles;
        int[] newTris = new int[oldTris.Length];
        for (int i = 0; i < oldTris.Length; i++)
        {
            newTris[i] = old2new[oldTris[i]];
        }

        Vector3[] finalVertices = new Vector3[newSize];
        for (int i = 0; i < newSize; i++)
            finalVertices[i] = newVertices[i];

        mesh.Clear();
        mesh.vertices = finalVertices;
        mesh.triangles = newTris;
        mesh.RecalculateNormals();
        //mesh.Optimize();
    }

    // Use this for initialization
    void Start () {
        vertices = GetComponent<MeshFilter>().mesh.vertices;
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        //triangles = GetComponent<MeshFilter>().mesh.triangles;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
