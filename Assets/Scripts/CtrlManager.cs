using UnityEngine;

public class CtrlManager : MonoBehaviour
{
    [SerializeField]private Transform cameraPivot;
    private Transform playerBody;
    private float pitchRotation = 0f;

    private void Look(Vector2 delta)
    {
        float yawDelta = delta.x * Time.deltaTime;
        float pitchDelta = delta.y * Time.deltaTime;

        pitchRotation -= pitchDelta;
        pitchRotation = Mathf.Clamp(pitchRotation, -90f, 90f);
        // 카메라의 상하 회전
        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(pitchRotation, 0f, 0f);

        // 몸체의 좌우 회전
        if (playerBody != null)
            playerBody.Rotate(Vector3.up * yawDelta);
    }
    
    void Start()
    {
        playerBody = GameManager.Instance.player.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        Vector2 lookDelta = InputManager.Instance.GetLookAxis();
        if (InputManager.Instance.GetFireKey())
        {
            SkillManager.Instance.ShootBullet();
        }
        if (lookDelta != Vector2.zero)
        {
            Look(lookDelta);
        }
    }
}
