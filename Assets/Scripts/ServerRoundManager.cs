using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ServerRoundManager : NetworkBehaviour
{
    public float roundTime = 15f;
    public Image timerImage;

    [HideInInspector] public NetworkVariable<float> TimeRemaining =
        new NetworkVariable<float>(
            60f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            TimeRemaining.Value = roundTime;
        }

        TimeRemaining.OnValueChanged += OnTimeChanged;
    }

    public override void OnDestroy()
    {
        TimeRemaining.OnValueChanged -= OnTimeChanged;
    }

    private void Update()
    {
        if (!ServerValidator.Instance.IsValidateServer) return;

        TimeRemaining.Value -= Time.deltaTime;

        if (TimeRemaining.Value <= 0)
        {
            NextRound();
        }
    }

    private void NextRound()
    {
        Debug.Log("Round Ended!");
        TimeRemaining.Value = roundTime;
    }

    private void OnTimeChanged(float oldValue, float newValue)
    {
        if (timerImage != null)
        {
            timerImage.fillAmount = newValue / roundTime;
        }
    }
}