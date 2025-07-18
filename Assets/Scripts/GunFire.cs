using System; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GunFire : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject bullet, particle,skillCursor,player,caution,filter,skillWin;
    public int bulletMax,moreHP;
    public float reLoadTime, bulletDmg;

    public Image HPgaugeImage,reLoadGaugeImage,skillGauge;

    public Text bullet_text,caution_text;

    public AudioClip fireSound;

    public bool makeSkillTime,
                bulletNumUp,
                bulletSizeUp,
                bulletInfinite,
                isTriggered,
                godMode;

    //스킬 관리를 위한 스킬 명 리스트
    public List<string> skill = new List<string>();

    //사용 가능한 스킬 표시를 위한 오브젝트 배열
    public List<GameObject> skill_icon = new List<GameObject>();

    //스킬 아이콘 프리팹 배열
    public List<GameObject> skill_icon_prefab = new List<GameObject>();

    AudioSource audioSource;
    MapCreator mapCreator;
    float click_delay = 0, reLoadDelay = 0, bulletNum, bulletSize;
    int bulletDoubleShot=0;
    bool skill_icon_bool,skill_reload,skill_reload_ON;
    Transform infoui;
   //실제 UI에 생성된 스킬 아이콘 인스턴스 리스트
    public List<GameObject> icon= new List<GameObject>();
    // Update is called once per frame
    private void Start()
    {
        //스킬 재사용 대기
        skill_reload_ON=false;
        //스킬 사용을 위한 홀드 게이지 값
        click_delay = 0;

        mapCreator = GameObject.Find("SpawnSpot").GetComponent<MapCreator>();
        audioSource = this.GetComponent<AudioSource>();
        bulletMax = 25;
        //총알 재장전 시간
        reLoadTime = 1.5f;
        //총알 기본 사이즈 배수
        bulletSize = 1.0f;
        //기본 데미지
        bulletDmg = 1.0f;
        //스킬시 여러발 나가는 총알 갯수
        bulletDoubleShot=0;
        //스킬 재사용 대기시간
        reLoadDelay = 0.0f;
        //스테이터스 UI
        //infoui=GameObject.Find("infoUI").transform;
    }
    void Update()
    {
        //스킬 사용시 스킬 아이콘 제거
        if(skill_icon_bool){
            //스킬 재 정렬을 위한 스킬 아이콘 인스턴스 모두 파괴
            for(int i=0;i<icon.Count;i++) Destroy(icon[i]);
            //스킬 아이콘 인스턴스를 관리하기 위한 리스트 초기화
            icon.Clear();
            //현재 보유중인 스킬을 다시 인스턴스 리스트에 추가 및 인스턴스 소환
            for(int i=0;i<skill_icon.Count;i++){
                //인스턴스 리스트에 스킬 아이콘 인스턴스 소환 및 추가
                icon.Add(Instantiate(skill_icon[i],new Vector3(0,0,0),infoui.rotation,infoui));
                //해당 인스턴스의 이름을 스킬 이름으로 변경
                icon[i].name=skill_icon[i].name;
                //생성된 순서대로 인스턴스 나열을 위한 위치 변경
                icon[i].transform.localPosition=new Vector3(0.06f+0.06f*i,0.25f,0.65f);
            }
            //스킬창 재구성이 완료되었으므로 트리거 off
            skill_icon_bool=false;
        }
        //스킬 사용을 위한 클릭 홀드중이 아닐때, 스킬 사용을 위한 딜레이 게이지 감소
        if(!Input.GetMouseButton(0)&&click_delay>0)click_delay-=Time.deltaTime;
        //만약 스킬 대기시간이 끝났고 스킬게이지를 꽉채웠고, 스킬을 재장전 중이 아닐때
        //스킬 재사용 대기 코루틴 실행
        if(skill_reload&&click_delay>=1&&!skill_reload_ON) StartCoroutine(SkillReload());
           
        //스킬을 통한 총알 무한상태일떄는 99/99로 무한 표시
        if(bulletInfinite) bullet_text.text="99/99";
        //위와 같은 상황이 아닐땐 현재 총알 갯수/최대 총알 갯수
        //else bullet_text.text = bulletNum + "/" + bulletMax;
        //디버깅을 위한 레이저선 표시
        Debug.DrawRay(transform.position, this.transform.forward * 100, Color.green);


        //스킬 게이지는 클릭 홀드 시간만큼 채움
        //skillGauge.fillAmount=click_delay;
        //총알 재장전 게이지는 총알 재장전시간에 맞게 표시
        reLoadGaugeImage.fillAmount = reLoadDelay;
        //만약 총알을 다 사용할 시, 재장전 시간만큼 표시(전체를 채우는데 재사용대기시간만큼 걸릴 수 있도록)
        if (bulletNum <= 0) reLoadDelay += 1.0f / reLoadTime * Time.deltaTime;
        //만약 재장전 시간이 1초이상일때
        if (reLoadDelay >= 1.0f){
            //재장전 시간 초기화 후, 잔여 총알 갯수를 최대개수로 수정
            reLoadDelay = 0.0f;
            bulletNum = bulletMax;
        }
        //스킬 선택을 위한 레이캐스트 구현
        RaycastHit hit;
        //레이캐스트 거리를 플레이어가 바라보는 방향으로 1000만큼 지정
        Vector3 forward = this.transform.TransformDirection(Vector3.forward) * 1000;
        //만약 레이캐스트에 무언가 감지된 상태일때
        if (Physics.Raycast(this.transform.position, forward, out hit)){
            //스킬선택시간이 아닐때
            if (!makeSkillTime){
                //마우스를 클릭하고 있는 상태일때
                if (Input.GetMouseButton(0)){
                    //스킬 재사용시간이 아니라면
                    if(!skill_reload&&!skill_reload_ON){
                        //스킬 게이지값 증가
                        click_delay += Time.deltaTime;
                        // Debug.Log("long Tap/" + click_delay);
                        //만약 스킬 게이지가 꽉찼을 경우
                        if (click_delay >= 1.0f){
                            //스킬 사용으로 취급 후 스킬 재사용 대기 상태로 변경
                            skill_reload=true;
                            Debug.Log(skill_reload);
                            try{//스킬이 없을 때 사용한 경우 예외처리
                                //스킬 창에서 가장 앞에 있는 스킬 실행
                                StartCoroutine(skill[0]);
                                Debug.Log(skill[0]);
                                //사용한 스킬아이콘 인스턴스 제거
                                for(int i=0;i<icon.Count;i++) Destroy(icon[i]);
                                //skill reload
                                //스킬 인스턴스 리스트 초기화
                                icon.Clear();

                                //사용가능한 스킬 목록에서 제거
                                skill_icon.Remove(skill_icon[0]);
                                //스킬창의 상태를 재구성하기 위한 트리거 발동
                                skill_icon_bool=true;
                                Debug.Log(skill[0]);

                                //스킬 명 리스트의 0번 인덱스값 제거
                                skill.RemoveAt(0);
                            }catch(Exception ex){
                                //경고창 코루틴 실행
                                StartCoroutine(Caution());
                                Debug.Log(ex);
                            
                            }
                        }
                    }
                }
                //스킬 사용과 구분하기 위해 클릭에서 손가락을 땔때 사격 발생
                if (Input.GetMouseButtonUp(0) && bulletNum > 0){
                    //총알 무한상태가 아닐경우 잔여 총알 감소
                    if(!bulletInfinite)bulletNum--;
                    //사격 실행
                    BulletFire(this.transform.position,this.transform.rotation);
                    //더블샷 스킬을 통해 총알을 여러발이 한번에 나갈때, 추가 총알 수 만큼 더블 샷 발동
                    if(bulletDoubleShot!=0)StartCoroutine(BulletDoubleShot(bulletDoubleShot));
                }
            }
            //스킬 선택 시간일때
            else{//레이캐스트에 감지된 오브젝트가 스킬카드가 맞고, 신중한 선택을 위해 마우스 클릭에서 손을 땔때 스킬 적용
                if(Input.GetMouseButtonUp(0)&&hit.collider.tag=="card"){
                    //스킬 선택 시간동안 멈춰둔 시간 다시 재생
                    Time.timeScale=1;
                    //스킬 선택 상태 해제
                    makeSkillTime=false;
                    //선택한 스킬에 맞게 실행
                    switch (hit.collider.name){
                        case "bulletNumUp":
                            bulletMax+=25;
                            break;
                        case "bulletSizeUp":
                            bulletSizeUp=true;
                            bulletSize*=2.0f;
                            break;
                        case "bulletDoubleShot":
                            bulletDoubleShot++;
                            break; 
                        case "bulletPowerUp":
                            bulletDmg*=1.5f;
                            break;
                        case "OneHP":
                            GameObject life =Instantiate(skill_icon_prefab[0],new Vector3(0,0,0),infoui.rotation,infoui);
                            life.transform.localPosition=new Vector3(0.06f+0.06f*4,0.25f,0.65f);
                            life.name="life";
                            moreHP++;
                            break;
                        case "GodMode":
                            //선택한 스킬을 선택 후 스킬리스트에 추가
                            skill_icon.Add(skill_icon_prefab[1]);
                            skill.Add("GodMode");
                            break;                 
                        case "BulletInfinite":
                            skill_icon.Add(skill_icon_prefab[2]);
                            skill.Add("BulletInfinite");
                            break;
                        case "TimeSlow":
                            skill_icon.Add(skill_icon_prefab[3]);
                            skill.Add("TimeSlow");
                            break;
                        case "AllDirShot":
                            skill_icon.Add(skill_icon_prefab[4]);
                            skill.Add("AllDirShot");
                            break; 
                    }
                    //스킬 선택 후 스킬창 재구성을 위한 트리거 발동
                    skill_icon_bool=true;
                    //생성된 스킬카드 제거
                    GameObject[] card_dest=GameObject.FindGameObjectsWithTag("card");
                    for(int i=0;i<card_dest.Length;i++){
                        Destroy(card_dest[i]);
                    }
                    //스킬 선택을 위한 커서 비활성화
                    skillCursor.SetActive(false);
                    //스테이지 새로 시작으로 인한 총알 재장전
                    bulletNum = bulletMax;
                    //스킬선택동안 사라졌던 사격 UI 활성화
                    //스킬 선택을 위해 비활성화 했던 적 타격 콜라이더를 다시 활성화
                    GameObject.Find("SpawnSpot").GetComponent<BoxCollider>().enabled=true;
                    //플레이어 위치 재설정
                    StartCoroutine(mapCreator.Spawn());
                }
            }
        }
    }
