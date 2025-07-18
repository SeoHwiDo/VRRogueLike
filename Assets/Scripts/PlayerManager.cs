using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
   
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private CtrlManager _ctrlManager;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        if (_inputManager == null || _ctrlManager == null)
            return;

        Vector2 lookDelta = _inputManager.GetLookDelta();

        if (lookDelta != Vector2.zero)
        {
            _ctrlManager.Look(lookDelta);
        }


    }
}
