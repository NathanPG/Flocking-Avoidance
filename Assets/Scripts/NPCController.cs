﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCController : MonoBehaviour {
    // Store variables for objects
    private SteeringBehavior ai;    // Put all the brains for steering in its own module
    private Rigidbody rb;           // You'll need this for dynamic steering

    // For speed 
    public Vector3 position;        // local pointer to the RigidBody's Location vector
    public Vector3 velocity;        // Will be needed for dynamic steering

    // For rotation
    public float orientation;       // scalar float for agent's current orientation
    public float rotation;          // Will be needed for dynamic steering

    public float maxSpeed;          // what it says

    public int phase;               // use this to control which "phase" the demo is in

    private Vector3 linear;         // The resilts of the kinematic steering requested
    private float angular;          // The resilts of the kinematic steering requested

    public Text label;              // Used to displaying text nearby the agent as it moves around
    LineRenderer line;              // Used to draw circles and other things

    private void Start() {
        ai = GetComponent<SteeringBehavior>();
        rb = GetComponent<Rigidbody>();
        line = GetComponent<LineRenderer>();
        position = rb.position;
        orientation = transform.eulerAngles.y;
        //rotation = 0f;
    }

    /// <summary>
    /// Depending on the phase the demo is in, have the agent do the appropriate steering.
    /// 
    /// </summary>
    void FixedUpdate() {
        switch (phase) {
            case 1:
                if (label) {
                    // replace "First algorithm" with the name of the actual algorithm you're demoing
                    // do this for each phase
                    label.text = name.Replace("(Clone)","") + "\nAlgorithm: Follow Leader"; 
                }
                //linear = ai.FollowLeader();   // For example
                //angular = ai.Face();    // For example

                linear = ai.FollowLeader();
                angular = ai.Align();
                break;
            case 2:
                if (label) {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Leader";
                }
                //linear = ai.FollowLeader();
                //ai.Align();
                ai.Leader();
                velocity = rb.velocity;
                orientation = transform.eulerAngles.y;
                //rotation = transform.rotation;
                //rotation = orientation;
                //rotation = 
                //angular = ai.Align();
                //rotation = orientation;
                //Debug.Log("Angular " + orientation);
                // linear = ai.whatever();  -- replace with the desired calls
                // angular = ai.whatever();
                break;
            case 3:
                //PART 2 BIRD KINGS
                if (label) {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Leader Follow Path";
                }
                linear = ai.LeaderPath();
                angular = ai.Align();

                break;
            case 4:
                //PART2 cone check
                if (label) {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Cone check follow leader";
                }
                linear = ai.ConeCheckFollow();
                angular = ai.Align();

                break;
            case 5:
                //PART 2 collision prediction
                if (label) {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: CP follow leader";
                }
                linear = ai.PredictionFollow();
                angular = ai.Align();

                break;
            case 6:
                //PART 3
                if (label)
                {
                    label.text = name.Replace("(Clone)", "") + "\nAlgorithm: Raycast follow leader";
                }
                linear = ai.RaycastFollow();
                angular = ai.Align();
                break;

        }
        update(linear, angular, Time.deltaTime);
        if (label) {
            label.transform.position = Camera.main.WorldToScreenPoint(this.transform.position);
        }
    }

    private void update(Vector3 steeringlin, float steeringang, float time) {
        // Update the orientation, velocity and rotation
        orientation += rotation * time;
        velocity += steeringlin * time;
        rotation += steeringang * time;

        if (velocity.magnitude > maxSpeed) {
            velocity.Normalize();
            velocity *= maxSpeed;
        }

        rb.AddForce(velocity - rb.velocity, ForceMode.VelocityChange);
        position = rb.position;
        rb.MoveRotation(Quaternion.Euler(new Vector3(0, Mathf.Rad2Deg * orientation, 0)));
    }

    // <summary>
    // The next two methods are used to draw circles in various places as part of demoing the
    // algorithms.

    /// <summary>
    /// Draws a circle with passed-in radius around the center point of the NPC itself.
    /// </summary>
    /// <param name="radius">Desired radius of the concentric circle</param>
    public void DrawConcentricCircle(float radius) {
        line.positionCount = 51;
        line.useWorldSpace = false;
        float x;
        float z;
        float angle = 20f;

        for (int i = 0; i < 51; i++) {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            line.SetPosition(i, new Vector3(x, 0, z));
            angle += (360f / 51);
        }
    }

    /// <summary>
    /// Draws a circle with passed-in radius and arbitrary position relative to center of
    /// the NPC.
    /// </summary>
    /// <param name="position">position relative to the center point of the NPC</param>
    /// <param name="radius">>Desired radius of the circle</param>
    public void DrawCircle(Vector3 position, float radius) {
        line.positionCount = 51;
        line.useWorldSpace = true;
        float x;
        float z;
        float angle = 20f;

        for (int i = 0; i < 51; i++) {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            line.SetPosition(i, new Vector3(x, 0, z)+position);
            angle += (360f / 51);
        }
    }

    public void DrawRay(Vector3 endPoint)
    {
        line.positionCount = 2;
        line.SetPosition(0, transform.position);
        line.SetPosition(1, endPoint);
    }

    public void DestroyPoints() {
        if (line) {
            line.positionCount = 0;
        }
    }
}
