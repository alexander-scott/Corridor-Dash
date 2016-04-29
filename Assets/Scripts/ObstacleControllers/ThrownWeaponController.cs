using UnityEngine;
using System.Collections;

public class ThrownWeaponController : MonoBehaviour {

    private float movementSpeed = 2.5f;
    private bool moving = false;

    // Update is called once per frame
    // Update is called once per frame
    void FixedUpdate()
    {
        if (moving)
        {
            tag = "Death";

            Vector2 newVelocity = GetComponent<Rigidbody2D>().velocity;
            newVelocity.x = movementSpeed;
            GetComponent<Rigidbody2D>().velocity = newVelocity;
            GetComponent<Transform>().Rotate(new Vector3(0f, 0f, 10f));

            if (GetComponent<Rigidbody2D>().velocity.x > 0 && transform.position.x > 0)
            {
                if (!GetComponent<Renderer>().isVisible)
                {
                    tag = "Untagged";
                    moving = false;
                    GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
                }
            }
            else if (GetComponent<Rigidbody2D>().velocity.x < 0 && transform.position.x < 0)
            {
                if (!GetComponent<Renderer>().isVisible)
                {
                    tag = "Untagged";
                    moving = false;
                    GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
                }
            }
        }  
    }

    public void PrepObstacle(GameObject corridor)
    {
        GetComponent<Transform>().position = new Vector2(-10f, corridor.GetComponent<Transform>().position.y + 0.314f);
    }

    public void RunObstacle(float direction)
    {
        if (direction < 0)
        {
            GetComponent<Transform>().position = new Vector2(-3.382f , GetComponent<Transform>().position.y);
            moving = true;
            movementSpeed = direction * -1f;
        }
        else
        {
            GetComponent<Transform>().position = new Vector2(3.382f, GetComponent<Transform>().position.y);
            moving = true;
            movementSpeed = direction * -1f;
        }
    }

    public void StopObstacle()
    {
        moving = false;
        GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
        GetComponent<Transform>().Rotate(new Vector3(0f, 0f, 0f));
    }
}
