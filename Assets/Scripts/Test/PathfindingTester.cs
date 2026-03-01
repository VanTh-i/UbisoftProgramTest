using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingTester : MonoBehaviour
{
    public Transform startPoint;
    public Transform targetPoint;

    private void OnDrawGizmos()
    {
        if (startPoint != null && targetPoint != null && Pathfinding.Instance != null)
        {
            List<Vector3> path = Pathfinding.Instance.FindPath(startPoint.position, targetPoint.position);

            if (path != null && path.Count > 0)
            {
                Gizmos.color = Color.green;

                Gizmos.DrawLine(startPoint.position, path[0]);

                for (int i = 0; i < path.Count - 1; i++)
                {
                    Gizmos.DrawCube(path[i], Vector3.one * 0.4f);
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }

                Gizmos.DrawCube(path[path.Count - 1], Vector3.one * 0.4f);
            }
        }
    }
}
