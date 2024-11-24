using UnityEngine;

public class MenuScroll : MonoBehaviour
{
    [SerializeField] private Transform[] menuItems = new Transform[8];
    [SerializeField] private float verticalSpacing = 2f;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float selectZOffset = 2f;

    private bool isMoving = false;
    private float baseY;
    private float maxY;
    private Transform selectedItem = null;
    private float selectedOriginalZ;

    private void Start()
    {
        baseY = menuItems[0].localPosition.y;
        maxY = baseY + (verticalSpacing * 7);
    }

    private void Update()
    {
        if (isMoving) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (selectedItem != null)
            {
                // ���õ� �������� ����ġ��
                Vector3 pos = selectedItem.localPosition;
                pos.z = selectedOriginalZ;
                selectedItem.localPosition = pos;
                selectedItem = null;
            }
            MoveItems(1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (selectedItem != null)
            {
                // ���õ� �������� ����ġ��
                Vector3 pos = selectedItem.localPosition;
                pos.z = selectedOriginalZ;
                selectedItem.localPosition = pos;
                selectedItem = null;
            }
            MoveItems(-1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SelectThirdFromTop();
        }
    }

    private void SelectThirdFromTop()
    {
        if (isMoving) return;

        // ���� ���õ� �������� �ִٸ� ����ġ��
        if (selectedItem != null)
        {
            Vector3 pos = selectedItem.localPosition;
            pos.z = selectedOriginalZ;
            selectedItem.localPosition = pos;
            selectedItem = null;
        }

        // Y ��ġ�� ���ĵ� ������ ã��
        float[] yPositions = new float[menuItems.Length];
        for (int i = 0; i < menuItems.Length; i++)
        {
            yPositions[i] = menuItems[i].localPosition.y;
        }
        System.Array.Sort(yPositions);
        System.Array.Reverse(yPositions); // ���������� ����

        // ������ 3��° Y ��ġ ã��
        float targetY = yPositions[2];

        // �ش� ��ġ�� ������ ã��
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

    private System.Collections.IEnumerator SmoothMove(Transform item, Vector3 target)
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