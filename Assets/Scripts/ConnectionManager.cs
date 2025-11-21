using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConnectionManager : MonoBehaviour
{
    public TMP_Text connectionHindText;
    private bool isConnecting = false;   // 防止重复连接

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (NetworkManager.Singleton != null &&
            !NetworkManager.Singleton.IsConnectedClient &&
            !isConnecting &&
            keyboard.enterKey.wasPressedThisFrame)
        {
            Connect();
        }
    }


    async void Connect()
    {
        var nm = NetworkManager.Singleton;

        // 防止重复调用
        if (isConnecting) return;
        isConnecting = true;

        // 如果 network 已经在运行，先停掉
        if (nm.IsListening)
        {
            nm.Shutdown();
            await System.Threading.Tasks.Task.Yield();
        }

        Debug.Log("Trying StartClient...");
        nm.StartClient();

        // 等待最多 2 秒检查是否连接成功
        float timeout = 2f;
        float timer = 0f;

        while (timer < timeout)
        {
            if (nm.IsConnectedClient)
            {
                Debug.Log("Connected to host!");
                connectionHindText.gameObject.SetActive(false);
                isConnecting = false;
                return;
            }

            timer += Time.deltaTime;
            await System.Threading.Tasks.Task.Yield();
        }

        // 超时 → 没有 host，自己当 host
        Debug.Log("No host detected, starting as Host...");

        nm.Shutdown();
        await System.Threading.Tasks.Task.Yield();

        nm.StartHost();
        connectionHindText.gameObject.SetActive(false);

        isConnecting = false;
    }
}