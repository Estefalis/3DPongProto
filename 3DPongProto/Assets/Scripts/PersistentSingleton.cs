using UnityEngine;

public class PersistentSingleton<T> : MonoBehaviour where T : Component
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning($"Another variant of Instance {typeof(T).Name} was found. Destroying the duplicate.");
            Destroy(this.gameObject);
            return;
        }

        Instance = this as T;

        DontDestroyOnLoad(this.gameObject);
    }
}