using System;
using UnityEngine;

public class Vux : MonoBehaviour
{
    private static Vux _core = null;

    public static Vux Core
    {
        get => _core;
    }


    // Game Life Cycle
    public event Action<float> OnUpdate;
    public event Action<float> OnLateUpdate;
    public event Action<float> OnFixedUpdate;
    public event Action OnBeginRender;
    public event Action OnFinishRender;

    private void Awake()
    {
        if (_core != null && _core != this)
        {
            Destroy(this);
            return;
        }

        _core = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        CustomRendererFeature.CustomRenderPass.OnPrepareExecuteCmd.AddListener(HandleBeginRender);
        CustomRendererFeature.CustomRenderPass.OnExecuteCmd.AddListener(HandleFinishRender);
    }

    private void OnDisable()
    {
        CustomRendererFeature.CustomRenderPass.OnPrepareExecuteCmd.RemoveListener(HandleBeginRender);
        CustomRendererFeature.CustomRenderPass.OnExecuteCmd.RemoveListener(HandleFinishRender);
    }

    private void Update()
    {
        OnUpdate?.Invoke(Time.deltaTime);
    }

    private void LateUpdate()
    {
        OnLateUpdate?.Invoke(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        OnFixedUpdate?.Invoke(Time.fixedDeltaTime);
    }

    private void HandleBeginRender()
    {
        OnBeginRender?.Invoke();
    }

    private void HandleFinishRender()
    {
        OnFinishRender?.Invoke();
    }

}
