using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MapCreator : MonoBehaviour{

    [Header("Map Settings")]
    public int MAP_SIZE = 5;

    [Header("Map Prefabs")]
    [SerializeField] private GameObject[] mapTiles;
    [SerializeField] private GameObject enemySpawnPoint;
    [SerializeField] private GameObject[] drones;
    [SerializeField] private GameObject droneMovePrefab;
    [SerializeField] private GameObject droneSpawnEffect;
    [SerializeField] private GameObject[] skillCardPrefabs;

    //맵,드론 등 소환할 오브젝트 프리팹
    public GameObject[] map = new GameObject[7], drone = new GameObject[4], skillCard = new GameObject[10];
    //랜덤구성의 맵 관리를 위한 2중배열
    public GameObject[,] maps;
    public GameObject player, enemySpawn, droneMove, DroneSpawnPtc,levelupPtc,endUI;
    public int droneNum,droneKill, maxLevel=4, level;
    public float HP;
    public Text level_text,enemy_num_text,end_result_text;
    public AudioClip droneSpawnSound;
    public Image HPImage;
    GameObject[] enemySpawns = new GameObject[4];
    DroneCtrl droneCtrl;
    GunFire gunFire;
    Rigidbody player_rig;
    float yRot = 180;
    int room = 0, door = 0, parentI = 0, parentJ = 0, enemySpawnCnt = 0,moveSpeed;
    bool enemySpawnOn = false;//,player_ready=false, EZmode=false, spawner = false;
    List<GameObject> enterDrone = new List<GameObject>();
    Vector3 card_spawn;

    void Start()
    {
        //오브젝트 기초 설정
        HP=10;
        maps = new GameObject[MAP_SIZE, MAP_SIZE];
        player_rig =player.GetComponent<Rigidbody>();
        gunFire=GameObject.Find("bullet_spawn").GetComponent<GunFire>();
        moveSpeed=2;
        droneNum = 4;
        level = 1;
        GenerateMap();
        enemySpawnCnt = 0;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


    }
    private void Update(){
        //체력바 관리
        HPImage.fillAmount=HP/10;
        //만약 모든 드론 제거시
        if (droneNum == 0){
            //맵 재구축
            RebuildMap();
            //난이도 레벨 상승
            level++;
            //게임 모드(level)이 기본일때 레벨상한 존재, 하드모드는 상한없음
            if (PlayerPrefs.GetInt("level")==1){
                if(level>=maxLevel)level=maxLevel;
            }
            //레벨당 소환할 드론 수
            droneNum = level * 4;
            //드론 이동속도 증가
            moveSpeed++;
            //레벨업 파티클 
            GameObject levelupPtc_inst=Instantiate(levelupPtc,player.transform.position,Quaternion.identity);
            Destroy(levelupPtc_inst,1.5f);
        }
        //현재 스테이지 레벨 및 잔여 드론 수
        //level_text.text="lv."+level;
        //enemy_num_text.text=""+droneNum;
        //게임 아웃일떄
        if(HP<=0){
            //스킬을 통한 추가 생명이 있으면 추가 생명 차감 후 아이콘 제거
            if(gunFire.moreHP>0){
                gunFire.moreHP--;
                HP=10;
                Destroy(GameObject.Find("life"));
            }else{
                //게임 종료 프로세스
                
                end_result_text.text=
                "파괴한 드론\n"+droneKill+"\n최종 레벨\n"+level;
                endUI.SetActive(true);
                Time.timeScale=0;
                if(Input.GetMouseButtonUp(0)) {
                    //초기화면 호출
                    SceneManager.LoadScene(0);
                }
            }
        }
    }

    public void GenerateMap(){
        
        for (int i = 0; i < MAP_SIZE; i++){
            for (int j = 0; j < MAP_SIZE; j++){
                //좌우 벽일때
                if (i == 0 || i == MAP_SIZE-1){
                    //좌우벽이면서 상하 벽일때 즉, 코너일때
                    if (j == 0 || j == MAP_SIZE-1)
                    {   //소환할 맵 타입 코너 맵으로 설정
                        room = 0;
                        //코너의 위치에 따라 회전하여 모서리에 맞게 변경
                        if (i == j) yRot = i == 0 ? 180 : 0;
                        else yRot = i == 0 ? -90 : 90;
                    }
                    //코너가 아닐때
                    else{
                        //좌우 위치에 맞게 회전
                        yRot = i == 0 ? -90 : 90;
                        //벽의 가운데일때
                        if (j == MAP_SIZE/2)
                        {//문을 지정 갯수만큼 생성, 
                            if (door <= 1) room = 2;
                            else room = Random.Range(1, 3);
                            if (room == 2) door++;
                            //드론 스폰 지점 생성
                            enemySpawnOn = true;
                        }
                        else room = 1;
                    }
                }//상하 벽일때
                else{
                    if (j == 0 || j == 4){
                        yRot = j == 0 ? 180 : 0;
                        //벽의 가운데일때
                        if (i == 2){
                            if (door <= 1) room = 2;
                            else room = Random.Range(1, 3);
                            if (room == 2) door++;
                            enemySpawnOn = true;
                        }
                        else room = 1;
                    }
                    else{
                        //if (!spawner) spawner = true;
                        room = Random.Range(3,7);
                    }
                }
                //맵 생성
                maps[i, j] = Instantiate(map[room], new Vector3(i * 12, 0, j * 12), Quaternion.Euler(0, yRot, 0));
                //맵 타일 관리 용이를 위한 좌표위치 
                maps[i, j].name = map[room].name + "[" + i + "," + j + "]";
                //드론 스폰지점 생성 타일일때 생성
                if (enemySpawnOn){
                    //스폰지점 관리를 위한 배열화
                    enemySpawns[enemySpawnCnt] = Instantiate(enemySpawn, new Vector3(0, 0, 0), Quaternion.identity);
                    //현재 타일 아래로 귀속
                    enemySpawns[enemySpawnCnt].transform.parent = maps[i, j].transform;
                    //위치 조정
                    enemySpawns[enemySpawnCnt].transform.localPosition = new Vector3(0, 3.65f, 0);
                    //스폰지점 갯수(인덱스) 관리
                    enemySpawnCnt++;
                    enemySpawnOn = false;
                }
            }
        }//플레이어 위치 지정을 위한 랜덤 좌표
        parentI = Random.Range(1, 4);
        parentJ = Random.Range(1, 4);
        //플레이어 위치 지정 후 해당 타일을 부모로 설정
        this.transform.parent = maps[parentI, parentJ].transform;
        //위치 조정
        this.transform.localPosition = new Vector3(0, 0, 0);
        //플레이어 위치 조정
        player.transform.parent= this.transform;
        player.transform.localPosition = new Vector3(0,15,0);
        player.transform.parent=null;
        //플레이어 회전 축 고정(타격으로 인한 회전 방지)
        player_rig.constraints=RigidbodyConstraints.FreezeRotationX|RigidbodyConstraints.FreezeRotationZ;
        //스킬 카드 소환
        makeSkill();
        //물리엔진 영향으로부터 제외
        player_rig.isKinematic = true;
        
    }
    //현재 맵 파괴
    void DestroyMap(){
        //맵 파괴시 시스템관리 오브젝트는 제외
        this.transform.parent = null;
        for (int i = 0; i < 5; i++){
            for (int j = 0; j < 5; j++)Destroy(maps[i, j]);
        }
        enemySpawnCnt = 0;
    }
    //맵 재구성
    void RebuildMap(){
        DestroyMap();
        GenerateMap();
    }
    //드론 스폰 일정 시간마다 스폰
    public IEnumerator Spawn(){
        int spawnDroneNum = droneNum;
        for (int i = 0; i < spawnDroneNum; i++){  
            yield return new WaitForSeconds(0.5f);
            SpawnDrone();
            yield return new WaitForSeconds(6-level);
        }
    }
    //skill card instantiate
    void makeSkill(){
        //스킬 선택 상태
        gunFire.makeSkillTime=true;
        //드론 타격 감지 콜라이더 비활성화(레이캐스트를 위해)
        gameObject.GetComponent<BoxCollider>().enabled=false;
        //스킬 선택 커서 활성화
        gunFire.skillCursor.SetActive(true);
        //생성할 스킬 리스트 
        List<int> cardChk=new List<int>();
        GameObject[] skillCard_inst= new GameObject[3];
        for(int i =0;i<skillCard_inst.Length;i++){
            int cardN=Random.Range(0,9);
            card_spawn=player.transform.position;
            //랜덤으로 호출한 카드가 중복으로 호출되었거나 이미 보유중인 스킬일때
            if(cardChk.Contains(cardN)||gunFire.skill.Contains(skillCard[cardN].name)){
                //다시 선택
                i--;
                continue;
            }
            else{
                //제대로 호출된 카드를 생성할 카드 리스트에 삽입
                cardChk.Add(cardN);
                //카드의 스폰 위치 지정
                card_spawn.x+=-2+2*i;
                card_spawn.z+=3;
                //소환된 스킬 카드 인스턴스 관리를 위한 배열에 삽입
                skillCard_inst[i]=Instantiate(skillCard[cardN],card_spawn,Quaternion.identity);
                //해당 카드 이름을 스킬카드 이름과 같게 변경
                skillCard_inst[i].name=skillCard[cardN].name;
                //스킬카드를 보기 편하게 플레이어를 바라보도록 설정
                skillCard_inst[i].transform.LookAt(player.transform);
            }
        }
    }

    //드론 소환
    void SpawnDrone(){
        //랜덤 드론 소환 위치 선택
        GameObject enemySpawns_inst=enemySpawns[Random.Range(0, 4)];
        //드론 소환할 위치
        Vector3 droneSpawnPos = enemySpawns_inst.transform.position;
        // 드론 스폰지점과 겹치지 않게 높이 조절
        droneSpawnPos.y -= 1.0f;

        //드론 오브젝트 랜덤선택
        GameObject drone_inst = Instantiate(drone[Random.Range(0, 4)], droneSpawnPos, Quaternion.identity);
        //드론 부유모션 및 관리를 위한 인스턴스
        GameObject droneMove_inst = Instantiate(droneMove, new Vector3(-100.0f, -100.0f, -100.0f), Quaternion.identity);
        //드론 관리 인스턴스를 드론 오브젝트 아래로 추가
        droneMove_inst.transform.parent = drone_inst.transform;
        //미세 위치조정
        droneMove_inst.transform.localPosition=new Vector3(0,0,0);

        //소환 이펙트
        GameObject droneSpawnPtc_inst = Instantiate(DroneSpawnPtc,droneSpawnPos,Quaternion.identity);
        //소환 효과음
        enemySpawns_inst.GetComponent<AudioSource>().volume = 0.8f;
        enemySpawns_inst.GetComponent<AudioSource>().PlayOneShot(droneSpawnSound);
        //이펙트 제거
        Destroy(droneSpawnPtc_inst,1.5f); 
        //드론 설정 스크립트
        droneCtrl=droneMove_inst.GetComponent<DroneCtrl>();
        //난이도에 따른 드론 이동속도 변경
        droneCtrl.moveSpeed=moveSpeed;
    }
    //드론으로부터 타격받을때
    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.CompareTag("drone")){
            //처음 타격한 드론일떄 타격한 드론 배열에 추가
            if(!enterDrone.Contains(other.gameObject)){
                enterDrone.Add(other.gameObject);
                Debug.Log("enter");
                //진동 피드백
                Handheld.Vibrate();//진동
                //체력 감소
                if(!gunFire.godMode) HP--;
            }
            //드론의 이동속도를 0으로 바꿔 계속하여 전진하는것 방지
            other.gameObject.transform.GetChild(other.gameObject.transform.childCount-1).GetComponent<DroneCtrl>().moveSpeed=0;
        }
    }
}
