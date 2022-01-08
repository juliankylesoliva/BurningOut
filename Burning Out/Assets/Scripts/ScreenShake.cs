using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ScreenShake : MonoBehaviour
{
    CinemachineVirtualCamera vCam;
    private float shakeTime;

    void Awake()
    {
        vCam = this.gameObject.GetComponent<CinemachineVirtualCamera>();
    }

    void Update()
    {
        if (vCam != null && shakeTime > 0f)
        {
            shakeTime -= Time.deltaTime;
            if (shakeTime <= 0f)
            {
                CinemachineBasicMultiChannelPerlin perlin = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                perlin.m_AmplitudeGain = 0f;
            }
        }
    }

    public void DoShake(float amplitude, float time)
    {
        if (vCam != null)
        {
            CinemachineBasicMultiChannelPerlin perlin = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            perlin.m_AmplitudeGain = amplitude;
            shakeTime = time;
        }
    }
}
