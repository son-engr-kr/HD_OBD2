using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarViewVCamController : MonoBehaviour
{
    static CarViewVCamController _instance;

    Cinemachine.CinemachineVirtualCamera _VCam;
    [SerializeField] GameObject FrontSideViewPositionObject;
    [SerializeField] GameObject TopViewPositionObject;
    [SerializeField] GameObject SideViewPositionObject;
    private void Awake()
    {
        _instance = this;
        _VCam = GetComponent<Cinemachine.CinemachineVirtualCamera>();
    }
    public enum VIEW_POSITION
    {
        TOP,
        FRONT_SIDE,
        SIDE
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
            case VIEW_POSITION.SIDE:
                {
                    _instance._VCam.Follow = _instance.SideViewPositionObject.transform;
                    break;
                }
        }
    }
}
