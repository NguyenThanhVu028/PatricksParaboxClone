using UnityEngine;

public class TestEventSystem : MonoBehaviour
{
    void Start()
    {
        SceneEvents.Instance.InvokeEvent("TestEvent", 15);
        var testListenOnce = new TestListenOnce();
    }

    public class TestListenOnce : IEventListener
    {
        public TestListenOnce()
        {
            SceneEvents.Instance.AddListenerOnce(this, "TestEvent");
        }
        public void ReceiveEvent(string eventName, object value)
        {
            if (value is int intValue)
            {
                Debug.Log("Receive once: " + intValue);
            }
        }
    }
}
