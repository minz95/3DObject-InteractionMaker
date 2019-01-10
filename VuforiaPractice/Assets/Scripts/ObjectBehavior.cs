using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.AI;
using System.Linq;
using System;

public class ObjectBehavior : MonoBehaviour {
    public bool m_selected = false;
    bool m_physics = false;
    bool m_anchor = false;
    public Transform vertex_sphere;
    GameSystem m_system;
    const int sphere_num = 1000;
    int vert_length = 0;
    Transform[] Spheres = new Transform[sphere_num];
    List<Vector3> m_vertices;
    List<Vector2> m_uvs;
    List<Vector3> m_normals;
    Outline m_outline;
    Vector3 m_anchorpoint;
    int[] m_triangles;

    Renderer m_rend;
    Mesh m_mesh;

    LayerMask layer_mask;
    Animator m_anim;
    int pressHash = Animator.StringToHash("press");
    int idleHash = Animator.StringToHash("Base Layer.Idle");
    Vector3 m_direction = Vector3.zero;
    float m_force = 0f;
    private Transform startPosition = null;
    private Transform targetPosition = null;
    bool isPressing = false;

    void OnMouseDown()
    {
        switch(m_system.GetMode())
        {
            case 0:
                //GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * 30f);
                if (m_direction != Vector3.zero)
                {
                    transform.Translate(m_direction * 1.5f);
                    isPressing = true;
                    /*
                    AnimatorStateInfo stateInfo = m_anim.GetCurrentAnimatorStateInfo(0);
                    if (stateInfo.fullPathHash == idleHash)
                    {
                        m_anim.SetTrigger(pressHash);
                    }
                    */
                }
                break;
            case 1: // select object
                if (m_selected == false)
                {
                    //m_rend.material.color = Color.blue;

                    List<Vector3> r_verts = new List<Vector3>(m_vertices);

                    Vector3[] verts = r_verts.Distinct().ToList().ToArray();
                    //removeDuplicates(vertices);
                    drawSpheres(verts);
                    m_system.SetCurrentObject(gameObject);
                    m_system.SetMode(2);
                    m_selected = true;
                }
                // TODO: turn back m_selected to false if the operation has been finished
                /*
                else
                {
                    ClearSpheres();
                    m_system.SetCurrentObject(null);
                    m_system.SetMode(1);
                    m_selected = false;
                }
                */

                // change the layer of the object
                gameObject.layer = 2;

                break;
            case 2: // select vertices to split
                
                break;
            case 3: // physics dropdown
                break;
            case 4: // select the object for physics
                if(!m_physics)
                {
                    List<Vector3> r_verts = new List<Vector3>(m_vertices);

                    Vector3[] verts = r_verts.Distinct().ToList().ToArray();
                    // Debug.Log("m_vertices count: " + m_vertices.Count);
                    drawSpheres(verts);
                    DrawOutline();

                    m_system.SetPhysicsObject(gameObject);
                    m_physics = true;
                }
                break;
            case 5: // select the anchor object for physics
                Debug.Log("anchor object selected");
                if (!m_anchor)
                {
                    DrawOutline();
                    m_system.SetAnchorObject(gameObject);
                    m_anchor = true;
                }
                break;
            case 6: // point the anchor to attach the two objects
                Vector3 clickedPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Debug.Log("anchor position: " + clickedPosition.x + ", " + clickedPosition.y + ", " + clickedPosition.z);

                break;
            case 7: // material dropdown
                break;
        }
    }

    //Transform vertex_sphere = sphere_obj.transform;
    void drawSpheres(Vector3[] verts)
    {
        //Transform[] Spheres = new Transform[verts.Length];
        vert_length = verts.Length;
        //Debug.Log(vert_length);
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
            if(Spheres[i].gameObject != null)
            {
                Destroy(Spheres[i].gameObject);
            }   
        }

