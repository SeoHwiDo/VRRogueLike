
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InputManager : MonoBehaviour
{
    public static InputManager Instance {  get; private set; }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public enum InputType { Mouse, Gamepad, Touch }
    public InputType inputType = InputType.Mouse;

    public float mouseSensitivity = 100f;
    public float gamepadSensitivity = 100f;
    public float touchSensitivity = 0.1f;

 

    public Vector2 GetLookAxis()
    {
        switch (inputType)
        {
            case InputType.Mouse:
                return GetMouseAxis() * mouseSensitivity;
            case InputType.Gamepad:
                return GetGamepadAxis() * gamepadSensitivity;
            case InputType.Touch:
                return GetTouchAxis() * touchSensitivity;
            default:
                return Vector2.zero;
        }
    }

    private Vector2 GetMouseAxis()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    private Vector2 GetGamepadAxis()
    {
#if ENABLE_INPUT_SYSTEM
        if (Gamepad.current != null)
            return Gamepad.current.rightStick.ReadValue();
#endif
        return Vector2.zero;
    }

    private Vector2 GetTouchAxis()
    {
        Vector2 lastTouchPos;
        bool isTouching=false;
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPos = touch.position;
                isTouching = true;
            }
            else if (touch.phase == TouchPhase.Moved && isTouching)
            {
                return touch.deltaPosition;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isTouching = false;
            }
        }
        return Vector2.zero;
    }
    public bool GetFireKeyDown()
    {
        switch (inputType)
        {
            case InputType.Mouse:
                return GetMouseFireKeyDown();
            case InputType.Gamepad:
                return GetGamepadFireKeyDown();
            case InputType.Touch:
                return GetTouchFireKeyDown();
            default:
                return false;
        }
    }

    public bool GetFireKeyHeld()
    {
        switch (inputType)
        {
            case InputType.Mouse:
                return GetMouseFireKeyHeld();
            case InputType.Gamepad:
                return GetGamepadFireKeyHeld();
            case InputType.Touch:
                return GetTouchFireKeyHeld();
            default:
                return false;
        }
    }

    public bool GetFireKeyUp()
    {
        switch (inputType)
        {
            case InputType.Mouse:
                return GetMouseFireKeyUp();
            case InputType.Gamepad:
                return GetGamepadFireKeyUp();
            case InputType.Touch:
                return GetTouchFireKeyUp();
            default:
                return false;
        }
    }

    //Mouse Fire
    private bool GetMouseFireKeyDown() => Input.GetMouseButtonDown(0);
    private bool GetMouseFireKeyHeld() => Input.GetMouseButton(0);
    private bool GetMouseFireKeyUp() => Input.GetMouseButtonUp(0);

    //Gamepad Fire
    private bool GetGamepadFireKeyDown()
    {
#if ENABLE_INPUT_SYSTEM
        return Gamepad.current?.leftTrigger.wasPressedThisFrame ?? false;
#endif
        return false;
    }
    private bool GetGamepadFireKeyHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Gamepad.current?.leftTrigger.isPressed ?? false;
#endif
        return false;
    }
    private bool GetGamepadFireKeyUp()
    {
#if ENABLE_INPUT_SYSTEM
        return Gamepad.current?.leftTrigger.wasReleasedThisFrame ?? false;
#endif
        return false;
    }

    //Touch Fire
    private bool GetTouchFireKeyDown()
    {
        return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
    }
    private bool GetTouchFireKeyHeld()
    {
        if (Input.touchCount > 0)
        {
            var phase = Input.GetTouch(0).phase;
            return phase == TouchPhase.Moved || phase == TouchPhase.Stationary;
        }
        return false;
    }
    private bool GetTouchFireKeyUp()
    {
        return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;
    }
}
