using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the place to put all of the various steering behavior methods we're going
/// to be using. Probably best to put them all here, not in NPCController.
/// </summary>

public class SteeringBehavior : MonoBehaviour
{

    // The agent at hand here, and whatever target it is dealing with
    public NPCController agent;
    public NPCController target;




    // Below are a bunch of variable declarations that will be used for the next few
    // assignments. Only a few of them are needed for the first assignment.

    // For pursue and evade functions
    public float maxPrediction;
    public float maxAcceleration;

    // For arrive function
    public float maxSpeed = 1.0f;
    public float targetRadiusL;
    public float slowRadiusL;
    public float timeToTarget;

    // For Face function
    public float maxRotation;
    public float maxAngularAcceleration;
    public float targetRadiusA;
    public float slowRadiusA;

    // For wander function
    public float wanderOffset;
    public float wanderRadius;
    public float wanderRate;
    private float wanderOrientation;
    public float startTime;
    Vector3 targetPos;

    // Holds the path to follow
    public List<GameObject> Path;
    GameObject pathsManager;
    public int current = 0;
    bool pathFollow = false;
    bool change = false;

    [SerializeField]
    public List<NPCController> targets;

    public float separateSensorLength = 5f;


    protected void Start()
    {

        agent = GetComponent<NPCController>();
        pathsManager = GameObject.FindGameObjectWithTag("Paths");
        if (target != null) {
            GetComponent<NPCController>().NewTarget(target);
        }
        
        /*
        if (!pathsManager)
        {
            return;
        }
        foreach (Transform child in pathsManager.transform)
        {
            Path.Add(child.gameObject);
        }
        */
    }

    public void SetTarget(NPCController newTarget)
    {
        target = newTarget;
    }
    /*
    public SteeringOutput Seek()
    {
        return new DynamicSeek(agent.k, target.k, maxAcceleration).getSteering();
    }
    public SteeringOutput Flee()
    {
        return new DynamicFlee(agent.k, target.k, maxAcceleration).getSteering();
    }

    public SteeringOutput Pursue()
    {
        DynamicPursue dp = new DynamicPursue(agent.k, target.k, maxAcceleration, maxPrediction);
        SteeringOutput so = dp.getSteering();
        agent.DrawCircle(dp.predictPos, targetRadiusL);
        return so;
    }
    */

    public SteeringOutput CohesionSeek()
    {

        return new DynamicSeek(agent.k, target.k, maxAcceleration).getSteering();
    }

    public SteeringOutput Arrive()
    {
        DynamicArrive da = new DynamicArrive(agent.k, target.k, maxAcceleration, maxSpeed, targetRadiusL, slowRadiusL);
        agent.DrawCircle(target.k.position, slowRadiusL);
        SteeringOutput so = da.getSteering();
        if (pathFollow && !change && current < 3)
        {
            current++;
            change = true;
        }
        //else if (!agent.hit)
        //{
        //    agent.CaughtTarget();
        //}
        return so;
    }

    public SteeringOutput PathFollow()
    {
        pathFollow = true;
        change = false;
        SetTarget(Path[current].GetComponent<NPCController>());
        DynamicSeek sb = new DynamicSeek(agent.k, target.k, maxAcceleration);
        return ObstacleAvoidance(sb);
    }

    public SteeringOutput PursueArrive()
    {

        float dis = (agent.k.position - target.k.position).magnitude;
        if (dis <= slowRadiusL)
        {
            return Arrive();
        }
        DynamicPursue dp = new DynamicPursue(agent.k, target.k, maxAcceleration, maxPrediction);
        //agent.DrawCircle(dp.predictPos, targetRadiusL);
        agent.DrawLine(agent.transform.position, dp.predictPos);
        return dp.getSteering();
    }
    /*
    public SteeringOutput Evade()
    {
        DynamicEvade de = new DynamicEvade(agent.k, target.k, maxAcceleration, maxPrediction);
        SteeringOutput so = de.getSteering();
        agent.DrawCircle(de.predictPos, targetRadiusL);
        return so;
    }
    */
    private float randomBinomial()
    {
        return UnityEngine.Random.value - UnityEngine.Random.value;
    }


    private Vector3 asVector(float _orientation)
    {
        return new Vector3(Mathf.Sin(_orientation), 0f, Mathf.Cos(_orientation));
    }


