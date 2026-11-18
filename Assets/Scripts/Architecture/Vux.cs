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
    public event Action OnPreRender;
    public event Action OnRender;

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
        CustomRendererFeature.CustomRenderPass.OnPrepareExecuteCmd.AddListener(HandlePreRender);
        CustomRendererFeature.CustomRenderPass.OnExecuteCmd.AddListener(HandleRender);
    }

    private void OnDisable()
    {
        CustomRendererFeature.CustomRenderPass.OnPrepareExecuteCmd.RemoveListener(HandlePreRender);
        CustomRendererFeature.CustomRenderPass.OnExecuteCmd.RemoveListener(HandleRender);
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

    private void HandlePreRender()
    {
        OnPreRender?.Invoke();
    }

    private void HandleRender()
    {
        OnRender?.Invoke();
    }

}
