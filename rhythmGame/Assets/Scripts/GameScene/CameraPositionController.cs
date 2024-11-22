using UnityEngine;
using System.Collections;

public class CameraPositionController : MonoBehaviour
{
    public Transform[] cameraPositions = new Transform[3];  // 각 위치의 Transform을 할당
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;

    private int currentPosition = 0;
    private bool isMoving = false;

    void Start()
    {
        // 시작 시 첫 번째 위치로 설정
        if (cameraPositions[0] != null)
        {
            transform.position = cameraPositions[0].position;
            transform.rotation = cameraPositions[0].rotation;
        }
    }

    void Update()
    {
        if (isMoving) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            MoveCameraToPosition(0);  // 메인 메뉴
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            MoveCameraToPosition(1);  // 곡 선택
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            MoveCameraToPosition(2);  // 게임 진행
        }
    }

    public void MoveCameraToPosition(int index)
    {
        if (index < 0 || index >= cameraPositions.Length || cameraPositions[index] == null || isMoving)
            return;

        currentPosition = index;
        StartCoroutine(MoveCameraCoroutine(cameraPositions[index]));
    }

    private IEnumerator MoveCameraCoroutine(Transform targetTransform)
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, targetTransform.position) > 0.01f ||
               Quaternion.Angle(transform.rotation, targetTransform.rotation) > 0.01f)
        {
            // 위치 이동
            transform.position = Vector3.Lerp(transform.position, targetTransform.position,
                moveSpeed * Time.deltaTime);

            // 회전
            transform.rotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation,
                rotateSpeed * Time.deltaTime);

            yield return null;
        }

        // 정확한 위치와 회전값으로 설정
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;

        isMoving = false;
    }

    public int GetCurrentPosition()
    {
        return currentPosition;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}