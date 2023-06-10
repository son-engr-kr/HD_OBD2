using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HDOBD2MainUI : MonoBehaviour
{
    //static HDOBD2MainUI __instance;
    //static HDOBD2MainUI _instance {
    //    get
    //    {
    //        if (__instance == null)
    //        {
    //            __instance = GameObject.FindObjectOfType<HDOBD2MainUI>();
    //        }
    //        return __instance;
    //    }
    //}
    static HDOBD2MainUI _instance;
    Label _DebugLabel;
    VisualElement _CarStatusFrame;
    VisualElement _DTCFrame;
    VisualElement _CarViewFrame;
    ScrollView _CarStatusScrollView;
    ScrollView _DTCScrollView;

    VisualElement _LeftTab;
        Button _ButtonMainDashboard;
        Button _ButtonDiagnosis;

    VisualElement _LeftDiagnosisTab;
        Button _ButtonCarStatus;
        Button _ButtonDTC;

    VisualTreeAsset _CarStatusRowResource;

    //selected info
    Button _CurrentSelectedLeftTabButton;
    Button _CurrentSelectedLeftDiagnosisTabButton;


    CircularGaugeCustomControl _CustomControlTest;
    RPMGaugeCustomControl _RPMGaugeCustomControl;
    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 200;

        _instance = this;
        var root = GetComponent<UIDocument>().rootVisualElement;

        _DebugLabel = root.Q<Label>("DebugLabel");

        _CarStatusFrame = root.Q<VisualElement>("CarStatusFrame");
        _DTCFrame = root.Q<VisualElement>("DTCFrame");
        _CarViewFrame = root.Q<VisualElement>("CarViewFrame");
        _CarStatusScrollView = root.Q<ScrollView>("CarStatusScrollView");
        _DTCScrollView = root.Q<ScrollView>("DTCScrollView");

        _LeftTab = root.Q<VisualElement>("LeftTab");
        _LeftDiagnosisTab = root.Q<VisualElement>("LeftDiagnosisTab");

        _ButtonMainDashboard = _LeftTab.Q<Button>("ButtonMain");
        _ButtonDiagnosis = _LeftTab.Q<Button>("ButtonDiagnosis");
        var leftTabButtonList = new List<Button>() { _ButtonMainDashboard, _ButtonDiagnosis };
        foreach (var dstButton in leftTabButtonList)
        {
            dstButton.RegisterCallback<ClickEvent>((ClickEvent evt) =>
            {
                foreach (var otherButton in leftTabButtonList)
                {
                    otherButton.RemoveFromClassList("LeftTabButton--Selected");
                }
                dstButton.AddToClassList("LeftTabButton--Selected");
                Debug.Log(dstButton);
                if(dstButton == _ButtonMainDashboard)
                {
                    CarViewVCamController.ChangeView(CarViewVCamController.VIEW_POSITION.REAR_TOP);
                    HideAllDiagnosisWindow();
                }
                else if(dstButton == _ButtonDiagnosis)
                {
                    if(_CurrentSelectedLeftDiagnosisTabButton == null)
                    {
                        _CurrentSelectedLeftDiagnosisTabButton = _ButtonCarStatus;
                    }
                    using (var e = new ClickEvent() { target = _CurrentSelectedLeftDiagnosisTabButton })
                        _CurrentSelectedLeftDiagnosisTabButton.SendEvent(e);
                    //if (_CurrentSelectedLeftTabButton != _ButtonDiagnosis)
                    //{
                    //    using (var e = new ClickEvent() { target = _CurrentSelectedLeftDiagnosisTabButton })
                    //        _CurrentSelectedLeftDiagnosisTabButton.SendEvent(e);
                    //}
                    _LeftDiagnosisTab.RemoveFromClassList("LeftDiagnosisTab--Hide");
                }
                _CurrentSelectedLeftTabButton = dstButton;
            });
        }


        _ButtonCarStatus = _LeftDiagnosisTab.Q<Button>("ButtonCarStatus");
        _ButtonDTC = _LeftDiagnosisTab.Q<Button>("ButtonDTC");
        var LeftDiagnosisTabButtonList = new List<Button>() { _ButtonCarStatus , _ButtonDTC };
        foreach(var dstButton in LeftDiagnosisTabButtonList)
        {
            dstButton.RegisterCallback<ClickEvent>((ClickEvent evt) =>
            {
                foreach (var otherButton in LeftDiagnosisTabButtonList)
                {
                    otherButton.RemoveFromClassList("LeftDiagnosisTabButton--Selected");
                }
                dstButton.AddToClassList("LeftDiagnosisTabButton--Selected");
                if (dstButton == _ButtonCarStatus)
                {
                    UIStatusMode();
                    CarViewVCamController.ChangeView(CarViewVCamController.VIEW_POSITION.FRONT_SIDE);
                }
                else if (dstButton == _ButtonDTC)
                {
                    UIDTCMode();
                    CarViewVCamController.ChangeView(CarViewVCamController.VIEW_POSITION.TOP);
                }
                _CurrentSelectedLeftDiagnosisTabButton = dstButton;
            });
        }

        List<string> statusNameList = new List<string>() { "1", "2", "3" };
        _CarStatusRowResource = Resources.Load<VisualTreeAsset>("CarStatusRow");
        _CarStatusScrollView.Clear();
        foreach (string statusName in statusNameList)
        {
            UpdatePIDStatus(statusName, "test");
        }
        _DTCScrollView.Clear();

        _CustomControlTest = root.Q<CircularGaugeCustomControl>("circular-gauge");
        _RPMGaugeCustomControl = root.Q<RPMGaugeCustomControl>("rpm-gauge");


    }
    Queue<string> _debugStrings = new Queue<string>();
    static public void PrintlnDebugLabel(string text)
    {
        while(_instance._debugStrings.Count >= 20)
        {
            _instance._debugStrings.Dequeue();
        }
        _instance._debugStrings.Enqueue(text);

        _instance._DebugLabel.text = "";
        foreach (var item in _instance._debugStrings)
        {
            _instance._DebugLabel.text += $"{item}\n";

        }
    }
    private object _lock = new object();
    private System.Action _callbacks;
    public void Invoke(System.Action callback)
    {
        lock (_lock)
        {
            _callbacks += callback;
        }
    }
    void Update()
    {
        //sure, this updates constantly, again, for demo purposes... implement the actual update hook however you please


        //here we pull the delegates out, incase the callback calls 'Invoke'
        //if Invoke is called, the lock would create a deadlock, and freeze the game
        System.Action action = null;
        lock (_lock)
        {
            if (_callbacks != null)
            {
                action = _callbacks;
                _callbacks = null;
            }
        }
        action?.Invoke();

        _CustomControlTest.Value = (_CustomControlTest.Value + 0.005f);
        if(_CustomControlTest.Value >= 0.95f)
        {
            _CustomControlTest.Value = 0f;
        }
        if(_CustomControlTest.Value > 0.5f)
        {
            _CustomControlTest.IsBlink = true;
            _CustomControlTest.Value = _CustomControlTest.Value;
        }
        else
        {
            _CustomControlTest.IsBlink = false;

        }

    }

    void UIStatusMode()
    {
        _CarStatusFrame.RemoveFromClassList("CarStatusFrame--Hide");
        _DTCFrame.AddToClassList("DTCFrame--Hide");
        _CarViewFrame.RemoveFromClassList("CarViewFrame--DTC");
    }
    void UIDTCMode()
    {
        _CarStatusFrame.AddToClassList("CarStatusFrame--Hide");
        _DTCFrame.RemoveFromClassList("DTCFrame--Hide");

        _CarViewFrame.AddToClassList("CarViewFrame--DTC");
    }
    void HideAllDiagnosisWindow()
    {
        _LeftDiagnosisTab.AddToClassList("LeftDiagnosisTab--Hide");
        _CarStatusFrame.AddToClassList("CarStatusFrame--Hide");
        _DTCFrame.AddToClassList("DTCFrame--Hide");
        _CarViewFrame.RemoveFromClassList("CarViewFrame--DTC");
    }


    Dictionary<string, TemplateContainer> PIDDict = new Dictionary<string, TemplateContainer>();
    static public void UpdatePIDStatus(string code, string value)
    {
        _instance.Invoke(() =>
        {
            if (_instance.PIDDict.ContainsKey(code))
            {

                var carStatusRowInstance = _instance.PIDDict[code];
                var nameLabel = carStatusRowInstance.Q<Label>("StatusName");
                var valueLabel = carStatusRowInstance.Q<Label>("StatusValue");

                valueLabel.text = value;
            }
            else
            {
                var carStatusRowInstance = _instance._CarStatusRowResource.Instantiate();
                var nameLabel = carStatusRowInstance.Q<Label>("StatusName");
                var valueLabel = carStatusRowInstance.Q<Label>("StatusValue");

                nameLabel.text = code;
                valueLabel.text = value;
                _instance._CarStatusScrollView.Add(carStatusRowInstance);
                _instance.PIDDict[code] = carStatusRowInstance;
            }
        });
        
    }
    Dictionary<string, TemplateContainer> DTCDict = new Dictionary<string, TemplateContainer>();
    static public void UpdateDTC(string code)
    {
        _instance.Invoke(() =>
        {
            if (_instance.DTCDict.ContainsKey(code))
            {

                var carStatusRowInstance = _instance.DTCDict[code];
                var nameLabel = carStatusRowInstance.Q<Label>("StatusName");
                var valueLabel = carStatusRowInstance.Q<Label>("StatusValue");
                valueLabel.text = $"{int.Parse(valueLabel.text) + 1}";
            }
            else
            {
                var carStatusRowInstance = _instance._CarStatusRowResource.Instantiate();
                var nameLabel = carStatusRowInstance.Q<Label>("StatusName");
                var valueLabel = carStatusRowInstance.Q<Label>("StatusValue");

                nameLabel.text = code;
                valueLabel.text = "1";
                _instance._DTCScrollView.Add(carStatusRowInstance);
                _instance.DTCDict[code] = carStatusRowInstance;

            }
        });
    }
}
