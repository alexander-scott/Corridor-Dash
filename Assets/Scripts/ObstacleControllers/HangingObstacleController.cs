using UnityEngine;
using System;
using System.Collections;

public class HangingObstacleController : MonoBehaviour {

    public bool moving = false;
    public bool reset = false;

    public static EventHandler Reset;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (moving && reset)
        {
            moving = false;
            reset = false;
        }

        if (moving)
        {
            GetComponent<Transform>().Rotate(new Vector3(0f, 0f, 10f));
        }

        if (reset)
            moving = true;
    }

    public void PrepObstacle(GameObject corridor, float direction)
    {
        reset = true;
        if (direction < 0)
        {
            GetComponent<Transform>().position = new Vector2(corridor.GetComponent<Transform>().position.x + 1f, corridor.GetComponent<Transform>().position.y + 0.809f);
            GetComponent<DistanceJoint2D>().connectedAnchor = new Vector2(corridor.GetComponent<Transform>().position.x, corridor.GetComponent<Transform>().position.y + 0.809f);
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
            moving = false;
        }
        else
        {
            GetComponent<Transform>().position = new Vector2(corridor.GetComponent<Transform>().position.x - 1f, corridor.GetComponent<Transform>().position.y + 0.809f);
            GetComponent<DistanceJoint2D>().connectedAnchor = new Vector2(corridor.GetComponent<Transform>().position.x, corridor.GetComponent<Transform>().position.y + 0.809f);
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
            moving = false;
        }     
    }

    public void RunObstacle()
    {
        moving = true;
        GetComponent<Transform>().Rotate(new Vector3(0f, 0f, 10f));
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
    }

    public void StopObstacle()
    {
        GetComponent<Transform>().Rotate(new Vector3(0f, 0f, 0f));
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        moving = false;
    }
}
