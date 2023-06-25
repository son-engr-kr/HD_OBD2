using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UIElements;

public class DebugUI : MonoBehaviour
{
    static DebugUI _Instance;
    VisualElement _DebugFrame;
    TextField _TextFieldServerIP;
    IntegerField _IntegerFieldServerPort;
    Button _ButtonStartTCPIP;
    // Start is called before the first frame update
    void Start()
    {
        _Instance = this;
        var root = GetComponent<UIDocument>().rootVisualElement;
        _DebugFrame = root.Q<VisualElement>("DebugFrame");

        _TextFieldServerIP = _DebugFrame.Q<TextField>("TextFieldServerIP");
        _IntegerFieldServerPort = _DebugFrame.Q<IntegerField>("IntegerFieldServerPort");
        _ButtonStartTCPIP = _DebugFrame.Q<Button>("ButtonStartTCPIP");
        _ButtonStartTCPIP.RegisterCallback((ClickEvent evt) =>
        {
            HDOBD2MainUI.PrintlnDetailDebugLabel($"tcp button clicked:{_TextFieldServerIP.value}, {_IntegerFieldServerPort.value}");

            TCPIPTest.SetIPAndPort(_TextFieldServerIP.value, _IntegerFieldServerPort.value);
            TCPIPTest.StartTCPIP();
        });

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
