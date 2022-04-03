using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


// Allows an object with a navmeshagent to patrol to preset waypoints on most efficient path
public class Enemy : MonoBehaviour
{
    public Transform[] wayPoints;
    private Vector3[] searchWaypoints;
    private NavMeshAgent agent;
    public int currWaypoint = 0;
    private int searchWaypoint = 0;
    private bool isWaiting = false;
    private bool isChasing = false;
    public float searchTime = 2f;
    public float suspicion = 0f;
    private float minSuspicion = 0f;
    private float maxSuspicion = 8f;
    // which state the enemy is in
    public EnemyState state = EnemyState.patrol;
    public Transform lastDetectedArea;
    private GameObject player;
    private float t = 0f;
    // Start is called before the first frame update
    void Start()
    {
        wayPoints[0].parent.parent = null;
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = true;
        agent.autoRepath = true;
        if (wayPoints.Length <= 0) {
            Debug.Log(string.Format("No waypoints set for object {}", transform.gameObject.name));
        }
        SetNextWaypoint(wayPoints[0].position);
    }

    // Update is called once per frame
    void Update()
    {
        if (suspicion < minSuspicion)
            suspicion = minSuspicion;
        if (suspicion > maxSuspicion)
            suspicion = maxSuspicion;
        if (suspicion < 1f && state != EnemyState.patrol) {
            currWaypoint -= 1;
            state = EnemyState.patrol;
        }
        else if (suspicion >= 1f && state != EnemyState.search && lastDetectedArea && !isChasing) {
            state = EnemyState.search;
        }
        if (state == EnemyState.patrol && !agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting) {
            isWaiting = true;
            if (suspicion > minSuspicion) {
                suspicion -= Time.deltaTime / 5;
            }
            SearchForPlayer();
        }
        else if (state == EnemyState.search && !agent.pathPending && agent.remainingDistance < 0.5f) {
            if (searchWaypoint + 1 >= searchWaypoints.Length && suspicion >= 1f)
                suspicion -= Time.deltaTime / 2;
            if (!isWaiting)
                SearchForPlayer();
        }
        else if (state == EnemyState.chase) {
            chasePlayer();
        }
            
    }

    void SetNextWaypoint(Vector3 point) {
        agent.SetDestination(point);
        isWaiting = false;
    }

    void DoAnAction() {
        if (state == EnemyState.search && searchWaypoint + 1 < searchWaypoints.Length) {
            searchWaypoint = (searchWaypoint + 1) % searchWaypoints.Length;
            SetNextWaypoint(searchWaypoints[searchWaypoint]);
        }
        else if (state == EnemyState.patrol) {
            currWaypoint = (currWaypoint + 1) % wayPoints.Length;
            SetNextWaypoint(wayPoints[currWaypoint].position);
        }
        isWaiting = false;
    }
    public void attractAttention(Transform pos, float addedSuspicion) {
        minSuspicion = 0.5f;
        suspicion += addedSuspicion;
        if (suspicion >= 1f) {
            agent.isStopped = true;
            agent.ResetPath();
            lastDetectedArea = pos;
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(transform.position, lastDetectedArea.position, NavMesh.AllAreas, path);
            searchWaypoints = path.corners;
        }
    }
    public void SearchForPlayer() {
        isWaiting = true;
        StartCoroutine(LookAround());
    }

    public void chasePlayer() {
        isChasing = true;
        agent.isStopped = true;
        agent.ResetPath();
        RaycastHit hitinfo;
        Physics.Linecast(transform.position, player.transform.position, out hitinfo);
        if (hitinfo.collider.gameObject.tag == "Player") {
            lastDetectedArea = player.transform;
            SetNextWaypoint(player.transform.position);
        }
        else {
            state = EnemyState.search;
            isChasing = false;
            searchWaypoints = new Vector3[] {lastDetectedArea.position};
            searchWaypoint = 0;
            SetNextWaypoint(searchWaypoints[searchWaypoint]);
        }
    }

    // When something enters the look trigger
    void OnTriggerEnter (Collider col) {
        RaycastHit hitinfo;
        if (col.gameObject.tag == "Player") {
            Physics.Linecast(transform.position, col.transform.position, out hitinfo);
            if (hitinfo.collider.gameObject.tag == "Player" && hitinfo.collider.gameObject.GetComponent<PlayerState>().state == PlayerStates.suspicious) {
                state = EnemyState.chase;
                player = col.gameObject;
                if (suspicion < maxSuspicion)
                    suspicion += 5f;
                minSuspicion = 0.5f;
                chasePlayer();
            }
        }
    }

    IEnumerator LookAround() {
        t = 0;
        float RotationSpeed = 90f;
        while (t < 350) { // full left then right
            if (t <= 90)
                transform.Rotate (Vector3.up * (RotationSpeed * Time.deltaTime));
            else if (t >= 150 && t <= 320)
                transform.Rotate (-Vector3.up * (RotationSpeed * Time.deltaTime));
            t += RotationSpeed * Time.deltaTime;
            yield return null;
        }
        DoAnAction();
    }
}
