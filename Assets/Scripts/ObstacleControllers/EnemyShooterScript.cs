using UnityEngine;
using System.Collections;

public class EnemyShooterScript : MonoBehaviour {

    public bool dead = false;
    private bool canCollide = true;
    private bool playing = false;
    private float shootTimer = 0f;

    private bool showMuzzleFlash = false;
    private float muzzleFlashTimer = 0f;

    private float xPos = 0f;

    void Start()
    {
        foreach (Transform trans in transform)
        {
            if (trans.gameObject.tag == "MuzzleFlash")
            {
                xPos = trans.position.x;
            }
        }
    }

	// Update is called once per frame
	void FixedUpdate () {
	    if (playing)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer < 0)
            {
                if (GetComponent<SpriteRenderer>().flipX)
                {
                    GetComponent<SpriteRenderer>().flipX = false;
                }
                else
                {
                    GetComponent<SpriteRenderer>().flipX = true;
                }
                playing = false;
            }
        }

        if (showMuzzleFlash)
        {
            muzzleFlashTimer -= Time.deltaTime;
            if (muzzleFlashTimer < 0f)
            {
                foreach (Transform trans in transform)
                {
                    if (trans.gameObject.tag == "MuzzleFlash")
                    {
                        trans.GetComponent<SpriteRenderer>().enabled = false;
                        showMuzzleFlash = false;
                    }
                }
            }
        }
	}

    public void PrepObstacle(GameObject corridor, float direction)
    {
        if (direction > 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            foreach (Transform trans in transform)
            {
                if (trans.gameObject.tag == "MuzzleFlash")
                {
                    trans.GetComponent<SpriteRenderer>().flipX = true;
                    trans.position = new Vector3(-xPos, trans.position.y, trans.position.z);
                }
            }
        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = false;
            foreach (Transform trans in transform)
            {
                if (trans.gameObject.tag == "MuzzleFlash")
                {
                    trans.GetComponent<SpriteRenderer>().flipX = false;
                    trans.position = new Vector3(xPos, trans.position.y, trans.position.z);
                }
            }
        }
        GetComponent<Transform>().position = new Vector2(0f, corridor.GetComponent<Transform>().position.y - 0.14f);
        foreach (Transform trans in transform)
        {
             trans.GetComponent<SpriteRenderer>().enabled = false; 
        }
        dead = false;
        canCollide = true;
        GetComponent<Animator>().ResetTrigger("shoot");
        GetComponent<Animator>().ResetTrigger("dead");
        GetComponent<Animator>().Play("PixelCharAnim_Gun_idle");
    }

    public void RunObstacle()
    {
        foreach (Transform trans in transform)
        {
            if (trans.gameObject.tag != "MuzzleFlash")
            {
                trans.GetComponent<SpriteRenderer>().enabled = true;
                iTween.ScaleTo(trans.gameObject, iTween.Hash("scale", new Vector3(0.032f, 0.032f, 0), "time", 0.4f, "easetype", iTween.EaseType.linear, "looptype", iTween.LoopType.pingPong));
            }       
        }

        playing = true;
        shootTimer = 0.3f;

        GetComponent<Animator>().SetTrigger("shoot");
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (canCollide)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                canCollide = false;
                if (collider.gameObject.GetComponent<PlayerController>().attacking)
                {
                    GetComponent<Animator>().SetTrigger("dead");
                    foreach (Transform trans in transform)
                    {
                        trans.GetComponent<SpriteRenderer>().enabled = false;
                    }

                    dead = true;
                }
                else
                {
                    foreach (Transform trans in transform)
                    {
                        if (trans.gameObject.tag == "MuzzleFlash")
                        {
                            trans.GetComponent<SpriteRenderer>().enabled = true;
                            showMuzzleFlash = true;
                            muzzleFlashTimer = 0.1f;
                        }
                        else
                        {
                            trans.GetComponent<SpriteRenderer>().enabled = false;
                        }
                    }
                }
            }
        }
    }
}
