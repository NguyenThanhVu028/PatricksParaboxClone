using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInputsManager : MonoBehaviour
{
    public enum MovementInputs { Up, Down, Left, Right, None }

    private static PlayerInputsManager instance;

    [SerializeField] bool allowTakingMovementInput = true;
    [SerializeField] bool allowUsingMovementInput = true;
    private List<MovementInputs> movementInputsList = new(); // Store all the receivec inputs

    private MainInputSystem mainInputSystem;

    public static PlayerInputsManager Instance { get => instance; }

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(instance);
        instance = this;

        mainInputSystem = new MainInputSystem();

        // Subscribe actions 
        mainInputSystem.Normal.MoveUp.started += onMoveUpStarted => { OnMoveUp(true); };
        mainInputSystem.Normal.MoveUp.canceled += onMoveUpCanceled => { OnMoveUp(false); };
        mainInputSystem.Normal.MoveDown.started += onMoveDownStarted => { OnMoveDown(true); };
        mainInputSystem.Normal.MoveDown.canceled += onMoveDownCanceled => { OnMoveDown(false); };
        mainInputSystem.Normal.MoveLeft.started += onMoveLeftStarted => { OnMoveLeft(true); };
        mainInputSystem.Normal.MoveLeft.canceled += onMoveLeftCanceled => { OnMoveLeft(false); };
        mainInputSystem.Normal.MoveRight.started += onMoveRightStarted => { OnMoveRight(true); };
        mainInputSystem.Normal.MoveRight.canceled += onMoveRightCanceled => { OnMoveRight(false); };
        mainInputSystem.Normal.Reset.started += onResetPerformed => { OnReset(); };
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
    public void OnReset()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
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
}
