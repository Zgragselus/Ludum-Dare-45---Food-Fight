using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static bool s_shuttingDown = false;
    private static object s_lock = new object();
    private static T s_instance;

    public static T Instance
    {
        get
        {
            if (s_shuttingDown)
            {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed. Returning null.");
                return null;
            }

            lock (s_lock)
            {
                if (s_instance == null)
                {
                    s_instance = (T)FindObjectOfType(typeof(T));

                    if (s_instance == null)
                    {
                        var singletonObject = new GameObject();
                        s_instance = singletonObject.AddComponent<T>();
                        singletonObject.name = $"{typeof(T)} (Singleton)";

                        DontDestroyOnLoad(singletonObject);
                    }
                }

                return s_instance;
            }
        }
    }

    private void OnApplicationQuit()
    {
        s_shuttingDown = true;
    }

    private void OnDestroy()
    {
        s_shuttingDown = true;
    }
}
