using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public class PromptGenerator : NetworkBehaviour
{
    public bool randomize;
    [SerializeField] private Prompt[] prompts;
    [SerializeField] private List<Prompt> usedPrompts = new List<Prompt>();
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
        var unusedPrompts = prompts.Where(p => !usedPrompts.Contains(p)).ToList();

        if (unusedPrompts.Count == 0)
        {
            usedPrompts.Clear();
            unusedPrompts = prompts.ToList();
        }

        var randomPrompt = unusedPrompts[Random.Range(0, unusedPrompts.Count)];
        if (randomize)
        {
            randomPrompt = new Prompt(RandomTypeExceptNone(), RandomContentExceptNone());
        }

        CurrentPrompt.Value = randomPrompt;
        usedPrompts.Add(randomPrompt);
    }

    PromptType RandomTypeExceptNone()
    {
        var all = System.Enum.GetValues(typeof(PromptType)).Cast<PromptType>().ToList();
        all.Remove(PromptType.None);
        return all[Random.Range(0, all.Count)];
    }

    PromptContent RandomContentExceptNone()
    {
        var all = System.Enum.GetValues(typeof(PromptContent)).Cast<PromptContent>().ToList();
        all.Remove(PromptContent.None);
        return all[Random.Range(0, all.Count)];
    }

    private void OnPromptChanged(Prompt oldVal, Prompt newVal)
    {
        Debug.Log($"Received new prompt: {newVal.type} {newVal.content}");
    }

    public enum PromptType
    {
        None,
        StartWith,
        Contains,
        EndWith
    }

    public enum PromptContent
    {
        None,
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        T,
        U,
        V,
        W,
        X,
        Y,
        Z,
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
            return Regex.Replace(type.ToString(), "([a-z])([A-Z])", "$1 $2") + " " + " \"" + content + "\"";
        }
    }
}