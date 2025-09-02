using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AssetManager : MonoBehaviour
{
    public static AssetManager Instance {  get; private set; }
    [SerializeField] private string[] labels = { "Map","MapTile", "Player", "Enemy",  "Ptc" };

    private readonly Dictionary<string,GameObject> prefabCache = new Dictionary<string,GameObject>();
    private readonly Dictionary<string, List<GameObject>> labelCache = new Dictionary<string, List<GameObject>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    public IEnumerator AssetPreload(System.Action onComplete = null)
    {
        prefabCache.Clear();
        labelCache.Clear();
        // Addressables 초기화 (최초 호출 히치 방지용)
        var init = Addressables.InitializeAsync();
        yield return init;

        // 라벨별 GameObject 전부 로드
        foreach (var label in labels.Distinct())
        {
            var handle = Addressables.LoadAssetsAsync<GameObject>(
                label,
                go =>{},
                true
            );
            yield return handle;

            if(!labelCache.ContainsKey(label)) labelCache[label] = new List<GameObject>();
            foreach (var go in handle.Result)
            {
                if (go == null) continue;

                // 라벨 캐시 추가
                labelCache[label].Add(go);

                // 프리팹 캐시에 이름으로 추가
                if (!prefabCache.ContainsKey(go.name))
                    prefabCache[go.name] = go;
                else
                    Debug.LogWarning($"[AssetPreload] Duplicate prefab name '{go.name}' in label '{label}'");
            }
        }
            onComplete?.Invoke(); // 프리팹 로딩 완료 시 콜백
    }
    public Dictionary<string, GameObject> GetPrefabCache()
    {
        return prefabCache;
    }
    public List<GameObject> GetLabelCache(string label)
    {
        if (labelCache.TryGetValue(label, out var list)) return list;
        Debug.LogWarning($"[Preload] No prefabs for label {label}");
        return null;
    }
}
