using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    [SerializeField] private InputActionAsset inputActions;
    private InputAction _upAction;
    public InputAction UpAction => _upAction;
    private InputAction _downAction;
    public InputAction DownAction => _downAction;
    private InputAction _leftAction;
    public InputAction LeftAction => _leftAction;
    private InputAction _rightAction;
    public InputAction RightAction => _rightAction;

    private void OnEnable()
    {
        inputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        inputActions.FindActionMap("Player").Disable();
    }

    protected override void Awake()
    {
        base.Awake();
        _upAction = InputSystem.actions.FindAction("Up");
        _downAction = InputSystem.actions.FindAction("Down");
        _leftAction = InputSystem.actions.FindAction("Left");
        _rightAction = InputSystem.actions.FindAction("Right");
    }

}
