using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the place to put all of the various steering behavior methods we're going
/// to be using. Probably best to put them all here, not in NPCController.
/// </summary>

public class SteeringBehavior : MonoBehaviour {

    // The agent at hand here, and whatever target it is dealing with
    public NPCController agent;
    public NPCController target;

    public StateController stateController;
    // Below are a bunch of variable declarations that will be used for the next few
    // assignments. Only a few of them are needed for the first assignment.

    // For pursue and evade functions
    public float maxPrediction;
    public float maxAcceleration;

    // For arrive function
    public float maxSpeed;
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

    // Holds the path to follow
    public GameObject[] Path1;
    public GameObject[] Path2;
    public GameObject[] Path3;
    public int current = 0;

    public GameObject[] flock;

    //public GameObject[] bird_group1;
    //public GameObject[] bird_group2;

    float centerWeight = .5f;
    float velocityWeight = 1.2f;
    float flockWeight = 2.5f;

    public Vector3 averageVelocity = Vector3.zero;
    public Vector3 centerPoint = Vector3.zero;
    public Vector3 leaderVelocity = Vector3.zero;

    public float distFloat = 100f;


    bool avoidIt;
    bool l;
    bool r;
    bool mid;
    bool ls0;
    bool rs0;
    bool lastLeft;
    bool rside;
    bool lside;

    protected void Start() {
        current = 0;
        agent = GetComponent<NPCController>();
        stateController = GameObject.FindGameObjectWithTag("GameController").GetComponent<StateController>();
        wanderOrientation = agent.orientation;
        if(stateController.statenum == 1 || stateController.statenum == 2)
        {
            Path1 = GameObject.FindGameObjectsWithTag("Path1");
            Path2 = GameObject.FindGameObjectsWithTag("Path2");
        }
        else
        {
            Path3 = GameObject.FindGameObjectsWithTag("Path3");
        }
        
        
        l = false;
        r = false;
        mid = false;
        avoidIt = false;
        ls0 = false;
        rs0 = false;
        lastLeft = false;
        rside = false;
        lside = false;
    }

    public Vector3 Seek() {
        return new Vector3(0f, 0f, 0f);
    }

    public Vector3 Flee()
    {
        return new Vector3(0f, 0f, 0f);
    }


    // Calculate the target to pursue
    public Vector3 Pursue() {
        return new Vector3(0f, 0f, 0f);
    }

    public float Face()
    {
        return 0f;
    }

    public float Align()
    {
        //Debug.Log("Velocity " + agent.velocity);
        if (agent.velocity.magnitude == 0)
        {
            return 0;
        }
        float targetRotation;

        //Sets the direction you need to face based upon the agent's velocity
        float x = agent.velocity.x;
        float y = agent.velocity.z;
        float orient = Mathf.Atan2(x, y);
        //Debug.Log("Agent " + agent.rotation);

        //Subtracts the agent's current orientation from the place it needs to go
        orient -= agent.orientation;
        orient = TurnToAngle(orient);
        //Debug.Log("Orient " + orient);

        //Finds if the acceleration needs to slow down or if the agent is in the right direction
        float absoluteOrient = Mathf.Abs(orient);
        if (absoluteOrient < (targetRadiusA))
        {
            agent.rotation = 0;
        }

        if (absoluteOrient > (slowRadiusA))
        {
            targetRotation = maxRotation;
        }
        else
        {
            targetRotation = maxRotation * absoluteOrient / slowRadiusA;
        }

        targetRotation *= orient / absoluteOrient;
        float angular = targetRotation - agent.rotation;
        angular /= timeToTarget;

        //Checks if the acceleration is too great, fixes it to match if it is not
        float angularAcceleration = Mathf.Abs(angular);
        if (angularAcceleration > maxAngularAcceleration)
        {
            angular /= angularAcceleration;
            angular *= maxAngularAcceleration;
        }
        return angular;
    }
    public float TurnToAngle(float f)
    {
        //Is used in multiple functions, helps turn to the angle to something the agent can spin to
        //without spinning in circles
        float turn = Mathf.PI * 2;
        while (f > Mathf.PI)
        {
            f -= turn;
        }
        while (f < -Mathf.PI)
        {
            f += turn;
        }
        return f;
    }

