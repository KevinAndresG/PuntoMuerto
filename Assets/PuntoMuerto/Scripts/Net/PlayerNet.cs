using Unity.Netcode;
using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Jugador en red: activa control/cámara solo para el dueño.</summary>
    public class PlayerNet : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            var pc = GetComponent<PlayerController>();
            var pi = GetComponent<PlayerInteraction>();
            if (pc != null) pc.enabled = IsOwner;
            if (pi != null) pi.enabled = IsOwner;
            var cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = IsOwner;

            // el prefab guarda materiales creados en memoria → llegan rotos (magenta).
            // Rehacer los materiales en runtime para todos.
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            Color overol = IsOwner ? new Color(0.25f, 0.35f, 0.55f) : new Color(0.3f, 0.5f, 0.8f);
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                var m = new Material(lit);
                if (r.name == "Cabeza") m.SetColor("_BaseColor", new Color(0.85f, 0.65f, 0.5f));
                else if (r.name == "Gorra") m.SetColor("_BaseColor", new Color(0.15f, 0.15f, 0.18f));
                else m.SetColor("_BaseColor", overol);
                r.material = m;
            }

            if (IsOwner)
            {
                transform.position = new Vector3(40f, 0.1f, -14f) + Vector3.right * (OwnerClientId * 1.5f);
                var cam = Object.FindFirstObjectByType<FirstPersonCamera>();
                if (cam != null) cam.SetTarget(transform);
            }
        }
    }
}
