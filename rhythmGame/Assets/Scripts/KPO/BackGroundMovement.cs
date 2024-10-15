using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundMovement : MonoBehaviour
{
    [Header("전체 속도")] public float Speed = 2;

    [Header("하늘 속도 배율")][Range(0f, 1f)]public float SkySpeedScale = 0.3f;
    [Header("구름 속도 배율")][Range(0f, 1f)] public float CloudSpeedScale = 0.6f;
    [Header("땅 및 울타리 속도 배율")][Range(0f, 1f)] public float GroundSpeedScale = 1f;

    [Header("하늘 이미지")] public GameObject[] SkyObject = new GameObject[2];
    [Header("구름 이미지")] public GameObject[] CloudObject = new GameObject[2];
    [Header("땅 및 울타리 이미지")] public GameObject[] GroundObject = new GameObject[2];


    // Update is called once per frame
    void Update()
    {
        Vector3 speed = Vector3.left * Speed * Time.deltaTime;
        BackGroundMove(SkyObject[0], speed, SkySpeedScale);
        BackGroundMove(SkyObject[1], speed, SkySpeedScale);
        BackGroundMove(CloudObject[0], speed, CloudSpeedScale);
        BackGroundMove(CloudObject[1], speed, CloudSpeedScale);
        BackGroundMove(GroundObject[0], speed, GroundSpeedScale);
        BackGroundMove(GroundObject[1], speed, GroundSpeedScale);

    }

    private void BackGroundMove(GameObject BG, Vector3 speed, float SpeedScale)
    {
        BG.transform.Translate(speed * SpeedScale);
        if(BG.transform.position.x <= -12.5f)
        {
            BG.transform.position = new Vector3(12.5f, 0, 0);
        }
    }
}