    public void Leader()
    {
        Vector3 flockVelocity = Vector3.zero;
        Vector3 center = CalcCenter(ref flockVelocity);

        averageVelocity = flockVelocity;
        centerPoint = center;
        leaderVelocity = gameObject.GetComponent<Rigidbody>().velocity;
        //agent.DrawConcentricCircle(4f);
    }

    public Vector3 LeaderPath()
    {
        //agent.DrawConcentricCircle(2f);
        stateController = GameObject.FindGameObjectWithTag("GameController").GetComponent<StateController>();
        if (stateController.statenum == 3)
        {
            Vector3 avoidance = RayCast(ref l, ref r, ref mid, ref ls0, ref rs0, ref lside, ref rside);
            if (avoidIt)
            {
                avoidIt = false;
                return avoidance;
            }
        }
        if(this.gameObject.tag == "BK1")
        {
            if (CloseEnough(agent.transform.position.x, Path1[current].transform.position.x) &&
            CloseEnough(agent.transform.position.z, Path1[current].transform.position.z))
            {
                current++;
            }
        }
        else if (this.gameObject.tag == "BK2"){
            if (CloseEnough(agent.transform.position.x, Path2[current].transform.position.x) &&
            CloseEnough(agent.transform.position.z, Path2[current].transform.position.z))
            {
                current++;
            }
        }
        else if (this.gameObject.tag == "BK3")
        {
            if (CloseEnough(agent.transform.position.x, Path3[current].transform.position.x) &&
            CloseEnough(agent.transform.position.z, Path3[current].transform.position.z))
            {
                current++;
            }
        }
        else
        {
            Debug.Log("You called this on a object that is not the leader");
            return Vector3.zero;
        }

        if (current > 8)
        {
            current = 8;
        }
        //Does the standard seek behavior on the new point
        Vector3 direction;
        if (this.gameObject.tag == "BK1")
        {
            direction = Path1[current].transform.position;
        }
        //BK2
        else if(this.gameObject.tag == "BK2")
        {
            direction = Path2[current].transform.position;
        }
        else
        {
            direction = Path3[current].transform.position;
        }
        //Debug.Log("Path point " + direction);

        Vector3 direct = direction - agent.position;
        float dist = direct.magnitude;
        float speed = agent.velocity.magnitude;
        if (dist < targetRadiusL)
        {
            return new Vector3(0, 0, 0);
        }
        else if (dist > slowRadiusL)
        {
            speed = maxSpeed;
        }
        else
        {
            speed = maxSpeed * dist / slowRadiusL;
        }
        //Get target velocity
        Vector3 targetVelocity = direct;
        targetVelocity.Normalize();
        targetVelocity *= speed;
        //Get the linear acceleration with predict time
        Vector3 linear_acc = (targetVelocity - agent.velocity) / timeToTarget;
        //Restric the max linear acceleration
        if (linear_acc.magnitude > maxAcceleration)
        {
            linear_acc.Normalize();
            linear_acc *= maxAcceleration;
        }

        Vector3 flockVelocity = Vector3.zero;
        Vector3 center = CalcCenter(ref flockVelocity);        //Vector3 others = CheckFlockMembers();
        //Vector3 leader = CheckLeader();
        //Debug.Log("Flock velocity " + flockVelocity);
        //Debug.Log("Center " + center);
        //Debug.Log("Separation " + others);
        //Debug.Log("Velocity " + leaderVelocity);

        averageVelocity = flockVelocity;
        centerPoint = center;
        leaderVelocity = gameObject.GetComponent<Rigidbody>().velocity;
        //Debug.Log("Acceleration " + linear_acc);

        return linear_acc;
    }

