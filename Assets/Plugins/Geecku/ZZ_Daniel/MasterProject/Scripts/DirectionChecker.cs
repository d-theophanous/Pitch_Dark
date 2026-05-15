using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

namespace Daniel.Master
{
    public class DirectionChecker : MonoBehaviour
    {
        private const float PathUpdateRate = 0.5f;

        [SerializeField] private NavMeshAgent PlayerNavMesh;
        [SerializeField] private Transform Destination;
        private Vector3 PlayerPos => GameManager.Instance.Player.transform.position;
        private NavMeshPath CurPath;
        private List<Vector3> Waypoints = new();

        private void CalculatePath()
        {
            // Calculate the NavMesh path from player to door
            bool path_found = NavMesh.CalculatePath(
                PlayerPos, Destination.position, NavMesh.AllAreas, CurPath);

            if (path_found)
            {
                FillWayPoints();
            }
        }
        private void FillWayPoints()
        {
            //- idea clip the points to closest floor!!!!!!
        }
    }
}