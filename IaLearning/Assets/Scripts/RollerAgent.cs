using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
public class RollerAgent : Agent
{
    public Transform child;
    public MeshCollider mC;
    private Vector3 newPos;
    Rigidbody rBody;
    Vector3 targetStartPos;
    Vector3 playerStartPos;
    public float wallMinDistance = 1f;
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        targetStartPos = target.transform.position;
        playerStartPos = transform.position;
    }
    public Transform target;
    public override void OnEpisodeBegin()
    {
        // If the Agent fell, zero its momentum
        if (this.transform.localPosition.y < 0)
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }
        newPos = new Vector3(Random.Range(-mC.bounds.size.x, mC.bounds.size.x), 0f, Random.Range(-mC.bounds.size.z, mC.bounds.size.z));
        if(!mC.bounds.Contains(newPos))
        {
            newPos = new Vector3(Random.Range(-mC.bounds.size.x, mC.bounds.size.x), 0f, Random.Range(-mC.bounds.size.z, mC.bounds.size.z));
        }
        else
        {
            transform.position = newPos;    
        }
        // Move the target to a new spot
        target.localPosition = new Vector3(Random.value * 8 - 4,
                                           0.5f,
                                           Random.value * 8 - 4);
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(this.transform.localPosition);
        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }
    public float forceMultiplier = 1;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);
        RaycastHit hitForward;
        RaycastHit hitRight;
        RaycastHit hitMinusForward;
        RaycastHit hitMinusRight;
        // RAYCAST FORWARD +
        if(Physics.Raycast(child.transform.position, child.transform.forward, out hitForward, wallMinDistance))
        {
            if(hitForward.transform.tag == "Wall")
            {
                EndEpisode();
            }            
            Debug.DrawLine(child.transform.position, hitForward.transform.position, Color.black);
        }
        // RAYCAST RIGHT +
        if(Physics.Raycast(child.transform.position, child.transform.right, out hitRight, wallMinDistance))
        {
            if(hitRight.transform.tag == "Wall")
            {
                EndEpisode();
            }
            Debug.DrawLine(child.transform.position, hitRight.transform.position, Color.black);
        }
        // RAYCAST FORWARD -
        if(Physics.Raycast(child.transform.position, -child.transform.forward, out hitMinusForward, wallMinDistance))
        {
            if(hitMinusForward.transform.tag == "Wall")
            {
                EndEpisode();
            }
            Debug.DrawLine(child.transform.position, hitMinusForward.transform.position, Color.black);
        }
        // RAYCAST RIGHT -
        if(Physics.Raycast(child.transform.position, -child.transform.right, out hitMinusRight, wallMinDistance))
        {
            if(hitMinusRight.transform.tag == "Wall")
            {
                EndEpisode();
            }
            Debug.DrawLine(child.transform.position, hitMinusRight.transform.position, Color.black);
        }
        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, target.localPosition);
        // Reached target
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
        }
        // Fell off platform
        else if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}