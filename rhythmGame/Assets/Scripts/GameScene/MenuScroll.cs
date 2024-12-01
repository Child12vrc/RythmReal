using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class MenuScroll : MonoBehaviour
{
    [Header("����")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private RhythmGameManager gameManager;
    public GameObject[] menuItems;
    public Transform uiPivot;
    public float verticalSpacing = 2.0f;
    public List<Vector3> menuPos = new List<Vector3>();

    [Header("�̵� ����")]
    [SerializeField] private float moveTime = 0.2f;        // �̵� �ð�
    [SerializeField] private Ease moveEase = Ease.OutQuad; // �̵� �ִϸ��̼� Ŀ��

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

        // �ӽ� ����
        GameObject tempItem = menuItems[0];

        // ������ �迭 ������Ʈ
        for (int i = 0; i < menuItems.Length - 1; i++)
        {
            menuItems[i] = menuItems[i + 1];
        }
        menuItems[menuItems.Length - 1] = tempItem;

        // ��� �������� �� ��ġ�� �̵�
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

        // �ӽ� ����
        GameObject tempItem = menuItems[menuItems.Length - 1];

        // ������ �迭 ������Ʈ
        for (int i = menuItems.Length - 1; i > 0; i--)
        {
            menuItems[i] = menuItems[i - 1];
        }
        menuItems[0] = tempItem;

        // ��� �������� �� ��ġ�� �̵�
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