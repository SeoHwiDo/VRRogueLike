using UnityEngine;

public class CtrlManager : MonoBehaviour
{
    [SerializeField]private Transform cameraPivot;
    private Transform playerBody;
    private float pitchRotation = 0f;
    public float maxChargeTime = 2f; // 최대 충전 시간
    private float currentCharge; // 현재 충전량
    private bool isCharging;

    void Start()
    {
        playerBody = GameManager.Instance.player.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
  
    void Update()
    {
        Vector2 lookDelta = InputManager.Instance.GetLookAxis();
        HandleShootingInput();
        if (lookDelta != Vector2.zero)
        {
            Look(lookDelta);
        }
    }
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
    private void HandleShootingInput()
    {
        // 발사 버튼을 눌렀을 때
        if (InputManager.Instance.GetFireKeyDown())
        {
            //스킬 쿨타임이 아닐때 게이지 충전
            if (!SkillManager.Instance.IsSkillReloading())
            {
                isCharging = true;
                currentCharge = 0f;
                Debug.Log("게이지 충전을 시작합니다!");
                UIManager.Instance.UpdateSkillUI(0f);
            }
            else
            {
                UIManager.Instance.ShowCaution("스킬 재장전 중입니다.", 1f);
            }
        }
        if (isCharging && InputManager.Instance.GetFireKeyHeld())
        {
            currentCharge += Time.deltaTime;
            currentCharge = Mathf.Min(currentCharge, maxChargeTime);
            float chargeRatio = currentCharge / maxChargeTime;
            UIManager.Instance.UpdateSkillUI(chargeRatio);
            if (chargeRatio >= 1f)
            {
                Debug.Log("스킬발동");
                SkillManager.Instance.ReloadSkill();
                isCharging = false;
            }
    
        }
        if (InputManager.Instance.GetFireKeyUp())
        {
            if (isCharging)
            {
                UIManager.Instance.UpdateSkillUI(0f);
                isCharging=false;
            }
            if (!SkillManager.Instance.IsBulletReloading())
            {
                SkillManager.Instance.StartShooting();
            }
            else
            {
                UIManager.Instance.ShowCaution("총알 재장전 중입니다.", 1f);

            }
        }
    }
}
