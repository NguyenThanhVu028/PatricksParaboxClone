public class MyComponent
{
    private MyObject _object;

    public MyObject Object
    {
        get => _object;
    }

    public void RegisterObject(MyObject myObject)
    {
        _object = myObject;
    }
}
