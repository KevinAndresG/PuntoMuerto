namespace PuntoMuerto
{
    public enum MPModeKind
    {
        None,
        Host,        // LAN / IP directa
        Client,      // LAN / IP directa
        RelayHost,   // online gratis vía Unity Relay (código de unión)
        RelayClient
    }

    /// <summary>Selección de modo multijugador desde el menú (sobrevive el cambio de escena).
    /// Para Steam en el futuro: agregar SteamHost/SteamClient y un transport de Steamworks
    /// en NetworkBootstrap — el resto del juego no cambia.</summary>
    public static class MPMode
    {
        public static MPModeKind Mode = MPModeKind.None;
        public static string Ip = "127.0.0.1";
        public static ushort Port = 7777;
        public static string JoinCode = "";
    }
}
