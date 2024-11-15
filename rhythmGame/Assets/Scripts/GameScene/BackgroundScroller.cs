using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public float scrollSpeed = 2f;   // �̵� �ӵ�
    public float resetPoint = 160f;  // z�� �̵� �Ѱ���
    public float resetOffset = -160f; // �ǵ��ư� ��ġ (�ʱ�ȭ�� ������)

    void Update()
    {
        // z������ Ÿ�ϸ� �̵�
        transform.position += Vector3.forward * scrollSpeed * Time.deltaTime;

        // resetPoint ��ġ�� �����ϸ� ��ġ�� resetOffset��ŭ �̵��Ͽ� �ݺ�
        if (transform.position.z >= resetPoint)
        {
            transform.position += new Vector3(0, 0, resetOffset - resetPoint);
        }
    }
}
