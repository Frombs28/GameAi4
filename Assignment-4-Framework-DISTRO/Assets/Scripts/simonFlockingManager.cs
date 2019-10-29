using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class simonFlockingImplementation : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject boidParent;
    private void Start()
    {
        boidParent = GameObject.Find("Boids");
    }

    public void setBoidVelocity() {
        SteeringOutput steering;
        steering.linear = Vector3.zero;

        steering.angular = 0f;

        Vector3 returnVel = Vector3.zero;
        for (int i = 0; i < boidParent.transform.childCount; i++) {
            Transform currentBoid = boidParent.transform.GetChild(i);
            NPCController currentBoidNPC = currentBoid.GetComponent<NPCController>();
            if (!currentBoidNPC) {
                Debug.Log("No NPC Controller attatched to Boid " + currentBoid.gameObject.name);
                return;
            }
            if (currentBoid == transform) {
                continue;
            }
            if ((transform.position - currentBoid.position).magnitude < 10f) {
                returnVel += currentBoidNPC.so.linear;
            }
        }
    }
}
