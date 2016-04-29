using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class TrapDoorController : MonoBehaviour
{
    // Update is called once per frame
    //void FixedUpdate()
    //{

    //}

    public void PrepObstacle(GameObject corridor)
    {
        GetComponent<Transform>().position = new Vector2(corridor.GetComponent<Transform>().position.x, corridor.GetComponent<Transform>().position.y - 0.82f);

        foreach (Transform obj in GetComponentInChildren<Transform>())
        {
            if (obj.CompareTag("ActualTrapDoor"))
            {
                obj.localEulerAngles = new Vector3(0f, 0f, 0f);
            }
        }

        HingeJoint2D[] hinges = GetComponentsInChildren<HingeJoint2D>();
        for (int i = 0; i < hinges.Length; i++)
        {
            if (i == 0)
            {
                hinges[i].connectedAnchor = new Vector2(corridor.GetComponent<Transform>().position.x - 0.63f, corridor.GetComponent<Transform>().position.y - 0.8f);
                hinges[i].GetComponent<Transform>().localEulerAngles = new Vector3(0f, 0f, 0f);
                hinges[i].GetComponent<Transform>().position = new Vector2(GetComponent<Transform>().position.x - 0.268f, GetComponent<Transform>().position.y + 0.02164723f);
                hinges[i].GetComponent<Rigidbody2D>().isKinematic = true;
                hinges[i].GetComponent<BoxCollider2D>().isTrigger = false;
            }
            else
            {
                hinges[i].connectedAnchor = new Vector2(corridor.GetComponent<Transform>().position.x + 0.76f, corridor.GetComponent<Transform>().position.y - 0.8f);
                hinges[i].GetComponent<Transform>().localEulerAngles = new Vector3(0f, 0f, 0f);
                hinges[i].GetComponent<Transform>().position = new Vector2(GetComponent<Transform>().position.x + 0.4328f, GetComponent<Transform>().position.y + 0.02164723f);
                hinges[i].GetComponent<Rigidbody2D>().isKinematic = true;
                hinges[i].GetComponent<BoxCollider2D>().isTrigger = false;
            }
        }
    }

    public void RunObstacle()
    {
        HingeJoint2D[] hinges = GetComponentsInChildren<HingeJoint2D>();

        for (int i = 0; i < hinges.Length; i++)
        {
            hinges[i].GetComponent<Rigidbody2D>().isKinematic = false;
            hinges[i].GetComponent<BoxCollider2D>().isTrigger = true;
        }

        //GetComponentInChildren<ParticleSystem>().Play();
    }

    public void StopObstacle()
    {
        HingeJoint2D[] hinges = GetComponentsInChildren<HingeJoint2D>();

        //GetComponentInChildren<ParticleSystem>().Stop();

        for (int i = 0; i < hinges.Length; i++)
        {
            hinges[i].GetComponent<Rigidbody2D>().isKinematic = true;
        }
    }
}
