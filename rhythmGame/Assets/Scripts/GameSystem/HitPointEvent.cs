using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPointEvent : MonoBehaviour
{
    public GameObject effect;
    public void ShowEffect()
    {
        Instantiate(effect, transform.position, effect.transform.rotation);
    }
}
