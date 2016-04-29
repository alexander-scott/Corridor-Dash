using UnityEngine;
using System.Collections;

public class EnemyScript : MonoBehaviour {

    private float movementSpeed = 3.0f;
    private bool moving = false;
    public bool dead = false;

    // Update is called once per frame
    // Update is called once per frame
    void FixedUpdate()
    {
        if (moving && !dead)
        {
            Vector2 newVelocity = GetComponent<Rigidbody2D>().velocity;
            newVelocity.x = movementSpeed;
            GetComponent<Rigidbody2D>().velocity = newVelocity;

            if (GetComponent<Rigidbody2D>().velocity.x > 0 && transform.position.x > 0)
            {
                if (!GetComponent<Renderer>().isVisible)
                {
                    moving = false;
                    GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
                }
            }
            else if (GetComponent<Rigidbody2D>().velocity.x < 0 && transform.position.x < 0)
            {
                if (!GetComponent<Renderer>().isVisible)
                {
                    moving = false;
                    GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
                }
            }
        }
    }

    public void PrepObstacle(GameObject corridor)
    {
        GetComponent<Transform>().position = new Vector2(-10f, corridor.GetComponent<Transform>().position.y - 0.14f);
        dead = false;       
        GetComponent<Animator>().ResetTrigger("Dead");
        GetComponent<Animator>().Play("PixelCharAnim_Sword_run");
    }

    public void RunObstacle(float direction)
    {
        if (direction < 0)
        {
            GetComponent<Transform>().position = new Vector2(-3.382f, GetComponent<Transform>().position.y);
            moving = true;
            movementSpeed = direction * -1f;
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            GetComponent<Transform>().position = new Vector2(3.382f, GetComponent<Transform>().position.y);
            moving = true;
            movementSpeed = direction * -1f;
            GetComponent<SpriteRenderer>().flipX = true;
        }
    }

    public void StopObstacle()
    {
        GetComponent<Animator>().Stop();
        moving = false;
        GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            if (collider.gameObject.GetComponent<PlayerController>().attacking)
            {
                if (!dead && moving)
                {
                    GetComponent<Animator>().SetTrigger("Dead");
                    dead = true;
                    moving = false;
                    GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
                }             
            }          
            else
            {
                if (!dead && moving)
                {
                    GetComponent<Animator>().SetTrigger("Attack");
                    moving = false;
                    GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
                }         
            } 
        }
    }
}
