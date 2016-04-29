using UnityEngine;
using System;
using System.Collections;

public class Link : MonoBehaviour
{
    private LineRenderer line;                           // Line Renderer

    // Use this for initialization
    void Start()
    {
        // Add a Line Renderer to the GameObject
        line = this.gameObject.AddComponent<LineRenderer>();
        // Set the width of the Line Renderer
        line.SetWidth(0.08F, 0.08F);
        // Set the number of vertex fo the Line Renderer
        line.SetVertexCount(2);
        line.SetColors(Color.blue, Color.blue);
        line.material.color = Color.blue;
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<HangingObstacleController>().moving)
        {
            // Update position of the two vertex of the Line Renderer
            line.SetPosition(0, GetComponent<Transform>().transform.position + new Vector3(0f, 0f, -1f));
            line.SetPosition(1, new Vector3(GetComponent<DistanceJoint2D>().connectedAnchor.x, GetComponent<DistanceJoint2D>().connectedAnchor.y, 0f) + new Vector3(0f, 0f, -1f));
        }       
    }
}
