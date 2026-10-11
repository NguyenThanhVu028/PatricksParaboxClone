public class MyComponent
{
    private MyObject _object;

    public MyObject Object
    {
        get => _object;
    }

    public bool RegisterObject(MyObject myObject)
    {
        _object = myObject;
        return true;
    }
}
