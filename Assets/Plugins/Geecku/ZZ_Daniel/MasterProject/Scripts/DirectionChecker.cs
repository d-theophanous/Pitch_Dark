using Geecku.DefaultNetworking;
using Geecku.GlobalMangers;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Daniel.Master
{
    [System.Serializable]
    public class DestinationWaypoints
    {
        public Transform Destination;
        public List<Transform> Waypoints;
        public List<int> WayPointValueList;
    }

    public class DirectionChecker : Singleton<DirectionChecker>
    {
        [SerializeField] private List<DestinationWaypoints> DestinationWaypointsList;

        private Vector3 PlayerPos => GameManager.Instance.Player.transform.position;

        private const float WayPointRadius = 2.2f;
        private const float PathUpdateRate = 0.5f;
        private float AccumulatedTime = PathUpdateRate;

        // Current destination state
        private DestinationWaypoints CurrentDestinationWaypoints;
        private Vector3 CurrentWaypointPos;
        private int CurrentWaypointListIdx = 0;

        public bool CanReceiveDirectionInfo;

        #region MonoBehaviour Commons

        public void UpdateDirectionChecker(object sender, System.EventArgs e)
        {
            if (CurrentDestinationWaypoints == null) return;

            if (AccumulatedTime >= PathUpdateRate)
            {
                UpdateWaypointIndex();
                AccumulatedTime = 0f;
            }
            else
            {
                AccumulatedTime += Time.deltaTime;
            }
            if (IsOnRightPath())
            {
                    Rumbler.Instance.StartRumble();
            }
            else
            {
                    Rumbler.Instance.StopRumble();
            }
        }
        #endregion

        #region Public Functions
        public void StartDirectionChecking()
        {
            Debug.Log("start direction check");
            if (CurrentWaypointListIdx >= DestinationWaypointsList.Count)
            {
                Debug.LogWarning("DirectionChecker: destination index out of range.");
                return;
            }

            CurrentDestinationWaypoints = DestinationWaypointsList[CurrentWaypointListIdx];
            CurrentWaypointPos = CurrentDestinationWaypoints.Waypoints[0].position;
            CanReceiveDirectionInfo = true;
            GameManager.Instance.UpdateEvent += UpdateDirectionChecker;

            CurrentWaypointListIdx++;
        }

        public void StopDirectionChecking()
        {
            GameManager.Instance.UpdateEvent -= UpdateDirectionChecker;
            CanReceiveDirectionInfo = false;
        }
        #endregion

        #region Waypoint Logic

        /// <summary>
        /// Checks all waypoints and finds the furthest one the player has reached,
        /// accounting for backwards movement.
        /// </summary>
        
        private void UpdateWaypointIndex()
        {
            var waypoints = CurrentDestinationWaypoints.Waypoints;
            if (waypoints == null || waypoints.Count == 0) return;

            List<(Transform, int)> CurrentClosestWaypoints = new();

            for (int i = 0; i < waypoints.Count; i++)
            {
                float dist = Vector3.Distance(PlayerPos, waypoints[i].position);

                if (!HasClearLineOfSight(waypoints[i].position))
                    continue; // skip waypoints behind walls

                CurrentClosestWaypoints.Add((CurrentDestinationWaypoints.Waypoints[i], CurrentDestinationWaypoints.WayPointValueList[i]));
            }
            List<(Transform, int)> value_list = SortByValueDescending(CurrentClosestWaypoints);
            List<(Transform, int)> player_pos_list = SortByDistanceToPlayer(CurrentClosestWaypoints);

            CurrentWaypointPos = value_list[0].Item1.position;
        }
        private List<(Transform, int)> SortByDistanceToPlayer(List<(Transform, int)> waypoints)
        {
            return waypoints.OrderBy(w => Vector3.Distance(PlayerPos, w.Item1.position)).ToList();
        }
        private List<(Transform, int)> SortByValueDescending(List<(Transform, int)> waypoints)
        {
            return waypoints.OrderByDescending(w => w.Item2).ToList();
        }

        private bool IsOnRightPath()
        {
            if (!GameManager.Instance.Player.IsMoving) return false;

            var waypoints = CurrentDestinationWaypoints?.Waypoints;
            if (waypoints == null || waypoints.Count == 0) return false;

            var targetPos = CurrentWaypointPos;
            var targetFlat = new Vector3(targetPos.x, PlayerPos.y, targetPos.z);
            var vecToTarget = targetFlat - PlayerPos;

            if (vecToTarget.sqrMagnitude < 0.01f) return false;

            var angle = Vector3.Angle(vecToTarget, GameManager.Instance.Player.WalkDirection);
            return angle <= GetAllowedAngle(vecToTarget) / 2f;
        }

        private float GetAllowedAngle(Vector3 vecToTarget)
        {
            var targetPos = CurrentWaypointPos;
            var perpendicularVec = Vector3.Cross(vecToTarget, Vector3.up).normalized;
            var posA = targetPos + perpendicularVec * WayPointRadius;
            var posB = targetPos - perpendicularVec * WayPointRadius;
            return Vector3.Angle(posB - PlayerPos, posA - PlayerPos);
        }
        private bool HasClearLineOfSight(Vector3 waypointPos)
        {
            var direction = waypointPos - PlayerPos;
            float distance = direction.magnitude;

            RaycastHit[] hits = Physics.RaycastAll(PlayerPos, direction.normalized, distance);
            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Wall"))
                    return false;
            }
            return true;
        }
        #endregion

        private void OnDrawGizmosSelected()
        {
            if (CurrentDestinationWaypoints == null) return;
            var waypoints = CurrentDestinationWaypoints.Waypoints;
            if (waypoints == null) return;

            for (int i = 0; i < waypoints.Count; i++)
            {
                Gizmos.color = (Vector3.Distance(waypoints[i].position,CurrentWaypointPos) < 0.5f) ? Color.green : Color.red;
                Gizmos.DrawWireSphere(waypoints[i].position, WayPointRadius);

                if (i < waypoints.Count - 1)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }

            if (!Application.isPlaying) return;
            var targetPos = CurrentWaypointPos;
            var targetFlat = new Vector3(targetPos.x, PlayerPos.y, targetPos.z);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(PlayerPos, targetFlat);
        }
    }
}