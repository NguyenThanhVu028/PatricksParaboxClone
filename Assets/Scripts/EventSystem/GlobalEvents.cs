public class GlobalEvents : EventPublisher
{
    private static GlobalEvents _instance = null;

    public static GlobalEvents Instance
    {
        get
        {
            _instance ??= new();
            return _instance;
        }
    }
}
