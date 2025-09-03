using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
public class DroneCtrl : MonoBehaviour
{
    //오디오 소스 관리
    private AudioClip droneMoveSound;
    //private AudioClip droneDeadSound;
    private AudioSource audioSource;

    //드론의 체력 바 
    [SerializeField]private Image hpGauge;
    [SerializeField]private Canvas enemyHP;
    private float damage=1f;
    private Coroutine hpHideCoroutine;

    


    private GameObject player;
    //드론의 이동 속도
    private float moveSpeed =2.0f;
    private float tempMoveSpeed;
    //드론의 부유 모션 파라미터
    private float upDownSpeed = 1.0f;
    private float timer = 0f;
    private float moveTime = 1.0f;
    //드론의 기본 체력 설정
    private float maxHP = 5f;
    private float HP;

    //드론의 플레이어 타격 관리 및 게임 몰입도를 위한 체력바 표시 관리
    private bool hit_player = false;
    private bool hpFlot = false;
    private bool droneHPbarEnd = true;
    private bool goingUp = true;
    private bool isDead = false;
    
    void Awake()
    {
        // 해당 오브젝트의 오디오컴포넌트 호출
        audioSource = this.GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        // 상태 초기화
        HP = maxHP;
        isDead = false;
        tempMoveSpeed = moveSpeed;
        hpGauge.fillAmount = 1f; // UI도 초기화
        enemyHP.enabled = false; // 체력바는 숨긴 상태로 시작

        // 싱글톤 인스턴스는 OnEnable에서 찾는 것이 더 안전
        if (GameManager.Instance != null)
        {
            player = GameManager.Instance.player;
        }

        // 오디오 설정 및 재생
        // 오디오는 비활성화 시 자동 중지
        if (audioSource != null && droneMoveSound != null)
        {
            audioSource.loop = true;
            //audioSource.clip = droneMoveSound;
            audioSource.Play();
        }

        // 부유 모션 코루틴 시작
        StartCoroutine(DroneUpDown());
    }
    // Update is called once per frame
    void Update()
    {
        if (isDead) return;
        //만약 드론의 체력이 모두 닳았을때
        if (HP <= 0)
        {
            OnDead();
        }
        if (player != null)
        {
            this.transform.LookAt(player.transform);
            this.transform.position += this.transform.forward * tempMoveSpeed * Time.deltaTime;
        }
    }
    void OnDisable()
    {
        // 모든 코루틴을 중지시켜서, 비활성화된 상태에서 불필요한 연산을 막습니다.
        StopAllCoroutines();
        hpHideCoroutine = null; // 코루틴 참조도 초기화
    }
    private void OnDead()
    {
        if (isDead) return; // 이미 죽었으면 리턴
        isDead = true;
        EnemyManager.Instance.DeadEnemy();
        EnemyManager.Instance.InstanceEnemyDeadPtc(this.transform.position);

        this.gameObject.SetActive(false);
    }
    //체력 감소 함수
    public void loseHP(float dmg){

        this.HP -= dmg;
        hpGauge.fillAmount = HP / maxHP;
    }

    private void ShowHPBar(){
        //코루틴 실행시 체력바 플로팅
        Debug.Log("start ShowHPBar");
        enemyHP.enabled = true;
        if (hpHideCoroutine != null)
        {
            StopCoroutine(hpHideCoroutine);
        }
        hpHideCoroutine = StartCoroutine(HideHPBar());
    }
    private IEnumerator HideHPBar()
    {
        yield return new WaitForSeconds(3f);
        enemyHP.enabled = false;
        hpHideCoroutine = null; // 끝난 후 null로 초기화
    }
    void MoveUpdown()
    {
        //타이머를 통해 일정 시간마다 상하운동 반복
        timer += Time.deltaTime;
        if (goingUp){
            transform.position += transform.up * upDownSpeed * Time.deltaTime;
            if (timer>=moveTime) goingUp = false;
        }
        else{
            transform.position -= transform.up * upDownSpeed * Time.deltaTime;
            if (timer >= moveTime * 2.0f){
                goingUp = true;
                timer = 0;
            }
        }
    }
    //기능과 코루틴 분리
    IEnumerator DroneUpDown(){
        while (!isDead){
            MoveUpdown();
             yield return null;  // 한 프레임 대기 (없으면 무한 루프 됨)
        }
    }
    
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //처음 타격한 드론일떄 타격한 드론 배열에 추가
            List<GameObject> atkPlayerEnemy = PlayerManager.Instance.getatkEnemy();
            if (!atkPlayerEnemy.Contains(other.gameObject))
            {
                atkPlayerEnemy.Add(other.gameObject);
                PlayerManager.Instance.LosePlayerHP(damage);
                UIManager.Instance.UpdateHPUI(PlayerManager.Instance.GetPlayerHP());
                //진동 피드백
                Handheld.Vibrate();
                //체력 감소
                //if (!gunFire.godMode) 
                
            }
            //드론의 이동속도를 0으로 바꿔 계속하여 전진하는것 방지
            tempMoveSpeed = 0;
        }
        if (other.gameObject.CompareTag("bullet"))
        {
            Debug.Log("hit "+other.gameObject.name);
            EnemyManager.Instance.InstanceHitPtc(other.gameObject.transform.position);
            ShowHPBar();
            loseHP(GameManager.Instance.GetBulletDamage());
        }
    }
}

