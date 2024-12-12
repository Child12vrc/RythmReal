using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public AudioSource audioSource; // AudioSource 컴포넌트
    public AudioClip soundS; // S 키 소리
    public AudioClip soundD; // D 키 소리
    public AudioClip soundJ; // J 키 소리
    public AudioClip soundK; // K 키 소리

    void Update()
    {
        // 각 키 입력에 대해 소리 재생 처리
        if (Input.GetKeyDown(KeyCode.S))
        {
            PlaySound(soundS);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            PlaySound(soundD);
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            PlaySound(soundJ);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            PlaySound(soundK);
        }
    }

    void PlaySound(AudioClip clip)
    {
        // PlayOneShot을 사용하여 소리가 겹쳐서 재생되도록 함
        audioSource.PlayOneShot(clip);
    }
}