    public SteeringOutput Wander()
    {
        if (startTime > wanderRate)
        {

            wanderOrientation += randomBinomial() * wanderRate;
            startTime = 0f;
        }
        startTime += Time.deltaTime;
        DynamicAlign a = new DynamicAlign(agent.k, new Kinematic(), maxAngularAcceleration, maxRotation, targetRadiusA, slowRadiusA);
        DynamicFace f = new DynamicFace(new Kinematic(), a);
        DynamicSeek ds = new DynamicSeek(agent.k, new Kinematic(), maxAcceleration);
        DynamicWander dw = new DynamicWander(wanderOffset, wanderRadius, wanderRate, maxAcceleration, wanderOrientation, ds);
        SteeringOutput so = dw.getSteering();
        agent.DrawCircle(dw.targetPos, wanderRadius);
        //agent.DrawLine(agent.k.position, asVector(wanderOrientation));
        return so;


    }


    public SteeringOutput Face()
    {
        try
        {
            DynamicAlign a = new DynamicAlign(agent.k, target.k, maxAngularAcceleration, maxRotation, targetRadiusA, slowRadiusA);
            return new DynamicFace(target.k, a).getSteering();
        }
        catch(Exception e) {
            return new SteeringOutput();
        }
        
    }

    public SteeringOutput Align()
    {
        return new DynamicAlign(agent.k, target.k, maxAngularAcceleration, maxRotation, targetRadiusA, slowRadiusA).getSteering();
    }

    // Pursue with Obstacle Avoidance and Arrival

    private float stationaryTime = 0f;
    float theta = 0.005f;
    private Vector3 deltaPos = Vector3.zero;
    private Vector3 lastFramePos = Vector3.zero;
    bool stationaryTimeIncrimented = false;
    bool seekingUnstuckPoint = false;
    Kinematic unstuckTarget = new Kinematic();

    public SteeringOutput ObstacleAvoidance(SteeringBehaviour behaviourWhenNotAvoiding)
    {
        Kinematic currentTarget = target.k;

        float dis = (agent.k.position - currentTarget.position).magnitude;
        if (dis <= slowRadiusL && agent.mapState != 7)
        {
            return Arrive();
        }
        stationaryTimeIncrimented = false;

        //trigger, sets unstuck position
        if (stationaryTime > 5f)
        {
            seekingUnstuckPoint = true;
            Debug.Log("Looking for new target");
            stationaryTime = 0f;
            //unstuckTarget.position = agent.k.position - (Quaternion.Euler(0f, Random.Range(0,360f), 0f) * Vector3.forward)*15f;
            unstuckTarget.position = agent.k.position + getEscapeVector(agent.k.position, 20).normalized * 10f;
        }

        SteeringBehaviour s;
        if (seekingUnstuckPoint)
        {
            currentTarget = unstuckTarget;
            if ((agent.k.position - unstuckTarget.position).magnitude < slowRadiusL)
            {
                seekingUnstuckPoint = false;
            }
            s = new DynamicSeek(agent.k, currentTarget, maxAcceleration);
        }
        else
        {
            s = behaviourWhenNotAvoiding;
        }



        //DynamicPursue dp = new DynamicPursue(agent.k, target.k, maxAcceleration, maxPrediction);
        deltaPos = lastFramePos - agent.k.position;
        //check if x is stagnant

        //check if it is heading in the direction of the target

        if (s.isStuck())
        {
            stationaryTime += Time.deltaTime;
        }
        else
        {
            stationaryTime -= Time.deltaTime;
            stationaryTime = Mathf.Max(0, stationaryTime);
        }
        //if (Vector3.Dot(agent.k.velocity.normalized, (currentTarget.position - agent.k.position).normalized) < 0.8f)
        //{

        //    if (deltaPos.x < theta)
        //    {
        //        stationaryTime += Time.deltaTime;
        //        stationaryTimeIncrimented = true;
        //    }

        //    //check for z
        //    else if (deltaPos.z < theta)
        //    {
        //        if (!stationaryTimeIncrimented)
        //        {
        //            stationaryTime += Time.deltaTime;
        //        }
        //    }
        //}
        //else {


        //    //stationaryTime = 0;
        //}
        DynamicObstacleAvoidance doa = new DynamicObstacleAvoidance(3f, 2f, s, maxAcceleration);
        SteeringOutput so = doa.getSteering();
        if (agent.mapState == 7)
        {
            agent.DrawCircle(targetPos, wanderRadius);
        }
        else
        {
            agent.DrawLine(agent.k.position, doa.targetPos);
        }
        lastFramePos = agent.k.position;
        return so;
    }

