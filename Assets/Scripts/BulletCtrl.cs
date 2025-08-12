using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCtrl : MonoBehaviour
{
    // Update is called once per frame
    private void Awake()
    {
        this.gameObject.SetActive(true);
    }
    void Update()
    {
        //매 프레임마다 갱신
        //총알이 바라보는 방향으로 설정한 속도만큼 매 초마다 이동
        this.transform.position += transform.forward* GameManager.Instance.GetBulletSpeed() * Time.deltaTime;
        
    }
    //타격 이벤트 설정
    private void OnTriggerEnter(Collider other) {
        //만약 타격한 물체의 태그가 drone일때,
        if(other.transform.CompareTag("drone")|| other.transform.CompareTag("wall"))
            this.gameObject.SetActive(false);
    }
}
