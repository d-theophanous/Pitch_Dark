using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Patrolling : MonoBehaviour
{
    [SerializeField]
    private GameObject[] points;
    [SerializeField]
    private NavMeshAgent agent;

    private int currPoint;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;
        currPoint = 0;
        agent.destination = points[currPoint].transform.position;
    }
    private void Update()
    {
        if(Vector3.Distance(this.transform.position,points[currPoint].transform.position)<=2f)
        {
            Iterate();
        }
    }

    void Iterate()
    {
        if(currPoint<points.Length-1)
        {
            currPoint++;
        }
        else
        {
            currPoint = 0;
        }
        agent.destination = points[currPoint].transform.position;
    }
}


