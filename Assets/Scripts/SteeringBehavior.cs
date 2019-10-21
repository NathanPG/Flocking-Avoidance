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

    public Vector3 Leader()
    {
        return Vector3.zero;
    }

    public Vector3 FollowLeader()
    {
        return Vector3.zero;
    }


    // ETC.

}
