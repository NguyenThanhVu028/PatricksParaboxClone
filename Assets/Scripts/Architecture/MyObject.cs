using System.Collections.Generic;

public class MyObject
{
    private List<MyComponent> _components = new();

    public MyObject()
    {

    }

    public void RegisterComponent(MyComponent component)
    {
        if (component == null)
        {
            return;
        }

        component.RegisterObject(this);
        _components.Add(component);
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
