using Unity.Netcode;
using UnityEngine;

public class ServerNetworkObjects : NetworkBehaviour
{
    public Client host;
    public Client client;
    
    public void AddToServerNetworkObjects(Client c, ulong clientID)
    {
        if(!IsServer) return;
        
        Debug.Log("ClientID: " + clientID);
        if (clientID == 0)
        {
            host = c;
        }
        else
        {
            client = c;
        }
    }
}
