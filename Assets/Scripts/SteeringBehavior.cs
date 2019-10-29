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
    public int current = 0;

    public GameObject[] flock;

    //public GameObject[] bird_group1;
    //public GameObject[] bird_group2;

    public float centerWeight = .10f;
    public float velocityWeight = .65f;
    public float flockWeight = .25f;

    public Vector3 averageVelocity = Vector3.zero;
    public Vector3 centerPoint = Vector3.zero;
    public Vector3 leaderVelocity = Vector3.zero;


    bool avoidIt;
    bool l;
    bool r;
    bool mid;
    bool ls0;
    bool rs0;
    bool b;
    bool lastLeft;
    bool rside;
    bool lside;

    protected void Start() {
        agent = GetComponent<NPCController>();
        stateController = GameObject.FindGameObjectWithTag("GameController").GetComponent<StateController>();
        wanderOrientation = agent.orientation;
        Path1 = GameObject.FindGameObjectsWithTag("Path1");
        Path2 = GameObject.FindGameObjectsWithTag("Path2");
        l = false;
        r = false;
        mid = false;
        avoidIt = false;
        ls0 = false;
        rs0 = false;
        b = false;
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
        Debug.Log("Velocity " + agent.velocity);
        if (agent.velocity.magnitude == 0)
        {
            return 0;
        }
        float targetRotation;

        //Sets the direction you need to face based upon the agent's velocity
        float x = agent.velocity.x;
        float y = agent.velocity.z;
        float orient = Mathf.Atan2(x, y);
        Debug.Log("Agent " + agent.rotation);

        //Subtracts the agent's current orientation from the place it needs to go
        orient -= agent.orientation;
        orient = TurnToAngle(orient);
        Debug.Log("Orient " + orient);

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
    }

    public Vector3 LeaderPath()
    {
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
        else
        {
            Debug.Log("You called this on a object that is not the leader");
            return Vector3.zero;
        }

        if (current > 5)
        {
            current = 5;
        }
        //Does the standard seek behavior on the new point
        Vector3 direction;
        if (this.gameObject.tag == "BK1")
        {
            direction = Path1[current].transform.position;
        }
        //BK2
        else 
        {
            direction = Path2[current].transform.position;
        }

        Vector3 linear_acc = direction - agent.position;
        linear_acc.Normalize();
        //Get linear acceleration and return it
        linear_acc *= maxAcceleration;

        Vector3 flockVelocity = Vector3.zero;
        Vector3 center = CalcCenter(ref flockVelocity);        //Vector3 others = CheckFlockMembers();
        //Vector3 leader = CheckLeader();
        Debug.Log("Flock velocity " + flockVelocity);
        Debug.Log("Center " + center);
        //Debug.Log("Separation " + others);
        Debug.Log("Velocity " + leaderVelocity);

        averageVelocity = flockVelocity;
        centerPoint = center;
        leaderVelocity = gameObject.GetComponent<Rigidbody>().velocity;
        Debug.Log("Acceleration " + linear_acc);

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
        }else if(stateController.statenum == 2 && stateController.CorP == 1)
        {
            if (l || r || mid || ls0 || rs0)
            {
                return true;
            }
        }
        else if (stateController.statenum == 3 )
        {
            if(l || r || mid || ls0 || rs0 || b || rside || lside)
            {
                return true;
            }
            
        }
        return false;
    }

    public Vector3 FollowLeader()
    {
        Vector3 separation = CheckFlockMembers();
        Vector3 leaderVelocity = target.GetComponent<SteeringBehavior>().leaderVelocity;
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

        Debug.Log("Leader " + leaderVelocity);
        Debug.Log("Center " + moveToCenter);
        Debug.Log("Separation " + separation);

        if (target.GetComponent<Rigidbody>().velocity == Vector3.zero)
        {
            Debug.Log("Leader " + leaderVelocity);
            return Vector3.zero;
        }

        if (weightedVelocity.magnitude > maxAcceleration)
        {
            weightedVelocity.Normalize();
            weightedVelocity *= maxAcceleration;
        }

        return weightedVelocity;
    }

    public Vector3 Avoid(RaycastHit hit, RaycastHit leftHit, RaycastHit rightHit, RaycastHit lsHit0, RaycastHit rsHit0, RaycastHit back,
        RaycastHit lsideHit, RaycastHit rsideHit)
    {
        //All do the seek to the new position, using the normal off the point it hit the object multiplied
        //by a number for the new target to seek until it has been avoided

        //PART 3 AVOID
        if(stateController.statenum == 3)
        {
            //If only back collision
            if (!l && !r && !ls0 && !rs0 && !mid && b)
            {

                Vector3 newTarget = back.point + new Vector3(5f, 0, 5f);
                newTarget.Normalize();
                newTarget *= maxAcceleration;
                ResetBools();
                return newTarget;
            }

            //If only left short collision detected (back dose not matter)
            if (ls0 && !l && !r && !rs0 && !mid)
            {

                Vector3 newTarget = lsHit0.point + new Vector3(5f, 0, 5f);
                newTarget.Normalize();
                newTarget *= maxAcceleration;
                ResetBools();
                return newTarget;
            }
            //If only right short collision detected (back dose not matter)
            else if (rs0 && !l && !r && !ls0 && !mid)
            {

                Vector3 newTarget = rsHit0.point + new Vector3(5f, 0, 5f);
                newTarget.Normalize();
                newTarget *= maxAcceleration;
                ResetBools();
                return newTarget;
            }

            //If only right short collision detected (back dose not matter)
            else if (lside)
            {
                Vector3 newTarget = lsideHit.point + new Vector3(5f, 0, 5f);
                newTarget.Normalize();
                newTarget *= maxAcceleration;
                ResetBools();

                return newTarget;
            }//If only right short collision detected (back dose not matter)
            else if (rside)
            {
                Vector3 newTarget = rsideHit.point + new Vector3(5f, 0, 5f);
                newTarget.Normalize();
                newTarget *= maxAcceleration;
                ResetBools();

                return newTarget;
            }

            //For left and right or left, middle and right collisions
            if ((l && r && mid) || (l && r) || (ls0 && mid) || (rs0 && mid) || (rs0 && ls0))
            {
                Vector3 newTarget = hit.point + new Vector3(5f, 0, 5f);
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
            else if ((l && mid) || (r && mid) || (rs0 && mid) || (ls0 && mid))
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
                        Vector3 newTarget = hit.point + new Vector3(5f, 0, 5f);
                        newTarget.Normalize();
                        newTarget *= maxAcceleration;
                        ResetBools();
                        return newTarget;

                    }
                    else
                    {
                        lastLeft = false;
                        Vector3 newTarget = hit.point - new Vector3(5f, 0, 5f);
                        newTarget.Normalize();
                        newTarget *= maxAcceleration;
                        ResetBools();
                        return newTarget;
                    }
                }
                else
                {
                    //Otherwise does the standard seek 
                    Vector3 linear_acc = hit.point + hit.normal * 5f;
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
                Vector3 target = leftHit.point + leftHit.normal * 5f;
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
                Vector3 target = rightHit.point + rightHit.normal * 5f;
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
                        Vector3 newTarget = hit.point + new Vector3(5f, 0, 5f);
                        newTarget.Normalize();
                        newTarget *= maxAcceleration;
                        ResetBools();
                        return newTarget;
                    }
                    else
                    {
                        Vector3 newTarget = hit.point - new Vector3(5f, 0, 5f);
                        newTarget.Normalize();
                        newTarget *= maxAcceleration;
                        ResetBools();
                        return newTarget;
                    }
                }
                else
                {
                    Vector3 linear_acc = hit.point + hit.normal * 5f;
                    linear_acc.Normalize();
                    linear_acc *= maxAcceleration;
                    ResetBools();
                    return linear_acc;
                }
            }
        }
        //Part 2 cone check
        else if (stateController.statenum == 2 && stateController.CorP == 0)
        {
            //If only left short collision detected (back dose not matter)
            if (ls0 && !l && !r && !rs0 && !mid)
            {

                Vector3 newTarget = lsHit0.point + new Vector3(5f, 0, 5f);
                newTarget.Normalize();
                newTarget *= maxAcceleration;
                ResetBools();
                return newTarget;
            }
            //If only right short collision detected (back dose not matter)
            else if (rs0 && !l && !r && !ls0 && !mid)
            {

                Vector3 newTarget = rsHit0.point + new Vector3(5f, 0, 5f);
                newTarget.Normalize();
                newTarget *= maxAcceleration;
                ResetBools();
                return newTarget;
            }

            //If only right short collision detected (back dose not matter)
            else if (lside)
            {
                Vector3 newTarget = lsideHit.point + new Vector3(5f, 0, 5f);
                newTarget.Normalize();
                newTarget *= maxAcceleration;
                ResetBools();

                return newTarget;
            }//If only right short collision detected (back dose not matter)
            else if (rside)
            {
                Vector3 newTarget = rsideHit.point + new Vector3(5f, 0, 5f);
                newTarget.Normalize();
                newTarget *= maxAcceleration;
                ResetBools();

                return newTarget;
            }

            //For left and right or left, middle and right collisions
            if ((l && r && mid) || (l && r) || (ls0 && mid) || (rs0 && mid) || (rs0 && ls0))
            {
                Vector3 newTarget = hit.point + new Vector3(5f, 0, 5f);
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
            else if ((l && mid) || (r && mid) || (rs0 && mid) || (ls0 && mid))
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
                        Vector3 newTarget = hit.point + new Vector3(5f, 0, 5f);
                        newTarget.Normalize();
                        newTarget *= maxAcceleration;
                        ResetBools();
                        return newTarget;

                    }
                    else
                    {
                        lastLeft = false;
                        Vector3 newTarget = hit.point - new Vector3(5f, 0, 5f);
                        newTarget.Normalize();
                        newTarget *= maxAcceleration;
                        ResetBools();
                        return newTarget;
                    }
                }
                else
                {
                    //Otherwise does the standard seek 
                    Vector3 linear_acc = hit.point + hit.normal * 5f;
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
                Vector3 target = leftHit.point + leftHit.normal * 5f;
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
                Vector3 target = rightHit.point + rightHit.normal * 5f;
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
                        Vector3 newTarget = hit.point + new Vector3(5f, 0, 5f);
                        newTarget.Normalize();
                        newTarget *= maxAcceleration;
                        ResetBools();
                        return newTarget;
                    }
                    else
                    {
                        Vector3 newTarget = hit.point - new Vector3(5f, 0, 5f);
                        newTarget.Normalize();
                        newTarget *= maxAcceleration;
                        ResetBools();
                        return newTarget;
                    }
                }
                else
                {
                    Vector3 linear_acc = hit.point + hit.normal * 5f;
                    linear_acc.Normalize();
                    linear_acc *= maxAcceleration;
                    ResetBools();
                    return linear_acc;
                }
            }
        }
        //Part 2 collision prediction
        else if (stateController.statenum == 2 && stateController.CorP == 1)
        {



        }

        ResetBools();
        return new Vector3(0f, 0f, 0f);

    }

    #region RAYCAST AND AVOID
    //method of collision detection
    public Vector3 RayCast(ref bool l, ref bool r, ref bool mid, ref bool ls0, ref bool rs0, ref bool b,
        ref bool lside, ref bool rside)
    {
        //All directional rays for checking on whether there is something to hit
        Vector3 directRight = agent.transform.right;
        Vector3 right = agent.transform.forward + (agent.transform.right / 4);
        Vector3 rs = agent.transform.forward + (agent.transform.right / 2);
        Vector3 rightThree = agent.transform.forward + (agent.transform.right / (2 / 3));

        Vector3 rightMid = agent.transform.forward + ((agent.transform.right * 5) / 4);

        right.Normalize();
        rs.Normalize();
        rightThree.Normalize();
        rightMid.Normalize();

        Vector3 directLeft = -agent.transform.right;
        Vector3 left = agent.transform.forward - (agent.transform.right / 4);
        Vector3 ls = agent.transform.forward - (agent.transform.right / 2);
        Vector3 leftThree = agent.transform.forward - (agent.transform.right / (2 / 3));
        Vector3 leftMid = agent.transform.forward - ((agent.transform.right * 5) / 4);

        //Debug.Log(agent.transform.right);
        left.Normalize();
        ls.Normalize();
        leftThree.Normalize();
        leftMid.Normalize();

        Vector3 leftsidedirect = agent.transform.forward - ((agent.transform.right * 9) / 10);
        Vector3 rightsidedirect = agent.transform.forward - ((agent.transform.right * 9) / 10);

        Vector3 backdirection = -1 * agent.transform.forward;

        //Shifting the position the rays come out of to the cube so that they don't overlap it and cause it 
        //to be a hit
        Vector3 cube = agent.transform.GetChild(0).transform.position;
        cube -= new Vector3(0f, .5f, 0f);

        //Raycast hits for all of them
        RaycastHit hit;
        RaycastHit leftHit;
        RaycastHit rightHit;

        RaycastHit backHit;
        RaycastHit lsHit0;
        RaycastHit rsHit0;

        RaycastHit lsideHit;
        RaycastHit rsideHit;

        //Gizmos.color = Color.cyan;

        Debug.DrawRay(cube, right * 2.5f, Color.black);
        Debug.DrawRay(cube, left * 2.5f, Color.black);


        //checks if things have been hit
        if (Physics.Raycast(cube, agent.transform.TransformDirection(Vector3.forward), out hit, 2.5f))
        {
            Debug.DrawRay(cube, transform.TransformDirection(Vector3.forward) * 2.5f, Color.yellow);

            //Part 2 ConeCheck
            if(stateController.statenum == 2 && stateController.CorP == 0)
            {
                if(this.gameObject.tag == "B1" && hit.transform.tag == "B2")
                {
                    agent.DrawRay(hit.transform.position);
                    mid = true;
                }else if(this.gameObject.tag == "B2" && hit.transform.tag == "B1")
                {
                    agent.DrawRay(hit.transform.position);
                    mid = true;
                }
            }
            //Part 2 Collision Prediction
            else if (stateController.statenum == 2 && stateController.CorP == 1)
            {
                if (this.gameObject.tag == "B1" && hit.transform.tag == "B2")
                {
                    agent.DrawRay(hit.transform.position);
                    mid = true;
                }
                else if (this.gameObject.tag == "B2" && hit.transform.tag == "B1")
                {
                    agent.DrawRay(hit.transform.position);
                    mid = true;
                }
            }
            //Part 3
            else if (stateController.statenum == 3)
            {
                if(hit.transform.tag != "Obstacle")
                {
                    agent.DrawRay(hit.transform.position);
                    mid = true;
                }
            }
        }




        if (Physics.Raycast(cube, right, out rightHit, 2.5f))
        {
            Debug.DrawRay(cube, right * 2.5f, Color.green);

            //Part 2 ConeCheck
            if (stateController.statenum == 2 && stateController.CorP == 0)
            {
                if (this.gameObject.tag == "B1" && rightHit.transform.tag == "B2")
                {
                    agent.DrawRay(rightHit.transform.position);
                    r = true;
                }
                else if (this.gameObject.tag == "B2" && rightHit.transform.tag == "B1")
                {
                    agent.DrawRay(rightHit.transform.position);
                    r = true;
                }
            }
            //Part 2 Collision Prediction
            else if (stateController.statenum == 2 && stateController.CorP == 1)
            {
                if (this.gameObject.tag == "B1" && rightHit.transform.tag == "B2")
                {
                    agent.DrawRay(rightHit.transform.position);
                    r = true;
                }
                else if (this.gameObject.tag == "B2" && rightHit.transform.tag == "B1")
                {
                    agent.DrawRay(rightHit.transform.position);
                    r = true;
                }
            }
            //Part 3
            else if (stateController.statenum == 3)
            {
                if (rightHit.transform.tag != "Obstacle")
                {
                    agent.DrawRay(rightHit.transform.position);
                    r = true;
                }
            }

        }




        if (Physics.Raycast(cube, left, out leftHit, 2.5f))
        {
            Debug.DrawRay(cube, left * 2.5f, Color.blue);

            //Part 2 ConeCheck
            if (stateController.statenum == 2 && stateController.CorP == 0)
            {
                if (this.gameObject.tag == "B1" && leftHit.transform.tag == "B2")
                {
                    agent.DrawRay(leftHit.transform.position);
                    l = true;
                }
                else if (this.gameObject.tag == "B2" && leftHit.transform.tag == "B1")
                {
                    agent.DrawRay(leftHit.transform.position);
                    l = true;
                }
            }
            //Part 2 Collision Prediction
            else if (stateController.statenum == 2 && stateController.CorP == 1)
            {
                if (this.gameObject.tag == "B1" && leftHit.transform.tag == "B2")
                {
                    agent.DrawRay(leftHit.transform.position);
                    l = true;
                }
                else if (this.gameObject.tag == "B2" && leftHit.transform.tag == "B1")
                {
                    agent.DrawRay(leftHit.transform.position);
                    l = true;
                }
            }
            //Part 3
            else if (stateController.statenum == 3)
            {
                if (leftHit.transform.tag != "Obstacle")
                {
                    agent.DrawRay(leftHit.transform.position);
                    l = true;
                }
            }
        }




        if (Physics.Raycast(cube, ls, out lsHit0, 0.75f))
        {
            Debug.DrawRay(cube, ls * 2.5f, Color.cyan);

            //Part 2 ConeCheck
            if (stateController.statenum == 2 && stateController.CorP == 0)
            {
                if (this.gameObject.tag == "B1" && lsHit0.transform.tag == "B2")
                {
                    agent.DrawRay(lsHit0.transform.position);
                    ls0 = true;
                }
                else if (this.gameObject.tag == "B2" && lsHit0.transform.tag == "B1")
                {
                    agent.DrawRay(lsHit0.transform.position);
                    ls0 = true;
                }
            }
            //Part 2 Collision Prediction
            else if (stateController.statenum == 2 && stateController.CorP == 1)
            {
                if (this.gameObject.tag == "B1" && lsHit0.transform.tag == "B2")
                {
                    agent.DrawRay(lsHit0.transform.position);
                    ls0 = true;
                }
                else if (this.gameObject.tag == "B2" && lsHit0.transform.tag == "B1")
                {
                    agent.DrawRay(lsHit0.transform.position);
                    ls0 = true;
                }
            }
            //Part 3
            else if (stateController.statenum == 3)
            {
                if (lsHit0.transform.tag != "Obstacle")
                {
                    agent.DrawRay(lsHit0.transform.position);
                    ls0 = true;
                }
            }
        }




        if (Physics.Raycast(cube, rs, out rsHit0, 0.75f))
        {
            Debug.DrawRay(cube, rs * 2.5f, Color.grey);

            //Part 2 ConeCheck
            if (stateController.statenum == 2 && stateController.CorP == 0)
            {
                if (this.gameObject.tag == "B1" && rsHit0.transform.tag == "B2")
                {
                    agent.DrawRay(rsHit0.transform.position);
                    rs0 = true;
                }
                else if (this.gameObject.tag == "B2" && rsHit0.transform.tag == "B1")
                {
                    agent.DrawRay(rsHit0.transform.position);
                    rs0 = true;
                }
            }
            //Part 2 Collision Prediction
            else if (stateController.statenum == 2 && stateController.CorP == 1)
            {
                if (this.gameObject.tag == "B1" && rsHit0.transform.tag == "B2")
                {
                    agent.DrawRay(rsHit0.transform.position);
                    rs0 = true;
                }
                else if (this.gameObject.tag == "B2" && rsHit0.transform.tag == "B1")
                {
                    agent.DrawRay(rsHit0.transform.position);
                    rs0 = true;
                }
            }
            //Part 3
            else if (stateController.statenum == 3)
            {
                if (rsHit0.transform.tag != "Obstacle")
                {
                    agent.DrawRay(rsHit0.transform.position);
                    rs0 = true;
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
        if (Physics.Raycast(cube, backdirection, out backHit, 1f))
        {
            Debug.DrawRay(cube, backdirection * 1f, Color.blue);
            if (backHit.transform.tag == "Obstacle" && stateController.statenum == 3)
            {
                agent.DrawRay(backHit.transform.position);
                b = true;
            }
        }
        if (Physics.Raycast(cube, leftsidedirect, out lsideHit, 1f))
        {
            Debug.DrawRay(cube, leftsidedirect * 1f, Color.blue);
            if (lsideHit.transform.tag != "Obstacle" && stateController.statenum == 3)
            {
                agent.DrawRay(lsideHit.transform.position);
                lside = true;
            }
        }
        if (Physics.Raycast(cube, rightsidedirect, out rsideHit, 1f))
        {
            Debug.DrawRay(cube, rightsidedirect * 1f, Color.blue);
            if (rsideHit.transform.tag != "Obstacle" && stateController.statenum == 3)
            {
                agent.DrawRay(rsideHit.transform.position);
                rside = true;
            }
        }
        //Sees if something needs to be avoided, returns the new target direction if so
        if (CheckAvoid())
        {
            avoidIt = true;
            return Avoid(hit, leftHit, rightHit, lsHit0, rsHit0, backHit, lsideHit, rsideHit);
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
        b = false;
        rside = false;
        lside = false;

    }


    public Vector3 ConeCheckFollow()
    {
        Vector3 avoidance = RayCast(ref l, ref r, ref mid, ref ls0, ref rs0, ref b, ref lside, ref rside);
        if (avoidIt)
        {
            avoidIt = false;
            return avoidance;
        }
        else
        {
            return FollowLeader();
        }
    }
    /*
    public Vector3 PredictionFollow()
    {
        
        if ()
        {

        }
        else
        {
            return FollowLeader();
        }
    }
    */

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
        agent.DrawCircle(center, 1f);
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
        List<Transform> closeColliders = new List<Transform>();
        for (int i = 0; i < flock.Length; i++)
        {
            for (int j = 0; j < hitColliders.Length; j++)
            {
                //Debug.Log("Collider " + hitColliders[j].transform.name);
                if (hitColliders[j].transform.name == "Ground" || hitColliders[j].transform.name == "Collider Cube") 
                {
                    continue;
                }
                if (hitColliders[j].transform.position != agent.transform.position && hitColliders[j].transform == flock[i].transform)
                {
                    //Debug.Log("Collider " + hitColliders[i].transform.name);
                    average += hitColliders[j].transform.position;
                    count++;
                }
            }
        }
        Debug.Log("average " + average);
        if (average == Vector3.zero)
        {
            return Vector3.zero;
        }
        else
        {
            return average / count;
        }
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
