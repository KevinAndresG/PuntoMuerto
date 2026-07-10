using System.Collections.Generic;
using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Carro por waypoints: frena tras carros lentos, adelanta si va más rápido y atropella al jugador.</summary>
    public class VehicleAI : MonoBehaviour
    {
        public float Speed = 8f;
        public float TurnSpeed = 4f;
        public bool Loop = true;
        public bool DespawnAtEnd;
        public int MaxLaps;   // >0: despawnea tras N vueltas (variedad de tráfico)
        int laps;

        static readonly List<VehicleAI> All = new List<VehicleAI>();
        static PlayerController player;
        static float playerSearchAt;

        Transform[] points;
        int index;
        float currentSpeed;
        float blockedTime;
        float lateral;      // desvío de carril mientras adelanta
        float hitCooldown;
        public bool Finished { get; private set; }

        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        public void SetPath(Transform[] path, bool loop, bool despawnAtEnd = false, int startIndex = 0)
        {
            points = path;
            Loop = loop;
            DespawnAtEnd = despawnAtEnd;
            Finished = false;
            currentSpeed = Speed;
            if (points != null && points.Length > 0)
            {
                startIndex = Mathf.Clamp(startIndex, 0, points.Length - 1);
                // el siguiente objetivo es el waypoint DESPUÉS del de arranque
                // (si no, un carro spawneado a mitad del loop cruza el pueblo en línea recta hacia el punto 0)
                index = points.Length > 1 ? (startIndex + 1) % points.Length : 0;
                transform.position = points[startIndex].position;
                if (points.Length > 1)
                    transform.rotation = Quaternion.LookRotation(points[index].position - points[startIndex].position);
            }
        }

        void Update()
        {
            if (Finished || points == null || points.Length == 0) return;
            if (GameManager.I != null && GameManager.I.GamePaused) return;
            if (points[index] == null) { Finished = true; return; }

            Vector3 target = points[index].position + transform.right * lateral;
            Vector3 to = target - transform.position;
            to.y = 0f;

            if (to.magnitude < 1.2f)
            {
                index++;
                if (index >= points.Length)
                {
                    if (Loop)
                    {
                        index = 0;
                        laps++;
                        Speed = Random.Range(5.5f, 9.5f); // ritmo nuevo cada vuelta
                        if (MaxLaps > 0 && laps >= MaxLaps) { Destroy(gameObject); return; }
                    }
                    else
                    {
                        Finished = true;
                        if (DespawnAtEnd) Destroy(gameObject);
                        return;
                    }
                }
                return;
            }

            // respetar al carro de adelante; adelantar si el mío es más rápido
            var ahead = CarAhead(out float gap);
            float targetSpeed = Speed;
            if (ahead != null)
            {
                targetSpeed = gap < 3.5f ? 0f : Mathf.Min(Speed, ahead.currentSpeed);
                if (targetSpeed < Speed - 0.5f) blockedTime += Time.deltaTime;
                // ponytail: adelanta por la izquierda sin chequear contravía; suficiente para tráfico placeholder
                if (blockedTime > 2f && Mathf.Abs(lateral) < 0.1f)
                {
                    lateral = -2.6f;
                    blockedTime = 0f;
                }
                // en plena maniobra no frena detrás del otro: acelera y pasa
                if (Mathf.Abs(lateral) > 1f && gap > 2f)
                    targetSpeed = Speed * 1.2f;
            }
            else
            {
                blockedTime = 0f;
                lateral = Mathf.MoveTowards(lateral, 0f, 2f * Time.deltaTime);
            }
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, 14f * Time.deltaTime);

            Quaternion look = Quaternion.LookRotation(to.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, TurnSpeed * Time.deltaTime);
            transform.position += transform.forward * currentSpeed * Time.deltaTime;

            hitCooldown -= Time.deltaTime;
            if (hitCooldown <= 0f && currentSpeed > 2.5f) TryHitPlayer();
        }

        VehicleAI CarAhead(out float gap)
        {
            gap = float.MaxValue;
            VehicleAI best = null;
            foreach (var o in All)
            {
                if (o == this || o == null || o.Finished) continue;
                Vector3 rel = o.transform.position - transform.position;
                float along = Vector3.Dot(rel, transform.forward);
                float side = Mathf.Abs(Vector3.Dot(rel, transform.right));
                if (along < 0.5f || along > 8f || side > 2.2f) continue;
                if (Vector3.Dot(transform.forward, o.transform.forward) < 0.4f) continue; // sentido contrario
                if (along < gap) { gap = along; best = o; }
            }
            return best;
        }

        void TryHitPlayer()
        {
            if (player == null && Time.time > playerSearchAt)
            {
                playerSearchAt = Time.time + 2f;
                player = Object.FindFirstObjectByType<PlayerController>();
            }
            if (player == null || player.KnockedDown) return;

            Vector3 rel = player.transform.position - transform.position;
            rel.y = 0f;
            float along = Vector3.Dot(rel, transform.forward);
            float side = Vector3.Dot(rel, transform.right);
            if (along > 0f && along < 2.6f && Mathf.Abs(side) < 1.2f)
            {
                hitCooldown = 2f;
                player.Knockdown(transform.forward + transform.right * (side > 0f ? 0.6f : -0.6f));
            }
        }
    }
}
