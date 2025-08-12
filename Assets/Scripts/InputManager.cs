
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

    private Vector2 lastTouchPos;
    private bool isTouching;

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
    private bool GetFireKey()
    {
        switch (inputType)
        {
            case InputType.Mouse:
                return GetMouseFireKey();
            case InputType.Gamepad:
                return GetGamepadFireKey();
            case InputType.Touch:
                return GetTouchFireKey();
            default:
                return false;
        }
    }
    private bool GetMouseFireKey()
    {
        return Input.GetMouseButtonDown(0);
    }
    private bool GetGamepadFireKey()
    {
#if ENABLE_INPUT_SYSTEM
        if (Gamepad.current != null)
        {
           return Gamepad.current.leftTrigger.wasPressedThisFrame;
        }
#endif
        return false;
    }
    private bool GetTouchFireKey()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // 터치 시작 → 게이지 충전 시작
            if (touch.phase == TouchPhase.Began)
            {
                // TODO: 게이지 충전 시작
            }
            // 터치 유지 중 → 게이지 증가
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                // TODO: 게이지 충전
            }
            // 터치 끝날 때 발사
            else if (touch.phase == TouchPhase.Ended)
            {
                // TODO: 게이지 충전 종료
                return true;
            }
        }
        return false;
    }
}
