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
    private int m_mode; // 0: default window, 1: select object, 2: select vertices to split, 
                        // 3: physics drop down, 4: select vertices for physics 5: material drop down
    private List<GameObject> objects;
    public ObjectBehavior m_script;

    bool repeatButtonDown = false;
    
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

    /*
     * ClickSplit
     * deactivate main buttons, and show a message to select the object
     * give the hover effect to all the objects in the scene
     */
    void ClickSplit()
    {
        m_mode = 1;

        // deactivate the main buttons
        GameObject.FindGameObjectWithTag("split_btn").SetActive(false);
        GameObject.FindGameObjectWithTag("physics_btn").SetActive(false);
        GameObject.FindGameObjectWithTag("material_btn").SetActive(false);

        // activate the confirm and cancel buttons
        GameObject.FindGameObjectWithTag("confirm").SetActive(true);
        GameObject.FindGameObjectWithTag("cancel").SetActive(true);

        // show the message to select the object
        // TODO: enable objects' onclick event only here
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

        GameObject.FindGameObjectWithTag("confirm").SetActive(false);
        GameObject.FindGameObjectWithTag("cancel").SetActive(false);
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
