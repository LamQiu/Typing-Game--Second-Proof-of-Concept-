using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utilities
{
    public class SceneReloader : NetworkBehaviour
    {
        [ContextMenu("Reload Scene")]
        public void ReloadCurrentScene()
        {
            if (!IsServer)
                return;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            NetworkManager.SceneManager.LoadScene(
                scene.name,
                LoadSceneMode.Single
            );
        }
    }
}