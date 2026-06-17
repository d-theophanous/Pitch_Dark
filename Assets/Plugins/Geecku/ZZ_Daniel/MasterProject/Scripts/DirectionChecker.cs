using Geecku.DefaultNetworking;
using Geecku.GlobalMangers;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Daniel.Master
{
    public class DirectionChecker : Singleton<DirectionChecker>
    {
        private const float PathUpdateRate = 0.5f;
        private float AccumulatedTime = PathUpdateRate;

        [SerializeField] private NavMeshAgent PlayerNavMesh;
        [SerializeField] private Transform Destination;
        [SerializeField] private GameObject FloorParent;
        private Vector3 PlayerPos => GameManager.Instance.Player.transform.position;
        private NavMeshPath CurPath;
        private Vector3[] Waypoints;
        private List<Vector3> FinalWaypoints = new();
        private Transform[] FloorTransformList;
        private Vector3 CurWaypoint;
        private const float WayPointRadius = 2.2f;

        public List<Transform> DestinationList;
        private int DestinationIdx;

        public bool CanReceiveDirectionInfo;

        #region MonoBehaviour Commons
        protected override void Start()
        {
            base.Start();
            CurPath = new NavMeshPath();
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

            if (IsOnRightPath())
            {
                Rumbler.Instance.StartRumble();
                //if (NetworkManager.Client.IsInConnection)
                //    GameManager.Instance.ClientSend_DirectionCheck(1);
                //else
                //    Rumbler.Instance.StartRumble();
            }
            else
            {
                Rumbler.Instance.StopRumble();
                //if (NetworkManager.Client.IsInConnection)
                //    GameManager.Instance.ClientSend_DirectionCheck(0);
                //else
                //    Rumbler.Instance.StopRumble();
            }
        }
        #endregion

        #region Public Functions
        public void SetDestination(Transform new_destination)
        {
            Destination = new_destination;
        }
        public void StartDirectionChecking()
        {
            CanReceiveDirectionInfo = true;
            GameManager.Instance.UpdateEvent += UpdateDirectionChecker;
            SetDestination(DestinationList[DestinationIdx]);
            DestinationIdx++;
        }
        public void StopDirectionChecking()
        {
            GameManager.Instance.UpdateEvent -= UpdateDirectionChecker;
        }
        #endregion

        private bool IsOnRightPath()
        {
            if (!GameManager.Instance.Player.IsMoving)
                return false;
            if (FinalWaypoints.Count > 0 &&
                Vector3.Distance(PlayerPos, CurWaypoint) < WayPointRadius)
            {
                FinalWaypoints.RemoveAt(0);
                if (FinalWaypoints.Count > 0)
                    CurWaypoint = FinalWaypoints[0];
                else
                    return false;
            }
            //- get angle between walk direction and vector to target
            var cur_waypoint = new Vector3(CurWaypoint.x, PlayerPos.y, CurWaypoint.z);
            var vec_to_target = cur_waypoint - PlayerPos;
            var angle = Vector3.Angle(vec_to_target, GameManager.Instance.Player.WalkDirection);

            //- check if angle is in threshold
            if (angle <= GetAllowedAngle(vec_to_target) / 2)
            {
                return true;
            }
            return false;
        }

        Vector3 TestA;
        Vector3 TestB;
        private float GetAllowedAngle(Vector3 vec_to_target)
        {
            var perpendicular_vec = Vector3.Cross(vec_to_target, Vector3.up).normalized;
            var pos_a = CurWaypoint + perpendicular_vec * WayPointRadius;
            var pos_b = CurWaypoint - perpendicular_vec * WayPointRadius;
            TestA = pos_a;
            TestB = pos_b;
            var vec_a = pos_a - PlayerPos;
            var vec_b = pos_b - PlayerPos;
            return Vector3.Angle(vec_b, vec_a);
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
                //- if the player already is very close to the new current waypoint
                //- skip this waypoint
                int index = 0;
                while (Vector3.Distance(FinalWaypoints[index], PlayerPos) <= (1f)
                    && index < FinalWaypoints.Count - 1)
                    index++;
                FinalWaypoints.RemoveRange(0, index);
                CurWaypoint = FinalWaypoints[0];
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
                Gizmos.DrawSphere(item, 1f);
            }
            Gizmos.color = Color.black;
            foreach (var item in Waypoints)
            {
                Gizmos.DrawSphere(item, 1f);
            }

            var cur_waypoint = new Vector3(CurWaypoint.x, PlayerPos.y, CurWaypoint.z);
            var vec_to_target = cur_waypoint - PlayerPos;
            var angle = Vector3.Angle(vec_to_target, GameManager.Instance.Player.WalkDirection);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(PlayerPos, PlayerPos + vec_to_target);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(PlayerPos, PlayerPos + GameManager.Instance.Player.WalkDirection.normalized * 4); 
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(PlayerPos, TestA);
            Gizmos.DrawLine(PlayerPos, TestB);
            Gizmos.color = Color.purple;
            Gizmos.DrawSphere(CurWaypoint, 1f);
        }
    }    
}