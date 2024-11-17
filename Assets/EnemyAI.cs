using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform playerVariable;
    public float maxDistance = 1.0f;
    public float EnemyDetectionDistance = 5.0f;
    NavMeshAgent NavAgent;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        NavAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = (playerVariable.position - NavAgent.destination).magnitude;
        float detectionDistance = (playerVariable.position - transform.position).magnitude;
        if(distance > maxDistance && detectionDistance < EnemyDetectionDistance){
            NavAgent.destination = playerVariable.position;    
        }

        animator.SetFloat("Speed", NavAgent.velocity.magnitude);
    }
}