    //Checks if something needs to be avoided
    public bool CheckAvoid()
    {
        //Part 2 cone check
        if(stateController.statenum == 2 && stateController.CorP == 0)
        {
            if(l || r || mid || ls0 || rs0)
            {
                return true;
            }
        }
        //Part 3
        else if (stateController.statenum == 3 )
        {
            if(l || r || mid || rside || lside)
            {
                return true;
            }
            
        }
        return false;
    }

    public Vector3 FollowLeader()
    {
        Vector3 separation = CheckFlockMembers();
        Vector3 leaderVelocity = CalcVelocity(target.transform.position, 1);
        centerPoint = target.GetComponent<SteeringBehavior>().centerPoint;

        Vector3 moveToCenter = CalcVelocity(centerPoint, 1);
        if (separation != Vector3.zero)
        {
            separation = CalcVelocity(separation, 0);
        }

        leaderVelocity *= velocityWeight;
        moveToCenter *= centerWeight;
        separation *= flockWeight;

        Vector3 weightedVelocity = leaderVelocity + moveToCenter + separation;
        weightedVelocity.Normalize();
        weightedVelocity *= maxAcceleration;

        //Debug.Log("Leader " + leaderVelocity);
        //Debug.Log("Center " + moveToCenter);
        //Debug.Log("Separation " + separation);

        /*if (target.GetComponent<Rigidbody>().velocity == Vector3.zero)
        {
            //Debug.Log("Leader " + leaderVelocity);
            return Vector3.zero;
        }*/

        /*if (weightedVelocity.magnitude > maxAcceleration)
        {
            weightedVelocity.Normalize();
            weightedVelocity *= maxAcceleration;
        }*/

        return weightedVelocity;
    }

