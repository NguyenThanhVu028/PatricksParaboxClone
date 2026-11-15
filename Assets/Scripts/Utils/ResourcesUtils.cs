using System.Collections.Generic;
using UnityEngine;

public class ResourcesUtils
{
    public static readonly Dictionary<ResourceTypes, string> ResourcesPathPrefixes = new Dictionary<ResourceTypes, string>()
    {
        {ResourceTypes.LevelDesigns, "LevelDesigns"},
        {ResourceTypes.Prefabs, "Prefabs"},
        {ResourceTypes.ScriptableObjects, "ScriptableObjects"},
        {ResourceTypes.Sprites, "Sprites"}
    };

    public static T Load<T>(ResourceTypes resourceType, string resourceName) where T : Object
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

        return Resources.Load<T>(path);
    }

    public static bool TryLoad<T>(ResourceTypes resourceType, string resourceName, out T resource) where T : Object
    {
        resource = Load<T>(resourceType, resourceName);
        return resource == null ? false : true;
    }

    public static T[] LoadAll<T>(ResourceTypes resourceType, string resourceName) where T : Object
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

    public static bool TryLoadAll<T>(ResourceTypes resourceType, string resourceName, out T[] resources) where T : Object
    {
        resources = LoadAll<T>(resourceType, resourceName);
        return (resources == null || resources.Length == 0) ? false : true;
    }
}
