using UnityEngine;

public class GenericSingleton<T> : MonoBehaviour where T : Component
{
    private static T m_instance;

    public static T Instance
    {
        get
        {
            if (m_instance == null)
                m_instance = FindObjectOfType<T>();

            if (m_instance == null)
            {
                GameObject gameObject = new GameObject();
                gameObject.name = typeof(T).Name;
                m_instance = gameObject.AddComponent<T>();
            }

            return m_instance;
        }
    }

    public virtual void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this as T;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(gameObject);
    }
}