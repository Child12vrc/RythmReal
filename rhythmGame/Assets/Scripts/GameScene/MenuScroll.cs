using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class MenuScroll : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private RhythmGameManager gameManager;
    public GameObject[] menuItems;
    public Transform uiPivot;
    public float verticalSpacing = 2.0f;
    public List<Vector3> menuPos = new List<Vector3>();

    [Header("이동 설정")]
    [SerializeField] private float moveTime = 0.2f;        // 이동 시간
    [SerializeField] private Ease moveEase = Ease.OutQuad; // 이동 애니메이션 커브

    private bool isMoving = false;
    public int currentTopIndex = 0;

    private void Start()
    {
        InitializeMenuItems();
    }

    private void Update()
    {
        if (!isMoving)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ScrollUp();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ScrollDown();
            }
        }
    }

    private void InitializeMenuItems()
    {
        int itemCount = gameManager.availableTracks.Count;
        menuItems = new GameObject[itemCount];
        menuPos.Clear();

        for (int i = 0; i < itemCount; i++)
        {
            Vector3 position = new Vector3(0.25f, -i * verticalSpacing, 0.0f);
            menuPos.Add(position);
            menuItems[i] = Instantiate(itemPrefab, uiPivot);
            menuItems[i].transform.localPosition = position;

            TrackMenuItem trackMenu = menuItems[i].GetComponent<TrackMenuItem>();
            trackMenu.index = i;
            trackMenu.trackText.text = gameManager.availableTracks[i].trackName;
            trackMenu.albumArt = gameManager.availableTracks[i].albumArt;
            trackMenu.previewAudioSource = gameManager.availableTracks[i].audioClip;
        }
    }

    private void ScrollUp()
    {
        if (isMoving) return;
        isMoving = true;

        // 임시 저장
        GameObject tempItem = menuItems[0];

        // 아이템 배열 업데이트
        for (int i = 0; i < menuItems.Length - 1; i++)
        {
            menuItems[i] = menuItems[i + 1];
        }
        menuItems[menuItems.Length - 1] = tempItem;

        // 모든 아이템을 새 위치로 이동
        int moveCount = 0;
        for (int i = 0; i < menuItems.Length; i++)
        {
            menuItems[i].transform.DOLocalMove(menuPos[i], moveTime)
                .SetEase(moveEase)
                .OnComplete(() => {
                    moveCount++;
                    if (moveCount >= menuItems.Length)
                    {
                        isMoving = false;
                    }
                });
        }

        currentTopIndex = (currentTopIndex + 1) % menuItems.Length;
    }

    private void ScrollDown()
    {
        if (isMoving) return;
        isMoving = true;

        // 임시 저장
        GameObject tempItem = menuItems[menuItems.Length - 1];

        // 아이템 배열 업데이트
        for (int i = menuItems.Length - 1; i > 0; i--)
        {
            menuItems[i] = menuItems[i - 1];
        }
        menuItems[0] = tempItem;

        // 모든 아이템을 새 위치로 이동
        int moveCount = 0;
        for (int i = 0; i < menuItems.Length; i++)
        {
            menuItems[i].transform.DOLocalMove(menuPos[i], moveTime)
                .SetEase(moveEase)
                .OnComplete(() => {
                    moveCount++;
                    if (moveCount >= menuItems.Length)
                    {
                        isMoving = false;
                    }
                });
        }

        currentTopIndex = (currentTopIndex - 1 + menuItems.Length) % menuItems.Length;
    }
}