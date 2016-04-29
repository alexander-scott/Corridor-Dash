using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerController : MonoBehaviour
{
    public bool dead = false;
    public bool moving = false;
    public bool attacking = false;
    public float maximumSpeed;
    public float startingSpeed;
    public float forwardMovementSpeed;

    public static EventHandler PlayerDead;

    // Use this for initialization
    void Start()
    {
        GetComponent<Rigidbody2D>().freezeRotation = true;
        forwardMovementSpeed = -startingSpeed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!dead && moving)
        {
            Vector2 newVelocity = GetComponent<Rigidbody2D>().velocity;
            newVelocity.x = forwardMovementSpeed;
            GetComponent<Rigidbody2D>().velocity = newVelocity;

            if (GetComponent<PlayerController>().forwardMovementSpeed < maximumSpeed && GetComponent<PlayerController>().forwardMovementSpeed > -maximumSpeed)
            {
                // Update the players speed
                if (GetComponent<PlayerController>().forwardMovementSpeed > 0f)
                    GetComponent<PlayerController>().forwardMovementSpeed += Time.deltaTime / 50f;
                else
                    GetComponent<PlayerController>().forwardMovementSpeed -= Time.deltaTime / 50f;
            }
        }
    }

    void KillPlayer()
    {
        dead = true;
        GetComponent<Rigidbody2D>().velocity = new Vector2(0f, 0f);
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        if (PlayerDead != null) PlayerDead(null, null);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (moving)
        {
            if (collider.gameObject.CompareTag("Death"))
            {
                Debug.Log("DEAD " + collider.gameObject.name);
                KillPlayer();
                //GetComponent<Animator>().Stop();
            }
            else if (collider.gameObject.CompareTag("DeathEnemy"))
            {
                if (!attacking && !collider.gameObject.GetComponent<EnemyScript>().dead)
                {
                    Debug.Log("DEAD " + collider.gameObject.name);
                    KillPlayer();
                    //GetComponent<Animator>().Stop();
                }
            }
            else if (collider.gameObject.CompareTag("EnemyShooter"))
            {
                if (!attacking && !collider.gameObject.GetComponent<EnemyShooterScript>().dead)
                {
                    Debug.Log("DEAD " + collider.gameObject.name);
                    KillPlayer();
                    //GetComponent<Animator>().Stop();
                }
            }
            else if (collider.gameObject.CompareTag("ActualTrapDoor"))
            {
                moving = false;
                Debug.Log("DEAD " + collider.gameObject.name);
                dead = true;
                if (PlayerDead != null) PlayerDead(null, null);               
            }
        }
    }

    public bool iStrigger;
    public PhysicsMaterial2D _material;

    private SpriteRenderer spriteRenderer;
    private List<Sprite> spritesList;
    private Dictionary<int, PolygonCollider2D> spriteColliders;
    private bool _processing;

    private int _frame;
    public int Frame
    {
        get { return _frame; }
        set
        {
            if (value != _frame)
            {
                if (value > -1)
                {
                    spriteColliders[_frame].enabled = false;
                    _frame = value;
                    spriteColliders[_frame].enabled = true;
                }
                else
                {
                    _processing = true;
                    StartCoroutine(AddSpriteCollider(spriteRenderer.sprite));
                }
            }
        }
    }

    private IEnumerator AddSpriteCollider(Sprite sprite)
    {
        spritesList.Add(sprite);
        int index = spritesList.IndexOf(sprite);
        PolygonCollider2D spriteCollider = gameObject.AddComponent<PolygonCollider2D>();
        spriteCollider.isTrigger = iStrigger;
        //    spriteCollider.sharedMaterial = _material;
        spriteColliders.Add(index, spriteCollider);
        yield return new WaitForEndOfFrame();
        Frame = index;
        _processing = false;
    }

    private void OnEnable()
    {
        spriteColliders[Frame].enabled = true;
    }

    private void OnDisable()
    {
        spriteColliders[Frame].enabled = false;
    }

    private void Awake()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();

        spritesList = new List<Sprite>();

        spriteColliders = new Dictionary<int, PolygonCollider2D>();

        Frame = spritesList.IndexOf(spriteRenderer.sprite);
    }

    private void LateUpdate()
    {
        if (!_processing)
            Frame = spritesList.IndexOf(spriteRenderer.sprite);
    }
}
