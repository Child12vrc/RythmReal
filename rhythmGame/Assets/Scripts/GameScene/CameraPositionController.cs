using UnityEngine;
using System.Collections;

public class CameraPositionController : MonoBehaviour
{
    public Transform[] cameraPositions = new Transform[3];
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;
    private int currentPosition = 0;
    private bool isMoving = false;
    private Coroutine currentMoveCoroutine;  // 현재 실행 중인 코루틴 참조

    void Start()
    {
        if (cameraPositions[0] != null)
        {
            transform.position = cameraPositions[0].position;
            transform.rotation = cameraPositions[0].rotation;
        }

        MoveCameraToPosition(1);  // 곡 선택
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

        // 이전 코루틴이 실행 중이면 중지
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
            if (!gameObject.activeInHierarchy) yield break;  // 게임오브젝트가 비활성화되면 종료

            elapsedTime += Time.deltaTime * moveSpeed;
            float t = Mathf.Clamp01(elapsedTime);

            // 부드러운 보간을 위해 SmoothStep 사용
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPosition, targetTransform.position, smoothT);
            transform.rotation = Quaternion.Lerp(startRotation, targetTransform.rotation, smoothT);

            yield return null;
        }

        // 정확한 위치와 회전값으로 설정
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
        // 스크립트가 비활성화될 때 코루틴 정리
        if (currentMoveCoroutine != null)
        {
            StopCoroutine(currentMoveCoroutine);
            currentMoveCoroutine = null;
        }
        isMoving = false;
    }
}