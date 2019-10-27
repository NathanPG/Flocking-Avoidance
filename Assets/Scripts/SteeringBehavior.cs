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
    public GameObject[] Path;
    public int current = 0;

    public GameObject[] flock;

    public float centerWeight = .10f;
    public float velocityWeight = .65f;
    public float flockWeight = .25f;

    public Vector3 averageVelocity = Vector3.zero;
    public Vector3 centerPoint = Vector3.zero;
    public Vector3 leaderVelocity = Vector3.zero;

    protected void Start() {
        agent = GetComponent<NPCController>();
        wanderOrientation = agent.orientation;
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
        if (agent.velocity.magnitude == 0)
        {
            return 0;
        }
        float targetRotation;

        //Sets the direction you need to face based upon the agent's velocity
        float x = agent.velocity.x;
        float y = agent.velocity.z;
        float orient = Mathf.Atan2(x, y);

        //Subtracts the agent's current orientation from the place it needs to go
        orient -= agent.orientation;
        orient = TurnToAngle(orient);

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
        //Vector3 others = CheckFlockMembers();
        //Vector3 leader = CheckLeader();
        Debug.Log("Flock velocity " + flockVelocity);
        Debug.Log("Center " + center);
        //Debug.Log("Separation " + others);
        Debug.Log("Velocity " + leaderVelocity);

        averageVelocity = flockVelocity;
        centerPoint = center;
        leaderVelocity = gameObject.GetComponent<Rigidbody>().velocity;
        //Debug.Log("Velocity " + leaderVelocity);

        //flockVelocity *= velocityWeight;
        //center *= centerWeight;
        //others *= flockWeight;

        /*Vector3 weightedVelocity = flockVelocity + center + others;
        weightedVelocity = new Vector3(weightedVelocity.x, 1f, weightedVelocity.z);
        weightedVelocity.Normalize();
        weightedVelocity *= maxAcceleration;*/

        //return weightedVelocity;
        //return Vector3.zero;
    }

    public Vector3 LeaderPath()
    {

        if (CloseEnough(agent.transform.position.x, Path[current].transform.position.x) &&
            CloseEnough(agent.transform.position.z, Path[current].transform.position.z))
        {
            current++;
        }
        if (current > 5)
        {
            current = 5;
        }
        //Does the standard seek behavior on the new point
        Vector3 direction;
        direction = Path[current].transform.position;

        Vector3 linear_acc = direction - agent.position;
        linear_acc.Normalize();
        //Get linear acceleration and return it
        linear_acc *= maxAcceleration;

        Vector3 flockVelocity = Vector3.zero;
        Vector3 center = CalcCenter(ref flockVelocity);
        //Vector3 others = CheckFlockMembers();
        //Vector3 leader = CheckLeader();
        Debug.Log("Flock velocity " + flockVelocity);
        Debug.Log("Center " + center);
        //Debug.Log("Separation " + others);
        Debug.Log("Velocity " + leaderVelocity);

        averageVelocity = flockVelocity;
        centerPoint = center;
        leaderVelocity = gameObject.GetComponent<Rigidbody>().velocity;


        return linear_acc;
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
        //Debug.Log(weightedVelocity);
        weightedVelocity.Normalize();
        weightedVelocity *= maxAcceleration;

        return weightedVelocity;
        /*Vector3 flockVelocity = Vector3.zero;
        Vector3 center = CalcCenter(ref flockVelocity);
        Vector3 others = CheckFlockMembers();
        //Vector3 leader = CheckLeader();
        Debug.Log("Flock velocity " + flockVelocity);
        Debug.Log("Center " + center);
        Debug.Log("Separation " + others);


        flockVelocity *= velocityWeight;
        center *= centerWeight;
        others *= flockWeight;

        Vector3 weightedVelocity = flockVelocity + center + others;
        weightedVelocity = new Vector3(weightedVelocity.x, 1f, weightedVelocity.z);
        weightedVelocity.Normalize();
        weightedVelocity *= maxAcceleration;

        return weightedVelocity;*/
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
        agent.DrawCircle(center, 1f);
        velocity /= (flock.Length + 1);
        Debug.Log("Calc " + velocity);

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
                //Debug.Log("Collider " + hitColliders[i].transform.name);
                if (hitColliders[j].transform.name == "Ground" || hitColliders[j].transform.name == "Collider Cube") 
                {
                    continue;
                }
                if (hitColliders[j].transform.position != agent.transform.position && hitColliders[j].transform == flock[i].transform)
                {
                    Debug.Log("Collider " + hitColliders[i].transform.name);
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
