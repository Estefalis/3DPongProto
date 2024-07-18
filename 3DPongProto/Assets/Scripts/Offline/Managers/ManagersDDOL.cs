using UnityEngine;

public class ManagersDDOL : MonoBehaviour
{
    public static ManagersDDOL Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
}