using UnityEngine;

/// <summary>
/// Base class for the scene's manager components. Gives every manager one global access point
/// without repeating a static field in each of them.
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    /// <summary>
    /// True when an instance already exists. Use this instead of a null-check on <see cref="Instance"/>
    /// while tearing down (OnDestroy / OnDisable): reading Instance would resurrect the singleton by
    /// creating a fresh GameObject, which then leaks into the next play session.
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

            if (_instance == null)
            {
                var singletonObject = new GameObject(typeof(T).Name);
                _instance = singletonObject.AddComponent<T>();
            }

            return _instance;
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
