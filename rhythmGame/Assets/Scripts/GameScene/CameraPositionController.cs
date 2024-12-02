using UnityEngine;
using System.Collections;

public class CameraPositionController : MonoBehaviour
{
    public Transform[] cameraPositions = new Transform[3];
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;
    private int currentPosition = 0;
    private bool isMoving = false;
    private Coroutine currentMoveCoroutine;  // ���� ���� ���� �ڷ�ƾ ����

    void Start()
    {
        if (cameraPositions[0] != null)
        {
            transform.position = cameraPositions[0].position;
            transform.rotation = cameraPositions[0].rotation;
        }

        MoveCameraToPosition(1);  // �� ����
    }

    void Update()
    {
        if (isMoving) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            MoveCameraToPosition(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            MoveCameraToPosition(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            MoveCameraToPosition(2);
        }
    }

    public void MoveCameraToPosition(int index)
    {
        if (index < 0 || index >= cameraPositions.Length || cameraPositions[index] == null)
            return;

        // ���� �ڷ�ƾ�� ���� ���̸� ����
        if (currentMoveCoroutine != null)
        {
            StopCoroutine(currentMoveCoroutine);
            isMoving = false;
        }

        currentPosition = index;
        currentMoveCoroutine = StartCoroutine(MoveCameraCoroutine(cameraPositions[index]));
    }

    private IEnumerator MoveCameraCoroutine(Transform targetTransform)
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            if (!gameObject.activeInHierarchy) yield break;  // ���ӿ�����Ʈ�� ��Ȱ��ȭ�Ǹ� ����

            elapsedTime += Time.deltaTime * moveSpeed;
            float t = Mathf.Clamp01(elapsedTime);

            // �ε巯�� ������ ���� SmoothStep ���
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPosition, targetTransform.position, smoothT);
            transform.rotation = Quaternion.Lerp(startRotation, targetTransform.rotation, smoothT);

            yield return null;
        }

        // ��Ȯ�� ��ġ�� ȸ�������� ����
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;

        isMoving = false;
        currentMoveCoroutine = null;
    }

    public int GetCurrentPosition()
    {
        return currentPosition;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    private void OnDisable()
    {
        // ��ũ��Ʈ�� ��Ȱ��ȭ�� �� �ڷ�ƾ ����
        if (currentMoveCoroutine != null)
        {
            StopCoroutine(currentMoveCoroutine);
            currentMoveCoroutine = null;
        }
        isMoving = false;
    }
}