using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vertex_sphere : MonoBehaviour {
    public bool selected = false;
    Color m_color;
    Renderer m_rend;
    GameSystem m_system;

    void OnMouseDown()
    {
        
        m_rend.material.color = Color.red;
        if (selected == false)
        {
            // send to the game system
            m_system.AddSelectedVertex(gameObject.transform.position);
            Debug.Log(gameObject.transform.position);
            selected = true;
        }
        else
        {
            m_rend.material.color = m_color;
            m_system.RemoveSelectedVertex(gameObject.transform.position);
            selected = false;
        }
    }

    // Use this for initialization
    void Start () {
        m_rend = GetComponent<Renderer>();
        m_color = m_rend.material.color;
        m_system = FindObjectOfType<GameSystem>().Instance;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
