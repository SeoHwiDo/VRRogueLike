using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
//using UnityEngine.AddressableAssets;
//using UnityEngine.ResourceManagement.AsyncOperations;


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
public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    //맵 크기
    private int mapSize = 5;
    private float tileSize = 12;
    //맵 프리팹 관리
    [SerializeField] private MapConfig stage01;
    [SerializeField] private CommonMapConfig commonMapPrefab;
    //맵 타일 생성 방향
    private enum Direction { TopLeft, TopRight, BottomLeft, BottomRight, Up, Down, Left, Right, Center }

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

    //맵 타일 풀링 관리
    private Dictionary<string, List<GameObject>> mapTilePool = new Dictionary<string, List<GameObject>>();

    private GameObject[] enemySpawnPoint = new GameObject[4];

    private GameObject map;

    private Vector3[] mapInnerTransform;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

    }

    private void SetInstance(GameObject instance, Vector3 localPoistion, Quaternion localRotation, Transform parent, string nameSuffix = "", string poolName = null)
    {
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPoistion;
        instance.transform.localRotation = localRotation;

        instance.name = instance.name.Replace("(Clone)", nameSuffix);
        if (poolName != null)
        {
            mapTilePool[poolName].Add(instance);
        }
    }
    //외부 스크립트용 getter,setter

    public void SetMapConfig(int mapSize, float tileSize)
    {
        this.mapSize = mapSize;
        this.tileSize = tileSize;
    }
    public GameObject[] GetEnemySpawnPoint()
    {
        return enemySpawnPoint;
    }
    public Vector3 GetPlayerStartPosition()
    {
        if (mapInnerTransform!=null) return mapInnerTransform[ Random.Range(0, mapInnerTransform.Length)];
        else return Vector3.zero;
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
            SetInstance(Instantiate(stage01.cornerPrefab), data.position * offset, data.rotation, map.transform, $"_{corner}");
        }
        //벽 타일 생성
        int cnt = 0;
        for (int i = 4; i < 8; i++)
        {
            Direction side = (Direction)i;
            var data = tileTransform[side];
            GameObject sideWall = new GameObject("side_wall_" + side);
            sideWall.transform.position = Vector3.zero;

            int doorIdx = (mapSize-2)/2;//문 위치를 중앙으로 고정
            for (int j = 0; j < (mapSize - 2); j++)
            {
                bool isCenter = doorIdx == j;
                GameObject wallType = isCenter ? stage01.wallPrefab : stage01.wallPrefab;
                GameObject side_Map_Tile = Instantiate(wallType);

                Vector3 localPosition = new Vector3((j - (mapSize - 3) / 2f) * tileSize, 0, 0);
                SetInstance(side_Map_Tile, localPosition, Quaternion.identity, sideWall.transform, $"_{side}");
                //문 위치에 드론 스폰 지점
                if (isCenter)
                {
                    GameObject SpawnPointInstance = Instantiate(commonMapPrefab.enemySpawnPointPrefab);
                    SetInstance(SpawnPointInstance, Vector3.zero + Vector3.up * 3.65f, Quaternion.Euler(0,180f,0), side_Map_Tile.transform, $"_{side}");
                    if (cnt < enemySpawnPoint.Length)
                    {
                        enemySpawnPoint[cnt] = SpawnPointInstance;
                        cnt++;
                    }
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
            if (stage01.innerTilePrefab == null || stage01.innerTilePrefab.Length == 0)
            {
                Debug.LogError("No map tiles found with label 'MapTile'");
                continue;
            }
            var prefab = stage01.innerTilePrefab[Random.Range(0, stage01.innerTilePrefab.Length)];
            if (!mapTilePool.ContainsKey(prefab.name))
            {
                mapTilePool[prefab.name] = new List<GameObject>();
            }
            GameObject inner_Map_Tile = Instantiate(prefab);
            SetInstance(inner_Map_Tile, mapInnerTransform[i], Quaternion.identity, map.transform, $"_{i}", prefab.name);
        }
    }
    public void refreshMap()
    {
        //맵 초기화
        foreach (var mapTileAddr in mapTilePool)
        {
            foreach (var mapTile in mapTileAddr.Value)
            {
                mapTile.SetActive(false);
            }
        }
        for (int i = 0; i < (mapSize - 2) * (mapSize - 2); i++)
        {
            var prefab = stage01.innerTilePrefab[Random.Range(0, stage01.innerTilePrefab.Length)];
            //생성할 타일 종류의 pool리스트가 존재하는지 확인
            if (!mapTilePool.ContainsKey(prefab.name))
            {
                mapTilePool[prefab.name] = new List<GameObject>();
            }
            GameObject reusable = mapTilePool[prefab.name].Find(t => t != null && !t.activeInHierarchy);
            if (reusable != null)
            {
                //타일 재활용이 가능한 경우
                reusable.transform.position = mapInnerTransform[i];
                reusable.SetActive(true);
            }
            else
            {
                //타일 재활용이 불가능한 경우 새로 소환 후 pool에 등록
                GameObject inner_Map_Tile = Instantiate(prefab);
                SetInstance(inner_Map_Tile, mapInnerTransform[i], Quaternion.identity, map.transform, $"_{i}", prefab.name);
            }
        }
    }

}
#if UNITY_EDITOR