    public Vector3 getEscapeVector(Vector3 pos, int rays)
    {
        Vector3 returnVector = pos;
        float increment = 360f / rays;
        RaycastHit hit;
        for (int i = 0; i < rays; i++)
        {

            if (Physics.Raycast(pos, Quaternion.Euler(0f, (increment * i), 0f) * Vector3.forward, out hit, Mathf.Infinity))
            {
                if ((hit.point - pos).magnitude > returnVector.magnitude)
                {
                    returnVector = (hit.point - pos);
                }
            }
            //didn't hit anything
            else
            {
                return Quaternion.Euler(0f, (increment * i), 0f) * Vector3.forward;
            }

        }

        return returnVector;

    }

    public SteeringOutput ObstacleSeek()
    {
        try
        {
            DynamicSeek sb = new DynamicSeek(agent.k, target.k, maxAcceleration);
            return ObstacleAvoidance(sb);
        }
        catch (Exception e) {
            Debug.Log(e);
            return new SteeringOutput();
        }
        
        
    }
    public SteeringOutput ObstacleFlee()
    {
        DynamicFlee sb = new DynamicFlee(agent.k, target.k, maxAcceleration);
        return ObstacleAvoidance(sb);
    }
    public SteeringOutput TempObstacleFlee(NPCController newTarget)
    {
        DynamicFlee sb = new DynamicFlee(agent.k, newTarget.k, maxAcceleration);
        return ObstacleAvoidance(sb);
    }

    /*
    public SteeringOutput ObstaclePursue()
    {
        DynamicPursue sb = new DynamicPursue(agent.k, target.k, maxAcceleration, maxPrediction);
        agent.DrawLine(agent.transform.position, sb.predictPos);
        return ObstacleAvoidance(sb);
    }
    */

    public SteeringOutput ObstacleWander()
    {
        //if (startTime > wanderRate)
        //{
        //    wanderOrientation += randomBinomial() * wanderRate;
        //    startTime = 0f;
        //}
        startTime += Time.deltaTime;
        DynamicSeek ds = new DynamicSeek(agent.k, new Kinematic(), maxAcceleration);
        DynamicWander dw = new DynamicWander(wanderOffset, wanderRadius, wanderRate, maxAcceleration, 100f, ds);
        targetPos = dw.targetPos;
        //agent.DrawCircle(dw.targetPos, wanderRadius);
        return ObstacleAvoidance(dw);
    }


    public SteeringOutput CollisionAvoidance()
    {

        float radius = 1f;
        SteeringOutput so = new SteeringOutput();
        SteeringOutput dca = new DynamicCollisionAvoidance(agent.k, radius, targets, maxAcceleration).getSteering();
        so.linear = ObstacleSeek().linear + dca.linear;
        so.angular = ObstacleSeek().angular + dca.angular;
        return so;
    }

