using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// This is the code to get movement input from keyboard or controller into the PC.
/// It moves the player object on-screen using built-in physics, which is to say, it
/// applies a force to the PC, which responds accordingly.
/// </summary>
public class PlayerController : MonoBehaviour {

    public float speed;     
    private Rigidbody rb;
    float orientation;

    /// <summary>
    /// Start() is called only once for any GameObject. Here, we want to retrieve
    /// the RigidBody and save it in variable rb. We do this now and save it so we
    /// don't have to retrieve it every frame, not a good practice.
    /// </summary>
    void Start() {
        rb = GetComponent<Rigidbody>();
        GetComponent<NPCController>().rotation = 0;
    }

    /// <summary>
    /// This is called at the desired framerate, no matter what. This prevents having to
    /// take delta-T into accound as you would when using regular Update().
    /// Note the code for computing movement and applying forces; you may find that 
    /// useful later on.
    /// </summary>
    void FixedUpdate() {
        orientation = transform.eulerAngles.y;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
            transform.eulerAngles = new Vector3(0, Quaternion.FromToRotation(Vector3.forward, movement).eulerAngles.y, 0);
            rb.AddForce(movement * speed);
            //Align();
        }

        /*
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        */


        // This simply moves the avatar based on arrow keys.
        // Note that the nose isn't getting correctly aligned. Use your SteeringBehavior to fix that.
        // Change speed on Inspector for "Red"
        // You could instead map this to the mouse if you like.
        //this.transform.position = new Vector3(transform.position.x + speed * moveHorizontal, 1, transform.position.z + speed * moveVertical);

        // This is the physics based movement used in earlier assignments, not needed here.
        //Vector3 movement = new Vector3(moveHorizontal, 1f, moveVertical);
        //Debug.Log("movement " + movement);
        //rb.AddForce(movement * speed);
    }
    /*public float Align()
    {
        //Debug.Log("Velocity " + agent.velocity);
        /*if (rb.velocity.magnitude == 0)
        {
            return 0;
        }
        float targetRotation;

        //Sets the direction you need to face based upon the agent's velocity
        float x = rb.velocity.x;
        float y = rb.velocity.z;
        float orient = Mathf.Atan2(x, y);
        //Debug.Log("Agent " + agent.rotation);

        //Subtracts the agent's current orientation from the place it needs to go
        orient -= orientation;
        orient = TurnToAngle(orient);
        //Debug.Log("Orient " + orient);

        //Finds if the acceleration needs to slow down or if the agent is in the right direction
        float absoluteOrient = Mathf.Abs(orient);
        if (absoluteOrient < (1))
        {
            rotation = 0f;
        }

        if (absoluteOrient > (2))
        {
            targetRotation = 90f;
        }
        else
        {
            targetRotation = 90f * absoluteOrient / 2;
        }

        targetRotation *= orient / absoluteOrient;
        float angular = targetRotation - rb.rotation;
        angular /= timeToTarget;

        //Checks if the acceleration is too great, fixes it to match if it is not
        float angularAcceleration = Mathf.Abs(angular);
        if (angularAcceleration > maxAngularAcceleration)
        {
            angular /= angularAcceleration;
            angular *= maxAngularAcceleration;
        }
        return angular;
    }*/
    /*public float TurnToAngle(float f)
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
    }*/


}