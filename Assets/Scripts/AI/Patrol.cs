using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Patrol : MonoBehaviour
{
    public Transform[] wayPoints;
    private NavMeshAgent agent;
    public int currWaypoint = 0;
    private bool isWaiting = false;
    public float searchTime = 2f;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = true;
        agent.autoRepath = true;
        SetNextWaypoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting) {
            isWaiting = true;
            StartCoroutine(DoAnAction());
        }
    }

    void SetNextWaypoint() {
        if (wayPoints.Length <= 0) {
            Debug.Log(string.Format("No waypoints set for object {}", transform.gameObject.name));
        }

        agent.SetDestination(wayPoints[currWaypoint].position);
        currWaypoint = (currWaypoint + 1) % wayPoints.Length;
        isWaiting = false;
    }

    IEnumerator DoAnAction() {
        yield return new WaitForSeconds(searchTime);
        SetNextWaypoint();
    }
}
