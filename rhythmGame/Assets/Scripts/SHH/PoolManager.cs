using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Pool
{
    public string tag;              // pool을 구분할 tag
    public Transform poolObject;   // 비활성화된 오브젝트들을 모아둘곳
    public GameObject prefab;       // 저장할 오브젝트
    public int size;                // pool의 최대 사이즈
    public Queue<GameObject> gameObjects = new Queue<GameObject>();
}

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    [SerializeField] private List<Pool> pools = new List<Pool>();                      // 시작전 pool들을 사전에 만들 경우 사용할 List

    public Dictionary<string, Pool> poolDictionary;    // pool을 찾기 편하게 Dictionary

    void Start()
    {
        poolDictionary = new Dictionary<string, Pool>();   // 딕셔너리 생성

        foreach (Pool pool in pools)                                    // List에 있는 풀들을 생성함
        {
            CreatePool(pool);
        }
    }
    /// <summary>
    /// 풀을 생성하는 함수
    /// </summary>
    /// <param name="pool">풀을 받으면 오브젝트를 생성한다</param>
    private void CreatePool(Pool pool)
    {
        Queue<GameObject> objectPool = new Queue<GameObject>();         // FIFO (먼저 들어온애를 먼저 꺼냄) 배운거 써보고 싶어서 사용해봄

        GameObject parentObject = new GameObject($"@{pool.tag}_Pool");
        pool.poolObject = parentObject.transform;

        for (int i = 0; i < pool.size; i++)                             // 먼저 입력해둔 사이즈 만큼
        {
            GameObject obj = Instantiate(pool.prefab);                  // 오브젝트를 생성
            obj.name = pool.tag;                                        // 오브젝트의 이름 변경
            obj.SetActive(false);                                       // 비활성화 시킴
            obj.transform.parent = pool.poolObject;                     // 저장공간에 넣어둠
            objectPool.Enqueue(obj);                                    // Queue에 추가함
        }

        pool.gameObjects = objectPool;                                  // pool 클래스의 Queue를 바꿔줌

        poolDictionary.Add(pool.tag, pool);                             // Dictionary에 추가함
    }
    /// <summary>
    /// 풀을 생성한다
    /// </summary>
    /// <param name="tag">풀의 테그</param>
    /// <param name="prefab">생성할 오브젝트</param>
    /// <param name="size">풀의 크기(비워두면 10)</param>
    public void AddNewPool(string tag, GameObject prefab, int size = 10)            // 게임 중 풀을 만들 경우 (기본 사이즈는 10)
    {
        if (poolDictionary.ContainsKey(tag))                                         // 만약 해당 테그의 풀이 존재할 경우
        {
            Debug.LogWarning($"{tag}를 가진 pool이 이미 존재 합니다.");               // 에러 메세지 출력
            return;
        }

        Pool newPool = new Pool { tag = tag, prefab = prefab, size = size };
        pools.Add(newPool);                                                         // 리스트에도 추가
        CreatePool(newPool);                                                        // 풀을 생성
    }
    /// <summary>
    /// 풀의 사이즈를 키운다
    /// </summary>
    /// <param name="tag">사이즈를 키울 풀의 테그</param>
    /// <param name="additionalSize">늘릴 양</param>
    public void ReSizePool(string tag, int additionalSize)                          // 풀의 사이즈를 늘릴 경우
    {
        Pool pool = poolDictionary[tag];                            // tag를 지닌 풀을 가져옴
        pool.size += additionalSize;                                // 사이즈를 늘림

        Queue<GameObject> objectPool = new Queue<GameObject>();     // Queue를 새로 만듬

        for (int i = 0; i < additionalSize; i++)                    // 늘린 사이즈 만큼 오브젝트를 생성해서 Queue에 추가함
        {
            GameObject obj = Instantiate(pool.prefab);
            obj.name = pool.tag;
            obj.SetActive(false);
            obj.transform.parent = pool.poolObject;
            objectPool.Enqueue(obj);
        }

        foreach (var obj in pool.gameObjects)                        // 이후 기존 Queue에 있는 오브젝트들을 추가함
        {
            objectPool.Enqueue(obj);
        }

        pool.gameObjects = objectPool;                              // 새로 만든 Queue로 바꿈
    }
    /// <summary>
    /// 풀에 있는 오브젝트를 생성한다
    /// </summary>
    /// <param name="tag">생성할 오브젝트의 tag</param>
    /// <param name="position">생성할 위치</param>
    /// <param name="rotation">생성시 회전</param>
    /// <returns>생성된 오브젝트를 리턴함</returns>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))                       // 만약 tag를 가진 pool이 존재하지 않을 경우
        {
            Debug.LogWarning($"{tag}의 풀이 없습니다!");             // 경고 메세지 출력
            return null;
        }

        Pool pool = poolDictionary[tag];                            // tag를 지닌 풀을 가져옴

        GameObject objectToSpawn = pool.gameObjects.Dequeue();      // 해당 테그를 가진 Queue에서 오브젝트를 꺼내옴

        if (objectToSpawn.activeSelf == true)                        // 만약 찾아온 오브젝트가 활성화된 상태일 경우
        {
            ReSizePool(tag, pool.size * 2);                         // 풀 사이즈 두배로 늘림
            objectToSpawn = pool.gameObjects.Dequeue();             // Queue에서 오브젝트를 다시 꺼내옴
        }

        objectToSpawn.SetActive(true);                              // 오브젝트를 활성화 시킴
        objectToSpawn.transform.parent = null;                      // 저장 공간에서 뺌
        objectToSpawn.transform.position = position;                // 위치를 이동
        objectToSpawn.transform.rotation = rotation;                // 물체를 회전

        pool.gameObjects.Enqueue(objectToSpawn);                    // Queue 마지막으로 이동 시킴

        return objectToSpawn;                                       // 생성한 오브젝트를 반환함
    }
    /// <summary>
    /// 오브젝트를 다시 비활성화 하는 함수
    /// </summary>
    /// <param name="obj">비활성화 할 오브젝트를 여기에 넣으면 된다.</param>
    public void ReturnToPool(GameObject obj)                        // 오브젝트를 비 활성화 하는 함수
    {
        obj.transform.parent = poolDictionary[obj.name].poolObject;
        obj.SetActive(false);
    }

    public bool HasThisPool(string tag)                             // 해당 tag의 pool이 존재하는지 반환하는 함수(있어도 없어도 그만인 함수를 왜 만들었을까?)
    {
        return poolDictionary.ContainsKey(tag);
    }
}
