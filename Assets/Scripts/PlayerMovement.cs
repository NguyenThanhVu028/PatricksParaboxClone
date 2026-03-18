using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : CubeMovement
{
    [SerializeField] bool allowInput = true;
    private List<Vector2> inputList = new();

    protected  void FixedUpdate()
    {
        if (inputList.Count > 0) Move(inputList[inputList.Count - 1]);
    }

    // ----- PLAYER INPUTS ----- //
    public void StopTakingInput()
    {
        allowInput = false; inputList.Clear();
    }

    public void OnMoveUp(InputAction.CallbackContext context)
    {
        if (!allowInput) return;
        if (context.started) AddToInputList(Vector2.up);
        if (context.canceled) RemoveFromInputList(Vector2.up);
    }
    public void OnMoveDown(InputAction.CallbackContext context)
    {
        if (!allowInput) return;
        if (context.started) AddToInputList(Vector2.down);
        if (context.canceled) RemoveFromInputList(Vector2.down);
    }
    public void OnMoveLeft(InputAction.CallbackContext context)
    {
        if (!allowInput) return;
        if (context.started) AddToInputList(Vector2.left);
        if (context.canceled) RemoveFromInputList(Vector2.left);
    }
    public void OnMoveRight(InputAction.CallbackContext context)
    {
        if (!allowInput) return;
        if (context.started) AddToInputList(Vector2.right);
        if (context.canceled) RemoveFromInputList(Vector2.right);
    }

    // ----- ACTIONS ON INPUT LIST ----- //

    private void AddToInputList(Vector2 movementInput)
    {
        if (movementInput == Vector2.zero) return;
        if (inputList.Contains(movementInput)) return;
        inputList.Add(movementInput);
    }

    private void RemoveFromInputList(Vector2 movementInput)
    {
        inputList.Remove(movementInput);
    }
}
