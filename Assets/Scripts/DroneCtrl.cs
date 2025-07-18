using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DroneCtrl : MonoBehaviour
{
    // Start is called before the first frame update
    //드론 처치시 식별 파티클
    public GameObject deadPtc;
    //드론의 이동시 및 처치시 식별 오디오
    public AudioClip droneMoveSound,droneDeadSound;
    //드론의 이동 속도
    public float moveSpeed;
    //드론의 체력 바 이미지
    public Image drHpbar;
    //오디오 소스 관리
    AudioSource audioSource;
    //플레이어 
    GameObject player;
    //드론 생성 및 전반적인 게임 시스템 관리
    MapCreator mapCreator;

    //드론의 부유 모션을 위한 상하운동 속도
    public float upDownSpeed = 1.0f, timer = 0, moveTime = 1.0f;
    bool goingUp = true;
    //드론의 기본 체력 설정
    public float setHP = 5;
    protected float HP;
    //드론의 플레이어 타격 관리 및 게임 몰입도를 위한 체력바 표시 관리
    public bool hit_player, hpFlot = false, droneHPbarEnd=true;
    void Start()
    {
        HP = 5;
        //해당 오브젝트의 오디오컴포넌트 호출
        audioSource=this.GetComponent<AudioSource>();
        //오디오 반복 재생
        audioSource.loop=true;
        //호출한 오디오컴포넌트에 미리 설정한 드론 부유음 할당
        audioSource.clip=droneMoveSound;
        //오디오 재생
        audioSource.Play();
        //식별하기 위한 플레이어 탐색 후 호출
        player = GameObject.Find("Player");
        //드론 생성 관리를 위한 시스템 오브젝트 호출
        mapCreator = GameObject.Find("SpawnSpot").GetComponent<MapCreator>();
        //부유모션을 위한 상하운동 반복
        StartCoroutine(DroneUpDown());
    }

    // Update is called once per frame
    void Update()
    {   
        //hp바가 안떠있고, HP바 플로팅 시간이 다 지났을때
        if(hpFlot&&droneHPbarEnd){
            //플로팅시간 체크를 false로 변경
            droneHPbarEnd =false;
            //hp바 시간을 비동기로 처리
            StartCoroutine(DroneHPbar());
        }

        //드론의 HP바를 전체 체력
        drHpbar.fillAmount=HP/setHP;

        // Debug.Log(timer);
        //만약 드론의 체력이 모두 닳았을때
        if (HP <= 0)
        {
            //게임 시스템 관리 코드에서 전체 드론의 갯수 감소
            mapCreator.droneNum--;
            //처치한 드론의 갯수 증가
            mapCreator.droneKill++;
            //사망 이펙트 및 사운드 재생
            GameObject deadPtc_i = Instantiate(deadPtc, this.transform.parent.position, Quaternion.identity);
            deadPtc_i.GetComponent<AudioSource>().PlayOneShot(droneDeadSound);
            //드론 제거
            Destroy(deadPtc_i, 1.0f);
            Destroy(this.transform.parent.gameObject);
            //이펙트 제거
            
        }
        if (!hit_player)//플레이어에 닿을때까지 플레이어 방향으로 이동
        {
            this.transform.parent.LookAt(player.transform);
            this.transform.parent.position += this.transform.parent.forward * moveSpeed * Time.deltaTime;
        }
    }
    //체력 감소 함수
    public void loseHP(float dmg){
        this.HP -= dmg;
    }

    IEnumerator DroneHPbar(){
        //코루틴 실행시 체력바 플로팅
        this.transform.GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        this.transform.GetChild(0).gameObject.SetActive(false);
        //hp바 수명이 끝났으므로 false;
        hpFlot = false;
        //위와 같은 이유로 true;
        droneHPbarEnd = true;
    }

    void MoveUpdown()
    {
        //타이머를 통해 일정 시간마다 상하운동 반복
        timer += Time.deltaTime;
        if (goingUp){
            transform.parent.position += transform.up * upDownSpeed * Time.deltaTime;
            if (timer>=moveTime) goingUp = false;
        }
        else{
            transform.parent.position -= transform.up * upDownSpeed * Time.deltaTime;
            if (timer >= moveTime * 2.0f){
                goingUp = true;
                timer = 0;
            }
        }
    }
    //기능과 코루틴 분리
    IEnumerator DroneUpDown(){
        while (true){
            MoveUpdown();
             yield return null;  // 한 프레임 대기 (없으면 무한 루프 됨)
        }
    }
}
