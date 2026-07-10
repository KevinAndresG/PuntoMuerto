using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace PuntoMuerto
{
    /// <summary>Arranca el multijugador según MPMode:
    /// - Host/Client: conexión directa por IP (LAN o puerto abierto).
    /// - RelayHost/RelayClient: Unity Relay vía Sessions (gratis, código de 6 letras, sin abrir puertos).
    /// Para Steam más adelante: reemplazar el transport y el flujo de join, nada más.</summary>
    public class NetworkBootstrap : MonoBehaviour
    {
        public GameObject PlayerPrefab;
        GameObject syncPrefab;

        void Start()
        {
            if (MPMode.Mode == MPModeKind.None) return;

            if (PlayerPrefab == null) PlayerPrefab = Resources.Load<GameObject>("PlayerNet");
            syncPrefab = Resources.Load<GameObject>("GameSyncNet");
            if (PlayerPrefab == null)
            {
                Debug.LogWarning("PlayerNet prefab no encontrado; multijugador deshabilitado.");
                return;
            }

            var nmGo = new GameObject("NetworkManager");
            var nm = nmGo.AddComponent<NetworkManager>();
            var utp = nmGo.AddComponent<UnityTransport>();
            nm.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = utp,
                PlayerPrefab = PlayerPrefab
            };
            if (syncPrefab != null) nm.AddNetworkPrefab(syncPrefab);

            // el jugador offline de la escena sobra en multijugador
            var offline = GameObject.Find("Player");
            if (offline != null) offline.SetActive(false);

            switch (MPMode.Mode)
            {
                case MPModeKind.Host:
                    utp.ConnectionData.Address = "0.0.0.0";
                    utp.ConnectionData.Port = MPMode.Port;
                    if (nm.StartHost())
                    {
                        SpawnGameSync();
                        GameEvents.Notify("Partida LAN creada. Comparte tu IP (puerto " + MPMode.Port + ").");
                    }
                    break;

                case MPModeKind.Client:
                    utp.ConnectionData.Address = MPMode.Ip;
                    utp.ConnectionData.Port = MPMode.Port;
                    nm.StartClient();
                    GameEvents.Notify("Conectando a " + MPMode.Ip + "...");
                    break;

                case MPModeKind.RelayHost:
                case MPModeKind.RelayClient:
                    StartCoroutine(RelayFlow(nm));
                    break;
            }
        }

        void SpawnGameSync()
        {
            if (syncPrefab == null) return;
            var sync = Instantiate(syncPrefab);
            sync.GetComponent<NetworkObject>().Spawn();
        }

        IEnumerator RelayFlow(NetworkManager nm)
        {
            var task = RelayAsync(nm);
            while (!task.IsCompleted) yield return null;
            if (task.IsFaulted)
            {
                Debug.LogException(task.Exception);
                GameEvents.Notify("Error de servicios online. ¿Está el proyecto vinculado a Unity Gaming Services? (Project Settings → Services)");
            }
        }

        async System.Threading.Tasks.Task RelayAsync(NetworkManager nm)
        {
            await Unity.Services.Core.UnityServices.InitializeAsync();
            if (!Unity.Services.Authentication.AuthenticationService.Instance.IsSignedIn)
                await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();

            if (MPMode.Mode == MPModeKind.RelayHost)
            {
                var options = new SessionOptions { MaxPlayers = 4 }.WithRelayNetwork();
                var session = await MultiplayerService.Instance.CreateSessionAsync(options);
                string code = session.Code;
                GUIUtility.systemCopyBuffer = code;
                GameEvents.Notify("PARTIDA ONLINE CREADA — Código: " + code + " (copiado al portapapeles). Compártelo con tu amigo.");
                // Sessions arranca el host por su cuenta; esperar y spawnear el sync compartido
                float t = 0f;
                while (!nm.IsListening && t < 15f) { await System.Threading.Tasks.Task.Delay(200); t += 0.2f; }
                if (nm.IsListening) SpawnGameSync();
            }
            else
            {
                string code = MPMode.JoinCode.Trim().ToUpperInvariant();
                GameEvents.Notify("Uniéndose con código " + code + "...");
                await MultiplayerService.Instance.JoinSessionByCodeAsync(code);
                GameEvents.Notify("¡Conectado a la partida!");
            }
        }
    }
}
