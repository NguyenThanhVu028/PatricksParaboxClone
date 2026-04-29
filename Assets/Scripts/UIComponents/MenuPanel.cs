using System.Collections.Generic;
using UnityEngine;

// This class is used to navigate between menu items
public class MenuPanel : MonoBehaviour
{
    [SerializeField] List<MenuItem> menuItems = new();
    [SerializeField] int currentIndex = 0;

    private void OnEnable()
    {
        if (PlayerInputsManager.Instance != null)
        {
            PlayerInputsManager.Instance.UIInputs.OnNavigateUpEvent.AddListener(OnNavigateUp);
            PlayerInputsManager.Instance.UIInputs.OnNavigateDownEvent.AddListener(OnNavigateDown);
        }

        foreach (var menuItem in menuItems)
        {
            if (menuItem == null) return;
            menuItem.OnSelected(false);
        }
        if (currentIndex < menuItems.Count && menuItems[currentIndex] != null) menuItems[currentIndex].OnSelected(true);
    }

    private void OnDisable()
    {
        if (PlayerInputsManager.Instance != null)
        {
            PlayerInputsManager.Instance.UIInputs.OnNavigateUpEvent.RemoveListener(OnNavigateUp);
            PlayerInputsManager.Instance.UIInputs.OnNavigateDownEvent.RemoveListener(OnNavigateDown);
        }
    }

    private void OnNavigateDown()
    {
        if (menuItems.Count == 0) return;
        if (currentIndex < 0)
        {
            currentIndex = 0; return;
        }
        if (currentIndex >= menuItems.Count)
        {
            currentIndex = menuItems.Count - 1; return;
        }
        if (menuItems[currentIndex] != null) menuItems[currentIndex].OnSelected(false);
        currentIndex++; currentIndex %= menuItems.Count;
        if (menuItems[currentIndex] != null) menuItems[currentIndex].OnSelected(true);
    }

    private void OnNavigateUp()
    {
        if (menuItems.Count == 0) return;
        if (currentIndex < 0)
        {
            currentIndex = 0; return;
        }
        if (currentIndex >= menuItems.Count)
        {
            currentIndex = menuItems.Count - 1; return;
        }
        if (menuItems[currentIndex] != null) menuItems[currentIndex].OnSelected(false);
        currentIndex--; 
        if (currentIndex < 0) currentIndex = (menuItems.Count + currentIndex % menuItems.Count) % menuItems.Count;
        if (menuItems[currentIndex] != null) menuItems[currentIndex].OnSelected(true);
    }
}
