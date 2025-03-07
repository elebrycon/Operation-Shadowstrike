using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class AIControl : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animationController;
    public enum States
    {
        GUARD,
        PATROL,
        CHASE,
        RUNAWAY,
        ALARMED
    }
    public States currentState;
    
    public int health = 100;
    public int maxHealth = 100;
    public float patrolSpeed = 2.0f;
    public float pursueSpeed = 4.0f;
    public int attackDamage = 10;
    public float attackRadius = 3f;
    private Vector3 lastPosition = Vector3.zero;
    public float viewRadius = 8f;
    public float soundRadius = 3f;
    public float gunRadius = 10f;
    [Range(0, 360)]
    public float viewAngle = 140f;
    public LayerMask obstacleMask;

    public Transform guardPosition;

    public Transform[] patrolPositions;
    private int patrolIndex = 0;

    public GameObject player;
    
    private PlayerMovement playerStatus;
    public float detectionTime = 3f;
    public float detectionTimeCurrent = 0f;
    public float attackCooldown = 2f;
    private float attackCooldownCurrent = 0f;
    public bool searchedLastPosition = true;
    public int searchIndex = 0;

    public int runAwayHealthTreshold = 25;
    public float runAwayDistance = 5;

    public Vector3 distractionPosition;
    public bool isDistracted = false;

    public AudioSource Hit;
    
    private bool isDead = false;
    private bool stopActivity = false;

    private bool patrolStop = false;
    bool animationStop = true;
    



    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animationController = GetComponentInChildren<Animator>();
        
        playerStatus = player.GetComponent<PlayerMovement>();
       
        isDistracted = false;

        if (patrolPositions.Length > 1)
            currentState = States.PATROL;
        else
            currentState = States.GUARD;
    }

    void Update()
    {
        if (stopActivity)
            return;

        StartCoroutine(AttackTimer());

        if (health < runAwayHealthTreshold)
        {
            currentState = States.RUNAWAY;
        }

        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance <= soundRadius && !playerStatus.sneak)
        {
            if (currentState != States.RUNAWAY)
                currentState = States.CHASE;
        }

        if(distance <= gunRadius && Input.GetMouseButtonDown(0))
        {
            if (currentState != States.RUNAWAY || currentState != States.CHASE)
            {
                currentState = States.ALARMED;
                lastPosition = player.transform.position;
            }
        }

        if (LookForPlayer() && currentState != States.RUNAWAY)
        {
            currentState = States.CHASE;
        }
 
        if (currentState == States.GUARD)
        {
            Guard();
        }
        else if (currentState == States.PATROL)
        {
            Patrol();
        }
        else if (currentState == States.CHASE)
        {
            Chase();
        }
        else if (currentState == States.RUNAWAY)
        {
            RunAway();
        }
        else if (currentState == States.ALARMED)
        {
            Alarmed();
        }

    }

    private void Alarmed()
    {
        
        agent.SetDestination(lastPosition);
        if(agent.remainingDistance <= agent.stoppingDistance)
        StartCoroutine(waitAtGun());
    }

    IEnumerator waitAtGun()
    {
        if (!animationStop)
            animationController.SetBool("isWalking", false);
        yield return new WaitForSeconds(5); 
        agent.SetDestination(patrolPositions[patrolIndex].position);
        patrolStop = false;
        animationController.SetBool("isWalking", true);
        animationStop = false;
        if(currentState != States.CHASE)
        currentState = States.PATROL;
    }

    private void Guard()
    {
        
        agent.speed = patrolSpeed;

        if (agent.transform.position != guardPosition.position)
            agent.destination = guardPosition.position;
        else
            agent.transform.rotation = guardPosition.transform.rotation;
    }

    private void Patrol()
    {
        if (!patrolStop)
        {
            agent.speed = patrolSpeed;
            animationController.SetBool("isRunning", false);
            animationController.SetBool("isWalking", true);
        }

        if (patrolPositions.Length < 2)
            return;

        agent.SetDestination(patrolPositions[patrolIndex].position);
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!patrolStop && animationController.GetBool("isWalking"))
            {
                patrolStop = true;
                StartCoroutine(waitAtPoint());
            }
            
        }
    }

    IEnumerator waitAtPoint()
    {
        if(!animationStop)
        animationController.SetBool("isWalking", false);
        yield return new WaitForSeconds(5);
        patrolIndex = (patrolIndex + 1) % patrolPositions.Length;
        agent.SetDestination(patrolPositions[patrolIndex].position);
        patrolStop = false;
        animationController.SetBool("isWalking", true);
        animationStop = false;
    }
    private void Chase()
    {
       

        Vector3 direction = (player.transform.position - transform.position).normalized;
        StartCoroutine(DetectionTimer());

        animationController.SetBool("isRunning", true);

        agent.speed = pursueSpeed;

        
        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance <= soundRadius && !playerStatus.sneak)
        {
            lastPosition = player.transform.position;
            agent.destination = player.transform.position - (direction * 2);
            LookAtTarget();
            detectionTimeCurrent = detectionTime;
        }

            if (LookForPlayer())
        {
            
            lastPosition = player.transform.position;
            agent.destination = player.transform.position - (direction * 2);
            LookAtTarget();      
            if (distance <= attackRadius)
            {
                Attack();
            }
            detectionTimeCurrent = detectionTime;
        }

        if (detectionTimeCurrent > 0)
        {
            agent.destination = player.transform.position - (direction * 2);
            LookAtTarget();
            searchedLastPosition = false;

            searchIndex = 0;
        }
        else if (detectionTimeCurrent < 0 && !searchedLastPosition)
        {
            Vector3 searchPosOne = RandomLocation();
            Vector3 searchPosTwo = RandomLocation();
   
            if (searchIndex == 0)
            {  
                agent.destination = lastPosition;

                if (Vector3.Distance(transform.position, lastPosition) < 1)
                    searchIndex++;
            }
            else if (searchIndex == 1)
            {
                agent.destination = searchPosOne;

                if (agent.remainingDistance <= agent.stoppingDistance)
                    searchIndex++;
            }
            else if (searchIndex == 2)
            {
                agent.destination = searchPosTwo;

                if (agent.remainingDistance <= agent.stoppingDistance)
                    searchedLastPosition = true;
            }
        }
        else if (detectionTimeCurrent < 0 && searchedLastPosition)
        {
            agent.speed = patrolSpeed;
            patrolStop = false;
            animationStop = true;
            if (patrolPositions.Length > 1)
                currentState = States.PATROL;
            else
                currentState = States.GUARD;
        }
    }

    private void RunAway()
    {
        animationController.SetBool("isRunning", true);
        agent.speed = pursueSpeed;

        float distance = Vector3.Distance(player.transform.position, transform.position);
        if (distance <= viewRadius)
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            agent.destination = transform.position - (direction * runAwayDistance);
        }
        if (agent.velocity.magnitude == 0f)
            LookAtTarget();
    }
    private void LookAtTarget()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private bool LookForPlayer()
    {
        float distance = Vector3.Distance(player.transform.position, transform.position);

        if (distance <= viewRadius)
        {
            Vector3 directionToTarget = (player.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
            {
                if (!Physics.Raycast(transform.position, directionToTarget, distance, obstacleMask))
                {
                  
                    return true;  
                }
            }
        }

        return false;
    }

    private void Attack()
    {
        if (attackCooldownCurrent <= 0 && player.activeSelf)
        {
            playerStatus.hitDamage(attackDamage);

            attackCooldownCurrent = attackCooldown;
            animationController.SetTrigger("Attack");   
            Hit.Play();

        }
    }

    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public Vector3 RandomLocation()
    {
        float radius = 4;
        Vector3 direction = Random.insideUnitSphere * radius;
        direction += transform.position;

        NavMeshHit hit;
        Vector3 final = Vector3.zero;
        if (NavMesh.SamplePosition(direction, out hit, radius, 1))
        {
            final = hit.position;
        }

        if (!Physics.CheckSphere(final, 1))
        {
            return final;
        }
        else
        {
            return transform.position;
        }
    }
    public void TakeDamage(int value)
    {
        health -= value;
        animationController.SetTrigger("Hit");
        detectionTimeCurrent = detectionTime;
        currentState = States.CHASE;

        if (health <= 0 && !isDead)
        {  
            isDead = true;
            animationController.SetTrigger("isDead");
            StartCoroutine(DeathDelay());
        }
        else if (health <= 0 && isDead)
        {
            animationController.SetTrigger("isDone");      
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Sound radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, soundRadius);

        // View radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        // Gun radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, gunRadius);

        // View angle
        Gizmos.color = Color.blue;
        Vector3 viewAngleA = DirectionFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirectionFromAngle(viewAngle / 2, false);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * 5);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * 5);
    }

    private IEnumerator DetectionTimer()
    {
        detectionTimeCurrent -= Time.deltaTime;

        yield return new WaitForSeconds(1);
    }

    private IEnumerator AttackTimer()
    {
        attackCooldownCurrent -= Time.deltaTime;

        yield return new WaitForSeconds(1);
    }
    private IEnumerator DeathDelay()
    {
        
        yield return new WaitForSeconds(0.5f);
        TakeDamage(0);

    }
}

