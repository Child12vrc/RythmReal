using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public float scrollSpeed = 2f;   // 이동 속도
    public float resetPoint = 160f;  // z축 이동 한계점
    public float resetOffset = -160f; // 되돌아갈 위치 (초기화할 오프셋)

    void Update()
    {
        // z축으로 타일맵 이동
        transform.position += Vector3.forward * scrollSpeed * Time.deltaTime;

        // resetPoint 위치에 도달하면 위치를 resetOffset만큼 이동하여 반복
        if (transform.position.z >= resetPoint)
        {
            transform.position += new Vector3(0, 0, resetOffset - resetPoint);
        }
    }
}
