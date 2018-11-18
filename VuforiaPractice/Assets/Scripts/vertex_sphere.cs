using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class vertex_sphere : MonoBehaviour {
    //public bool selected = false;
    Color m_color;
    Renderer m_rend;
    GameSystem m_system;
    RaycastHit hit;
    NavMeshAgent agent;
    public Vector2 ScreenPos;
    bool OnScreen = false;

    /*
    void OnMouseDown()
    {      
        m_rend.material.color = Color.red;
        if (selected == false)
        {
            // send to the game system
            m_system.AddSelectedVertex(gameObject.transform.position);
            selected = true;
        }
        else
        {
            m_rend.material.color = m_color;
            m_system.RemoveSelectedVertex(gameObject.transform.position);
            selected = false;
        }
    }
    */

    // initialization
    void Start () {
        m_rend = GetComponent<Renderer>();
        m_color = m_rend.material.color;
        m_system = FindObjectOfType<GameSystem>().Instance;
        agent = this.gameObject.GetComponent<NavMeshAgent>();
    }
	
	// 
	void Update () {
        ScreenPos = Camera.main.WorldToScreenPoint(this.transform.position);
        if (m_system.UnitWithinScreenSpace(ScreenPos))
        {
            OnScreen = true;
            if (!m_system.UnitsOnScreenSpace.Contains(this.gameObject))
                m_system.UnitsOnScreenSpace.Add(this.gameObject);
        }
        else
        {
            if (OnScreen)
            {
                m_system.UnitsOnScreenSpace.Remove(this.gameObject);
                OnScreen = false;
            }
        }
    }

    public void ChangeColorRed()
    {
        m_rend.material.color = Color.red;
    }

    public void ChangeColorDefault()
    {
        m_rend.material.color = m_color;
    }
}
