using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
    // Start is called before the first frame update
    //외부에서 bullet속도 변경가능
    public float bulletSpeed= 50.0f;
    //외부에서 방향 설정 가능
    //public int dir;
    //bullet 타격 파티클
    public GameObject hitParticle;
    //외부 접근 불가, 동일 프로젝트의 다른 스크립트 호출하여 선언.
    GunFire gunFire;
    
    //해당 스크립트 컴포넌트가 적용된 엔티티가 생성될때 실행
    void Start()
    {
        //bullet의 속도를 50으로 설정
        //bulletSpeed = 50.0f;
        //총알 발사 관리 오브젝트
        gunFire=GameObject.Find("bullet_spawn").GetComponent<GunFire>();
        //리소스 관리를 위해 발사 4초 후 제거
        Destroy(this.gameObject, 4.0f);
    }

    // Update is called once per frame
    void Update()
    {
        //매 프레임마다 갱신
        //총알이 바라보는 방향으로 설정한 속도만큼 매 초마다 이동
        this.transform.position += transform.forward* bulletSpeed * Time.deltaTime;
        
    }
    //타격 이벤트 설정
    private void OnTriggerEnter(Collider other) {
        //만약 타격한 물체의 태그가 drone일때,
        if(other.gameObject.CompareTag("drone")){
            //타격한 드론의 가장 아래차일드(MoveDrone)의 DroneCtrl컴포넌트 호출(실제 드론 생성시 가장 아래에 생성되게 구성됨)
            DroneCtrl dronCtrl=other.transform.GetComponent<DroneCtrl>();
            //타격되었을때 식별할 수 있도록 파티클 호출
            GameObject hit_ptc = Instantiate(hitParticle,this.transform.position,Quaternion.identity);
            //파티클 바로 제거
            Destroy(hit_ptc,0.5f);
            //타격된 드론의 체력을 총알 데미지만큼 감소
            dronCtrl.loseHP(gunFire.bulletDmg);
            //dronCtrl.HP-=gunFire.bulletDmg;
            //드론의 잔여 체력을 확인할 수 있도록 체력바 띄우기
            //dronCtrl.hpFlot=true;
            //총알이 타격되었으므로 리소스 관리를위해 바로 제거
            Destroy(this.gameObject);
        }
        else if(other.gameObject.CompareTag("wall")){
            //벽에 부딫힌 경우도 제거
            Debug.Log("wall");
            Destroy(this.gameObject);
        } else{
            //만약 벽도, 적도 아닌곳에 도달하는 경우 디버깅을 위해 로깅
            Debug.Log("name:"+other.gameObject.name+", tag:"+other.gameObject.tag);
        }  
    }
}
