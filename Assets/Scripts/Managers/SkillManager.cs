using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance {  get; private set; }
    private AudioSource audioSource;

    //[SerializeField]private AudioClip fireSound;
    [SerializeField]private GameObject bullet;
    [SerializeField]private GameObject firePtc;
    private float tempBulletSize;
    private int tempBullet;
    private float tempBulletReloadTime;
    private bool bulletReloading;
    private Vector3 bulletLegacySize;
    private int shootCnt;
    private Queue<GameObject> bulletPool = new Queue<GameObject>();
    private Queue<GameObject> firePtcPool = new Queue<GameObject>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 유지해야 한다면 추가
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        bulletReloading = false;
        tempBulletReloadTime = GameManager.Instance.GetBulletReloadTime();
        shootCnt = 1;
        audioSource = GetComponent<AudioSource>();
        tempBulletSize = GameManager.Instance.GetBulletSize();
        bulletLegacySize=bullet.transform.localScale;
        tempBullet = GameManager.Instance.GetBulletMax();
        UIManager.Instance.UpdateBulletUI(GameManager.Instance.GetBulletMax(), tempBullet);

    }
    private void Update()
    {
        if (tempBullet <= 0)
        {
            ReloadBullet();
        }
    }
    private void PoolingObj(Queue<GameObject> pool,GameObject obj)
    {
        if (pool.Count > 0)
        {
            GameObject reusable = pool.Dequeue();
            reusable.transform.position = this.transform.position;
            reusable.transform.rotation = this.transform.rotation;
            reusable.SetActive(true);
            reusable.transform.localScale = bulletLegacySize* tempBulletSize;
        }
        else
        {
            GameObject newBullet = Instantiate(obj, this.transform.position, this.transform.rotation);
            newBullet.transform.localScale = bulletLegacySize* tempBulletSize;
        }
    }
    public void InPool(GameObject obj)
    {
        switch (obj.tag)
        {
            case "bullet":
                bulletPool.Enqueue(obj);
                return;
            case "ptc":
                firePtcPool.Enqueue(obj);
                return;
            default:
                return;
        }
    }

    private void ShootBullet()
    {
        //audioSource.PlayOneShot(fireSound);
        tempBullet--;
        UIManager.Instance.UpdateBulletUI(GameManager.Instance.GetBulletMax(),tempBullet);
        PoolingObj(firePtcPool, firePtc);
        PoolingObj(bulletPool, bullet);
    }
    private IEnumerator ShootRoutine()
    {
        for (int i = 0; i < shootCnt; i++)
        {
            // 1. 먼저 한 발을 발사합니다.
            ShootBullet();

            // 2. 다음 발사를 위해 0.1초 동안 대기합니다.
            yield return new WaitForSeconds(0.1f);
        }
    }
    public void StartShooting()
    {
        StartCoroutine(ShootRoutine());
    }
    private IEnumerator ReloadTimerGaze()
    {
        float elapsedTime = 0f;

        while (elapsedTime < tempBulletReloadTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / tempBulletReloadTime;
            UIManager.Instance.UpdateReloadGaze(progress);
            yield return null;
        }
        tempBullet = GameManager.Instance.GetBulletMax();
        UIManager.Instance.UpdateBulletUI(GameManager.Instance.GetBulletMax(), tempBullet);
        if (bulletReloading) bulletReloading = false;
    }
    public void ReloadBullet()
    {
        bulletReloading=true;
        StartCoroutine(ReloadTimerGaze());
    }
    private void BulletMaxUp()
    {
        GameManager.Instance.AddBulletMax(25);
    }
    private void BulletSizeUp()
    {
        tempBulletSize *= 2f;
    }
    private void BulletDoubleShot()
    {
        shootCnt++;
    }
    public bool IsBulletReloading()
    {
        return bulletReloading;
    }
}
