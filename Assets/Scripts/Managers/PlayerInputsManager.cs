using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInputsManager : MonoBehaviour 
{
    public enum MovementInputs { Up, Down, Left, Right, None }
    public enum ActionMaps { Gameplay, UI}

    private static PlayerInputsManager instance;
    public static PlayerInputsManager Instance { get => instance; }

    [SerializeField] ActionMaps defaultActionMap = ActionMaps.Gameplay;
    [SerializeField] GameplayInputsReceiver gameplayInputsReceiver = new();
    [SerializeField] UIInputsReceiver uiInputsReceiver = new();

    private MainInputSystem mainInputSystem;
    private Stack<ActionMaps> actionMapsStack = new();

    public GameplayInputsReceiver GameplayInputs { get => gameplayInputsReceiver; }
    public UIInputsReceiver UIInputs { get => uiInputsReceiver; }

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(instance);
        instance = this;

        mainInputSystem = new MainInputSystem();

        // Subscribe actions 
        mainInputSystem.Normal.SetCallbacks(gameplayInputsReceiver);
        mainInputSystem.UI.SetCallbacks(uiInputsReceiver);

    }

    private void OnEnable()
    {
        PushActionMap(defaultActionMap);
    }
    private void OnDisable()
    {
        if (mainInputSystem != null)
        {
            mainInputSystem.Normal.RemoveCallbacks(gameplayInputsReceiver);
            mainInputSystem.UI.RemoveCallbacks(uiInputsReceiver);
            actionMapsStack.Clear();
            mainInputSystem.Disable();
        }
    }

    // Action map functions
    public void PushActionMap(ActionMaps actionMap)
    {
        mainInputSystem.Disable();
        actionMapsStack.Push(actionMap);
        EnableActionMap(actionMap);
    }
    private void EnableActionMap(ActionMaps actionMap)
    {
        if (mainInputSystem == null) return;
        switch (actionMap)
        {
            case ActionMaps.Gameplay:
                mainInputSystem.Normal.Enable();
                break;
            case ActionMaps.UI:
                mainInputSystem.UI.Enable();
                break;
        }
    }

    public void PopActionMap()
    {
        if (mainInputSystem == null || actionMapsStack.Count == 0) return;
        mainInputSystem.Disable();
        actionMapsStack.Pop();
        if (actionMapsStack.Count > 0) EnableActionMap(actionMapsStack.Peek());
    }

    public void PopActionMap(ActionMaps actionMap)
    {
        if (mainInputSystem == null || actionMapsStack.Count == 0) return;
        mainInputSystem.Disable();
        if (actionMapsStack.Peek() == actionMap) actionMapsStack.Pop();
        if (actionMapsStack.Count > 0) EnableActionMap(actionMapsStack.Peek());
    }

    // Helper functions
    public static Vector2Int ConvertMovementInputToGridDirection(MovementInputs input)
    {
        switch (input)
        {
            case MovementInputs.Up:
                return new Vector2Int(-1, 0);
            case MovementInputs.Down:
                return new Vector2Int(1, 0);
            case MovementInputs.Left:
                return new Vector2Int(0, -1);
            case MovementInputs.Right:
                return new Vector2Int(0, 1);
            default:
                return Vector2Int.zero;
        }
    }
    public static MovementInputs FilpMovementInput(MovementInputs input, bool horizontal)
    {
        switch (input)
        {
            case MovementInputs.Up:
                return (horizontal)? MovementInputs.Up : MovementInputs.Down;
            case MovementInputs.Down:
                return (horizontal)? MovementInputs.Down : MovementInputs.Up;
            case MovementInputs.Left:
                return (horizontal)? MovementInputs.Right : MovementInputs.Left;
            case MovementInputs.Right:
                return (horizontal)? MovementInputs.Left : MovementInputs.Right;
            default:
                return MovementInputs.None;
        }
    }
    public static MovementInputs ReverseMovementInput(MovementInputs input)
    {
        switch (input)
        {
            case MovementInputs.Up:
                return MovementInputs.Down;
            case MovementInputs.Down:
                return MovementInputs.Up;
            case MovementInputs.Left:
                return MovementInputs.Right;
            case MovementInputs.Right:
                return MovementInputs.Left;
            default:
                return MovementInputs.None;
        }
    }

    [Serializable]
    public class GameplayInputsReceiver: MainInputSystem.INormalActions
    {
        [SerializeField] bool allowTakingMovementInput = true;
        [SerializeField] bool allowUsingMovementInput = true;

        [SerializeField] UnityEvent onResetEvent = new();
        [SerializeField] UnityEvent onUndoEvent = new();
        [SerializeField] UnityEvent onRedoEvent = new();
        [SerializeField] UnityEvent onPauseEvent = new();

        public UnityEvent OnResetEvent { get => onResetEvent; }
        public UnityEvent OnUndoEvent { get => onUndoEvent; }
        public UnityEvent OnRedoEvent { get => onRedoEvent; }
        public UnityEvent OnPauseEvent { get => onPauseEvent; }

        private List<MovementInputs> movementInputsList = new(); // Store all the received inputs

        // Movement inputs
        public void StopUsingMovementInputs()
        {
            allowUsingMovementInput = false;
        }
        public void StopTakingMovementInputs()
        {
            allowTakingMovementInput = false;
            movementInputsList.Clear();
        }
        public void ContinueTakingMovementInputs()
        {
            allowTakingMovementInput = true;
        }
        public void ContinueUsingMovementInputs()
        {
            allowUsingMovementInput = true;
        }

        public void OnMoveUp(bool isActive)
        {
            if (!allowTakingMovementInput) return;
            if (isActive) AddToInputList(MovementInputs.Up);
            else RemoveFromInputList(MovementInputs.Up);
        }
        public void OnMoveDown(bool isActive)
        {
            if (!allowTakingMovementInput) return;
            if (isActive) AddToInputList(MovementInputs.Down);
            else RemoveFromInputList(MovementInputs.Down);
        }
        public void OnMoveLeft(bool isActive)
        {
            if (!allowTakingMovementInput) return;
            if (isActive) AddToInputList(MovementInputs.Left);
            else RemoveFromInputList(MovementInputs.Left);
        }
        public void OnMoveRight(bool isActive)
        {
            if (!allowTakingMovementInput) return;
            if (isActive) AddToInputList(MovementInputs.Right);
            else RemoveFromInputList(MovementInputs.Right);
        }

        public void OnMove(InputAction.CallbackContext context) { }
        public void OnMoveUp(InputAction.CallbackContext context)
        {
            if (context.started) OnMoveUp(true);
            if (context.canceled) OnMoveUp(false);
        }
        public void OnMoveDown(InputAction.CallbackContext context)
        {
            if (context.started) OnMoveDown(true);
            if (context.canceled) OnMoveDown(false);
        }
        public void OnMoveLeft(InputAction.CallbackContext context)
        {
            if (context.started) OnMoveLeft(true);
            if (context.canceled) OnMoveLeft(false);
        }
        public void OnMoveRight(InputAction.CallbackContext context)
        {
            if (context.started) OnMoveRight(true);
            if (context.canceled) OnMoveRight(false);
        }

        // Actions on movement inputs list
        private void AddToInputList(MovementInputs movementInput)
        {
            if (movementInput == MovementInputs.None) return;
            //if (movementInputsList.Contains(movementInput)) return;
            movementInputsList.Remove(movementInput); // Remove the input if it already exists to avoid duplicates and add it to the end of the list
            movementInputsList.Add(movementInput);
        }
        private void RemoveFromInputList(MovementInputs movementInput)
        {
            movementInputsList.Remove(movementInput);
        }
        public MovementInputs GetLatestMovementInput()
        {
            if (movementInputsList == null || movementInputsList.Count == 0 || !allowUsingMovementInput) return MovementInputs.None;
            return movementInputsList[movementInputsList.Count - 1];
        }

        // Other inputs
        public void OnReset(InputAction.CallbackContext context)
        {
            //SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            if (context.started) onResetEvent.Invoke();
        }

        public void OnUndo(InputAction.CallbackContext context)
        {
            if (context.started) onUndoEvent.Invoke();
        }

        public void OnRedo(InputAction.CallbackContext context)
        {
            if (context.started) onRedoEvent.Invoke();
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.started) onPauseEvent.Invoke();
        }
    }
    [Serializable]
    public class UIInputsReceiver : MainInputSystem.IUIActions
    {
        [SerializeField] UnityEvent onAcceptEvent = new();
        [SerializeField] UnityEvent onReturnEvent = new();
        [SerializeField] UnityEvent onNavigateUpEvent = new();
        [SerializeField] UnityEvent onNavigateDownEvent = new();
        [SerializeField] UnityEvent onNavigateLeftEvent = new();
        [SerializeField] UnityEvent onNavigateRightEvent = new();

        public UnityEvent OnAcceptEvent { get => onAcceptEvent; }
        public UnityEvent OnReturnEvent { get => onReturnEvent; }
        public UnityEvent OnNavigateUpEvent { get => onNavigateUpEvent; }
        public UnityEvent OnNavigateDownEvent { get => onNavigateDownEvent; }
        public UnityEvent OnNavigateLeftEvent { get => onNavigateLeftEvent; }
        public UnityEvent OnNavigateRightEvent { get => onNavigateRightEvent; }

        public void OnAccept(InputAction.CallbackContext context)
        {
            if (context.started) onAcceptEvent.Invoke();
        }

        public void OnReturn(InputAction.CallbackContext context)
        {
            if (context.started) onReturnEvent.Invoke();
        }

        public void OnNavigateUp(InputAction.CallbackContext context)
        {
            if (context.started) onNavigateUpEvent.Invoke();
        }

        public void OnNavigateDown(InputAction.CallbackContext context)
        {
            if (context.started) onNavigateDownEvent.Invoke();
        }

        public void OnNavigateLeft(InputAction.CallbackContext context)
        {
            if (context.started) onNavigateLeftEvent.Invoke();
        }

        public void OnNavigateRight(InputAction.CallbackContext context)
        {
            if (context.started) onNavigateRightEvent.Invoke();
        }
    }
}