        m_selected = false;
        gameObject.layer = 0;
        
    }

    public void DrawOutline()
    {
        m_outline.enabled = true;
    }

    public void ClearOutline()
    {
        m_outline.enabled = false;
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
        List<Vector3> n_vertices = new List<Vector3>();
        List<Vector2> n_uvs = new List<Vector2>();
        List<Vector2> n_uv2 = new List<Vector2>();
        List<Vector2> n_uv3 = new List<Vector2>();
        List<Vector2> n_uv4 = new List<Vector2>();
        List<List<int>> n_triangles = new List<List<int>>();
        List<Vector3> n_normals = new List<Vector3>();
        
        // remaining results (removed selected vertices from the original data)
        List<List<int>> r_triangles = new List<List<int>>();
        List<int> r_idx = new List<int>();
        List<Vector3> r_vertices = new List<Vector3>();
        List<Vector3> r_normals = new List<Vector3>();
        List<Vector2> r_uvs = new List<Vector2>();
        List<Vector2> r_uv2 = new List<Vector2>();
        List<Vector2> r_uv3 = new List<Vector2>();
        List<Vector2> r_uv4 = new List<Vector2>();

        Vector3 n_position = Vector3.zero;
        Vector3 r_position = Vector3.zero;

        // handle triangles
        int cumulate_index = 0;
        for (int c = 0; c < m_mesh.subMeshCount; c++)
        {
            m_triangles = m_mesh.GetTriangles(c);
            List<int> temp_ntriangles = new List<int>();
            List<int> temp_rtriangles = new List<int>();
            for (int index = 0; index < m_triangles.Length; index += 3)
            {
                // if all vertices of the triangle are contained in the selected vertices,
                // this triangle can be contained in the new triangle list
                if (s_vertices.Contains(m_vertices[m_triangles[index]]) &&
                    s_vertices.Contains(m_vertices[m_triangles[index + 1]]) &&
                    s_vertices.Contains(m_vertices[m_triangles[index + 2]]))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (n_idx.Contains(m_triangles[index + i]))
                        {
                            temp_ntriangles.Add(Array.IndexOf(n_idx.ToArray(), m_triangles[index + i]));

                        }
                        else
                        {
                            n_idx.Add(m_triangles[index + i]);
                            n_vertices.Add(m_vertices[m_triangles[index + i]]);
                            n_position += m_vertices[m_triangles[index + i]];
                            n_normals.Add(m_normals[m_triangles[index + i]]);
                            if (m_uvs.Count > 0)
                                n_uvs.Add(m_uvs[m_triangles[index + i]]);
                            if (m_mesh.uv2.Length > 0)
                                n_uv2.Add(m_mesh.uv2[m_triangles[index + i]]);
                            if (m_mesh.uv3.Length > 0)
                                n_uv3.Add(m_mesh.uv3[m_triangles[index + i]]);
                            if (m_mesh.uv4.Length > 0)
                                n_uv4.Add(m_mesh.uv4[m_triangles[index + i]]);
                            temp_ntriangles.Add(n_idx.Count - 1);
                        }
                    }

                    /*
                    n_triangles.Add(Array.IndexOf(n_idx.ToArray(), m_triangles[index]));
                    n_triangles.Add(Array.IndexOf(n_idx.ToArray(), m_triangles[index + 1]));
                    n_triangles.Add(Array.IndexOf(n_idx.ToArray(), m_triangles[index + 2]));
                    */
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {

                        if (!r_idx.Contains(m_triangles[index + i]))
                        {
                            r_idx.Add(m_triangles[index + i]);
                            r_vertices.Add(m_vertices[m_triangles[index + i]]);
                            r_position += m_vertices[m_triangles[index + i]];
                            r_normals.Add(m_normals[m_triangles[index + i]]);
                            if (m_uvs.Count > 0)
                                r_uvs.Add(m_uvs[m_triangles[index + i]]);
                            if (m_mesh.uv2.Length > 0)
                                r_uv2.Add(m_mesh.uv2[m_triangles[index + i]]);
                            if (m_mesh.uv3.Length > 0)
                                r_uv3.Add(m_mesh.uv3[m_triangles[index + i]]);
                            if (m_mesh.uv4.Length > 0)
                                r_uv4.Add(m_mesh.uv4[m_triangles[index + i]]);
                            temp_rtriangles.Add(r_vertices.Count - 1);
                            //Debug.Log(String.Concat("new: ", r_vertices[r_vertices.Count-1]));
                        }
                        else
                        {
                            //int temp_i = r_vertices.IndexOf(m_vertices[m_triangles[index + i]]);
                            temp_rtriangles.Add(Array.IndexOf(r_idx.ToArray(), m_triangles[index + i]));
                            //Debug.Log(r_vertices[Array.IndexOf(r_idx.ToArray(), m_triangles[index + i])]);
                        }
                    }
                }
            }
            cumulate_index += m_triangles.Length;
            n_triangles.Add(temp_ntriangles);
            r_triangles.Add(temp_rtriangles);
        }

        List<Vector3> common_vertices = n_vertices.Intersect(r_vertices).ToList();

        n_position /= n_vertices.Count;
        r_position /= r_vertices.Count;
        Debug.Log("position: " + n_position + ", " + r_position);
        List<Vector3> pn_vertices = new List<Vector3>();
        for (int i = 0; i < n_vertices.Count; ++i)
        {
            pn_vertices.Add(n_vertices[i]);
            n_vertices[i] = n_vertices[i] - n_position;
        }

        for (int i = 0; i < r_vertices.Count; ++i)
        {
            r_vertices[i] = r_vertices[i] - r_position;
        }

        var _mesh = new Mesh
        {
            vertices = r_vertices.ToArray(),
            //triangles = r_triangles.ToArray(),
            uv = r_uvs.ToArray(),
            uv2 = r_uv2.ToArray(),
            uv3 = r_uv3.ToArray(),
            uv4 = r_uv4.ToArray()
        };

        for(int i = 0; i < r_triangles.Count; i++)
        {
            _mesh.SetTriangles(r_triangles[i], i);
            _mesh.subMeshCount++;
        }

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        GameObject _obj = new GameObject();
        _obj.transform.position = transform.position;
        _obj.transform.position = r_position;
        var _meshRenderer = _obj.AddComponent<MeshRenderer>();
        _meshRenderer.material = new Material(m_rend.material);
        _meshRenderer.materials = m_rend.materials;
        var _filter = _obj.AddComponent<MeshFilter>();
        _filter.mesh = _mesh;
        ObjectBehavior _script = _obj.AddComponent<ObjectBehavior>();
        _obj.AddComponent<MeshCollider>();
        _obj.name = "body_obj";
        _script.vertex_sphere = vertex_sphere;
        
        gameObject.SetActive(false);
        
        var mesh = new Mesh
        {
            vertices = n_vertices.ToArray(),
            //triangles = n_triangles.ToArray(),
            uv = n_uvs.ToArray(),
            uv2 = n_uv2.ToArray(),
            uv3 = n_uv3.ToArray(),
            uv4 = n_uv4.ToArray()
        };

        for (int i = 0; i < n_triangles.Count; i++)
        {
            mesh.SetTriangles(n_triangles[i], i);
            mesh.subMeshCount++;
        }

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GameObject temp_obj = new GameObject();
        temp_obj.transform.position = transform.position;
        temp_obj.transform.position = n_position;

        // Set up game object with mesh;
        var meshRenderer = temp_obj.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(m_rend.material);
        meshRenderer.materials = m_rend.materials;

        var filter = temp_obj.AddComponent<MeshFilter>();
        filter.mesh = mesh;
        ObjectBehavior temp_script = temp_obj.AddComponent<ObjectBehavior>();
        temp_obj.AddComponent<MeshCollider>();
        temp_obj.name = "part_obj";

        GameObject p_obj = new GameObject();
        p_obj.name = gameObject.name;
        _obj.transform.SetParent(p_obj.transform);
        temp_obj.transform.SetParent(p_obj.transform);

        // Find common vertices between n_vertices and r_vertices, 
        // and call the triangulate algorithm with those vertices 
        Vector3 avg_normal = Vector3.zero;
        for (int i = 0; i < common_vertices.Count; i++)
        {
            int index = Array.IndexOf(pn_vertices.ToArray(), common_vertices[i]);
            avg_normal += mesh.normals[index];
        }

        // no intersections between the body and the part
        if (common_vertices.Count == 0)
        {
            avg_normal = Vector3.down;
        }
        avg_normal /= common_vertices.Count;
        Debug.Log(avg_normal.x + ", " + avg_normal.y + ", " + avg_normal.z);
        Vector3 temp_world = temp_obj.transform.position;
        Vector3 _world = _obj.transform.position;
        if ((temp_world + avg_normal - _world).magnitude > (temp_world - _world).magnitude)
        {
            Debug.Log("normal magnitude: " + (temp_world + avg_normal - _world).magnitude);
            Debug.Log("original magnitude: " + (temp_world - _world).magnitude);
            Debug.Log("avg_normal magnitude: " + avg_normal.magnitude);
            avg_normal *= -1;
        }
        temp_script.SetDirection(avg_normal);
        temp_script.vertex_sphere = vertex_sphere;
        //temp_obj.transform.position += avg_normal * 5.0f;
        /*
        m_anim = temp_obj.AddComponent<Animator>();
        m_anim.SetFloat("speed", 0.4f);
        m_anim.runtimeAnimatorController = Resources.Load("button_controller") 
            as RuntimeAnimatorController;
        */
        return temp_obj;
    }

    public Vector3 GetPixelWorldPos(Texture2D mainTex, Color32 color, int index)
    {
        int w = mainTex.width;
        int h = mainTex.height;

        // find color in texture
        /*
        Color[] colors = mainTex.GetPixels();
        int index = -1;
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i] == color)
            {
                index = i;
                break;
            }
        }
        */
        if (index == -1)
            return Vector3.zero;

        // Find triangle and get local pos
        var point = new Vector2(index % w, index / w) - new Vector2(0.5f / w, 0.5f / h);
        var mf = gameObject.GetComponent<MeshFilter>();
        var mesh = m_mesh;
        var uvs = m_uvs.ToArray();
        var verts = mesh.vertices;
        var tris = mesh.triangles;
        for (int i = 0; i < m_triangles.Length; i += 3)
        {
            var uv0 = m_uvs[i + 0];
            var uv1 = m_uvs[i + 1];
            var uv2 = m_uvs[i + 2];
            var bary = new Barycentric(uv0, uv1, uv2, point);
            
            if (bary.IsInside)
            {
                Vector3 localPos = m_vertices[i + 0] * bary.u + m_vertices[i + 1] * bary.v + m_vertices[i + 2] * bary.w;
                // Transform to world pos and return
                return gameObject.transform.TransformPoint(localPos);
            }
        }
        return Vector3.zero;
    }

    public bool InTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        double s1 = C.y - A.y;
        double s2 = C.x - A.x;
        double s3 = B.y - A.y;
        double s4 = P.y - A.y;

        double w1 = (A.x * s1 + s4 * s2 - P.x * s1) / (s3 * s2 - (B.x - A.x) * s1);
        double w2 = (s4 - w1 * s3) / s1;
        return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
    }

    /*
     * ChangeMaterial
     * change the material of this object into the given one
     * also change the physical property accordingly
     */
    public void ChangeMaterial(Material s_material)
    {

    }

    /*
     * ChangeMaterial
     * change the physical property of this object into the given type
     */
    public void ChangePhysics(int type)
    {
        switch(type)
        {
            case 0: // push button (configurable joint)

                break;
        }
    }

    public void SetDirection(Vector3 d)
    {
        m_direction = d;
    }

    private void FindBoundingBox(List<Vector3> vertices)
    {
        // find min/max of x, y, z each
        Vector3 xminVertex = new Vector3(float.PositiveInfinity, float.PositiveInfinity,
            float.PositiveInfinity);
        Vector3 xmaxVertex = new Vector3(float.NegativeInfinity, float.NegativeInfinity,
            float.NegativeInfinity);
        Vector3 yminVertex = new Vector3(float.PositiveInfinity, float.PositiveInfinity,
            float.PositiveInfinity);
        Vector3 ymaxVertex = new Vector3(float.NegativeInfinity, float.NegativeInfinity,
            float.NegativeInfinity);
        Vector3 zminVertex = new Vector3(float.PositiveInfinity, float.PositiveInfinity,
            float.PositiveInfinity);
        Vector3 zmaxVertex = new Vector3(float.NegativeInfinity, float.NegativeInfinity,
            float.NegativeInfinity);
        List<Vector3> boundary = new List<Vector3>(6);

        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertices[i].x > xmaxVertex.x) xmaxVertex = vertices[i];
            if (vertices[i].y > ymaxVertex.y) ymaxVertex = vertices[i];
            if (vertices[i].x > zmaxVertex.z) zmaxVertex = vertices[i];

            if (vertices[i].x < xminVertex.x) xminVertex = vertices[i];
            if (vertices[i].y < yminVertex.y) yminVertex = vertices[i];
            if (vertices[i].z < zminVertex.z) zminVertex = vertices[i];
        }

        boundary.Add(xminVertex);
        boundary.Add(xmaxVertex);
        boundary.Add(yminVertex);
        boundary.Add(ymaxVertex);
        boundary.Add(zminVertex);
        boundary.Add(zmaxVertex);

        float minBoundSize = -10000f;
        Vector3[] minBoundingBox = new Vector3[2];
        // translate each edges, and find the minimum bounding box
        for (int i = 0; i < boundary.Count; i++)
        {
            for (int j = i + 1; j < boundary.Count; j++)
            {
                // if two vertices are the same, no edge exists
                if (vertices[i] == vertices[j]) continue;

                // transform vertices according to vertex i and j -> (0, 0, 0), (1, 0, 0)
                xminVertex = (boundary[0] - vertices[i]);
                xmaxVertex = (boundary[1] - vertices[i]);
                yminVertex = (boundary[2] - vertices[i]);
                ymaxVertex = (boundary[3] - vertices[i]);
                zminVertex = (boundary[4] - vertices[i]);
                zmaxVertex = (boundary[5] - vertices[i]);

                // calculate the bounding box and remember if it is the minimum
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        m_vertices = new List<Vector3>();
        m_uvs = new List<Vector2>();
        m_normals = new List<Vector3>();
        m_anchorpoint = Vector3.zero;
        m_mesh = GetComponent<MeshFilter>().mesh;
        m_rend = GetComponent<Renderer>();
        m_mesh.GetVertices(m_vertices);
        m_mesh.GetUVs(0, m_uvs);
        m_triangles = m_mesh.triangles;
        m_mesh.GetNormals(m_normals);
        m_system = FindObjectOfType<GameSystem>().Instance;
        m_outline = FindObjectOfType<Outline>();
        if (m_outline == null) m_outline = gameObject.AddComponent<Outline>();
        m_outline.OutlineMode = Outline.Mode.OutlineAll;
        m_outline.OutlineColor = Color.yellow;
        m_outline.OutlineWidth = 5f;
        m_outline.enabled = false;

        Transform tr = gameObject.transform;
        for (int i = 0; i < m_vertices.Count; ++i)
        {
            m_vertices[i] = tr.TransformPoint(m_vertices[i]);
        }

        m_anim = null;
        //m_rend.material.shader = Shader.Find("VertexColorUnlit2");

        //m_rend.material.renderQueue = 2002;
        //layer_mask = ~1 << 2;
    }
	
	// Update is called once per frame
	void Update () {
        // Lerp button click action
        
    }
}
