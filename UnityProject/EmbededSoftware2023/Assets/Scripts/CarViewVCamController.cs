using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarViewVCamController : MonoBehaviour
{
    static CarViewVCamController _instance;

    Cinemachine.CinemachineVirtualCamera _VCam;
    Cinemachine.CinemachineBasicMultiChannelPerlin _Noise;
    [SerializeField] GameObject FrontSideViewPositionObject;
    [SerializeField] GameObject FrontSideFarViewPositionObject;
    [SerializeField] GameObject TopViewPositionObject;
    [SerializeField] GameObject SideViewPositionObject;
    [SerializeField] GameObject RearTopViewPositionObject;
    private void Awake()
    {
        _instance = this;
        _VCam = GetComponent<Cinemachine.CinemachineVirtualCamera>();
        _Noise = _VCam.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
        _Noise.m_AmplitudeGain = 0;
    }
    public enum VIEW_POSITION
    {
        TOP,
        FRONT_SIDE,
        FRONT_SIDE_FAR,
        SIDE,
        REAR_TOP
    }
    static public void ChangeView(VIEW_POSITION viewPosition)
    {
        GameObject viewPositionObject = null;
        switch (viewPosition)
        {
            case VIEW_POSITION.TOP:
                {
                    _instance._VCam.Follow = _instance.TopViewPositionObject.transform;
                    break;
                }
            case VIEW_POSITION.FRONT_SIDE:
                {
                    _instance._VCam.Follow = _instance.FrontSideViewPositionObject.transform;
                    break;
                }
            case VIEW_POSITION.FRONT_SIDE_FAR:
                {
                    _instance._VCam.Follow = _instance.FrontSideFarViewPositionObject.transform;
                    break;
                }
            case VIEW_POSITION.SIDE:
                {
                    _instance._VCam.Follow = _instance.SideViewPositionObject.transform;
                    break;
                }
            case VIEW_POSITION.REAR_TOP:
                {
                    _instance._VCam.Follow = _instance.RearTopViewPositionObject.transform;
                    break;
                }
        }
    }
    static public void SetCameraNoise(float amplitudeGain, float freqGain)
    {
        _instance._Noise.m_AmplitudeGain = amplitudeGain;
        _instance._Noise.m_FrequencyGain = freqGain;
    }
}
