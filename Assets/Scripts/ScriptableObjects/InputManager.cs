using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputManager", menuName = "Game/Input Manager")]
public class InputManager : ScriptableObject, InputActions.IPlayerActions
{
    //public event UnityAction jumpEvent;
    //public event UnityAction jumpCanceledEvent;
    public event UnityAction<Vector2> moveEvent;
    public event UnityAction attackEvent;

    [SerializeField] private InputActions inputActions;

    void OnEnable()
    {
        if (inputActions == null) {
            inputActions = new InputActions();
            inputActions.Player.SetCallbacks(this);
            inputActions.Enable();
        }
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        if (moveEvent != null) {
            moveEvent.Invoke(context.ReadValue<Vector2>());
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (attackEvent != null) {
            attackEvent.Invoke();
        }
    }

    // public void OnJump(InputAction.CallbackContext context){
    //     if (jumpEvent != null && context.phase == InputActionPhase.Started) {
    //         jumpEvent.Invoke();
    //     } else if (jumpCanceledEvent != null && context.phase == InputActionPhase.Canceled) {
    //         jumpCanceledEvent.Invoke();
    //     }
    // }


    void OnDisable()
    {
        inputActions.Disable();
    }
}
