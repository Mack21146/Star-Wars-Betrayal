using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class B1_AiBehaviour : MonoBehaviour
{
    public Transform player;
    public List<Transform> patrolPoints;

    public float moveSpeed = 2f;
    public float viewDistance = 100f;
    public float viewAngle = 60f;
    public float searchDuration = 4f;
    public float searchRadius = 3f;

    private Rigidbody2D rb;
    private int currentPatrolIndex = 0;
    private Vector2 lastKnownPlayerPosition;
    private float searchTimer = 0f;
    private bool hasReachedLastKnownPosition = false;

    private enum State { Patrolling, Chasing, Searching, Returning }
    private State currentState = State.Patrolling;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (patrolPoints.Count > 0)
        {
            GoToNextPatrolPoint();
        }
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
                Chase();
                break;

            case State.Searching:
                Search();
                break;

            case State.Returning:
                ReturnToPatrol();
                break;
        }
    }

    void Patrol()
    {
        MoveTowards(patrolPoints[currentPatrolIndex].position);

        if (Vector2.Distance(transform.position, patrolPoints[currentPatrolIndex].position) < 0.2f)
        {
            GoToNextPatrolPoint();
        }
    }

    void Chase()
    {
        MoveTowards(player.position);

        if (!CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;
            hasReachedLastKnownPosition = false;
            searchTimer = 0f;
            currentState = State.Searching;
        }
    }

    void Search()
    {
        searchTimer += Time.deltaTime;

        if (!hasReachedLastKnownPosition)
        {
            MoveTowards(lastKnownPlayerPosition);

            if (Vector2.Distance(transform.position, lastKnownPlayerPosition) < 0.2f)
            {
                hasReachedLastKnownPosition = true;
            }
        }
        else
        {
            Vector2 randomPos = lastKnownPlayerPosition + Random.insideUnitCircle * searchRadius;
            MoveTowards(randomPos);
        }

        if (searchTimer >= searchDuration)
        {
            hasReachedLastKnownPosition = false;
            currentState = State.Returning;
        }

        if (CanSeePlayer())
        {
            currentState = State.Chasing;
            hasReachedLastKnownPosition = false;
        }
    }

    void ReturnToPatrol()
    {
        MoveTowards(patrolPoints[currentPatrolIndex].position);

        if (Vector2.Distance(transform.position, patrolPoints[currentPatrolIndex].position) < 0.2f)
        {
            currentState = State.Patrolling;
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Count == 0) return;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
    }

    void MoveTowards(Vector2 target)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.velocity = dir * moveSpeed;
    }

    bool CanSeePlayer()
    {
        Vector2 dirToPlayer = (player.position - transform.position);
        float distanceToPlayer = dirToPlayer.magnitude;

        if (distanceToPlayer < viewDistance)
        {
            // Use the direction the enemy is facing — adjust depending on your sprite orientation
            Vector2 facingDir = transform.up; // or transform.right if your enemy faces right in the Sprite Editor

            float angle = Vector2.Angle(facingDir, dirToPlayer);
            if (angle < viewAngle * 0.5f)
            {
                // Raycast toward the player to check for obstacles
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer.normalized, viewDistance);

                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        Debug.DrawLine(transform.position, hit.point, Color.green);
                        return true;
                    }
                    else
                    {
                        Debug.DrawLine(transform.position, hit.point, Color.red);
                    }
                }
            }
        }

        return false;
    }
}
