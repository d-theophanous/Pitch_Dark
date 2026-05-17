using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Daniel.Master
{
    public class DirectionChecker : MonoBehaviour
    {
        private const float PathUpdateRate = 10f;
        private float AccumulatedTime = PathUpdateRate;

        [SerializeField] private NavMeshAgent PlayerNavMesh;
        [SerializeField] private Transform Destination;
        [SerializeField] private GameObject FloorParent;
        private Vector3 PlayerPos => GameManager.Instance.Player.transform.position;
        private NavMeshPath CurPath;
        public Vector3[] Waypoints;
        public List<Vector3> FinalWaypoints = new();
        private Transform[] FloorTransformList;

        private void Start()
        {
            CurPath = new NavMeshPath();
            GameManager.Instance.AddEventAndSubscribe(
                GameManager.Instance.MovementEvents, UpdateDirectionChecker );
        }
        public void UpdateDirectionChecker(object sender, System.EventArgs e)
        {
            if (AccumulatedTime >= PathUpdateRate)
            {
                CalculatePath();
                AccumulatedTime = 0f;
            }
            else
            {
                AccumulatedTime += Time.deltaTime;
            }
        } 

        private void CalculatePath()
        {
            FloorTransformList = FloorParent.GetComponentsInChildren<Transform>();
            // Calculate the NavMesh path from player to door
            bool path_found = NavMesh.CalculatePath(
                PlayerPos, Destination.position, NavMesh.AllAreas, CurPath);

            if (path_found)
            {
                Waypoints = CurPath.corners;
                FillWayPoints();
            }
        }
        private void FillWayPoints()
        {
            FinalWaypoints.Clear();

            foreach (Vector3 waypoint in Waypoints)
            {
                FinalWaypoints.Add(GetClosestFloor(waypoint).position);
            }
        }
        private Transform GetClosestFloor(Vector3 waypoint)
        {
            int correct_idx = 0;
            for (int j = 1; j < FloorTransformList.Length; j++)
            {
                if (Vector3.Distance(FloorTransformList[j].position, waypoint) 
                    < Vector3.Distance(FloorTransformList[correct_idx].position, waypoint))
                {
                    correct_idx = j;
                }
            }
            var return_transform = FloorTransformList[correct_idx];
            var tmp_list = FloorTransformList.ToList();
            tmp_list.RemoveAt(correct_idx);
            FloorTransformList = tmp_list.ToArray();
            return return_transform;
        }

        private void OnDrawGizmosSelected()
        {
            foreach (var item in FinalWaypoints)
            {
                Gizmos.DrawSphere(item, 1.5f);
            }
        }
    }    
}