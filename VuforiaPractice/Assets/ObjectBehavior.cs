using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class ObjectBehavior : MonoBehaviour {
    public bool m_selected = false;
    const int sphere_num = 1000;
    int vert_length = 0;
    Transform[] Spheres = new Transform[sphere_num];
    List<Vector3> m_vertices;
    List<Vector2> m_uvs;
    int[] m_triangles;
    Renderer m_rend;
    Mesh m_mesh;

    void OnMouseDown()
    {
        if (m_selected == false)
        {
            //Renderer rend = GetComponent<Renderer>();
            m_rend.material.color = Color.blue;

            //Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
            Transform tr = gameObject.transform;
            for (int i = 0; i < m_vertices.Count; ++i)
            {
                m_vertices[i] = tr.TransformPoint(m_vertices[i]);
            }
            List<Vector3> r_verts = new List<Vector3>(m_vertices);

            Vector3[] verts = r_verts.Distinct().ToList().ToArray();
            //removeDuplicates(vertices);
            drawSpheres(verts);
            m_selected = true;
        }
        // instead of destroying mode, will make a button for selection mode
        else
        {
            for (int i = 0; i < vert_length; i++)
            {
                Destroy(Spheres[i].gameObject);
            }

            m_selected = false;
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
            //Spheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //Spheres[i].transform.position = verts[i];
            //Spheres[i].transform.localScale -= new Vector3(0.9F, 0.9F, 0.9F);
        }
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
    GameObject DivideMeshes(List<Vector3> s_vertices)
    {
        // data for making a new object
        List<int> n_idx = new List<int>();
        List<Vector2> n_uvs = new List<Vector2>();
        List<int> n_triangles = new List<int>();

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
        }

        // count the delete number of each index
        for (int i = 0; i < m_vertices.Count; i++)
        {
            delete_num[i] = 0;
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
            if (n_idx.Contains(m_triangles[index]) ||
                n_idx.Contains(m_triangles[index + 1]) ||
                n_idx.Contains(m_triangles[index + 2]))
            {
                // Do nothing, we don't want to save this triangle...
            }
            else
            {
                // these triangles are still contained in the original object
                r_triangles.Add(m_triangles[index] - 
                    delete_num[m_triangles[index]]);
                r_triangles.Add(m_triangles[index + 1] - 
                    delete_num[m_triangles[index + 1]]);
                r_triangles.Add(m_triangles[index + 2] - 
                    delete_num[m_triangles[index + 2]]);
            }

            // if all vertices of the triangle are contained in the selected vertices,
            // this triangle can be contained in the new triangle list
            if (n_idx.Contains(m_triangles[index]) &&
                n_idx.Contains(m_triangles[index + 1]) &&
                n_idx.Contains(m_triangles[index + 2]))
            {
                n_triangles.Add(Array.IndexOf(n_idx.ToArray(), m_triangles[index]));
                n_triangles.Add(Array.IndexOf(n_idx.ToArray(), m_triangles[index + 1]));
                n_triangles.Add(Array.IndexOf(n_idx.ToArray(), m_triangles[index + 2]));
            }
        }

        // remove the selected vertices & uvs from the original lists
        for (int i = 0; i < n_idx.Count; i++)
        {
            m_vertices.RemoveAt(n_idx[i]);
            m_uvs.RemoveAt(n_idx[i]);
        }

        // render the original object again
        m_mesh.SetVertices(m_vertices);
        m_mesh.SetUVs(0, m_uvs);
        m_mesh.SetTriangles(r_triangles.ToArray(), 0);
        m_mesh.RecalculateNormals();
        m_mesh.RecalculateTangents();
        m_mesh.RecalculateBounds();

        // return the game object in order to store in the game system
        GameObject instance = new GameObject();
        instance.AddComponent<MeshFilter>();
        instance.AddComponent<MeshCollider>();
        instance.AddComponent<MeshRenderer>();
        Mesh temp = instance.GetComponent<MeshFilter>().mesh;
        temp.SetVertices(s_vertices);
        temp.SetUVs(0, n_uvs);
        temp.SetTriangles(n_triangles.ToArray(), 0);
        temp.RecalculateBounds();
        temp.RecalculateNormals();
        temp.RecalculateTangents();
        return instance;
    }

    /*
     * ChangeMaterial
     * change the material of this object into the given one
     * also change the physical property accordingly
     */
    void ChangeMaterial(Material s_material)
    {

    }

    // Use this for initialization
    void Start () {
        m_mesh = GetComponent<MeshFilter>().mesh;
        m_rend = GetComponent<Renderer>();
        m_mesh.GetVertices(m_vertices);
        m_mesh.GetUVs(0, m_uvs);
        m_triangles = m_mesh.GetTriangles(0);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
