using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Experimental.UIElements;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SocialPlatforms;
using static UnityEngine.UI.GridLayoutGroup;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }
    //맵 크기
    private int mapSize = 5;
    private float tileSize = 12;
    //맵 타일 관리 좌표
    private Vector3 hiddenPosition = new Vector3(-9999, -9999, -9999);
    //맵 타일 addressable 주소
    List<string> tileKeys = new List<string>()
    {
        "map_tile_01",
        "map_tile_02",
        "map_tile_03",
        "map_tile_04"
    };
    List<string> prefabKeys = new List<string>()
    {
        "map_corner",
        "map_wall",
        "map_door",
        "DroneSpawnPoint"
    };
    Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();
    //맵 타일 풀링 관리
    private Dictionary<string, List<GameObject>> mapTilePool = new Dictionary<string, List<GameObject>>();
    //맵 타일 생성 방향
    private enum Direction { TopLeft, TopRight, BottomLeft, BottomRight, Up, Down, Left, Right, Center }
    //맵 타일 생성 위치 및 회전 정보 struct
    public class TransformStruct
    {
        public Vector3 position;
        public Quaternion rotation;

        public TransformStruct(Vector3 pos, float yRotation)
        {
            position = pos;
            rotation = Quaternion.Euler(0, yRotation, 0); // Y축 오일러 각도
        }
    }
    //타일 생성 위치 및 회전 정보
    private readonly Dictionary<Direction, TransformStruct> tileTransform = new Dictionary<Direction, TransformStruct>()
    {
        { Direction.TopLeft, new TransformStruct(new Vector3(-1, 0, 1), -90f)},
        { Direction.TopRight, new TransformStruct(new Vector3(1, 0, 1),0f) },
        { Direction.BottomLeft, new TransformStruct(new Vector3(-1, 0, -1),-180f)},
        { Direction.BottomRight, new TransformStruct(new Vector3(1, 0, -1),90f) },

        { Direction.Up, new TransformStruct(new Vector3(0, 0, 1), 0f) }, // Up
        { Direction.Down, new TransformStruct(new Vector3(0, 0, -1), -180f) }, // Down
        { Direction.Left, new TransformStruct(new Vector3(-1, 0, 0), -90f) }, // Left
        { Direction.Right, new TransformStruct(new Vector3(1, 0, 0), 90f) }, // Right
        { Direction.Center, new TransformStruct(Vector3.zero, 0f) } // Center
    };

    private GameObject[] droneSpawnPoint = new GameObject[4];
    private GameObject map;
    private Vector3[] mapInnerTransform;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public IEnumerator PreloadPrefabs(System.Action onComplete = null)
    {
        prefabKeys.AddRange(tileKeys);

        foreach (string addr in prefabKeys)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(addr);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                prefabCache[addr] = handle.Result;
            }
            else
            {
                Debug.LogError($"[Addressable] Failed to load {addr}");
            }
        }

        onComplete?.Invoke(); // 프리팹 로딩 완료 시 콜백
    }
    void OnPrefabLoaded(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("Room prefab failed to load: " + handle.Status);
        }
    }
    private void SetInstance(GameObject instance, Vector3 localPoistion, Quaternion localRotation, Transform parent, string nameSuffix = "", string poolName=null)
    {
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPoistion;
        instance.transform.localRotation = localRotation;
        
        instance.name = instance.name.Replace("(Clone)", nameSuffix);
        if (poolName!=null)
        {
            mapTilePool[poolName].Add(instance);
        }
    }

    public void SetMapConfig(int mapSize, float tileSize)
    {
        this.mapSize = mapSize;
        this.tileSize = tileSize;
    }
    public GameObject[] GetDroneSpawnPoint()
    {
        return droneSpawnPoint;
    }
    //맵 초기 생성
    public void GenerateMap()
    {
        //초기 맵 생성 후 맵 변경시 변경 없는 코너 및 벽 타일 일부
        map = new GameObject("Map");
        map.transform.position = Vector3.zero;
        //타일 위치 지정을 위한 오프셋
        float offset = ((mapSize - 1) / 2f) * tileSize;

        //코너타일 생성
        for (int i = 0; i < 4; i++)
        {
            Direction corner = (Direction)i;
            var data = tileTransform[corner];
            SetInstance(Instantiate(prefabCache["map_corner"]), data.position * offset, data.rotation, map.transform, $"_{corner}");
        }
        //벽 타일 생성
        for (int i = 4; i < 8; i++)
        {
            int cnt = 0;
            Direction side = (Direction)i;
            var data = tileTransform[side];
            GameObject sideWall = new GameObject("side_wall_" + side);
            sideWall.transform.position = Vector3.zero;

            int doorIdx = (mapSize-2)/2;//문 위치를 중앙으로 고정
            for (int j = 0; j < (mapSize - 2); j++)
            {
                bool isCenter = doorIdx == j;
                string wallType = isCenter ? "map_door" : "map_wall";
                Vector3 localPosition = new Vector3((j - (mapSize - 3) / 2f) * tileSize, 0, 0);
                GameObject side_Map_Tile = Instantiate(prefabCache[wallType]);
                SetInstance(side_Map_Tile, localPosition, Quaternion.identity, sideWall.transform, $"_{side}");
                //문 위치에 드론 스폰 지점
                if (isCenter)
                {
                    GameObject SpawnPointInstance = Instantiate(prefabCache["DroneSpawnPoint"]);
                    SetInstance(SpawnPointInstance, Vector3.zero + Vector3.up * 3.65f, Quaternion.Euler(0,180f,0), side_Map_Tile.transform, $"_{side}");
                    droneSpawnPoint[cnt] = SpawnPointInstance;

                }
            }
            //벽 타일 위치 지정
            sideWall.transform.position = data.position * offset;
            sideWall.transform.rotation = data.rotation;
            sideWall.transform.SetParent(map.transform, false);
        }
        //내부 타일 위치값
        mapInnerTransform = new Vector3[(mapSize - 2) * (mapSize - 2)];
        //맵 타일 생성
        //최초 생성시에는 pool 사용 않고 바로 생성
        for (int i = 0; i < (mapSize - 2) * (mapSize - 2); i++)
        {
            //i=row * (mapSize - 2) + col;
            int row = i / (mapSize - 2);
            int col = i % (mapSize - 2);

            mapInnerTransform[i] = new Vector3((row - (mapSize - 3) / 2f) * tileSize, 0, (col - (mapSize - 3) / 2f) * tileSize);

            string tileAdrr = tileKeys[Random.Range(0, tileKeys.Count)];
            if (!mapTilePool.ContainsKey(tileAdrr))
            {
                mapTilePool[tileAdrr] = new List<GameObject>();
            }
            GameObject inner_Map_Tile = Instantiate(prefabCache[tileAdrr]);
            SetInstance(inner_Map_Tile, mapInnerTransform[i], Quaternion.identity, map.transform, $"_{i}", tileAdrr);
        }
    }
    public void refreshMap()
    {
        //맵 초기화
        foreach (var mapTileAddr in mapTilePool)
        {
            foreach(var mapTile in mapTileAddr.Value)
            {
                mapTile.SetActive(false);
            }
        }
        for (int i=0;i< (mapSize - 2) * (mapSize - 2); i++)
        {
            string tileAdrr = tileKeys[Random.Range(0, tileKeys.Count)];
            if (!mapTilePool.ContainsKey(tileAdrr))
            {
                mapTilePool[tileAdrr] = new List<GameObject>();
            }
            GameObject reusable = mapTilePool[tileAdrr].Find(t => t != null && !t.activeInHierarchy);
            if (reusable != null)
            {
                reusable.transform.position = mapInnerTransform[i];
                reusable.SetActive(true);
            }
            else
            {
                GameObject inner_Map_Tile = Instantiate(prefabCache[tileAdrr]);
                SetInstance(inner_Map_Tile, mapInnerTransform[i], Quaternion.identity, map.transform, $"_{i}", tileAdrr);
            }
        }
    }
    

    ////드론 스폰지점 생성 타일일때 생성
    //if (enemySpawnOn)
    //{
    //    //스폰지점 관리를 위한 배열화
    //    enemySpawns[enemySpawnCnt] = Instantiate(enemySpawn, new Vector3(0, 0, 0), Quaternion.identity);
    //    //현재 타일 아래로 귀속
    //    enemySpawns[enemySpawnCnt].transform.parent = maps[i, j].transform;
    //    //위치 조정
    //    enemySpawns[enemySpawnCnt].transform.localPosition = new Vector3(0, 3.65f, 0);
    //    //스폰지점 갯수(인덱스) 관리
    //    enemySpawnCnt++;
    //    enemySpawnOn = false;
    //}
    //            }
    //        }//플레이어 위치 지정을 위한 랜덤 좌표
    //        parentI = Random.Range(1, 4);
    //parentJ = Random.Range(1, 4);
    ////플레이어 위치 지정 후 해당 타일을 부모로 설정
    //this.transform.parent = maps[parentI, parentJ].transform;
    ////위치 조정
    //this.transform.localPosition = new Vector3(0, 0, 0);
    ////플레이어 위치 조정
    //player.transform.parent = this.transform;
    //player.transform.localPosition = new Vector3(0, 15, 0);
    //player.transform.parent = null;
    ////플레이어 회전 축 고정(타격으로 인한 회전 방지)
    //player_rig.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    ////스킬 카드 소환
    //makeSkill();
    ////물리엔진 영향으로부터 제외
    //player_rig.isKinematic = true;

    //    }
}