    public SteeringOutput Separate()
    {
        SteeringOutput so = new SteeringOutput();
        so.linear = Vector3.zero;
        List<NPCController> nearby = new List<NPCController>();

        RaycastHit ray = new RaycastHit();
        int layerMask = 1 << 2;

        for(int i = 0; i < 12; i++){
            
            // Test if any other boids are close by. If so, figure out who is closest and call evade on them.

            if (i == 0)
            {
                Debug.DrawRay(gameObject.transform.position - gameObject.transform.right, -gameObject.transform.right* separateSensorLength, Color.green);
                // Straight left
                if (Physics.Raycast(gameObject.transform.position - gameObject.transform.right, -gameObject.transform.right, out ray, separateSensorLength, layerMask))
                {
                    if(ray.collider.gameObject.GetComponent<NPCController>() == null)
                    {
                        continue;
                    }
                    nearby.Add(ray.collider.gameObject.GetComponent<NPCController>());
                }
            }

            if (i == 1)
            {
                Debug.DrawRay(gameObject.transform.position + Quaternion.AngleAxis(-60f, transform.up) * transform.forward, Quaternion.AngleAxis(-60f, transform.up) * transform.forward * separateSensorLength, Color.red);
                // Angle left
                if (Physics.Raycast(gameObject.transform.position + Quaternion.AngleAxis(-60f, transform.up) * transform.forward, Quaternion.AngleAxis(-60f, transform.up) * transform.forward, out ray, separateSensorLength, layerMask))
                {
                    if (ray.collider.gameObject.GetComponent<NPCController>() == null)
                    {
                        continue;
                    }
                    nearby.Add(ray.collider.gameObject.GetComponent<NPCController>());
                }
            }

            if (i == 2)
            {
                Debug.DrawRay(gameObject.transform.position + Quaternion.AngleAxis(-30f, transform.up) * transform.forward, Quaternion.AngleAxis(-30f, transform.up) * transform.forward * separateSensorLength, Color.blue);
                // Angle right
                if (Physics.Raycast(gameObject.transform.position + Quaternion.AngleAxis(-30f, transform.up) * transform.forward, Quaternion.AngleAxis(-30f, transform.up) * transform.forward, out ray, separateSensorLength, layerMask))
                {
                    if (ray.collider.gameObject.GetComponent<NPCController>() == null)
                    {
                        continue;
                    }
                    nearby.Add(ray.collider.gameObject.GetComponent<NPCController>());
                }
            }

            if(i == 3)
            {
                Debug.DrawRay(gameObject.transform.position + gameObject.transform.forward, transform.forward * separateSensorLength, Color.white);
                // Straight forward
                if (Physics.Raycast(gameObject.transform.position + gameObject.transform.forward, gameObject.transform.forward, out ray, separateSensorLength, layerMask))
                {
                    if (ray.collider.gameObject.GetComponent<NPCController>() == null)
                    {
                        continue;
                    }
                    nearby.Add(ray.collider.gameObject.GetComponent<NPCController>());
                }
            }

            if (i == 4)
            {
                Debug.DrawRay(gameObject.transform.position + Quaternion.AngleAxis(30f, transform.up) * transform.forward, Quaternion.AngleAxis(30f, transform.up) * transform.forward * separateSensorLength, Color.blue);
                // Angle right
                if (Physics.Raycast(gameObject.transform.position + Quaternion.AngleAxis(30f, transform.up) * transform.forward, Quaternion.AngleAxis(30f, transform.up) * transform.forward, out ray, separateSensorLength, layerMask))
                {
                    if (ray.collider.gameObject.GetComponent<NPCController>() == null)
                    {
                        continue;
                    }
                    nearby.Add(ray.collider.gameObject.GetComponent<NPCController>());
                }
            }

            if (i == 5)
            {
                Debug.DrawRay(gameObject.transform.position + Quaternion.AngleAxis(60f, transform.up) * transform.forward, Quaternion.AngleAxis(60f, transform.up) * transform.forward * separateSensorLength, Color.red);
                // Angle left
                if (Physics.Raycast(gameObject.transform.position + Quaternion.AngleAxis(60f, transform.up) * transform.forward, Quaternion.AngleAxis(60f, transform.up) * transform.forward, out ray, separateSensorLength, layerMask))
                {
                    if (ray.collider.gameObject.GetComponent<NPCController>() == null)
                    {
                        continue;
                    }
                    nearby.Add(ray.collider.gameObject.GetComponent<NPCController>());
                }
            }

            if (i == 6)
            {
                Debug.DrawRay(gameObject.transform.position + gameObject.transform.right, transform.right * separateSensorLength, Color.green);
                // Straight right
                if (Physics.Raycast(gameObject.transform.position + gameObject.transform.right, gameObject.transform.right, out ray, separateSensorLength, layerMask))
                {
                    if (ray.collider.gameObject.GetComponent<NPCController>() == null)
                    {
                        continue;
                    }
                    nearby.Add(ray.collider.gameObject.GetComponent<NPCController>());
                }
            }

            if (i == 7)
            {
                Debug.DrawRay(gameObject.transform.position + Quaternion.AngleAxis(120f, transform.up) * transform.forward, Quaternion.AngleAxis(120f, transform.up) * transform.forward * separateSensorLength, Color.black);
                // Angle back right
                if (Physics.Raycast(gameObject.transform.position + Quaternion.AngleAxis(120f, transform.up) * transform.forward, Quaternion.AngleAxis(120f, transform.up) * transform.forward, out ray, separateSensorLength, layerMask))
                {
                    if (ray.collider.gameObject.GetComponent<NPCController>() == null)
                    {
                        continue;
                    }
                    nearby.Add(ray.collider.gameObject.GetComponent<NPCController>());
                }
            }

            if (i == 8)
            {
                Debug.DrawRay(gameObject.transform.position + Quaternion.AngleAxis(-120f, transform.up) * transform.forward, Quaternion.AngleAxis(-120f, transform.up) * transform.forward * separateSensorLength, Color.black);
                // Angle back left
                if (Physics.Raycast(gameObject.transform.position + Quaternion.AngleAxis(-120f, transform.up) * transform.forward, Quaternion.AngleAxis(-120f, transform.up) * transform.forward, out ray, separateSensorLength, layerMask))
                {
                    if (ray.collider.gameObject.GetComponent<NPCController>() == null)
                    {
                        continue;
                    }
                    nearby.Add(ray.collider.gameObject.GetComponent<NPCController>());
                }
            }

            if (i == 9)
            {
                Debug.DrawRay(gameObject.transform.position + Quaternion.AngleAxis(150f, transform.up) * transform.forward, Quaternion.AngleAxis(150f, transform.up) * transform.forward * separateSensorLength, Color.cyan);
                // Angle back right
                if (Physics.Raycast(gameObject.transform.position + Quaternion.AngleAxis(150f, transform.up) * transform.forward, Quaternion.AngleAxis(150f, transform.up) * transform.forward, out ray, separateSensorLength, layerMask))
                {
                    if (ray.collider.gameObject.GetComponent<NPCController>() == null)
                    {
                        continue;
                    }
                    nearby.Add(ray.collider.gameObject.GetComponent<NPCController>());
                }
            }

            if (i == 10)
            {
                Debug.DrawRay(gameObject.transform.position + Quaternion.AngleAxis(-150f, transform.up) * transform.forward, Quaternion.AngleAxis(-150f, transform.up) * transform.forward * separateSensorLength, Color.cyan);
                // Angle back left
                if (Physics.Raycast(gameObject.transform.position + Quaternion.AngleAxis(-150f, transform.up) * transform.forward, Quaternion.AngleAxis(-150f, transform.up) * transform.forward, out ray, separateSensorLength, layerMask))
                {
                    if (ray.collider.gameObject.GetComponent<NPCController>() == null)
                    {
                        continue;
                    }
                    nearby.Add(ray.collider.gameObject.GetComponent<NPCController>());
                }
            }

            if (i == 11)
            {
                Debug.DrawRay(gameObject.transform.position - gameObject.transform.forward, -transform.forward * separateSensorLength, Color.white);
                // Straight forward
                if (Physics.Raycast(gameObject.transform.position - gameObject.transform.forward, -gameObject.transform.forward, out ray, separateSensorLength, layerMask))
                {
                    if (ray.collider.gameObject.GetComponent<NPCController>() == null)
                    {
                        continue;
                    }
                    nearby.Add(ray.collider.gameObject.GetComponent<NPCController>());
                }
            }
        }

        // Now, determine which one is the closest.
        float min_distance = separateSensorLength + 100;

        if(nearby.Count == 0)
        {
            return so;
        }

        NPCController closest = nearby[0];

        foreach (NPCController npc in nearby)
        {
            if((gameObject.transform.position - npc.gameObject.transform.position).magnitude < min_distance)
            {
                closest = npc;
                min_distance = (gameObject.transform.position - npc.gameObject.transform.position).magnitude;
            }
        }

        return TempFlee(closest);
    }


