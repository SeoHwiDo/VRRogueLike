using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance {  get; private set; }
    private AudioSource audioSource;
    private AudioClip fireSound;
  
    [SerializeField]private GameObject Bullet;
    [SerializeField]private GameObject FirePtc;
    private Queue<GameObject> BulletPool = new Queue<GameObject>();
    private Queue<GameObject> FirePtcPool = new Queue<GameObject>();
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        fireSound = Resources.Load<AudioClip>("Sounds/FireSound");
        audioSource = GetComponent<AudioSource>();
    }
    private void PoolingObj(Queue<GameObject> pool,GameObject obj)
    {
        if (pool.Count > 0)
        {
            GameObject reusable = pool.Dequeue();
            reusable.transform.position = this.transform.position;
            reusable.transform.rotation = this.transform.rotation;
            reusable.SetActive(true);
        }
        else
        {
            GameObject newBullet = Instantiate(obj, this.transform.position, this.transform.rotation);
        }
    }
    public void InPool(GameObject obj)
    {
        switch (obj.tag)
        {
            case "bullet":
                BulletPool.Enqueue(obj);
                return;
            case "ptc":
                FirePtcPool.Enqueue(obj);
                return;
            default:
                return;
        }
    }
    
    public void ShootBullet()
    {
        audioSource.PlayOneShot(fireSound);
        PoolingObj(FirePtcPool, FirePtc);
        PoolingObj(BulletPool, Bullet);
    }
}
