using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    private void Awake() => instance = this;

    private void OnShake(float duration, float strength)
    {
        transform.DOShakePosition(duration, strength);
    }

    public static void Shake(float duration, float strength) => instance.OnShake(duration, strength);
}
