using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class ObjectBehavior : MonoBehaviour {
    public bool m_selected = false;
    GameSystem m_system;
    const int sphere_num = 1000;
    int vert_length = 0;
    Transform[] Spheres = new Transform[sphere_num];
    List<Vector3> m_vertices;
    List<Vector2> m_uvs;
    List<Vector3> m_normals;
    int[] m_triangles;

    Renderer m_rend;
    Mesh m_mesh;

    LayerMask layer_mask;

    void OnMouseDown()
    {
        switch(m_system.GetMode())
        {
            case 0:
                break;
            case 1: // select object
                if (m_selected == false)
                {
                    //m_rend.material.color = Color.blue;

                    Transform tr = gameObject.transform;
                    for (int i = 0; i < m_vertices.Count; ++i)
                    {
                        m_vertices[i] = tr.TransformPoint(m_vertices[i]);
                    }
                    List<Vector3> r_verts = new List<Vector3>(m_vertices);

                    Vector3[] verts = r_verts.Distinct().ToList().ToArray();
                    //removeDuplicates(vertices);
                    drawSpheres(verts);
                    m_system.SetCurrentObject(gameObject);
                    m_system.SetMode(2);
                    m_selected = true;
                }

                // change the layer of the object
                gameObject.layer = 2;

                break;
            case 2: // select vertices to split

                break;
            case 3: // physics dropdown
                break;
            case 4: // select vertices for physics
                break;
            case 5: // material dropdown
                break;
        }
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
            // how can we solve the occlusion problem here?
            Spheres[i].GetComponent<Renderer>().sortingOrder = 0;
            Spheres[i].GetComponent<Renderer>().material.renderQueue = 2001;
            //Spheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //Spheres[i].transform.position = verts[i];
            //Spheres[i].transform.localScale -= new Vector3(0.9F, 0.9F, 0.9F);
        }
    }

    public void ClearSpheres()
    {
        for (int i = 0; i < vert_length; i++)
        {
            Destroy(Spheres[i].gameObject);
        }

        m_selected = false;
        gameObject.layer = 0;
        
    }

    /*
     * MatchVertex
     * delegate function used in DivideMesh
     * true: vertex match, false: vertex does not match
     */
    static Predicate<Vector3> MatchVertex(Vector3 new_v)
    {
        return delegate (Vector3 curr_v)
        {
            return (curr_v[0] == new_v[0] &&
                    curr_v[1] == new_v[1] &&
                    curr_v[2] == new_v[2]);
        };
    }

    /*
     * DivideMeshes
     * in: s_vertices (selected vertices by the user)
     * divide meshes (correcting uvs, vertices, triangles)
     */
    public GameObject DivideMeshes(List<Vector3> s_vertices)
    {
        // data for making a new object
        List<int> n_idx = new List<int>();
        List<Vector2> n_uvs = new List<Vector2>();
        List<int> n_triangles = new List<int>();
        List<Vector3> n_normals = new List<Vector3>();
        
        // remaining results (removed selected vertices from the original data)
        List<int> r_triangles = new List<int>();
        List<Vector3> r_vertices = new List<Vector3>();
        List<Vector3> r_normals = new List<Vector3>();
        List<Vector2> r_uvs = new List<Vector2>();

        // list for storing the info: how many vertices are deleted before the given index
        List<int> delete_num = new List<int>(m_vertices.Count);

        // find selected vertices in m_vertices & remember the indices of those vertices
        for (int i = 0; i < s_vertices.Count; i++)
        {
            int idx = m_vertices.FindIndex(MatchVertex(s_vertices[i]));
            n_idx.Add(idx);
            n_uvs.Add(m_uvs[idx]);
            n_normals.Add(m_normals[idx]);
            Debug.Log(s_vertices[i]);
        }

        // count the delete number of each index
        for (int i = 0; i < m_vertices.Count; i++)
        {
            delete_num.Add(0);
        }
        for (int i = 0; i < m_vertices.Count; i++)
        {
            if(n_idx.Contains(i))
            {
                for(int j = i; j < m_vertices.Count; j++)
                {
                    delete_num[j]++;
                }
            }
        }

        // handle triangles
        for (int index = 0; index < m_triangles.Length; index += 3)
        {
            // triangle indices contain a vertex that should be removed.
            

            // if all vertices of the triangle are contained in the selected vertices,
            // this triangle can be contained in the new triangle list
            if (n_idx.Contains(m_triangles[index]) &&
                n_idx.Contains(m_triangles[index + 1]) &&
                n_idx.Contains(m_triangles[index + 2]))
            {
                n_triangles.Add(Array.IndexOf(n_idx.ToArray(), m_triangles[index]));
                n_triangles.Add(Array.IndexOf(n_idx.ToArray(), m_triangles[index + 1]));
                n_triangles.Add(Array.IndexOf(n_idx.ToArray(), m_triangles[index + 2]));

                // triangels above should be removed from the original triangle array

            }
            else
            {
                int next_i = r_vertices.Count;
                for(int i = 0; i < 3; i++)
                {
                    if(!r_vertices.Contains(m_vertices[m_triangles[index+i]]))
                    {
                        r_vertices.Add(m_vertices[m_triangles[index + i]]);
                        r_normals.Add(m_normals[m_triangles[index + i]]);
                        r_uvs.Add(m_uvs[m_triangles[index + i]]);
                        r_triangles.Add(next_i);
                        next_i++;
                    }
                    else
                    {
                        int temp_i = r_vertices.IndexOf(m_vertices[m_triangles[index + i]]);
                        r_triangles.Add(temp_i);
                    }
                        
                }
                /*
                r_vertices.Add(m_vertices[m_triangles[index]]);
                r_vertices.Add(m_vertices[m_triangles[index + 1]]);
                r_vertices.Add(m_vertices[m_triangles[index + 2]]);

                // keep these triangles in the original array
                // will change these indices later ('cause some of the vertices will be deleted)
                r_triangles.Add(m_triangles[index]);
                r_triangles.Add(m_triangles[index + 1]);
                r_triangles.Add(m_triangles[index + 2]);
                */
            }
        }

        // remove the selected vertices & uvs from the original lists
        /*
        for (int i = 0; i < n_idx.Count; i++)
        {
            m_vertices.RemoveAt(n_idx[i]);
            m_uvs.RemoveAt(n_idx[i]);
            m_normals.RemoveAt(n_idx[i]);
        }
        */

        // render the original object again
        m_mesh.SetTriangles(r_triangles.ToArray(), 0);
        m_mesh.SetVertices(r_vertices);
        m_mesh.SetNormals(r_normals);
        m_mesh.SetUVs(0, r_uvs);


        //m_mesh.RecalculateNormals();
        //m_mesh.RecalculateTangents();
        //m_mesh.RecalculateBounds();

        var mesh = new Mesh
        {
            vertices = s_vertices.ToArray(),
            triangles = n_triangles.ToArray(),
            //colors = colors
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GameObject temp_obj = new GameObject();

        // Set up game object with mesh;
        var meshRenderer = temp_obj.AddComponent<MeshRenderer>();
        //meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        meshRenderer.material = new Material(m_rend.material);

        var filter = temp_obj.AddComponent<MeshFilter>();
        filter.mesh = mesh;

        // return the game object in order to store in the game system
        /*
        GameObject instance = new GameObject();
        MeshFilter meshfilter = instance.AddComponent<MeshFilter>();
        instance.AddComponent<MeshCollider>();
        MeshRenderer n_meshrend = instance.AddComponent<MeshRenderer>();
        if(m_rend.material != null)
            n_meshrend.material = m_rend.material;
        Mesh temp = meshfilter.mesh;
        temp.SetVertices(s_vertices);
        temp.SetUVs(0, n_uvs);
        temp.SetTriangles(n_triangles.ToArray(), 0);
        temp.SetNormals(n_normals);
        */
        //temp.RecalculateBounds();
        //temp.RecalculateNormals();
        //temp.RecalculateTangents();

        // keep the same physical properties as the original object
        return temp_obj;
    }

    
    public void _DivideMeshes(List<Vector3> s_vertices)
    {
        // data for making a new object
        List<int> n_idx = new List<int>();
        List<Vector2> n_uvs = new List<Vector2>();
        List<int> n_triangles = new List<int>();
        List<Vector3> n_normals = new List<Vector3>();

        // removed results (removed selected vertices from the original data)
        List<int> r_triangles = new List<int>();

        // list for storing the info: how many vertices are deleted before the given index
        List<int> delete_num = new List<int>(m_vertices.Count);

        // find selected vertices in m_vertices & remember the indices of those vertices
        for (int i = 0; i < s_vertices.Count; i++)
        {
            int idx = m_vertices.FindIndex(MatchVertex(s_vertices[i]));
            n_idx.Add(idx);
            n_uvs.Add(m_uvs[idx]);
            n_normals.Add(m_normals[idx]);
            Debug.Log(s_vertices[i]);
        }
        
        for (int i = 0; i <  m_triangles.Length; i++)
        {
            // for each triangle entry...
            int t = m_triangles[i];
            Debug.Log(m_triangles[i]);
            // get its vertex, uv and normal
            Vector3 vert = m_vertices[t];
            Vector3 normal = m_normals[t];
            Vector2 uv = m_uvs[t];

            // vertexFound is the compatible vertex index, if any:
            int vertexFound = -1;

            // check if the vertex is already in the list:
            for (int j = 0; j < m_vertices.Count; j++)
            {
                // if compatible vertex already in the new list...
                if (s_vertices.Count <= j)
                    break;
                if (vert == s_vertices[j] && normal == n_normals[j] && uv == n_uvs[j])
                {
                    vertexFound = j; // get its index...
                    break; // and stop the loop
                }
            }
            if (vertexFound < 0)
            { // if no compatible vertex in the list...
              // get the index of the next new element...
                vertexFound = s_vertices.Count;
                // then add the vertex and attributes to the list
                s_vertices.Add(vert); // add new vertex...
                n_uvs.Add(uv); // and attributes to the list
                n_normals.Add(normal);
            }
            m_triangles[i] = vertexFound; // anyway, update triangle entry...
        }
        m_mesh.vertices = s_vertices.ToArray();
        m_mesh.uv = n_uvs.ToArray();
        m_mesh.normals = n_normals.ToArray();
    }

    public void DeleteTriangle()
    {
        
        
        int ver1 = m_triangles[0];
        int ver2 = m_triangles[1];
        int ver3 = m_triangles[2];

        
        int[] n_triangles = new int[m_triangles.Length - 3];
        for(int i = 0; i < m_triangles.Length-3; i++)
        {
            n_triangles[i] = m_triangles[i + 3];
        }

        /*
        m_vertices.RemoveAt(ver1);
        m_vertices.RemoveAt(ver2);
        m_vertices.RemoveAt(ver3);
        m_uvs.RemoveAt(ver1);
        m_uvs.RemoveAt(ver2);
        m_uvs.RemoveAt(ver3);
        m_normals.RemoveAt(ver1);
        m_normals.RemoveAt(ver2);
        m_normals.RemoveAt(ver3);
        */
        m_mesh.triangles = n_triangles;
        //m_mesh.vertices = m_vertices.ToArray();
        //m_mesh.uv = m_uvs.ToArray();
        //m_mesh.normals = m_normals.ToArray();

    }


    /*
     * ChangeMaterial
     * change the material of this object into the given one
     * also change the physical property accordingly
     */
    void ChangeMaterial(Material s_material)
    {

    }

    /*
     * ChangeMaterial
     * change the physical property of this object into the given type
     */
    void ChangePhysics(int type)
    {

    }

    // Use this for initialization
    void Start () {
        m_vertices = new List<Vector3>();
        m_uvs = new List<Vector2>();
        m_normals = new List<Vector3>();
        m_mesh = GetComponent<MeshFilter>().mesh;
        m_rend = GetComponent<Renderer>();
        m_mesh.GetVertices(m_vertices);
        m_mesh.GetUVs(0, m_uvs);
        m_triangles = m_mesh.GetTriangles(0);
        m_mesh.GetNormals(m_normals);
        m_system = FindObjectOfType<GameSystem>().Instance;

        m_rend.material.renderQueue = 2002;
        layer_mask = ~1 << 2;

        /*
        // Create Vector2 vertices
        var vertices2D = new Vector2[] {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,2),
            new Vector2(0,2),
            new Vector2(0,3),
            new Vector2(3,3),
            new Vector2(3,2),
            new Vector2(2,2),
            new Vector2(2,1),
            new Vector2(3,1),
            new Vector2(3,0),
        };

        var vertices3D = System.Array.ConvertAll<Vector2, Vector3>(vertices2D, v => v);

        // Use the triangulator to get indices for creating triangles
        var triangulator = new Triangulator(vertices2D);
        var indices = triangulator.Triangulate();

        // Generate a color for each vertex
        var colors = Enumerable.Range(0, vertices3D.Length)
            .Select(i => UnityEngine.Random.ColorHSV())
            .ToArray();

        // Create the mesh
        var mesh = new Mesh
        {
            vertices = vertices3D,
            triangles = indices,
            colors = colors
        };
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GameObject temp_obj = new GameObject();
       
        // Set up game object with mesh;
        var meshRenderer = temp_obj.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));

        var filter = temp_obj.AddComponent<MeshFilter>();
        filter.mesh = mesh;
        */
    }

	
	// Update is called once per frame
	void Update () {
		
	}
}
