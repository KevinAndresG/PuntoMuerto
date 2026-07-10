using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Ruta de waypoints tomada de los hijos del objeto.</summary>
    public class WaypointPath : MonoBehaviour
    {
        public bool Loop = true;
        Transform[] points;

        public Transform[] Points
        {
            get
            {
                if (points == null || points.Length != transform.childCount)
                {
                    points = new Transform[transform.childCount];
                    for (int i = 0; i < transform.childCount; i++)
                        points[i] = transform.GetChild(i);
                }
                return points;
            }
        }

        public static WaypointPath Find(string name)
        {
            var go = GameObject.Find(name);
            return go != null ? go.GetComponent<WaypointPath>() : null;
        }
    }
}
