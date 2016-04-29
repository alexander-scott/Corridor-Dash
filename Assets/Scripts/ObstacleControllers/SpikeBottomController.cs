using UnityEngine;
using System;
using System.Collections;

public class SpikeBottomController : MonoBehaviour {

    private bool exploded = false;
    private bool canCollide = false;

	// Update is called once per frame
	void Update () {
	
	}

    public void PrepObstacle(GameObject corridor)
    {
        canCollide = false;
        exploded = false;
        GetComponent<Animator>().ResetTrigger("explode");
        GetComponent<Animator>().Play("explosion_reset");
        GetComponent<Transform>().position = new Vector2(corridor.GetComponent<Transform>().position.x + (corridor.GetComponentInChildren<Renderer>().bounds.size.x / 2), corridor.GetComponent<Transform>().position.y - 0.56f);
    }

    public void RunObstacle()
    {
        canCollide = true;
        exploded = false;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (canCollide)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                if (!exploded)
                {
                    GetComponent<Animator>().SetTrigger("explode");
                    exploded = true;
                }
            }
        }   
    }
}

