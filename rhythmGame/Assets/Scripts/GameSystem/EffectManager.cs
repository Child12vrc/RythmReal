using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public GameObject[] effectObjects = new GameObject[11];
    public float effectDuration = 3f;

    private Dictionary<int, Coroutine> activeCoroutines = new Dictionary<int, Coroutine>();

    public bool GetEffectState(int effectIndex)
    {
        if (effectIndex >= 0 && effectIndex < effectObjects.Length && effectObjects[effectIndex] != null)
        {
            return effectObjects[effectIndex].activeSelf;
        }
        return false;
    }

    public void SetEffect(int effectIndex, bool state)
    {
        if (effectIndex < 0 || effectIndex >= effectObjects.Length || effectObjects[effectIndex] == null)
            return;

        // 현재 실행 중인 코루틴이 있다면 중지
        if (activeCoroutines.ContainsKey(effectIndex))
        {
            if (activeCoroutines[effectIndex] != null)
            {
                StopCoroutine(activeCoroutines[effectIndex]);
            }
            activeCoroutines.Remove(effectIndex);
            effectObjects[effectIndex].SetActive(false);  // 즉시 끄기
        }

        // 새로운 상태가 true인 경우에만 새 코루틴 시작
        if (state)
        {
            effectObjects[effectIndex].SetActive(true);
            var coroutine = StartCoroutine(DeactivateAfterDelay(effectIndex));
            activeCoroutines[effectIndex] = coroutine;

            if (debugMode)
            {
                Debug.Log($"Effect {effectIndex} started, will deactivate in {effectDuration} seconds");
            }
        }
    }

    private IEnumerator DeactivateAfterDelay(int effectIndex)
    {
        yield return new WaitForSeconds(effectDuration);

        if (effectObjects[effectIndex] != null)
        {
            effectObjects[effectIndex].SetActive(false);
            if (debugMode)
            {
                Debug.Log($"Effect {effectIndex} deactivated after {effectDuration} seconds");
            }
        }

        if (activeCoroutines.ContainsKey(effectIndex))
        {
            activeCoroutines.Remove(effectIndex);
        }
    }

    public void ResetEffects()
    {
        // 모든 실행 중인 코루틴 중지
        foreach (var coroutine in activeCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        activeCoroutines.Clear();

        // 모든 이펙트 즉시 비활성화
        for (int i = 0; i < effectObjects.Length; i++)
        {
            if (effectObjects[i] != null)
            {
                effectObjects[i].SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        ResetEffects();
    }

    // 디버그 모드 추가
    public bool debugMode = false;

    private void Start()
    {
        ResetEffects();
    }
}