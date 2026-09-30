using UnityEngine;

/// <summary>
/// Base class for the scene's manager components. Gives every manager one global access point
/// without repeating a static field in each of them.
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    /// <summary>
    /// Does not search or create objects, including during teardown.
    /// </summary>
    public static bool HasInstance => _instance != null;

    public static T Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindAnyObjectByType<T>();

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError($"Duplicate {typeof(T).Name} component.", this);
            enabled = false;
            Destroy(this); // Other managers may share this GameObject.
            return;
        }
        _instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
