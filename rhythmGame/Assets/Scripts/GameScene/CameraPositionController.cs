using UnityEngine;
using System.Collections;

public class CameraPositionController : MonoBehaviour
{
    public Transform[] cameraPositions = new Transform[3];  // �� ��ġ�� Transform�� �Ҵ�
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;

    private int currentPosition = 0;
    private bool isMoving = false;

    void Start()
    {
        // ���� �� ù ��° ��ġ�� ����
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
            MoveCameraToPosition(0);  // ���� �޴�
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            MoveCameraToPosition(1);  // �� ����
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            MoveCameraToPosition(2);  // ���� ����
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
            // ��ġ �̵�
            transform.position = Vector3.Lerp(transform.position, targetTransform.position,
                moveSpeed * Time.deltaTime);

            // ȸ��
            transform.rotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation,
                rotateSpeed * Time.deltaTime);

            yield return null;
        }

        // ��Ȯ�� ��ġ�� ȸ�������� ����
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