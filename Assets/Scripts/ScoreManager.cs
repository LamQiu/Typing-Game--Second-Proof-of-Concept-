using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    private TMP_Text _hostScore;
    private TMP_Text _clientScore;

    public NetworkList<PlayerScoreData> Scores = new NetworkList<PlayerScoreData>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Scores.Clear();
        }

        Scores.OnListChanged += OnScoresChanged;
        UpdateScoreUI();
    }
    private void OnScoresChanged(NetworkListEvent<PlayerScoreData> changeEvent)
    {
        UpdateScoreUI();
    }
    private void UpdateScoreUI()
    {
        int hostScoreValue = 0;
        int clientScoreValue = 0;

        foreach (var scoreData in Scores)
        {
            if (scoreData.clientId == 0)
                hostScoreValue = scoreData.score;
            else
                clientScoreValue = scoreData.score;
        }

        if (_hostScore != null)
            _hostScore.text = hostScoreValue.ToString();
        if (_clientScore != null)
            _clientScore.text = clientScoreValue.ToString();
    }


    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddScoreServerRpc(int amount, RpcParams rpcParams = default)
    {
        ulong playerId = rpcParams.Receive.SenderClientId;

        Debug.Log("This is from score manager");
        Debug.Log(playerId);

        bool isHost = playerId == 0;

        Debug.Log("Is host: " + isHost);

        for (int i = 0; i < Scores.Count; i++)
        {
            if (Scores[i].clientId == playerId)
            {
                var temp = Scores[i];
                temp.score += amount;
                Scores[i] = temp;
                return;
            }
        }

        // 如果不存在，创建新条目
        Scores.Add(new PlayerScoreData(playerId, amount));
    }


    public struct PlayerScoreData : INetworkSerializable, IEquatable<PlayerScoreData>
    {
        public ulong clientId;
        public int score;

        public PlayerScoreData(ulong id, int s)
        {
            clientId = id;
            score = s;
        }

        // --- NGO serialization ---
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref score);
        }

        // --- For NetworkList<T> requirement ---
        public bool Equals(PlayerScoreData other)
        {
            return clientId == other.clientId && score == other.score;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerScoreData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(clientId, score);
        }
    }
}