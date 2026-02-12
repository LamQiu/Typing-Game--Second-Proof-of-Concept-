using Unity.Netcode;
using UnityEngine;

namespace Utilities
{
    public abstract class NetworkSingleton<T> : NetworkBehaviour
        where T : NetworkBehaviour
    {
        private static T s_instance;

        public static T Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = FindAnyObjectByType<T>();

                    if (s_instance == null)
                    {
                        Debug.LogError($"{typeof(T)} NetworkSingleton not found in scene.");
                    }
                }

                return s_instance;
            }
        }

        protected virtual void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Debug.LogWarning($"{typeof(T)} duplicate destroyed.");
                Destroy(gameObject);
                return;
            }

            s_instance = this as T;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (s_instance == this)
            {
                s_instance = null;
            }
        }
    }
}