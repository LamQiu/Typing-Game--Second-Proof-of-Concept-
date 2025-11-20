using Unity.Netcode;
using UnityEngine;

public class ServerValidator : Singleton<ServerValidator>
{
    public int minPlayerCount = 2;
    public bool IsValidateServer
    {
        get
        {
            if(NetworkManager.Singleton == null)
                return false;
            return NetworkManager.Singleton.IsServer && NetworkManager.Singleton.ConnectedClientsList.Count >= minPlayerCount;
        }
    }
}
