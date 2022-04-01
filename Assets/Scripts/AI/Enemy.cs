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
    public float searchTime = 2f;
    public float suspicion = 0f;
    private float minSuspicion = 0f;
    // which state the enemy is in
    public EnemyState state = EnemyState.patrol;
    public Transform lastDetectedArea;
    // Start is called before the first frame update
    void Start()
    {
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
        if (suspicion < 1f && state != EnemyState.patrol) {
            currWaypoint -= 1;
            state = EnemyState.patrol;
        }
        else if (suspicion >= 1f && state != EnemyState.search && lastDetectedArea) {
            state = EnemyState.search;
        }
        if (state == EnemyState.patrol && !agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting) {
            if (suspicion >= minSuspicion)
                suspicion -= Time.deltaTime / 5;
            isWaiting = true;
            StartCoroutine(DoAnAction());
        }
        else if (state == EnemyState.search && !agent.pathPending && agent.remainingDistance < 0.5f) {
            if (searchWaypoint + 1 >= searchWaypoints.Length && suspicion >= 1f)
                suspicion -= Time.deltaTime / 2;
            else if (!isWaiting)
                SearchForPlayer();
        }
            
    }

    void SetNextWaypoint(Vector3 point) {
        agent.SetDestination(point);
        isWaiting = false;
    }

    IEnumerator DoAnAction() {
        yield return new WaitForSeconds(searchTime);
        if (state == EnemyState.search && searchWaypoint + 1 < searchWaypoints.Length) {
            searchWaypoint = (searchWaypoint + 1) % searchWaypoints.Length;
            SetNextWaypoint(searchWaypoints[searchWaypoint]);
        }
        else if (state == EnemyState.patrol) {
            currWaypoint = (currWaypoint + 1) % wayPoints.Length;
            SetNextWaypoint(wayPoints[currWaypoint].position);
        }
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
        Debug.Log("Search");
        isWaiting = true;
        StartCoroutine(DoAnAction());
    }
}
