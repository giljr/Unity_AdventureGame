
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;


/*
 * In Event System we need to interact with our environment (Ground);  
 * Event System have three requirements, they are Something to...
 * 
 * * * Send Events (trigger) ----------------- >  Physics Raycast Component attached to the Camera
 * 
 * * * Receive Events (Collider)-------------- >  Colliders & Event Triggers work together; 
 *                                                when colliders is hit to the raycast, it 
 *                                                signals to Event System that an event has happened
 *                                   
 * * * Manage Events (Event System) ---------- >  The Event System; we do not have to worry with it 
 *                                                because Unity creates this objectGame for us:)
 *                               
 *  So in this class we're going to divide control of player movement with Animator (route motion) and
 *  customize the approach of the caracter, slowing it down when destination position is reached 
 *  and blend walking & idle animation smoothly - it is a master peace written by Unitys's guys from 
 *  https://unity3d.com/learn/tutorials/projects/adventure-game-tutorial
 *  
 *  MADEIN//RO by JayThree - giljr.2009@gmail.com
 *  version 1.0
 *  date: March 2018
 * */

public class PlayerMovement : MonoBehaviour {

    public Animator animator;
    public NavMeshAgent agent;
    public float inputHoldDelay = 0.5f;
    public float turnSpeedThreshold = 0.5f;
    public float speedDampTime = 0.1f;
    public float slowingSpeed = 0.175f;
    public float turnSmoothing = 15f;

    private WaitForSeconds inputHoldWait;
    private Vector3 destinationPosition;

    private const float stopDistanceProportion = 0.1f;
    //distance awat from the click the navmesnh can be
    private const float navMeshSampleDistance = 4f;

    private readonly int hashSpeedPara = Animator.StringToHash("Speed");
    


    private void Start()
    {
        agent.updateRotation = false;

        inputHoldWait = new WaitForSeconds(inputHoldDelay);

        //the first destination is where player initially are
        destinationPosition = transform.position;

    }

    private void OnAnimatorMove()
    {
        agent.velocity = animator.deltaPosition / Time.deltaTime;
    }

    private void Update()
    {
        if (agent.pathPending)            
        {
            return;
        }

        float speed = agent.desiredVelocity.magnitude;

        if (agent.remainingDistance <= agent.stoppingDistance * stopDistanceProportion)
        {
            Stopping(out speed);
        }

        else if(agent.remainingDistance <= agent.stoppingDistance)
        {
            Slowing(out speed, agent.remainingDistance);
        }

        else if (speed > turnSpeedThreshold)
        {
            Moving();
        }

        animator.SetFloat(hashSpeedPara, speed, speedDampTime, Time.deltaTime);
                
    }

    private void Stopping (out float speed)
    {
        
        //agent.Stop();
        agent.isStopped = true;
        speed = 0.0f;
    }

    private void Slowing (out float speed, float distanceToDestination)
    {
        //agent.Stop();
        agent.isStopped = true;
        transform.position = Vector3.MoveTowards(transform.position, destinationPosition, slowingSpeed * Time.deltaTime );
        float proportionalDistance = 1f - distanceToDestination / agent.stoppingDistance;
        //when the distance to destination is close to agent.stoppingDistance then set a speed close to slowing speed
        speed = Mathf.Lerp(slowingSpeed, 0.0f, proportionalDistance);
        speed = 0.0f;
    }
    
    private void Moving ()
    {
        //on normal circustance this will be dealt with Animator
        //all we want here is rotate, not moving actually...
        Quaternion targetRotation = Quaternion.LookRotation(agent.desiredVelocity);
        //we need to know how fast we wanto to turn, so create a varible named turnSmoothing
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSmoothing * Time.deltaTime);
    }

    public void OnGroundClick(BaseEventData data)
    {
        //specifically pointer event
        PointerEventData pdata = (PointerEventData)data;
        //find the point closest to our click
        //hit will be populated by our sample position function call
        NavMeshHit hit;
        //params: NavMesh.SamplePosition()
        //1 - point in the world we want to check (at the moment of the click) - pdata.pointerCurrentRaycast.worldPosition
        //2 - all the info about what we hit -  out hit
        //3 - given the distance we sample over - navMeshSampleDistance
        //4 - areas we gong to use - NavMesh.AllAreas
        if (NavMesh.SamplePosition(pdata.pointerCurrentRaycast.worldPosition, out hit, navMeshSampleDistance, NavMesh.AllAreas))
        {
            destinationPosition = hit.position;
            //if we haven't hit anything?
        }
        else
        {
            destinationPosition = pdata.pointerCurrentRaycast.worldPosition;
        }

        agent.SetDestination(destinationPosition);
        //agent.Resume();
        agent.isStopped = false;
        
    }
    // to call OnGroundClick(BaseEventData data) on ground click
    //do not forget to set the reference to the EventTrigger on Ground Object

}
