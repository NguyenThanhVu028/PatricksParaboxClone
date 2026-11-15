using System.Collections.Generic;
using UnityEngine;

public class ResourcesUtils
{
    public static readonly Dictionary<ResourceType, string> ResourcesPathPrefixes = new Dictionary<ResourceType, string>()
    {
        {ResourceType.LevelDesigns, "LevelDesigns"},
        {ResourceType.Prefabs, "Prefabs"},
        {ResourceType.ScriptableObjects, "ScriptableObjects"}
    };

    public static Dictionary<(ResourceType, string), Object> _cache = new();

    public static T Load<T>(ResourceType resourceType, string resourceName) where T : Object
    {
        if (_cache.ContainsKey((resourceType, resourceName)))
        {
            return _cache[(resourceType, resourceName)] as T;
        }

        string path;
        if (!ResourcesPathPrefixes.ContainsKey(resourceType))
        {
            path = resourceName;
        }
        else
        {
            path = ResourcesPathPrefixes[resourceType] + "/" + resourceName;
        }

        T result = Resources.Load<T>(path);
        if (result != null)
        {
            if (!_cache.ContainsKey((resourceType, resourceName)))
            {
                _cache.Add((resourceType, resourceName), result);
            }
            else
            {
                _cache[(resourceType, resourceName)] = result;
            }
        }
        return Resources.Load<T>(path);
    }

    public static bool TryLoad<T>(ResourceType resourceType, string resourceName, out T resource) where T : Object
    {
        resource = Load<T>(resourceType, resourceName);
        return resource == null ? false : true;
    }

    public static T[] LoadAll<T>(ResourceType resourceType, string resourceName) where T : Object
    {
        string path;
        if (!ResourcesPathPrefixes.ContainsKey(resourceType))
        {
            path = resourceName;
        }
        else
        {
            path = ResourcesPathPrefixes[resourceType] + "/" + resourceName;
        }

        return Resources.LoadAll<T>(path);
    }

    public static bool TryLoadAll<T>(ResourceType resourceType, string resourceName, out T[] resources) where T : Object
    {
        resources = LoadAll<T>(resourceType, resourceName);
        return (resources == null || resources.Length == 0) ? false : true;
    }
}