    public Vector3 Avoid(RaycastHit hit, RaycastHit leftHit, RaycastHit rightHit, RaycastHit lsHit0, RaycastHit rsHit0,
        RaycastHit lsideHit, RaycastHit rsideHit)
    {
        //All do the seek to the new position, using the normal off the point it hit the object multiplied
        //by a number for the new target to seek until it has been avoided

        //PART 3 AVOID
        if(stateController.statenum == 3)
        {
            if ((l && r && mid))
            {
                Vector3 newTarget = hit.point + new Vector3(distFloat, 0, distFloat);
                //agent.DrawCircle(hit.point, .5f);
                newTarget.Normalize();
                newTarget *= maxAcceleration;
                //Debug.Log("3 hits");
                ResetBools();
                agent.velocity = Vector3.zero;
                if (lastLeft)
                {
                    agent.rotation = 4;
                }
                else
                {
                    agent.rotation = -4;
                }
                return newTarget;

            }
            //For left and right or left, middle and right collisions
            if ((l && r) || (ls0 && mid) || (rs0 && mid) || (rs0 && ls0))
            {
                Vector3 newTarget = hit.point + new Vector3(distFloat, 0, distFloat);
                //agent.DrawCircle(hit.point, .5f);
                newTarget.Normalize();
                newTarget *= maxAcceleration;
                //Debug.Log("3 hits");
                ResetBools();
                agent.velocity = Vector3.zero;
                //Checks last direction for which way it needs to turn
                if (lastLeft)
                {
                    agent.rotation = 2;
                }
                else
                {
                    agent.rotation = -2;
                }
                return newTarget;
            }
            //For left and mid, and right and mid collisions
            else if ((l && mid) || (r && mid))
            {

                //Debug.Log("2 hits");
                //Checks the dot product for if the normal is facing the NPC directly
                float dotProd = Vector3.Dot(hit.normal, agent.velocity);
                if (dotProd == 1)
                {
                    //If so, changes course so a new placement can be determined and get back on track
                    if (l)
                    {
                        lastLeft = true;
                        Vector3 newTarget = hit.point + new Vector3(distFloat, 0, distFloat);
                        newTarget.Normalize();
                        newTarget *= maxAcceleration;
                        ResetBools();
                        return newTarget;

                    }
                    else
                    {
                        lastLeft = false;
                        Vector3 newTarget = hit.point - new Vector3(distFloat, 0, distFloat);
                        newTarget.Normalize();
                        newTarget *= maxAcceleration;
                        ResetBools();
                        return newTarget;
                    }
                }
                else
                {
                    //Otherwise does the standard seek 
                    Vector3 linear_acc = hit.point + hit.normal * distFloat;
                    linear_acc.Normalize();
                    linear_acc *= maxAcceleration;
                    if (l)
                    {
                        lastLeft = true;
                        agent.rotation = 1;
                    }
                    else
                    {
                        lastLeft = false;
                        agent.rotation = -1;
                    }
                    ResetBools();
                    return linear_acc;
                }
            }
            //Left only
            else if (l)
            {

                lastLeft = true;
                Vector3 target = leftHit.point + leftHit.normal * distFloat;
                Vector3 linear_acc = target - agent.transform.position;
                linear_acc.Normalize();
                linear_acc *= maxAcceleration;
                ResetBools();
                return linear_acc;
            }
            //Right only
            else if (r)
            {

                lastLeft = false;
                Vector3 target = rightHit.point + rightHit.normal * distFloat;
                Vector3 linear_acc = target - agent.transform.position;
                linear_acc.Normalize();
                linear_acc *= maxAcceleration;
                ResetBools();
                return linear_acc;

            }
            //Middle only
            else if (mid)
            {
                lastLeft = false;
                float dotProd = Vector3.Dot(hit.normal, agent.velocity);
                if (dotProd == 1)
                {
                    float random = Random.Range(0, 1);
                    if (random > .5)
                    {
                        Vector3 newTarget = hit.point + new Vector3(distFloat, 0, distFloat);
                        newTarget.Normalize();
                        newTarget *= maxAcceleration;
                        ResetBools();
                        return newTarget;
                    }
                    else
                    {
                        Vector3 newTarget = hit.point - new Vector3(distFloat, 0, distFloat);
                        newTarget.Normalize();
                        newTarget *= maxAcceleration;
                        ResetBools();
                        return newTarget;
                    }
                }
                else
                {
                    Vector3 linear_acc = hit.point + hit.normal * distFloat;
                    linear_acc.Normalize();
                    linear_acc *= maxAcceleration;
                    ResetBools();
                    return linear_acc;
                }
            }
        }
        //Part 2 cone check, avoid the shortest
        else if (stateController.statenum == 2 && stateController.CorP == 0)
        {
            float shortestDist = 999f;
            Vector3 point_Avoid = new Vector3();
            //If only left short collision detected (back dose not matter)
            if (mid || l || r || ls0 || rs0)
            {
                List<Vector3> hitlist = new List<Vector3>();
                if (mid) hitlist.Add(hit.point);
                if (l) hitlist.Add(leftHit.point);
                if (r) hitlist.Add(rightHit.point);
                if (ls0) hitlist.Add(lsHit0.point);
                if (rs0) hitlist.Add(rsHit0.point);
                foreach(Vector3 hitpoint in hitlist)
                {
                    float dist = Mathf.Abs((hitpoint - agent.position).magnitude);
                    if (dist < shortestDist) {
                        shortestDist = dist;
                        point_Avoid = hitpoint;
                    }
                }
                Vector3 newTarget = point_Avoid + new Vector3(distFloat, 0, distFloat);
                newTarget.Normalize();
                newTarget *= maxAcceleration;
                ResetBools();
                return newTarget;
            }
        }
        ResetBools();
        return new Vector3(0f, 0f, 0f);
    }

