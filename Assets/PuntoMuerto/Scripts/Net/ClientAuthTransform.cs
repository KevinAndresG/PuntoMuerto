using Unity.Netcode.Components;

namespace PuntoMuerto
{
    /// <summary>NetworkTransform con autoridad del dueño. Archivo propio: va dentro del prefab PlayerNet.</summary>
    public class ClientAuthTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative() => false;
    }
}
