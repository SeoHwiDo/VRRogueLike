
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InputManager : MonoBehaviour
{
    public enum InputType { Mouse, Gamepad, Touch }
    public InputType inputType = InputType.Mouse;

    public float mouseSensitivity = 1.0f;
    public float gamepadSensitivity = 100f;
    public float touchSensitivity = 0.1f;

    private Vector2 lastTouchPos;
    private bool isTouching;

    public Vector2 GetLookDelta()
    {
        switch (inputType)
        {
            case InputType.Mouse:
                return GetMouseDelta() * mouseSensitivity;
            case InputType.Gamepad:
                return GetGamepadDelta() * gamepadSensitivity;
            case InputType.Touch:
                return GetTouchDelta() * touchSensitivity;
            default:
                return Vector2.zero;
        }
    }

    private Vector2 GetMouseDelta()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    private Vector2 GetGamepadDelta()
    {
#if ENABLE_INPUT_SYSTEM
        if (Gamepad.current != null)
            return Gamepad.current.rightStick.ReadValue();
#endif
        return Vector2.zero;
    }

    private Vector2 GetTouchDelta()
    {
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
}
