﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
    //public ObjectBehavior m_script;

    GameObject split_btn;
    GameObject physics_btn;
    GameObject material_btn;
    GameObject confirm_btn;
    GameObject cancel_btn;
    Text ui_text;
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
        s_vertices = new List<Vector3>();
        m_mode = 0;
        m_currobj = null;

        confirm_btn = GameObject.FindGameObjectWithTag("confirm");
        cancel_btn = GameObject.FindGameObjectWithTag("cancel");
        confirm_btn.SetActive(false);
        cancel_btn.SetActive(false);

        split_btn = GameObject.FindGameObjectWithTag("split_btn");
        split_btn.GetComponent<Button>().onClick.AddListener(SplitButtonClick);
        physics_btn = GameObject.FindGameObjectWithTag("physics_btn");
        physics_btn.GetComponent<Dropdown>().onValueChanged.AddListener(PhysicsButtonClick);
        material_btn = GameObject.FindGameObjectWithTag("material_btn");
        material_btn.GetComponent<Dropdown>().onValueChanged.AddListener(MaterialButtonClick);
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

    public int GetMode()
    {
        return m_mode;
    }

    public void SetMode(int m)
    {
        m_mode = m;
    }

    public void SetCurrentObject(GameObject gameObject)
    {
        if(m_currobj != gameObject && m_currobj != null)
        {
            ObjectBehavior objectBehavior = m_currobj.GetComponent<ObjectBehavior>();
            objectBehavior.ClearSpheres();
        }
        m_currobj = gameObject;
        m_currobj.GetComponent<MeshFilter>().mesh.GetVertices(m_vertices);

        Transform tr = m_currobj.transform;
        for (int i = 0; i < m_vertices.Count; ++i)
        {
            m_vertices[i] = tr.TransformPoint(m_vertices[i]);
        }
    }

    public void AddSelectedVertex(Vector3 v)
    {
        if (m_mode != 2)
        {
            return;
        }
        s_vertices.Add(v);

        CheckTriangle();
    }

    public void RemoveSelectedVertex(Vector3 v)
    {
        if (m_mode != 2)
        {
            return;
        }
        s_vertices.Remove(v);

        CheckTriangle();
    }

    void ActivateMenu()
    {
        // deactivate the main buttons
        split_btn.SetActive(true);
        physics_btn.SetActive(true);
        material_btn.SetActive(true);
        split_btn.GetComponent<Button>().onClick.AddListener(SplitButtonClick);
        physics_btn.GetComponent<Dropdown>().onValueChanged.AddListener(PhysicsButtonClick);
        material_btn.GetComponent<Dropdown>().onValueChanged.AddListener(MaterialButtonClick);

        // activate the confirm and cancel buttons
        confirm_btn.SetActive(false);
        cancel_btn.SetActive(false);
    }

    void DeactivateMenu ()
    {
        // deactivate the main buttons
        split_btn.SetActive(false);
        physics_btn.SetActive(false);
        material_btn.SetActive(false);

        // activate the confirm and cancel buttons
        confirm_btn.SetActive(true);
        cancel_btn.SetActive(true);
        confirm_btn.GetComponent<Button>().onClick.AddListener(ConfirmButtonClick);
        cancel_btn.GetComponent<Button>().onClick.AddListener(CancelButtonClick);
    }

    public void SplitButtonClick()
    {
        // clicked when split mode: cancel the split mode
        if (m_mode == 1)
        {
            CancelButtonClick();
            ActivateMenu();
        }
        // clicked when default mode: 
        else if (m_mode == 0)
        {
            m_mode = 1;
            DeactivateMenu();
        }
        // clicked when other modes: cancel the modes
        else
        {
            CancelButtonClick();
            m_mode = 0;
            ActivateMenu();
        }

        // TODO: give the hover effect to each object
        //       show the message to select the object
    }

    private void PhysicsButtonClick(int arg0)
    {
        Dropdown drop_physics = physics_btn.GetComponent<Dropdown>();
        drop_physics.options.RemoveAt(0);
        if (m_mode == 3)
        {
            CancelButtonClick();
            ActivateMenu();
            Dropdown.OptionData option_data = new Dropdown.OptionData();
            option_data.text = "Add Physics";
            drop_physics.options.Insert(0, option_data);
        }
        else if (m_mode == 0)
        {
            m_mode = 3;
            DeactivateMenu();
        }
        else
        {
            CancelButtonClick();
            m_mode = 0;
            ActivateMenu();
            Dropdown.OptionData option_data = new Dropdown.OptionData();
            option_data.text = "Add Physics";
            drop_physics.options.Insert(0, option_data);
        }

        switch (arg0)
        {
            case 0: // push button
                //Debug.Log("push button selected");
                break;
            case 1: // dial (spin)
                break;
            case 2: // hinge
                break;
            default:
                break;
        }
    }

    private void MaterialButtonClick(int arg0)
    {
        Dropdown drop_material = material_btn.GetComponent<Dropdown>();
        drop_material.options.RemoveAt(0);
        if (m_mode == 4)
        {
            CancelButtonClick();
            ActivateMenu();
            Dropdown.OptionData option_data = new Dropdown.OptionData();
            option_data.text = "Set Material";
            drop_material.options.Insert(0, option_data);
        }
        else if(m_mode == 0)
        {
            m_mode = 4;
            DeactivateMenu();
        }
        else
        {
            CancelButtonClick();
            m_mode = 1;
            ActivateMenu();
            Dropdown.OptionData option_data = new Dropdown.OptionData();
            option_data.text = "Set Material";
            drop_material.options.Insert(0, option_data);
        }

        switch(arg0)
        {
            case 0:
                break;
        }
    }

    public void ConfirmButtonClick()
    {
        switch (m_mode)
        {
            case 0: 
                break;
            case 1: // select object
                m_mode = 2;
                break;
            case 2: // select vertices to split
                m_currobj.GetComponent<ObjectBehavior>().DivideMeshes(s_vertices);
                break;
            case 3: // physics dropdown
                break;
            case 4: // select vertices for physics
                break;
            case 5: // material dropdown
                break;
        }
    }

    public void CancelButtonClick()
    {
        switch(m_mode)
        {
            case 0:
                break;
            case 1: // select object
                // undo the current selected object, if exists
                if(m_currobj != null)
                {
                    m_currobj.GetComponent<ObjectBehavior>().ClearSpheres();
                }
                m_currobj = null;
                break;
            case 2: // select vertices to split
                if (m_currobj != null)
                {
                    m_currobj.GetComponent<ObjectBehavior>().ClearSpheres();
                }
                m_currobj = null;
                break;
            case 3: // physics dropdown
                break;
            case 4: // select vertices for physics
                break;
            case 5: // material dropdown
                break;
        }

        m_mode = 0;
        ActivateMenu();
    }

    /// <summary>
    /// EXPENSIVE OPERATION
    /// check rather existing s_vertices make triangle faces
    /// if exists, hightlight the triangles to show users the faces selected
    /// </summary>
    void CheckTriangle()
    {
        if (m_currobj == null) return;

        var color = m_currobj.GetComponent<MeshRenderer>().material.color;
        var colors = new Color[m_vertices.Count];
        for(int i = 0; i < colors.Length; i++)
        {
            colors[i] = color;
        }

        // iterate the triangles of the object,
        // and check whether the three vertices are contained in the simultaneously
        int[] triangles = m_currobj.GetComponent<MeshFilter>().mesh.triangles;
        Debug.Log(m_currobj);
        for(int i = 0; i < triangles.Length; i += 3)
        {
            if(s_vertices.Contains(m_vertices[triangles[i]]) &&
                s_vertices.Contains(m_vertices[triangles[i+1]]) &&
                s_vertices.Contains(m_vertices[triangles[i+2]]))
            {
                // this triangle is contained in the selected faces
                // highlight it!
                Debug.Log("have a triangle: " + i);
                
                colors[triangles[i]] = Color.Lerp(Color.red, Color.green, m_vertices[triangles[i]].y);
                colors[triangles[i + 1]] = Color.Lerp(Color.red, Color.green, m_vertices[triangles[i+1]].y); ;
                colors[triangles[i + 2]] = Color.Lerp(Color.red, Color.green, m_vertices[triangles[i+2]].y); ;
            }
        }
        m_currobj.GetComponent<MeshFilter>().mesh.colors = colors;
    }

    public void AddVertex(Vector2 vertex)
    {
        if (m_mode != 2)
        {
            return;
        }

        s_vertices.Add(vertex);

        CheckTriangle();
    }
}
