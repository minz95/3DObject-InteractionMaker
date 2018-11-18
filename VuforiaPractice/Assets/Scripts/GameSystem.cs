using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSystem : MonoBehaviour {
    private GameSystem m_Instance;
    public GameSystem Instance { get { return m_Instance; } }
    private GameObject m_currobj;       // object which is currently selected by the user
    private GameObject m_physicsobj;    // object where a physical property is attached
    private GameObject m_anchorobj;     // object which is used as an anchor of the physics
    private Vector3 m_anchorcoord;      // Vector3 anchor position for physics
    private List<Vector3> m_vertices;   // initial vertices of the object (before selected),
                                        // redundant values removed
    private List<Vector3> s_vertices;   // vertices that are selected by the user
    private int m_mode; // 0: default window, 1: select object, 2: select vertices to split, 
                        // 3: physics drop down, 4: select thew object for physics, 5: select the anchor object for physics
                        // 6: point the anchor to attach two objects, 7: material drop down
    private List<GameObject> objects;   // a list containing newly-made separated objects
    //public ObjectBehavior m_script;
    public GameObject selectedunit;
    public List<GameObject> selectedunits = new List<GameObject>();
    RaycastHit hit;
    private Vector3 MouseDownPoint, CurrentDownPoint;
    public bool IsDragging;
    private float BoxWidth, BoxHeight, BoxLeft, BoxTop;
    private Vector2 BoxStart, BoxFinish;
    public List<GameObject> UnitsOnScreenSpace = new List<GameObject>();
    public List<GameObject> UnitInDrag = new List<GameObject>();

    // TODO: These game objects should be changed into individual buttons and dropdowns
    GameObject split_btn;
    GameObject physics_btn;
    GameObject material_btn;
    GameObject physics_drop;
    GameObject material_drop;
    GameObject confirm_btn;
    GameObject cancel_btn;

    Text ui_text;
    
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
        m_physicsobj = null;
        m_anchorobj = null;

        confirm_btn = GameObject.FindGameObjectWithTag("confirm");
        cancel_btn = GameObject.FindGameObjectWithTag("cancel");
        physics_drop = GameObject.FindGameObjectWithTag("physics_drop");
        material_drop = GameObject.FindGameObjectWithTag("material_drop");
        confirm_btn.SetActive(false);
        cancel_btn.SetActive(false);
        physics_drop.SetActive(false);
        material_drop.SetActive(false);
        ui_text = GameObject.Find("Text").GetComponent<Text>();
        Debug.Log(ui_text);

        split_btn = GameObject.FindGameObjectWithTag("split_btn");
        split_btn.GetComponent<Button>().onClick.AddListener(SplitButtonClick);
        physics_btn = GameObject.FindGameObjectWithTag("physics_btn");
        physics_btn.GetComponent<Button>().onClick.AddListener(PhysicsButtonClick);
        material_btn = GameObject.FindGameObjectWithTag("material_btn");
        material_btn.GetComponent<Button>().onClick.AddListener(MaterialButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                if (hit.transform.tag != "vertex_sphere")
                {
                    if (CheckIfMouseIsDragging())
                    {
                        IsDragging = true;
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            PutUnitsFromDragIntoSelectedUnits();
            IsDragging = false;
        }

        if (selectedunit == null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                {
                    if (hit.transform.tag == "vertex_sphere")
                    {
                        selectedunit = hit.transform.gameObject;
                        selectedunit.GetComponent<vertex_sphere>().ChangeColorRed();
                        for (int i = 0; i < selectedunits.Count; i++)
                        {
                            selectedunits[i].transform.gameObject.GetComponent<vertex_sphere>().ChangeColorRed();
                        }
                        selectedunits.Clear();
                    }
                    
                    if (hit.transform.tag == "Floor")
                    {
                        for (int i = 0; i < selectedunits.Count; i++)
                        {
                            selectedunits[i].transform.gameObject.GetComponent<vertex_sphere>().ChangeColorDefault();
                        }
                        selectedunits.Clear();
                    }
                    
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                {
                    if (hit.transform.tag == "vertex_sphere")
                    {
                        selectedunit.transform.gameObject.GetComponent<vertex_sphere>().ChangeColorDefault();
                        selectedunit = null;
                        selectedunit = hit.transform.gameObject;
                        selectedunit.transform.gameObject.GetComponent<vertex_sphere>().ChangeColorRed();

                    }
                    
                    if (hit.transform.tag == "Floor")
                    {
                        selectedunit.transform.gameObject.GetComponent<vertex_sphere>().ChangeColorDefault();
                        selectedunit = null;
                    }
                    
                }
            }
        }
        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                if (hit.transform.tag == "vertex_sphere")
                {
                    if (selectedunit != null)
                    {
                        selectedunits.Add(selectedunit);
                        AddSelectedVertex(selectedunit.transform.position);
                        selectedunit = null;
                    }
                    selectedunits.Add(hit.transform.gameObject);
                    AddSelectedVertex(hit.transform.position);
                    for (int i = 0; i < selectedunits.Count; i++)
                    {
                        selectedunits[i].transform.gameObject.GetComponent<vertex_sphere>().ChangeColorRed();
                    }

                }
            }
        }

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
        {
            CurrentDownPoint = hit.point;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                MouseDownPoint = hit.point;
        }

        if (IsDragging)
        {
            BoxWidth = Camera.main.WorldToScreenPoint(MouseDownPoint).x - Camera.main.WorldToScreenPoint(CurrentDownPoint).x;
            BoxHeight = Camera.main.WorldToScreenPoint(MouseDownPoint).y - Camera.main.WorldToScreenPoint(CurrentDownPoint).y;
            BoxLeft = Input.mousePosition.x;
            BoxTop = (Screen.height - Input.mousePosition.y) - BoxHeight;

            if (BoxWidth > 0f && BoxHeight < 0f)
            {
                BoxStart = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            }
            else if (BoxWidth > 0f && BoxHeight > 0f)
            {
                BoxStart = new Vector2(Input.mousePosition.x, Input.mousePosition.y + BoxHeight);
            }
            else if (BoxWidth < 0f && BoxHeight < 0f)
            {
                BoxStart = new Vector2(Input.mousePosition.x + BoxWidth, Input.mousePosition.y);
            }
            else if (BoxWidth < 0f && BoxHeight > 0f)
            {
                BoxStart = new Vector2(Input.mousePosition.x + BoxWidth, Input.mousePosition.y + BoxHeight);
            }
            BoxFinish = new Vector2(BoxStart.x + Unsigned(BoxWidth), BoxStart.y - Unsigned(BoxHeight));
        }

    }

    void LateUpdate()
    {
        UnitInDrag.Clear();
        if (IsDragging && UnitsOnScreenSpace.Count > 0)
        {
            selectedunit = null;
            for (int i = 0; i < UnitsOnScreenSpace.Count; i++)
            {
                GameObject UnitObj = UnitsOnScreenSpace[i] as GameObject;
                vertex_sphere PosScript = UnitObj.transform.GetComponent<vertex_sphere>();
                //GameObject selectmarker = UnitObj.transform.Find("vertex_sphere").gameObject;
                if (!UnitInDrag.Contains(UnitObj))
                {
                    if (UnitWithinDrag(PosScript.ScreenPos))
                    {
                        //selectmarker.SetActive(true);
                        UnitObj.GetComponent<vertex_sphere>().ChangeColorRed();
                        UnitInDrag.Add(UnitObj);
                    }
                    else
                    {
                        if (!UnitInDrag.Contains(UnitObj))
                            UnitObj.GetComponent<vertex_sphere>().ChangeColorDefault();
                        //selectmarker.SetActive(false);

                    }
                }
            }
        }
    }

    void OnGUI()
    {
        // draw box if dragging
        if (IsDragging)
        {
            GUI.Box(new Rect(BoxLeft, BoxTop, BoxWidth, BoxHeight), "");
        }
    }

    float Unsigned(float val)
    {
        if (val < 0f)
            val *= -1;
        return val;
    }

    private bool CheckIfMouseIsDragging()
    {
        if (CurrentDownPoint.x - 2 >= MouseDownPoint.x || CurrentDownPoint.y - 2 >= MouseDownPoint.y || CurrentDownPoint.z - 2 >= MouseDownPoint.z ||
            CurrentDownPoint.x < MouseDownPoint.x - 2 || CurrentDownPoint.y < MouseDownPoint.y - 2 || CurrentDownPoint.z < MouseDownPoint.z - 2)
            return true;
        else
            return false;
    }

    public bool UnitWithinScreenSpace(Vector2 UnitScreenPos)
    {
        if ((UnitScreenPos.x < Screen.width && UnitScreenPos.y < Screen.height) && (UnitScreenPos.x > 0f && UnitScreenPos.y > 0f))
            return true;
        else
            return false;
    }

    public bool UnitWithinDrag(Vector2 UnitScreenPos)
    {
        if ((UnitScreenPos.x > BoxStart.x && UnitScreenPos.y < BoxStart.y) && (UnitScreenPos.x < BoxFinish.x && UnitScreenPos.y > BoxFinish.y))
            return true;
        else
            return false;
    }

    public void PutUnitsFromDragIntoSelectedUnits()
    {
        if (UnitInDrag.Count > 0)
        {
            for (int i = 0; i < UnitInDrag.Count; i++)
            {
                if (!selectedunits.Contains(UnitInDrag[i]))
                {
                    selectedunits.Add(UnitInDrag[i]);
                    AddSelectedVertex(UnitInDrag[i].transform.position);
                }
            }
        }
        UnitInDrag.Clear();
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

    public void SetPhysicsObject(GameObject gameObject)
    {
        if (m_physicsobj != gameObject && m_physicsobj != null)
        {
            m_physicsobj.GetComponent<ObjectBehavior>().ClearOutline();
        }
        m_physicsobj = gameObject;
    }

    public void SetAnchorObject(GameObject gameObject)
    {
        if (m_anchorobj != gameObject && m_anchorobj != null)
        {
            m_anchorobj.GetComponent<ObjectBehavior>().ClearOutline();
        }
        m_anchorobj = gameObject;
    }

    public void SetAnchorPosition(Vector3 position)
    {
        m_anchorcoord = position;
    }

    public void AddSelectedVertex(Vector3 v)
    {
        if (m_mode != 2)
        {
            return;
        }
        s_vertices.Add(v);

        // CheckTriangle();
    }

    public void RemoveSelectedVertex(Vector3 v)
    {
        if (m_mode != 2)
        {
            return;
        }
        s_vertices.Remove(v);

        // CheckTriangle();
    }

    void ActivateMenu()
    {
        // deactivate the main buttons
        split_btn.SetActive(true);
        physics_btn.SetActive(true);
        material_btn.SetActive(true);
        split_btn.GetComponent<Button>().onClick.AddListener(SplitButtonClick);
        physics_btn.GetComponent<Button>().onClick.AddListener(PhysicsButtonClick);
        material_btn.GetComponent<Button>().onClick.AddListener(MaterialButtonClick);

        // activate the confirm and cancel buttons
        confirm_btn.SetActive(false);
        cancel_btn.SetActive(false);
        physics_drop.SetActive(false);
        material_drop.SetActive(false);
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
        //physics_drop.SetActive(true);
        //material_drop.SetActive(true);
        confirm_btn.GetComponent<Button>().onClick.AddListener(ConfirmButtonClick);
        cancel_btn.GetComponent<Button>().onClick.AddListener(CancelButtonClick);
        //physics_drop.GetComponent<Dropdown>().onValueChanged.AddListener(PhysicsDropdownClick);
        //material_drop.GetComponent<Dropdown>().onValueChanged.AddListener(MaterialDropdownClick);
    }

    public void SplitButtonClick()
    {
        // clicked when split mode: cancel the split mode
        if (m_mode == 1)
        {
            CancelButtonClick();
        }
        // clicked when default mode: 
        else if (m_mode == 0)
        {
            m_mode = 1;
            ui_text.text = "Select the object to split";
            DeactivateMenu();
        }
        // clicked when other modes: cancel the modes
        else
        {
            CancelButtonClick();
            m_mode = 1;
            ui_text.text = "Select the object to split";
            DeactivateMenu();
        }

        // TODO: give the hover effect to each object
        //       show the message to select the object
    }

    private void PhysicsDropdownClick(int arg0)
    {
        switch (arg0)
        {
            case 0:
                break;
            case 1: // push button
                if ((m_mode == 4 || m_mode == 5) && m_physicsobj != null)
                {
                    m_physicsobj = null;
                    m_anchorobj = null;
                }
                
                m_mode = 4;
                ui_text.text = "Select the object to be a button";
                DeactivateMenu();
                break;
            case 2: // dial (spin)
                break;
            case 3: // hinge
                if ((m_mode == 4 || m_mode == 5) && m_physicsobj != null)
                {
                    m_physicsobj = null;
                    m_anchorobj = null;
                }
                m_mode = 4;
                ui_text.text = "Select the object to be a button";
                DeactivateMenu();
                break;
            default:
                break;
        }
    }

    private void MaterialDropdownClick(int arg0)
    {
        switch(arg0)
        {
            case 0: // wood
                break;
        }
    }

    private void PhysicsButtonClick()
    {
        if(m_mode != 0)
        {
            CancelButtonClick();
            m_mode = 0;
        }
        else
        {
            DeactivateMenu();
            m_mode = 3;
            confirm_btn.SetActive(false);
            cancel_btn.SetActive(false);
            physics_drop.SetActive(true);
            physics_drop.GetComponent<Dropdown>().onValueChanged.AddListener(PhysicsDropdownClick);
        }
    }

    private void MaterialButtonClick()
    {
        if (m_mode != 0)
        {
            CancelButtonClick();
            m_mode = 0;
        }
        else
        {
            m_mode = 7;
            confirm_btn.SetActive(false);
            cancel_btn.SetActive(false);
            material_drop.SetActive(true);
            material_drop.GetComponent<Dropdown>().onValueChanged.AddListener(MaterialDropdownClick);
        }

    }

    public void ConfirmButtonClick()
    {
        switch (m_mode)
        {
            case 0: 
                break;
            case 1: // select object
                ui_text.text = "Select the vertices to be separated";
                m_mode = 2;
                break;
            case 2: // select vertices to split                
                if (m_currobj != null)
                {
                    m_currobj.GetComponent<ObjectBehavior>().DivideMeshes(s_vertices);
                    // erase the vertices
                    m_currobj.GetComponent<ObjectBehavior>().ClearSpheres();
                }
                m_mode = 0;
                
                ActivateMenu();
                break;
            case 3: // physics dropdown

                break;
            case 4: // select the object for physics
                if (m_physicsobj == null) break;

                Debug.Log("mode 4, confirm button clicked" + m_physicsobj.name);
                ui_text.text = "Select the object to which the button will be attached";
                m_mode = 5;
                break;
            case 5: // select the anchor object for physics
                if (m_anchorobj == null) break;

                Debug.Log("mode 5, confirm button clicked" + m_physicsobj.name);
                ui_text.text = "Select the anchor point of two objects";
                m_mode = 6;
                break;
            case 6: // point the anchor of two objects
                ChangePhysics(physics_drop.GetComponent<Dropdown>().value);
                // set the anchor point and axis here..?
                ui_text.text = "";
                m_mode = 0;
                if (m_physicsobj != null)
                {
                    m_physicsobj.GetComponent<ObjectBehavior>().ClearSpheres();
                    m_physicsobj.GetComponent<ObjectBehavior>().ClearOutline();
                }
                if (m_anchorobj != null)
                {
                    m_anchorobj.GetComponent<ObjectBehavior>().ClearOutline();
                }

                ActivateMenu();
                break;
            case 7: // material dropdown
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
                m_physicsobj = null;
                m_anchorcoord = Vector3.zero;
                m_anchorobj = null;
                break;
            case 4: // select the object for physics
                if (m_physicsobj != null)
                {
                    m_physicsobj.GetComponent<ObjectBehavior>().ClearSpheres();
                    m_physicsobj.GetComponent<ObjectBehavior>().ClearOutline();
                }
                m_physicsobj = null;
                m_anchorcoord = Vector3.zero;
                m_anchorobj = null;
                break;
            case 5: // select the anchor object for physics
                if (m_physicsobj != null)
                {
                    m_physicsobj.GetComponent<ObjectBehavior>().ClearSpheres();
                    m_physicsobj.GetComponent<ObjectBehavior>().ClearOutline();
                }
                if (m_anchorobj != null)
                {
                    m_anchorobj.GetComponent<ObjectBehavior>().ClearOutline();
                }
                m_physicsobj = null;
                m_anchorcoord = Vector3.zero;
                m_anchorobj = null;
                break;
            case 6: // point the anchor of two objects
                m_physicsobj = null;
                m_anchorcoord = Vector3.zero;
                m_anchorobj = null;
                break;
            case 7: // material dropdown
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
        for(int i = 0; i < triangles.Length; i += 3)
        {
            if(s_vertices.Contains(m_vertices[triangles[i]]) &&
                s_vertices.Contains(m_vertices[triangles[i+1]]) &&
                s_vertices.Contains(m_vertices[triangles[i+2]]))
            {
                // this triangle is contained in the selected faces
                // highlight it!
                
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

        // CheckTriangle();
    }

    void ChangePhysics(int arg0)
    {
        if (m_physicsobj == null || m_anchorobj == null) return;
        Rigidbody p_rigidbody = m_physicsobj.GetComponent<Rigidbody>();
        Rigidbody a_rigidbody = m_anchorobj.GetComponent<Rigidbody>();
        if (p_rigidbody == null) p_rigidbody = m_physicsobj.AddComponent<Rigidbody>();
        if (a_rigidbody == null) a_rigidbody = m_anchorobj.AddComponent<Rigidbody>();
        p_rigidbody.useGravity = false;
        a_rigidbody.useGravity = false;

        p_rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationZ |
                                  RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX |
                                  RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY;
        a_rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationZ |
                                  RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX |
                                  RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY;

        switch (arg0) {
            // push button
            case 1:
                ConfigurableJoint c_joint = m_physicsobj.GetComponent<ConfigurableJoint>();
                if (c_joint == null) c_joint = m_physicsobj.AddComponent<ConfigurableJoint>();
                c_joint.connectedBody = a_rigidbody;

                // CAUTION: values below are for the button on the right side of the anchor
                // anchor value should be set through the connected anchor
                c_joint.axis = Vector3.right;
                //c_joint.anchor = new Vector3(0, 0.5f, 0);

                c_joint.connectedAnchor = m_anchorcoord;
                break;
            // dial
            case 2:

                break;
            // hinge
            case 3:
                HingeJoint h_joint = m_physicsobj.GetComponent<HingeJoint>();
                if (h_joint == null) h_joint = m_physicsobj.AddComponent<HingeJoint>();
                h_joint.connectedBody = a_rigidbody;

                h_joint.connectedAnchor = m_anchorcoord;
                h_joint.enableCollision = true;
                break;
        }
    }
}