    #region RAYCAST AND AVOID
    //method of collision detection
    public Vector3 RayCast(ref bool l, ref bool r, ref bool mid, ref bool ls0, ref bool rs0,
        ref bool lside, ref bool rside)
    {
        //All directional rays for checking on whether there is something to hit
        Vector3 directRight = agent.transform.right;
        Vector3 right = agent.transform.forward + (agent.transform.right / 4);
        //CONE CHECK BOUND
        Vector3 rs = agent.transform.forward + (agent.transform.right / 2.3f);
        Vector3 rightThree = agent.transform.forward + (agent.transform.right / (2 / 3));
        Vector3 rightMid = agent.transform.forward + ((agent.transform.right * 5) / 4);

        right.Normalize();
        rs.Normalize();
        rightThree.Normalize();
        rightMid.Normalize();

        Vector3 directLeft = -agent.transform.right;
        Vector3 left = agent.transform.forward - (agent.transform.right / 4);
        //CONE CHECK BOUND
        Vector3 ls = agent.transform.forward - (agent.transform.right / 2.3f);
        Vector3 leftThree = agent.transform.forward - (agent.transform.right / (2 / 3));
        Vector3 leftMid = agent.transform.forward - ((agent.transform.right * 5) / 4);

        left.Normalize();
        ls.Normalize();
        leftThree.Normalize();
        leftMid.Normalize();

        Vector3 leftsidedirect = agent.transform.forward - ((agent.transform.right * 9) / 10);
        Vector3 rightsidedirect = agent.transform.forward - ((agent.transform.right * 9) / 10);

        //Shifting the position the rays come out of to the cube so that they don't overlap it and cause it 
        //to be a hit
        Vector3 cube = agent.transform.GetChild(0).transform.position;
        cube -= new Vector3(0f, .5f, 0f);

        //Raycast hits for all of them
        RaycastHit hit;
        RaycastHit leftHit;
        RaycastHit rightHit;
        RaycastHit lsHit0;
        RaycastHit rsHit0;
        RaycastHit lsideHit;
        RaycastHit rsideHit;

        //Gizmos.color = Color.cyan;

        //Debug.DrawRay(cube, right * 2.5f, Color.black);
        //Debug.DrawRay(cube, left * 2.5f, Color.black);


        // MID RAY 2.5F
        if (Physics.Raycast(cube, agent.transform.TransformDirection(Vector3.forward), out hit, 3f))
        {
            Debug.DrawRay(cube, transform.TransformDirection(Vector3.forward) * 3f, Color.yellow);
            //Debug.Log("Hit " + hit.transform.name);
            agent.DrawCircle(hit.point, 1f);
            //Part 2 ConeCheck
            if (stateController.statenum == 2 && stateController.CorP == 0)
            {
                if(this.gameObject.tag == "B1" && hit.transform.tag == "B2")
                {
                    agent.DrawCircle(hit.point, 1f);
                    mid = true;
                }else if(this.gameObject.tag == "B2" && hit.transform.tag == "B1")
                {
                    agent.DrawCircle(hit.point, 1f);
                    mid = true;
                }
            }
            //Part 3
            else if (stateController.statenum == 3)
            {
                if(hit.transform.tag != "Obstacle")
                {
                    agent.DrawCircle(hit.point, 1f);
                    mid = true;
                }
            }
        }
        // 1/4 RIGHT RAY 2.5F
        if (Physics.Raycast(cube, right, out rightHit, 2.5f))
        {
            Debug.DrawRay(cube, right * 2.5f, Color.green);
            //Debug.Log("Hit " + rightHit.transform.name);
            agent.DrawCircle(rightHit.point, 1f);
            //Part 2 ConeCheck
            if (stateController.statenum == 2 && stateController.CorP == 0)
            {
                if (this.gameObject.tag == "B1" && rightHit.transform.tag == "B2")
                {
                    agent.DrawCircle(rightHit.point, 1f);
                    r = true;
                }
                else if (this.gameObject.tag == "B2" && rightHit.transform.tag == "B1")
                {
                    agent.DrawCircle(rightHit.point, 1f);
                    r = true;
                }
            }
            //Part 3
            else if (stateController.statenum == 3)
            {
                if (rightHit.transform.tag != "Obstacle")
                {
                    agent.DrawCircle(rightHit.point, 1f);
                    r = true;
                }
            }

        }

        // 1/4 LEFT RAY 2.5F
        if (Physics.Raycast(cube, left, out leftHit, 2.5f))
        {
            Debug.DrawRay(cube, left * 2.5f, Color.blue);

            //Part 2 ConeCheck
            if (stateController.statenum == 2 && stateController.CorP == 0)
            {
                if (this.gameObject.tag == "B1" && leftHit.transform.tag == "B2")
                {
                    agent.DrawCircle(leftHit.point, 1f);
                    l = true;
                }
                else if (this.gameObject.tag == "B2" && leftHit.transform.tag == "B1")
                {
                    agent.DrawCircle(leftHit.point, 1f);
                    l = true;
                }
            }
            //Part 3
            else if (stateController.statenum == 3)
            {
                if (leftHit.transform.tag == "Obstacle")
                {
                    agent.DrawCircle(leftHit.point, 1f);
                    l = true;
                }
            }
        }

        // 1/2.3 LEFT SHORT RAY 0.75
        if (Physics.Raycast(cube, ls, out lsHit0, 0.75f))
        {
            Debug.DrawRay(cube, ls * .75f, Color.cyan);

            //Part 2 ConeCheck
            if (stateController.statenum == 2 && stateController.CorP == 0)
            {
                if (this.gameObject.tag == "B1" && lsHit0.transform.tag == "B2")
                {
                    agent.DrawCircle(lsHit0.point, 1f);
                    ls0 = true;
                }
                else if (this.gameObject.tag == "B2" && lsHit0.transform.tag == "B1")
                {
                    agent.DrawCircle(lsHit0.point, 1f);
                    ls0 = true;
                }
            }
            //Part 3
            else if (stateController.statenum == 3)
            {
                if (lsHit0.transform.tag == "Obstacle")
                {
                    //agent.DrawRay(lsHit0.transform.position);
                    //ls0 = true;
                }
            }
        }
        // 1/2.3 RIGHT SHORT RAY 0.75
        if (Physics.Raycast(cube, rs, out rsHit0, 0.75f))
        {
            Debug.DrawRay(cube, rs * .75f, Color.grey);

            //Part 2 ConeCheck
            if (stateController.statenum == 2 && stateController.CorP == 0)
            {
                if (this.gameObject.tag == "B1" && rsHit0.transform.tag == "B2")
                {
                    agent.DrawCircle(rsHit0.point, 1f);
                    rs0 = true;
                }
                else if (this.gameObject.tag == "B2" && rsHit0.transform.tag == "B1")
                {
                    agent.DrawCircle(rsHit0.point, 1f);
                    rs0 = true;
                }
            }
            //Part 3
            else if (stateController.statenum == 3)
            {
                if (rsHit0.transform.tag == "Obstacle")
                {
                    //agent.DrawRay(rsHit0.transform.position);
                    //rs0 = true;
                }
            }
        }

        /*
        if (Physics.Raycast(cube, leftThree, out lsHit0, 0.75f))
        {
            Debug.DrawRay(cube, leftThree * 2.5f, Color.magenta);

            if (lsHit0.transform.tag != "Hunter" && lsHit0.transform.name != "Player"
                && lsHit0.transform.tag != "Red" && lsHit0.transform.tag != "Wolf")
            {
                agent.DrawRay(lsHit0.transform.position);
                ls0 = true;
            }
        }
        if (Physics.Raycast(cube, rightThree, out rsHit0, 0.75f) || Physics.Raycast(cube, rightMid, out rsHit0, 0.75f))
        {
            Debug.DrawRay(cube, rightThree * 2.5f, Color.white);
            Debug.DrawRay(cube, rightMid * 2.5f, Color.cyan);

            if (rsHit0.transform.tag != "Hunter" && rsHit0.transform.name != "Player"
                && rsHit0.transform.tag != "Red" && rsHit0.transform.tag != "Wolf")
            {
                agent.DrawRay(rsHit0.transform.position);
                rs0 = true;
            }
        }

        if (Physics.Raycast(cube, directLeft, out lsHit0, 0.75f) || Physics.Raycast(cube, leftMid, out lsHit0, 0.75f))
        {
            Debug.DrawRay(cube, directLeft * 2.75f, Color.black);
            Debug.DrawRay(cube, leftMid * 2.75f, Color.black);

            if (lsHit0.transform.tag != "Hunter" && lsHit0.transform.name != "Player"
                && lsHit0.transform.tag != "Red" && lsHit0.transform.tag != "Wolf")
            {
                agent.DrawRay(lsHit0.transform.position);
                ls0 = true;
            }
        }
        if (Physics.Raycast(cube, directRight, out rsHit0, 0.75f))
        {
            Debug.DrawRay(cube, directRight * 2.5f, Color.blue);

            if (rsHit0.transform.tag != "Hunter" && rsHit0.transform.name != "Player"
                && rsHit0.transform.tag != "Red" && rsHit0.transform.tag != "Wolf")
            {
                agent.DrawRay(rsHit0.transform.position);
                rs0 = true;
            }
        }
        */
        if (Physics.Raycast(cube, leftsidedirect, out lsideHit, 1f))
        {
            Debug.DrawRay(cube, leftsidedirect * 1f, Color.blue);
            if (lsideHit.transform.tag == "Obstacle" && stateController.statenum == 3)
            {
                agent.DrawCircle(lsideHit.point, 1f);
                lside = true;
            }
        }
        if (Physics.Raycast(cube, rightsidedirect, out rsideHit, 1f))
        {
            Debug.DrawRay(cube, rightsidedirect * 1f, Color.blue);
            if (rsideHit.transform.tag == "Obstacle" && stateController.statenum == 3)
            {
                agent.DrawCircle(lsideHit.point, 1f);
                rside = true;
            }
        }
        //Sees if something needs to be avoided, returns the new target direction if so
        if (CheckAvoid())
        {
            avoidIt = true;
            return Avoid(hit, leftHit, rightHit, lsHit0, rsHit0, lsideHit, rsideHit);
        }
        else
        {
            return Vector3.zero;
        }
    }
    #endregion

