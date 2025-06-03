using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class B1_AiBehaviour : MonoBehaviour
{
    public Transform player;
    public List<Transform> patrolPoints;
    
    public float viewDistance = 10f;
    public float viewAngle = 60f;
    public float searchDuration = 5f;
    public float searchRadius = 5f;

    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private Vector3 lastKnownPlayerPosition;
    private float searchTimer = 0f;
    private Vector3 currentSearchTarget;
    private bool hasReachedLastKnownPosition;

    private enum State { Patrolling, Chasing, Searching, Returning }
    private State currentState = State.Patrolling;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToNextPatrolPoint();
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                if (CanSeePlayer())
                {
                    lastKnownPlayerPosition = player.position;
                    currentState = State.Chasing;
                }
                break;

            case State.Chasing:
                agent.SetDestination(player.position);
                if (!CanSeePlayer())
                {
                    lastKnownPlayerPosition = player.position;
                    currentState = State.Searching;
                    searchTimer = 0f;
                }
                break;

            case State.Searching:
                searchTimer += Time.deltaTime;

                if (!hasReachedLastKnownPosition)
                {
                    agent.SetDestination(lastKnownPlayerPosition);
                    if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 1.5f)
                    {
                        hasReachedLastKnownPosition = true;
                        PickNewSearchPosition();
                    }
                }
                else
                {
                    if (!agent.pathPending && agent.remainingDistance < 0.5f)
                    {
                        PickNewSearchPosition();
                    }
                }

                if (searchTimer >= searchDuration)
                {
                    hasReachedLastKnownPosition = false;
                    currentState = State.Returning;
                    GoToNextPatrolPoint();
                }

                if (CanSeePlayer())
                {
                    currentState = State.Chasing;
                    hasReachedLastKnownPosition = false;
                }
                break;

            case State.Returning:
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    currentState = State.Patrolling;
                    GoToNextPatrolPoint();
                }
                break;
        }
    }

    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = player.position - transform.position;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (dirToPlayer.magnitude < viewDistance && angle < viewAngle * 0.5f)
        {
            if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer.normalized, out RaycastHit hit, viewDistance))
            {
                if (hit.collider.CompareTag("Player"))
                    return true;
            }
        }

        return false;
    }

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPatrolPoint();
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Count == 0) return;

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
    }

    void PickNewSearchPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * searchRadius;
        Vector3 randomPos = lastKnownPlayerPosition + new Vector3(randomCircle.x, 0, randomCircle.y);

        NavMeshHit hit;

        if(NavMesh.SamplePosition(randomPos, out hit, 2f, NavMesh.AllAreas))
        {
            currentSearchTarget = hit.position;
            agent.SetDestination(currentSearchTarget);
        }
    }
}
