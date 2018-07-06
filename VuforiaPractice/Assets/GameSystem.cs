using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour {
    private GameSystem m_Instance;
    public GameSystem Instance { get { return m_Instance; } }
    private GameObject m_currobj;       // object which is currently selected by the user
    private List<Vector3> m_vertices;   // initial vertices of the object (before selected),
                                        // redundant values removed
    private List<Vector3> s_vertices;   // vertices that are selected by the user
    private int m_mode; // 0: divide meshes, 1: select physics, 2: select material
    private List<GameObject> objects;
    public ObjectBehavior m_script;

    void SetVertices(Vector3[] v)
    {
        for (int i = 0; i < v.Length; i++)
        {
            m_vertices[i] = v[i];
        }
    }

    void ClearVertices()
    {
        m_vertices.Clear();
    }

    void ScriptManager()
    {

    }

    void Awake()
    {
        m_Instance = this;
    }

    void OnDestroy()
    {
        m_Instance = null;
    }

    // Use this for initialization
    void Start()
    {
        m_Instance = this;
        m_vertices = new List<Vector3>();
        m_mode = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // if button clicked
        // select vertices (how do we know that vertices have chosen?)
        // if the object is selected, then the object script set the vertices of the game system instance
        // and also show the vertices on the screen
        // when clicked and dragged, vertices are selected if close enough

        // call the object's divide mesh function in here

        // if button clicked, make the user select the part
        // then gives the physics to that specific part

    }

    void OnGui()
    {
        // common GUI code goes here
    }
}
