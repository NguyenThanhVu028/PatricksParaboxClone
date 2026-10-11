using System.Collections.Generic;

public class MyObject
{
    protected List<MyComponent> _components = new();
    protected RelativeTransform _transform;

    public RelativeTransform Transform
    {
        get => _transform;
    }

    public MyObject()
    {
        // Every MyObject must have the transform component
        _transform = new RelativeTransform();
        RegisterComponent(_transform);
    }

    public virtual bool RegisterComponent(MyComponent component)
    {
        if (component == null)
        {
            return false;
        }

        if (component.RegisterObject(this))
        {
            _components.Add(component);
            return true;
        }

        return false;
    }

    public T GetComponent<T>() where T : MyComponent
    {
        foreach (var component in _components)
        {
            if (component == null)
            {
                continue;
            }

            if (component is T result)
            {
                return result;
            }
        }

        return null;
    }

    public List<T> GetComponents<T>() where T : MyComponent
    {
        List<T> result = null;

        foreach (var component in _components)
        {
            if (component == null)
            {
                continue;
            }

            if (component is T value)
            {
                result ??= new();
                result.Add(value);
            }
        }

        return result;
    }
}
