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

        // ���� ���� ���� �ڷ�ƾ�� �ִٸ� ����
        if (activeCoroutines.ContainsKey(effectIndex))
        {
            if (activeCoroutines[effectIndex] != null)
            {
                StopCoroutine(activeCoroutines[effectIndex]);
            }
            activeCoroutines.Remove(effectIndex);
            effectObjects[effectIndex].SetActive(false);  // ��� ����
        }

        // ���ο� ���°� true�� ��쿡�� �� �ڷ�ƾ ����
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
        // ��� ���� ���� �ڷ�ƾ ����
        foreach (var coroutine in activeCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        activeCoroutines.Clear();

        // ��� ����Ʈ ��� ��Ȱ��ȭ
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

    // ����� ��� �߰�
    public bool debugMode = false;

    private void Start()
    {
        ResetEffects();
    }
}