using UnityEngine;
using System;
using System.Collections;

public class CameraController : MonoBehaviour
{

    public float dampTime = 0.15f;
    private Vector3 velocity = Vector3.zero;
    public Transform target;

    private bool dead = false;

    void Start()
    {
        UIController.Restarting += Restarting;
        PlayerController.PlayerDead += IsDead;
    }

    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            if (!dead)
            {
                if (target.GetComponent<Transform>().position.y > 0)
                {
                    Vector3 point = GetComponent<Camera>().WorldToViewportPoint(target.position);
                    Vector3 delta = target.position - GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
                    Vector3 destination = transform.position + delta;
                    transform.position = Vector3.SmoothDamp(transform.position, new Vector3(transform.position.x, destination.y, transform.position.z), ref velocity, dampTime);
                }
            }
            else
            {
                Vector3 point = GetComponent<Camera>().WorldToViewportPoint(new Vector3(target.position.x, target.position.y + 0.1f, target.position.z));
                Vector3 delta = new Vector3(target.position.x, target.position.y + 0.1f, target.position.z) - GetComponent<Camera>().ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
                Vector3 destination = transform.position + delta;
                transform.position = Vector3.SmoothDamp(transform.position, new Vector3(destination.x, destination.y, destination.z), ref velocity, dampTime);

                if (GetComponent<Camera>().orthographicSize > 1f)
                    GetComponent<Camera>().orthographicSize -= 0.02f;
            }
        }
    }

    void IsDead(System.Object obj, EventArgs args)
    {
        dead = true;
    }

    void Restarting(System.Object obj, EventArgs args)
    {
        dead = false;
        transform.position = new Vector3(0f, 0f, -10f);
        GetComponent<Camera>().orthographicSize = 5f;
    }
}
