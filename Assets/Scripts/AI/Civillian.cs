using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Civillian : MonoBehaviour
{
    Destinations dests;
    public Vector3 dest;
    private NavMeshAgent agent;
    public float suspicion = 0f;
    public float suspicionThreshhold = 0f;
    private randNum r;
    public CivillianState state = CivillianState.normal;
    // Start is called before the first frame update
    void Start()
    {
        r = FindObjectsOfType<randNum>()[0];
        dests = FindObjectsOfType<Destinations>()[0];
        dests.onDestinationsGenerated += generateDest;
    }

    // Update is called once per frame
    void generateDest(object sender, EventArgs e)
    {
        dest = dests.destinationPoints[r.rand.Next(dests.destinationPoints.Count)];
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = true;
        agent.autoRepath = true;
        agent.SetDestination(dest);
    }

    void Update() { 
        if (suspicion > suspicionThreshhold)
            state = CivillianState.nosy;
        if (state == CivillianState.normal) {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
                Destroy(this.gameObject);
        }
        else {
            if (!agent.pathPending && agent.remainingDistance < 0.5f) {
                StartCoroutine(wait());
            }
        }
    }

    public void attractAttention(Transform pos, float addedSuspicion) {
        suspicion += addedSuspicion;
        if (suspicion >= 1f) {
            agent.isStopped = true;
            agent.ResetPath();
            agent.SetDestination(pos.position);
        }
    }

    IEnumerator wait() {
        yield return new WaitForSeconds(2);
        suspicion = 0f;
        agent.SetDestination(dest);
        state = CivillianState.normal;
    }
}
