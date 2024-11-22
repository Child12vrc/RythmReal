using UnityEngine;

public class ButtonPress : MonoBehaviour
{
    private float originalZ;
    private float pressedZ = 0.1f;
    private bool isPressed = false;
    private float pressSpeed = 15f;
    private float currentZ;

    private void Start()
    {
        originalZ = transform.localPosition.z;
        currentZ = originalZ;
    }

    private void Update()
    {
        float targetZ = isPressed ? pressedZ : originalZ;
        currentZ = Mathf.Lerp(currentZ, targetZ, Time.deltaTime * pressSpeed);

        // 로컬 위치의 Z값만 업데이트
        Vector3 localPos = transform.localPosition;
        localPos.z = currentZ;
        transform.localPosition = localPos;
    }

    public void PressButton()
    {
        isPressed = true;
    }

    public void ReleaseButton()
    {
        isPressed = false;
    }
}