using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInputsManager : MonoBehaviour, MainInputSystem.INormalActions
{
    public enum MovementInputs { Up, Down, Left, Right, None }

    private static PlayerInputsManager instance;

    [SerializeField] bool allowTakingMovementInput = true;
    [SerializeField] bool allowUsingMovementInput = true;
    [SerializeField] UnityEvent onUndoEvent = new();

    public UnityEvent OnUndoEvent { get => onUndoEvent; }

    private List<MovementInputs> movementInputsList = new(); // Store all the receivec inputs

    private MainInputSystem mainInputSystem;

    public static PlayerInputsManager Instance { get => instance; }

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(instance);
        instance = this;

        mainInputSystem = new MainInputSystem();

        // Subscribe actions 
        mainInputSystem.Normal.SetCallbacks(this);
    }

    private void OnEnable()
    {
        if (mainInputSystem != null) mainInputSystem.Enable();
    }
    private void OnDisable()
    {
        if (mainInputSystem != null) mainInputSystem.Disable();
    }


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
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnUndo(InputAction.CallbackContext context)
    {
        if (context.started) onUndoEvent.Invoke();
    }

    //Helper functions
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

    public void OnMove(InputAction.CallbackContext context)
    {
    }
}
