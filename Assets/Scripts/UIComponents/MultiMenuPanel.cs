using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MultiMenuPanel : MonoBehaviour
{
    [SerializeField] MenuPanel defaultPanel;
    [SerializeField] UnityEvent onEndOfPanels = new();
    private Stack<MenuPanel> menuPanels = new();

    private void OnEnable()
    {
        menuPanels.Clear();
        if (defaultPanel != null) AddPanel(defaultPanel);
    }

    private void OnDisable()
    {
        foreach (var panel in menuPanels)
        {
            if (panel != null) panel.gameObject.SetActive(false);
        }
        menuPanels.Clear();
    }

    public void AddPanel(MenuPanel panel)
    {
        if (menuPanels.Count > 0 && menuPanels.Peek() != null) menuPanels.Peek().gameObject.SetActive(false);
        panel.gameObject.SetActive(true);
        menuPanels.Push(panel);
    }
    public void OnReturn()
    {
        if (menuPanels.Count == 0) return;
        var panel = menuPanels.Pop();
        if (panel != null) panel.gameObject.SetActive(false);
        if (menuPanels.Count > 0)
        {
            var prevPanel = menuPanels.Peek();
            if (prevPanel != null) prevPanel.gameObject.SetActive(true);
        }
        else onEndOfPanels.Invoke();
    }
}