    public void ResetBools()
    {
        l = false;
        r = false;
        mid = false;
        ls0 = false;
        rs0 = false;
        rside = false;
        lside = false;
    }

    public Vector3 RaycastFollow()
    {
        Vector3 avoidance = RayCast(ref l, ref r, ref mid, ref ls0, ref rs0, ref lside, ref rside);
        if (avoidIt)
        {
            avoidIt = false;
            //Vector3 separation = CheckFlockMembers();
            //Debug.Log("Separation (ray) " + separation);
            return avoidance;
        }
        else
        {
            return FollowLeader();
        }
    }

    public Vector3 ConeCheckFollow()
    {
        Vector3 avoidance = RayCast(ref l, ref r, ref mid, ref ls0, ref rs0, ref lside, ref rside);
        if (avoidIt)
        {
            avoidIt = false;
            Vector3 separation = CheckFlockMembers();
            //Debug.Log("Separation (cone) " + separation);
            return avoidance + separation;
        }
        else
        {
            return FollowLeader();
        }
    }
    
    public Vector3 PredictionFollow()
    {
        float closest_time = 999f;
        float predict_time = 999f;
        GameObject[] OtherGroup;
        if (this.gameObject.tag == "B1")
        {
            OtherGroup = GameObject.FindGameObjectsWithTag("B2");
        }
        else
        {
            OtherGroup = GameObject.FindGameObjectsWithTag("B1");
        }
        foreach (GameObject bird in OtherGroup)
        {
            NPCController agent = this.gameObject.GetComponent<NPCController>();
            NPCController target = bird.GetComponent<NPCController>();

            predict_time = Vector3.Dot(target.position - agent.position, target.velocity - agent.velocity)
                / Mathf.Pow((target.velocity - agent.velocity).magnitude, 2);
            predict_time *= -1;
            if (predict_time < closest_time)
            {
                closest_time = predict_time;
            }
        }
        //Debug.Log("Closest Collision Time: "+ closest_time);
        if (closest_time < 1f && closest_time > 0)
        {
            //AVOID
            Vector3 newTarget = agent.position + closest_time * agent.velocity + new Vector3(5f, 0, 5f);
            agent.DrawCircle(newTarget, 0.5f);

            Vector3 separation = CheckFlockMembers();

            newTarget += separation;
            //Debug.Log("Separation " + separation);

            newTarget.Normalize();
            newTarget *= maxAcceleration;
            ResetBools();
            
            return newTarget;

        }
        else
        {
            return FollowLeader();
        }
    }
    

