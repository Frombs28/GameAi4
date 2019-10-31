using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCController : MonoBehaviour
{
    // Store variables for objects
    private SteeringBehavior ai;    // Put all the brains for steering in its own module
    private Rigidbody rb;           // You'll need this for dynamic steering

    private NPCController target;

    // For speed 
    public Vector3 position;        // local pointer to the RigidBody's Location vector
    public Vector3 velocity;        // Will be needed for dynamic steering

    // For rotation
    public float orientation;       // scalar float for agent's current orientation
    public float rotation;          // Will be needed for dynamic steering

    public float maxSpeed;          // what it says

    public int mapState;            // use this to control which "phase" the demo is in

    private Vector3 linear;         // The resilts of the kinematic steering requested
    private float angular;          // The resilts of the kinematic steering requested

    public Text label;              // Used to displaying text nearby the agent as it moves around
    LineRenderer line;              // Used to draw circles and other things

    bool stopped;
    public bool hit = false;


    public Kinematic k;
    public SteeringOutput so;

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public float weightSeparate = 1f;
    public float weightAlign = 1f;
    public float weightCohesion = 1f;
    public GameObject boids;
    Quaternion targetRotation;

    public List<NPCController> boidsList;

    //public NPCController coTarget;

    private void Start()
    {
        ai = GetComponent<SteeringBehavior>();
        
        rb = GetComponent<Rigidbody>();
        line = GetComponent<LineRenderer>();
        stopped = false;


        //intialize steering output
        so = new SteeringOutput();
        k = new Kinematic
        {
            owner = gameObject,
            position = rb.position,
            velocity = Vector3.zero,
            orientation = Mathf.Deg2Rad * rb.rotation.eulerAngles.y
        };

        boidsList = new List<NPCController>();
        GameObject boidParent = GameObject.Find("Boids");
        for (int i = 0; i < boidParent.transform.childCount; i++)
        {
            if (boidParent.transform.GetChild(i).gameObject.activeInHierarchy)
            {

                boidsList.Add(boidParent.transform.GetChild(i).GetComponent<NPCController>());
            }
            
        }

        boidsList.Add(GameObject.Find("Red").GetComponent<NPCController>());

    }

    //sets a new target for the ai
    public void NewTarget(NPCController newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Depending on the phase the demo is in, have the agent do the appropriate steering.
    /// 
    /// </summary>
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F)) {
            mapState = 13;
        }
        
        switch (mapState)
        {
            case 0:
                if (label)
                {
                    label.text = name.Replace("(Clone)", "") + "\nAt Rest";
                }
                stopped = true;
                linear = Vector3.zero;
                angular = 0;
                break;

            case 1:
                //dynamic seek
                if (label)
                {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Dynamic Seek with Obstacle Avoidance";
                }

                stopped = false;
                ai.SetTarget(target);
                linear = ai.ObstacleSeek().linear;
                angular = ai.Face().angular;
                DrawLine(gameObject.transform.position, target.gameObject.transform.position);
                break;

            case 2:
                if (label)
                {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Dynamic Flee with Obstacle Avoidance";
                }
                stopped = false;
                ai.SetTarget(target);
                linear = ai.ObstacleFlee().linear;
                angular = ai.Face().angular;
                DrawLine(gameObject.transform.position, target.gameObject.transform.position);
                break;

            case 3:
                if (label)
                {
                    label.text = name.Replace("(Clone)", "") + "\nPursue with Arrive with Obstacle Avoidance";
                }
                stopped = false;
                //rotation = ai.Face(rotation, linear);
                ai.SetTarget(target);
                linear = ai.ObstacleSeek().linear;
                angular = ai.Face().angular;
                break;

            case 4:
                if (label)
                {
                    label.text = name.Replace("(Clone)", "") + "\nEvade with Obstacle Avoidance";
                }
                stopped = false;
                ai.SetTarget(target);
                linear = ai.ObstacleFlee().linear;
                angular = ai.Face().angular;
                break;

            case 5:
                if (label)
                {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Dynamic Align";
                }
                stopped = false;
                ai.SetTarget(target);
                linear = ai.ObstacleSeek().linear;
                angular = ai.Align().angular;
                break;

            case 6:
                if (label)
                {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Dynamic Face";
                }
                stopped = false;
                ai.SetTarget(target);
                linear = ai.ObstacleSeek().linear;
                angular = ai.Face().angular;
                break;

            case 7:
                if (label)
                {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Dynamic Wander";
                }
                stopped = false;
                ai.SetTarget(target);
                SteeringOutput s = ai.ObstacleWander();
                linear = s.linear;
                angular = s.angular;

                break;

            case 8:
                if (label)
                {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Seek with Collision Avoidance";
                }
                stopped = false;
                //rotation = ai.Face(rotation, linear);
                ai.SetTarget(target);
                linear = ai.CollisionAvoidance().linear;
                angular = ai.Face().angular;
                break;
            case 9:
                if (label)
                {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Path Follow";
                }
                stopped = false;
                //rotation = ai.Face(rotation, linear);
                ai.SetTarget(target);
                linear = ai.PathFollow().linear;
                angular = ai.Face().angular;
                break;
            case 10:
                break;
            case 11:
                if (label)
                {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Seek with Collision Avoidance";
                }
                stopped = false;
                //rotation = ai.Face(rotation, linear);
                ai.SetTarget(target);
                linear = ai.CollisionAvoidance().linear;
                angular = ai.Face().angular;
                break;
            case 12:
                ai.SetTarget(target);
                //Vector3 separationVector = ai.Separate().linear * weightSeparate;
                //Vector3 alignVector = ai.PursueArrive().linear * weightAlign;
                //Vector3 cohesionVector = ai.Cohesion().linear * weightCohesion;
                //linear = separationVector + alignVector + cohesionVector;
                linear = ai.FlockBehavior(weightSeparate, weightAlign, weightCohesion).linear;
                angular = ai.Face().angular;
                /*
                Vector3 rotateDirection = new Vector3(linear.x, 0, linear.z);

                // Rotate
                if (rotateDirection.magnitude > 0)
                {
                    targetRotation = Quaternion.LookRotation(rotateDirection);
                    Debug.DrawLine(transform.position, rotateDirection, Color.cyan);
                    this.transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 180f * Time.deltaTime);
                    //Debug.Log("Rotating");
                }
                else
                {
                    //Debug.Log("Nothing");
                }
                */
                break;
            case 13:
                
                linear = ai.Flock().linear;
                angular = ai.Flock().angular;
                break;
        }
        if (mapState == 10)
        {
            return;
        }
        

        UpdateMovement(linear, angular, Time.deltaTime);
        if (label)
        {
            label.transform.position = Camera.main.WorldToScreenPoint(this.transform.position);
        }
    }

    public void UpdateFromPlayer(Vector3 newLinear, float newAngular)
    {
        so.linear = newLinear;
        so.angular = newAngular;
        k.position = rb.position;

        k.Update(so, maxSpeed, Time.deltaTime);

        //update player
        rb.position = k.position;
        rb.rotation = Quaternion.Euler(Vector3.up * (Mathf.Rad2Deg * -k.orientation));
    }

    /// <summary>
    /// UpdateMovement is used to apply the steering behavior output to the agent itself.
    /// It also brings together the linear and acceleration elements so that the composite
    /// result gets applied correctly.
    /// </summary>
    /// <param name="steeringlin"></param>
    /// <param name="steeringang"></param>
    /// <param name="time"></param>
    private void UpdateMovement(Vector3 _linear, float _angular, float time)
    {
        // Update the orientation, velocity and rotation

        if (mapState == 10)
        {
            return;
        }
        if (stopped)
        {
            rb.velocity = Vector3.zero;
            k.position = rb.position;
            return;
        }
        so.linear = _linear;
        so.angular = _angular;
        k.position = rb.position;

        k.Update(so, maxSpeed, Time.deltaTime);

        //update player
        rb.position = k.position;
        rb.rotation = Quaternion.Euler(Vector3.up * (Mathf.Rad2Deg * -k.orientation));

    }

    // <summary>
    // The next two methods are used to draw circles in various places as part of demoing the
    // algorithms.

    /// <summary>
    /// Draws a circle with passed-in radius around the center point of the NPC itself.
    /// </summary>
    /// <param name="radius">Desired radius of the concentric circle</param>
    public void DrawConcentricCircle(float radius)
    {
        //line.positionCount = 51;
        //line.useWorldSpace = false;
        //float x;
        //float z;
        //float angle = 20f;

        //for (int i = 0; i < 51; i++)
        //{
        //    x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
        //    z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

        //    line.SetPosition(i, new Vector3(x, 0, z));
        //    angle += (360f / 51);
        //}
    }

    /// <summary>
    /// Draws a circle with passed-in radius and arbitrary position relative to center of
    /// the NPC.
    /// </summary>
    /// <param name="position">position relative to the center point of the NPC</param>
    /// <param name="radius">>Desired radius of the circle</param>
    public void DrawCircle(Vector3 position, float radius)
    {
        //line.positionCount = 51;
        //line.useWorldSpace = true;
        //float x;
        //float z;
        //float angle = 20f;

        //for (int i = 0; i < 51; i++)
        //{
        //    x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
        //    z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

        //    line.SetPosition(i, new Vector3(x, 0, z) + position);
        //    angle += (360f / 51);
        //}
    }

    public void DrawLine(Vector3 myPos, Vector3 position)
    {
        //line.positionCount = 2;
        //line.useWorldSpace = true;
        //line.SetPosition(0, myPos);
        //line.SetPosition(1, position);
    }

    void SetFalse()
    {
        label.enabled = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// This is used to help erase the prevously drawn line or circle
    /// </summary>
    public void DestroyPoints()
    {
        if (line)
        {
            line.positionCount = 0;
        }
    }
}
