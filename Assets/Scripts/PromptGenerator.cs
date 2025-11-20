using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public class PromptGenerator : NetworkBehaviour
{
    [SerializeField] private Prompt prompt;
    public NetworkVariable<Prompt> CurrentPrompt = new NetworkVariable<Prompt>();
    public override void OnNetworkSpawn()
    {
        // 每个客户端收到广播时触发
        CurrentPrompt.OnValueChanged += OnPromptChanged;
    }
    [ContextMenu("Update Prompt")]
    public void TryUpdatePrompt()
    {
        UpdatePromptServerRpc();
    }
    [Rpc(SendTo.Server)]
    private void UpdatePromptServerRpc()
    {
        CurrentPrompt.Value = prompt;
    }
    private void OnPromptChanged(Prompt oldVal, Prompt newVal)
    {
        Debug.Log($"Received new prompt: {newVal.type} {newVal.content}");
    }
    public enum PromptType
    {
        StartWith,
        Contains,
        EndWith
    }

    public enum PromptContent
    {
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
        ER
    }
    [System.Serializable]
    public struct Prompt : INetworkSerializable
    {
        public PromptGenerator.PromptType type;
        public PromptGenerator.PromptContent content;

        public Prompt(PromptGenerator.PromptType t, PromptGenerator.PromptContent c)
        {
            type = t;
            content = c;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref type);
            serializer.SerializeValue(ref content);
        }

        public override string ToString()
        {
            return  Regex.Replace(type.ToString(), "([a-z])([A-Z])", "$1 $2") + " " + " \"" + content + "\"";
        }
    }
}