    public Vector3 CalcCenter(ref Vector3 velocity)
    {
        Vector3 center = Vector3.zero;
        velocity = Vector3.zero;
        for (int i = 0; i < flock.Length; i++)
        {
            //Debug.Log(flock[i].transform.position);
            center += flock[i].transform.position;
            //Debug.Log(flock[i].transform.position);
            velocity += flock[i].GetComponent<NPCController>().velocity;
            //Debug.Log(flock[i].GetComponent<NPCController>().velocity);
        }
        center += agent.transform.position;
        velocity += agent.GetComponent<Rigidbody>().velocity;
        center = center / (flock.Length + 1);
        agent.DrawCircle(center, 3f);
        velocity /= (flock.Length + 1);
        //Debug.Log("Calc " + velocity);

        return center;
        //Debug.Log("Velocity " + velocity);

        //Vector3 direction = center - agent.position;
        //direction.Normalize();
        //direction *= maxAcceleration;
        //return direction;
    }

    public Vector3 CheckFlockMembers()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f);
        Vector3 average = Vector3.zero;
        int count = 0;
        if (hitColliders.Length == 0)
        {
            return Vector3.zero;
        }
        //List<Transform> closeColliders = new List<Transform>();
        for (int i = 0; i < flock.Length; i++)
        {
            for (int j = 0; j < hitColliders.Length; j++)
            {
                //Debug.Log("Collider " + hitColliders[j].transform.position);
                if (hitColliders[j].transform.name == "Ground" || hitColliders[j].transform.name == "Cube" || hitColliders[j].transform.name == "Obstacle") 
                {
                    continue;
                }
                if (hitColliders[j].transform.position != agent.transform.position && hitColliders[j].transform == flock[i].transform)
                {
                    if (average == Vector3.zero)
                    {
                        average = hitColliders[j].transform.position;
                    }
                    else if (IsCloser(hitColliders[j].transform.position, average))
                    {
                        average = hitColliders[j].transform.position;
                    }
                    //Debug.Log("Collider " + hitColliders[j].transform.name);
                    average += hitColliders[j].transform.position;
                    count++;
                }
                //Debug.Log("average " + average);
            }
        }
        if (average == Vector3.zero)
        {
            return Vector3.zero;
        }
        else
        {
            return average;
        }
    }
    public bool IsCloser(Vector3 other, Vector3 old)
    {
        if ((agent.transform.position - other).magnitude > (agent.transform.position - old).magnitude)
        {
            return false;
        }
        return true;
    }
    public Vector3 CalcVelocity(Vector3 direction, int num)
    {
        Vector3 acceleration = direction - agent.position;
        if (num == 0)
        {
            acceleration = agent.position - acceleration;
        }
        acceleration.Normalize();
        acceleration *= maxAcceleration;
        return acceleration;
    }
    public bool CloseEnough(float a, float b)
    {
        if (a >= b - 1.5 && a <= b + 1.5)
        {
            return true;
        }
        return false;
    }
    //public Vector3 CheckLeader()
    //{

    //}


    // ETC.

}
