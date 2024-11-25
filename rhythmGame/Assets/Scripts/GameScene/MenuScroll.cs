using UnityEngine;
using System.Collections;
using TMPro;

public class MenuScroll : MonoBehaviour
{
    [SerializeField] private Transform[] menuItems = new Transform[8];
    [SerializeField] private TextMeshPro[] trackTexts;
    [SerializeField] private float verticalSpacing = 2f;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float selectZOffset = 2f;
    [SerializeField] private RhythmGameManager gameManager;

    private bool isMoving = false;
    private float baseY;
    private float maxY;
    private Transform selectedItem = null;
    private float selectedOriginalZ;
    private int currentIndex = 0;

    private void Start()
    {
        baseY = menuItems[0].localPosition.y;
        maxY = baseY + (verticalSpacing * 7);
        UpdateTrackList();
    }

    private void UpdateTrackList()
    {
        if (gameManager == null || gameManager.availableTracks == null) return;

        for (int i = 0; i < menuItems.Length; i++)
        {
            int trackIndex = (currentIndex + i) % gameManager.availableTracks.Count;
            if (trackTexts[i] != null)
            {
                var track = gameManager.availableTracks[trackIndex];
                trackTexts[i].text = $"{track.name} - {track.bpm}BPM";
            }
        }
    }

    private void Update()
    {
        if (isMoving) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ResetSelection();
            MoveItems(1);
            currentIndex = (currentIndex - 1 + gameManager.availableTracks.Count) % gameManager.availableTracks.Count;
            UpdateTrackList();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ResetSelection();
            MoveItems(-1);
            currentIndex = (currentIndex + 1) % gameManager.availableTracks.Count;
            UpdateTrackList();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SelectThirdFromTop();
        }
        else if (Input.GetKeyDown(KeyCode.Return) && selectedItem != null)
        {
            SelectTrack();
        }
    }

    private void SelectTrack()
    {
        if (selectedItem == null) return;

        int selectedIndex = System.Array.IndexOf(menuItems, selectedItem);
        if (selectedIndex >= 0)
        {
            int trackIndex = (currentIndex + selectedIndex) % gameManager.availableTracks.Count;
            gameManager.ChangeTrack(trackIndex);
        }
    }

    private void SelectThirdFromTop()
    {
        if (isMoving) return;

        ResetSelection();

        float[] yPositions = new float[menuItems.Length];
        for (int i = 0; i < menuItems.Length; i++)
        {
            yPositions[i] = menuItems[i].localPosition.y;
        }
        System.Array.Sort(yPositions);
        System.Array.Reverse(yPositions);

        float targetY = yPositions[2];

        foreach (Transform item in menuItems)
        {
            if (Mathf.Approximately(item.localPosition.y, targetY))
            {
                selectedItem = item;
                selectedOriginalZ = item.localPosition.z;
                Vector3 pos = item.localPosition;
                pos.z += selectZOffset;
                item.localPosition = pos;
                break;
            }
        }
    }

    private void ResetSelection()
    {
        if (selectedItem != null)
        {
            Vector3 pos = selectedItem.localPosition;
            pos.z = selectedOriginalZ;
            selectedItem.localPosition = pos;
            selectedItem = null;
        }
    }

    private void MoveItems(int direction)
    {
        isMoving = true;

        float[] currentYPositions = new float[menuItems.Length];
        for (int i = 0; i < menuItems.Length; i++)
        {
            currentYPositions[i] = menuItems[i].localPosition.y;
        }
        System.Array.Sort(currentYPositions);

        foreach (Transform item in menuItems)
        {
            Vector3 pos = item.localPosition;
            float targetY = pos.y + (direction * verticalSpacing);

            if (direction > 0 && pos.y >= maxY)
            {
                pos.y = currentYPositions[0] - verticalSpacing;
                item.localPosition = pos;
                targetY = pos.y + verticalSpacing;
            }
            else if (direction < 0 && pos.y <= baseY)
            {
                pos.y = currentYPositions[currentYPositions.Length - 1] + verticalSpacing;
                item.localPosition = pos;
                targetY = pos.y - verticalSpacing;
            }

            StartCoroutine(SmoothMove(item, new Vector3(pos.x, targetY, pos.z)));
        }
    }

    private IEnumerator SmoothMove(Transform item, Vector3 target)
    {
        Vector3 start = item.localPosition;
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * moveSpeed;
            float t = Mathf.SmoothStep(0, 1, elapsed);
            item.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        item.localPosition = target;
        isMoving = false;
    }
}