[CustomEditor(typeof(MapManager))]
public class MapManagerEditor : Editor
{
    private bool showPoolFoldout = true;
    private readonly Dictionary<string, bool> keyFoldouts = new Dictionary<string, bool>();
    private Vector2 poolScroll;

    public override void OnInspectorGUI()
    {
        // 기본 인스펙터
        DrawDefaultInspector();

        var map = (MapManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Map Controls", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            
            if (GUILayout.Button("Refresh Map"))
            {
                map.refreshMap();
            }
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("풀 상태는 Play 모드에서 가장 정확하게 확인할 수 있습니다.", MessageType.Info);
        }

        EditorGUILayout.Space();
        showPoolFoldout = EditorGUILayout.Foldout(showPoolFoldout, "Map Tile Pool (목록 보기)", true);

        if (!showPoolFoldout) return;

        // private 필드 mapTilePool 접근
        var poolField = typeof(MapManager).GetField("mapTilePool",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (poolField == null)
        {
            EditorGUILayout.HelpBox("mapTilePool 필드를 찾을 수 없습니다.", MessageType.Warning);
            return;
        }

        var poolDict = poolField.GetValue(map) as Dictionary<string, List<GameObject>>;
        if (poolDict == null || poolDict.Count == 0)
        {
            EditorGUILayout.HelpBox("현재 풀에 항목이 없습니다.", MessageType.Info);
            return;
        }

        // 스크롤 영역 (너무 많을 때 대비)
        EditorGUILayout.BeginVertical(GUI.skin.box);
        poolScroll = EditorGUILayout.BeginScrollView(poolScroll, GUILayout.MaxHeight(720));

        foreach (var kvp in poolDict)
        {
            string key = kvp.Key;
            var list = kvp.Value;

            if (!keyFoldouts.ContainsKey(key)) keyFoldouts[key] = false;
            keyFoldouts[key] = EditorGUILayout.Foldout(keyFoldouts[key], $"{key}  (총 {list?.Count ?? 0}개)", true);

            if (!keyFoldouts[key]) continue;

            EditorGUI.indentLevel++;
            if (list == null || list.Count == 0)
            {
                EditorGUILayout.LabelField("비어 있음");
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var go = list[i];

                    EditorGUILayout.BeginHorizontal();

                    // 오브젝트 필드 (씬 개체로 핑/선택 가능)
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField($"[{i}]", go, typeof(GameObject), true);
                    EditorGUI.EndDisabledGroup();

                    // 상태 표시 (Active/Inactive)
                    string state;
                    Color col;
                    if (go == null)
                    {
                        state = "Null";
                        col = Color.red;
                    }
                    else if (go.activeInHierarchy)
                    {
                        state = "Active";
                        col = Color.green;
                    }
                    else
                    {
                        state = "Inactive";
                        col = Color.gray;
                    }

                    var stateStyle = new GUIStyle(EditorStyles.miniBoldLabel);
                    stateStyle.normal.textColor = col;
                    GUILayout.Label(state, stateStyle, GUILayout.Width(70));

                    // 선택/핑 버튼
                    //if (go != null)
                    //{
                    //    if (GUILayout.Button("Select", GUILayout.Width(56)))
                    //    {
                    //        Selection.activeObject = go;
                    //    }
                    //    if (GUILayout.Button("Ping", GUILayout.Width(46)))
                    //    {
                    //        EditorGUIUtility.PingObject(go);
                    //    }

                    //    // 런타임에서 토글로 활성/비활성 변경
                    //    using (new EditorGUI.DisabledScope(!Application.isPlaying))
                    //    {
                    //        bool newActive = EditorGUILayout.Toggle(go.activeSelf, GUILayout.Width(18));
                    //        if (newActive != go.activeSelf)
                    //        {
                    //            go.SetActive(newActive);
                    //            // 씬 갱신
                    //            EditorUtility.SetDirty(go);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    GUILayout.Space(56 + 46 + 18); // 버튼/토글 자리 맞추기
                    //}

                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
}
#endif