//사격 기능 
void BulletFire(Vector3 pos,Quaternion rot){
        //오디오 재생
        audioSource.PlayOneShot(fireSound);
        //총알 생성 후 총알 생성 오브젝트의 위치에서 해당 오브젝트가 바라보는 방향으로 소환
        GameObject sub_bullet_inst = Instantiate(bullet, pos, rot);
        //총알 크기 변경 스킬 보유시 적용됨
        BulletSizeUp(sub_bullet_inst);
        //사격 파티클 생성
        GameObject ptc = Instantiate(particle, pos, rot);
        Destroy(ptc, 1.5f);
    }
    //일정 시간동안 스킬 없음 경고창 표시
    IEnumerator Caution(){
        caution_text.text="가지고있는 스킬이 없습니다.";
        caution.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        caution.SetActive(false);
        skill_reload = false;
    }
    //스킬 재사용 대기시간
    IEnumerator SkillReload(){
        Debug.Log("skill Reload");
        skill_reload_ON=true;
        while(click_delay>=0){
            Debug.Log(click_delay);
            click_delay -= Time.deltaTime;
            //yield return new WaitForSeconds(Time.deltaTime);
            yield return null;
        }
        skill_reload_ON=false;
        skill_reload=false;
        // yield return null;
    }
    //skill
    
   
    void BulletSizeUp(GameObject bullet){
        if (bulletSizeUp){
            Vector3 bulletScale = bullet.transform.lossyScale;//scale의 절대적인 값 저장
            bulletScale.x *= bulletSize;
            bulletScale.y *= bulletSize;
            bulletScale.z *= bulletSize;
            Transform parent = bullet.transform.parent;
            bullet.transform.parent = null;
            bullet.transform.localScale = bulletScale;
            bullet.transform.parent = parent;
        }
    }
    IEnumerator BulletDoubleShot(int shotlevel){
        for(int i=0;i<shotlevel;i++){
            yield return new WaitForSeconds(0.3f);
            BulletFire(this.transform.position,this.transform.rotation);
        } 

    }
    IEnumerator GodMode(){
        Color currentcolor=HPgaugeImage.color;
        godMode=true;
        HPgaugeImage.color=Color.yellow;
        yield return new WaitForSeconds(5.0f);
        HPgaugeImage.color=currentcolor;
        godMode=false;
 
    }
    IEnumerator BulletInfinite(){
        bulletInfinite=true;
        yield return new WaitForSeconds(5.0f);
        bulletInfinite=false;
    }

    IEnumerator TimeSlow(){
        Time.timeScale=0.1f;
        //필터 실행
        filter.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        Time.timeScale=1;
        filter.SetActive(false);
    }
    IEnumerator AllDirShot(){
        for(int i=0;i<5;i++){
            for(int j=0;j<24;j++)
                //해당 각만큼 돌리는게 아닌 해당 각으로 소환하기 위해 쿼터니언의 오일러각 함수 사용
                BulletFire(player.transform.position, Quaternion.Euler(0,15*j,0));
            yield return new WaitForSeconds(0.5f);
        }
    }

}
