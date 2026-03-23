using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementInputsManager : MonoBehaviour
{
    private static PlayerMovementInputsManager instance;

    [SerializeField] bool allowMovementInput = true;
    private List<Vector2> movementInputsList = new(); // Store all the receivec inputs

    private MainInputSystem mainInputSystem;

    public static PlayerMovementInputsManager Instance { get => instance; }

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
    }

    private void OnEnable()
    {
        if (mainInputSystem != null) mainInputSystem.Enable();
    }
    private void OnDisable()
    {
        if (mainInputSystem != null) mainInputSystem.Disable();
    }


    // ----- PLAYER INPUTS ----- //
    public void StopTakingInput()
    {
        allowMovementInput = false;
        movementInputsList.Clear();
    }
    public void ContinueTakingInput()
    {
        allowMovementInput = true;
    }

    public void OnMoveUp(bool isActive)
    {
        Debug.Log("Move up: " + isActive);
        if (!allowMovementInput) return;
        if (isActive) AddToInputList(Vector2.up);
        else RemoveFromInputList(Vector2.up);
    }
    public void OnMoveDown(bool isActive)
    {
        Debug.Log("Move down: " + isActive);
        if (!allowMovementInput) return;
        if (isActive) AddToInputList(Vector2.down);
        else RemoveFromInputList(Vector2.down);
    }
    public void OnMoveLeft(bool isActive)
    {
        Debug.Log("Move left: " + isActive);
        if (!allowMovementInput) return;
        if (isActive) AddToInputList(Vector2.left);
        else RemoveFromInputList(Vector2.left);
    }
    public void OnMoveRight(bool isActive)
    {
        Debug.Log("Move right: " + isActive);
        if (!allowMovementInput) return;
        if (isActive) AddToInputList(Vector2.right);
        else RemoveFromInputList(Vector2.right);
    }

    // ----- ACTIONS ON INPUT LIST ----- //
    private void AddToInputList(Vector2 movementInput)
    {
        if (movementInput == Vector2.zero) return;
        if (movementInputsList.Contains(movementInput)) return;
        movementInputsList.Add(movementInput);
    }

    private void RemoveFromInputList(Vector2 movementInput)
    {
        movementInputsList.Remove(movementInput);
    }

    public Vector2 GetLatestMovementInput()
    {
        if (movementInputsList == null || movementInputsList.Count == 0) return Vector2.zero;
        return movementInputsList[movementInputsList.Count - 1];
    }
}
