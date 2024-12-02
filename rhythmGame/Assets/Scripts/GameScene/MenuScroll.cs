using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class MenuScroll : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private RhythmGameManager gameManager;
    [SerializeField] private RhythmGameController gameController;
    [SerializeField] private Material albumMaterial;
    public GameObject[] menuItems;
    public Transform uiPivot;
    public float verticalSpacing = 2.0f;
    public List<Vector3> menuPos = new List<Vector3>();

    [Header("이동 설정")]
    [SerializeField] private float moveTime = 0.2f;
    [SerializeField] private float zOffset = 2.0f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;

    private bool isMoving = false;
    private bool isSetup = false;
    public int currentTopIndex = 0;
    private Vector3 originalPosition;
    private AudioSource previewAudioSource;
    public CameraPositionController cameraPositionController;

    private void Start()
    {
        InitializeMenuItems();
        previewAudioSource = gameObject.AddComponent<AudioSource>();
        previewAudioSource.playOnAwake = false;
    }

    private void Update()
    {
        if (!isMoving)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SetupMenu();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && isSetup)
            {
                ReturnToOriginalPosition();
                StopPreview();
            }
            else if (Input.GetKeyDown(KeyCode.Return) && isSetup)
            {
                StartGame();
            }
            else if (!isSetup)
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
    }

    private void StartGame()
    {
        StopPreview();
        cameraPositionController.MoveCameraToPosition(2);
        gameController.StartGame(currentTopIndex);
        
        gameObject.SetActive(false);
    }

    public void ReturnToMenu()
    {
        gameObject.SetActive(true);
    }

    private void PlayPreview()
    {
        StopPreview();
        TrackMenuItem selectedItem = menuItems[0].GetComponent<TrackMenuItem>();
        if (selectedItem != null && selectedItem.previewAudioSource != null)
        {
            previewAudioSource.clip = selectedItem.previewAudioSource;
            previewAudioSource.time = 0;
            previewAudioSource.Play();
        }
    }

    private void StopPreview()
    {
        if (previewAudioSource.isPlaying)
        {
            previewAudioSource.Stop();
        }
    }

    private void SetupMenu()
    {
        if (isMoving || isSetup) return;
        isMoving = true;

        originalPosition = menuItems[0].transform.localPosition;

        // Base Map과 Emission Map 모두 설정
        TrackMenuItem selectedItem = menuItems[0].GetComponent<TrackMenuItem>();
        if (selectedItem != null && albumMaterial != null)
        {
            albumMaterial.SetTexture("_BaseMap", selectedItem.albumArt);
            albumMaterial.SetTexture("_EmissionMap", selectedItem.albumArt);
            albumMaterial.EnableKeyword("_EMISSION"); // Emission 활성화
        }

        menuItems[0].transform.DOLocalMove(
            originalPosition + new Vector3(0, 0, zOffset),
            moveTime
        ).SetEase(moveEase)
        .OnComplete(() => {
            isMoving = false;
            isSetup = true;
            PlayPreview();
        });
    }

    private void ReturnToOriginalPosition()
    {
        if (isMoving || !isSetup) return;
        isMoving = true;

        // Base Map과 Emission Map 모두 초기화
        if (albumMaterial != null)
        {
            albumMaterial.SetTexture("_BaseMap", null);
            albumMaterial.SetTexture("_EmissionMap", null);
            albumMaterial.DisableKeyword("_EMISSION"); // Emission 비활성화
        }

        menuItems[0].transform.DOLocalMove(
            originalPosition,
            moveTime
        ).SetEase(moveEase)
        .OnComplete(() => {
            isMoving = false;
            isSetup = false;
        });
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

        GameObject tempItem = menuItems[0];
        for (int i = 0; i < menuItems.Length - 1; i++)
        {
            menuItems[i] = menuItems[i + 1];
        }
        menuItems[menuItems.Length - 1] = tempItem;

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

        GameObject tempItem = menuItems[menuItems.Length - 1];
        for (int i = menuItems.Length - 1; i > 0; i--)
        {
            menuItems[i] = menuItems[i - 1];
        }
        menuItems[0] = tempItem;

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