    public SteeringOutput TempFlee(NPCController newTarget)
    {
        return new DynamicFlee(agent.k, newTarget.k, maxAcceleration).getSteering();
    }

    public SteeringOutput TempSeek(NPCController center, float acceleration)
    {
        return new DynamicSeek(agent.k, center.k, acceleration).getSteering();
    }

    public SteeringOutput Cohesion()
    {
        // Find average of boids in flock
        Vector3 centroid = Vector3.zero;
        foreach(Transform child in gameObject.GetComponent<NPCController>().boids.transform)
        {
            centroid += child.transform.position;
        }
        centroid = centroid / 20;
        GameObject temp = new GameObject();
        temp.AddComponent<NPCController>();
        NPCController center = temp.GetComponent<NPCController>();
        center.gameObject.AddComponent<Rigidbody>();
        center.gameObject.AddComponent<SteeringBehavior>();
        center.mapState = 10;
        center.transform.position = centroid;
        SteeringOutput so = TempSeek(center, maxAcceleration);
        Destroy(temp);
        return so;
    }


    public SteeringOutput Flock() {
        DynamicFlocking f = new DynamicFlocking(agent.k, agent.boidsList, maxAcceleration);
        
        return f.getSteering();
    }

    public SteeringOutput FlockBehavior(float weightSeparate, float weightAlign, float weightCohesion)
    {
        Vector3 separationVector = Separate().linear * weightSeparate;
        Vector3 alignVector = PursueArrive().linear * weightAlign;
        Vector3 cohesionVector = Cohesion().linear * weightCohesion;
        Vector3 linear = separationVector + alignVector + cohesionVector;
        SteeringOutput so = new SteeringOutput();
        so.linear = linear;
        return so;
    }

    public void print() {
        Debug.Log("hello");
    }

   



}

