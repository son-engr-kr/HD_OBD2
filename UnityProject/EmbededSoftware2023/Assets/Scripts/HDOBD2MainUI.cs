using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    VisualElement _CarStatusFrame;
    VisualElement _DTCFrame;
    VisualElement _CarViewFrame;
    ScrollView _CarStatusScrollView;
    ScrollView _DTCScrollView;

    VisualElement _LeftTab;
        Button _ButtonMainDashboard;
        Button _ButtonDiagnosis;
        Button _ButtonSetting;
        Button _ButtonDebug;

    VisualElement _LeftDiagnosisTab;
        Button _ButtonCarStatus;
        Button _ButtonDTC;

    VisualTreeAsset _CarStatusRowResource;

    //selected info
    Button _CurrentSelectedLeftTabButton;
    Button _CurrentSelectedLeftDiagnosisTabButton;


    VisualElement _Cluster;

    //CircularGaugeCustomControl _CustomControlTest;
    CircularGaugeCustomControl _RPMGaugeCustomControl;
    Label _LabelRpm;
    CircularGaugeCustomControl _SpeedometerGaugeCustomControl;
    Label _LabelSpeed;
    CircularGaugeCustomControl _FuelLevelGaugeCustomControl;
    Label _LabelFuelLevel;

    VisualElement _DebugFrame;
        Label _LabelDebug;
        Label _LabelDebugDetail;
        Label _LabelDebugDetailCapture;
        Button _ButtonCaptureDebugDetail;

    VisualElement _SettingFrame;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 200;

        _instance = this;
        var root = GetComponent<UIDocument>().rootVisualElement;

        _CarStatusFrame = root.Q<VisualElement>("CarStatusFrame");
        _DTCFrame = root.Q<VisualElement>("DTCFrame");
        _CarViewFrame = root.Q<VisualElement>("CarViewFrame");
        _CarStatusScrollView = root.Q<ScrollView>("CarStatusScrollView");
        _DTCScrollView = root.Q<ScrollView>("DTCScrollView");

        _LeftTab = root.Q<VisualElement>("LeftTab");
        _LeftDiagnosisTab = root.Q<VisualElement>("LeftDiagnosisTab");

        _ButtonMainDashboard = _LeftTab.Q<Button>("ButtonMain");
        _ButtonDiagnosis = _LeftTab.Q<Button>("ButtonDiagnosis");
        _ButtonSetting = _LeftTab.Q<Button>("ButtonSetting");
        _ButtonDebug = _LeftTab.Q<Button>("ButtonDebug");
        var leftTabButtonList = new List<Button>() { _ButtonMainDashboard, _ButtonDiagnosis, _ButtonSetting, _ButtonDebug };
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
                    _Cluster.RemoveFromClassList("Cluster--Hide");
                }
                else
                {

                    _Cluster.AddToClassList("Cluster--Hide");
                }
                if (dstButton == _ButtonDiagnosis)
                {
                    if (_CurrentSelectedLeftDiagnosisTabButton == null)
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
                else
                {
                    HideAllDiagnosisWindow();
                    _LeftDiagnosisTab.AddToClassList("LeftDiagnosisTab--Hide");
                }
                if(dstButton == _ButtonSetting)
                {
                    _SettingFrame.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _SettingFrame.style.display = DisplayStyle.None;
                }
                if (dstButton == _ButtonDebug)
                {
                    _DebugFrame.style.display = DisplayStyle.Flex;
                }
                else
                {
                    _DebugFrame.style.display = DisplayStyle.None;
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
                    CarViewVCamController.ChangeView(CarViewVCamController.VIEW_POSITION.FRONT_SIDE_FAR);
                }
                _CurrentSelectedLeftDiagnosisTabButton = dstButton;
            });
        }

        //List<string> testStatusNameList = new List<string>() { "1", "2", "3" };
        _CarStatusRowResource = Resources.Load<VisualTreeAsset>("_s_CarStatusRow");
        _CarStatusScrollView.Clear();
        //foreach (string statusName in testStatusNameList)
        //{
        //    UpdatePIDStatus(statusName, "test");
        //}
        _DTCScrollView.Clear();


        _Cluster = root.Q<VisualElement>("Cluster");
        //_CustomControlTest = _Cluster.Q<CircularGaugeCustomControl>("circular-gauge");
        _RPMGaugeCustomControl = _Cluster.Q<CircularGaugeCustomControl>("RpmGauge");
        _LabelRpm = _Cluster.Q<Label>("RpmLabel");
        _SpeedometerGaugeCustomControl = _Cluster.Q<CircularGaugeCustomControl>("SpeedometerGauge");
        _LabelSpeed = _Cluster.Q<Label>("SpeedLabel");

        _FuelLevelGaugeCustomControl = _Cluster.Q<CircularGaugeCustomControl>("FuelLevelGauge");
        _LabelFuelLevel = _Cluster.Q<Label>("FuelLevelLabel");

        _SettingFrame = root.Q<VisualElement>("SettingFrame");



        _DebugFrame = root.Q<VisualElement>("DebugFrame");
        {
            _LabelDebug = _DebugFrame.Q<Label>("LabelDebug");
            _LabelDebugDetail = _DebugFrame.Q<Label>("LabelDebugDetail");
            _LabelDebugDetailCapture = _DebugFrame.Q<Label>("LabelDebugDetailCapture");
            _ButtonCaptureDebugDetail = _DebugFrame.Q<Button>("ButtonCaptureDebugDetail");

            _ButtonCaptureDebugDetail.RegisterCallback<ClickEvent>((ClickEvent evt) =>
            {
                _LabelDebugDetailCapture.text = _LabelDebugDetail.text;
            });
        }




        //init ui
        using (var e = new ClickEvent() { target = _ButtonMainDashboard })
            _ButtonMainDashboard.SendEvent(e);
    }

    private void OnDestroy()
    {
        SystemLogWriter?.Close();
    }
    Queue<string> _debugStrings = new Queue<string>();
    static public void PrintlnDebugLabel(string text)
    {
        _instance.Invoke(() =>
        {
            while (_instance._debugStrings.Count >= 20)
            {
                _instance._debugStrings.Dequeue();
            }
            _instance._debugStrings.Enqueue(text);

            _instance._LabelDebug.text = "";
            foreach (var item in _instance._debugStrings)
            {
                _instance._LabelDebug.text += $"{item}\n";

            }
        });
    }
    Queue<string> _detailDebugStrings = new Queue<string>();
    static public void PrintlnDetailDebugLabel(string text)
    {
        _instance.Invoke(() =>
        {
            var nowString = DateTime.Now.ToString("HH:mm:ss_FFF");

            while (_instance._detailDebugStrings.Count >= 200)
            {
                _instance._detailDebugStrings.Dequeue();
            }
            _instance._detailDebugStrings.Enqueue($"{nowString} {text}");

            _instance._LabelDebugDetail.text = "";
            foreach (var item in _instance._detailDebugStrings)
            {
                _instance._LabelDebugDetail.text += $"{item}\n";

            }
        });
    }
    StreamWriter SystemLogWriter;
    StreamWriter OBDLogWriter;
    static public void WriteSystemLog(string category, string title, string message)
    {
        _instance.Invoke(() =>
        {
            var nowString = DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss_FFF");
            if(_instance.SystemLogWriter == null)
            {
                var dir = $"{Application.persistentDataPath}/logs";
                var dirInfo = new DirectoryInfo(dir);
                PrintlnDebugLabel($"log write path: {dirInfo.FullName}");
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }
                var dateStringForFileName = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_FFF");

                string path = $"{dir}/hdobd2_system_log_{dateStringForFileName}.csv";
                try
                {
                    _instance.SystemLogWriter = new StreamWriter(path, true);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"fail to open Log. exception: {ex}");
                    PrintlnDebugLabel($"fail to open Log. exception: {ex}");
                    //Application.Quit();
                    return;
                }
            }
            _instance.SystemLogWriter.WriteLine($"{nowString},{category},{title},{message}");
            _instance.SystemLogWriter.Flush();
        });
    }
    static public void WriteOBDLog(string category, string code, string value)
    {
        _instance.Invoke(() =>
        {
            var nowString = DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss_FFF");
            if (_instance.OBDLogWriter == null)
            {
                var dir = $"{Application.persistentDataPath}/logs";
                var dirInfo = new DirectoryInfo(dir);
                PrintlnDebugLabel($"log write path: {dirInfo.FullName}");
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }
                var dateStringForFileName = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_FFF");

                string path = $"{dir}/hdobd2_obd_log_{dateStringForFileName}.csv";
                try
                {
                    _instance.OBDLogWriter = new StreamWriter(path, true);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"fail to open Log. exception: {ex}");
                    PrintlnDebugLabel($"fail to open Log. exception: {ex}");
                    //Application.Quit();
                    return;
                }
            }
            _instance.OBDLogWriter.WriteLine($"{nowString},{category},{code},{value}");
            _instance.OBDLogWriter.Flush();
        });
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

        //rpm += 10f;
        //if (rpm > 6000) rpm = 0;
        //speed += 0.5f;
        //if (speed > 200) speed = 0;

        //_LabelRpm.text = $"{rpm:0.}";
        //_LabelSpeed.text = $"{speed:0.}";

        //_RPMGaugeCustomControl.Value = (rpm / 3000f);
        //if (_RPMGaugeCustomControl.Value > 0.5f)
        //{
        //    _RPMGaugeCustomControl.IsBlink = true;
        //    _RPMGaugeCustomControl.Value = _RPMGaugeCustomControl.Value;
        //}
        //else
        //{
        //    _RPMGaugeCustomControl.IsBlink = false;

        //}
        //_SpeedometerGaugeCustomControl.Value = (speed / 240);

        //CarViewVCamController.SetCameraNoise(speed/240, speed/60);

        _RPMGaugeCustomControl.JustRepaint();//BLINK를 위해서
    }

    void UIStatusMode()
    {
        _CarStatusFrame.RemoveFromClassList("CarStatusFrame--Hide");
        _DTCFrame.AddToClassList("DTCFrame--Hide");
        //_CarViewFrame.RemoveFromClassList("CarViewFrame--DTC");
    }
    void UIDTCMode()
    {
        _CarStatusFrame.AddToClassList("CarStatusFrame--Hide");
        _DTCFrame.RemoveFromClassList("DTCFrame--Hide");

        //_CarViewFrame.AddToClassList("CarViewFrame--DTC");
    }
    void HideAllDiagnosisWindow()
    {
        _LeftDiagnosisTab.AddToClassList("LeftDiagnosisTab--Hide");
        _CarStatusFrame.AddToClassList("CarStatusFrame--Hide");
        _DTCFrame.AddToClassList("DTCFrame--Hide");
        //_CarViewFrame.RemoveFromClassList("CarViewFrame--DTC");
    }
    public class OBDDataHandler
    {
        
        public class OBDDataTimeSeries
        {
            public class OBDDataTimeStamp
            {
                public OBDDataTimeStamp(long unixTime, float value)
                {
                    UnixTime = unixTime;
                    Value = value;
                }
                public long UnixTime;
                public float Value;
            }
            public void AddToQueue(long unixTime, float value)
            {
                if(DataCount == 0)
                {
                    MaxValue = value;
                    MinValue = value;
                }
                else
                {
                    if(TimeSeriesQueue.Count > 100)
                    {
                        TimeSeriesQueue.Dequeue();
                    }
                    float timePassed = (float)(unixTime - LastTimeStamp.UnixTime)/1000f;
                    MaxValue = Mathf.Max(MaxValue, value);
                    MinValue = Mathf.Min(MinValue, value);
                    if(value > LastTimeStamp.Value)
                    {
                        float increaseValue = value - LastTimeStamp.Value;
                        IncreaseAbsAccumulate += increaseValue;
                        IncreaseRateAbsAccumulate += increaseValue/timePassed;
                    }
                    else
                    {
                        float decreaseValue = LastTimeStamp.Value - value;
                        DecreaseAbsAccumulate += decreaseValue;
                        DecreaseRateAbsAccumulate += decreaseValue / timePassed;
                    }

                }
                LastTimeStamp = new OBDDataTimeStamp(unixTime, value);
                TimeSeriesQueue.Enqueue(LastTimeStamp);
                DataCount++;
                
            }
            private Queue<OBDDataTimeStamp> TimeSeriesQueue = new Queue<OBDDataTimeStamp>();
            OBDDataTimeStamp LastTimeStamp;
            private long DataCount=0;
            private float MaxValue; 
            private float MinValue;
            private double IncreaseAbsAccumulate = 0.0;
            private double DecreaseAbsAccumulate = 0.0;
            private double IncreaseRateAbsAccumulate = 0.0;
            private double DecreaseRateAbsAccumulate = 0.0;
            public string Test_GetStatisticsString()
            {
                return $"Max:{MaxValue},Min:{MinValue},IncreaseAbsAccumulate:{IncreaseAbsAccumulate},DecreaseAbsAccumulate:{DecreaseAbsAccumulate}";
            }
        }
        public void PushData(string code, float value)
        {
            if (DataDict.ContainsKey(code))
            {
                DataDict[code].AddToQueue(TimeManager.GetNowUnixTimeMillis(), value);
            }
            else 
            {
                DataDict[code] = new OBDDataTimeSeries();
            }
        }
        public string Test_GetStatisticsString(string code)
        {
            return $"{DataDict[code].Test_GetStatisticsString()}";
        }
        public Dictionary<string, OBDDataTimeSeries> DataDict = new Dictionary<string, OBDDataTimeSeries>();
    }
    public OBDDataHandler _OBDDataHandler = new OBDDataHandler();
    Dictionary<string, TemplateContainer> PIDUIDict = new Dictionary<string, TemplateContainer>();
    static public void UpdatePIDStatus(string code, string value)
    {
        _instance.Invoke(() =>
        {
            try
            {
                float valueFloat = float.Parse(value);
                _instance._OBDDataHandler.PushData(code, valueFloat);

                if (_instance.PIDUIDict.ContainsKey(code))
                {
                    var carStatusRowInstance = _instance.PIDUIDict[code];
                    var nameLabel = carStatusRowInstance.Q<Label>("StatusName");
                    var valueLabel = carStatusRowInstance.Q<Label>("StatusValue");

                    valueLabel.text = $"{value}, {_instance._OBDDataHandler.Test_GetStatisticsString(code)}";
                }
                else
                {
                    var carStatusRowInstance = _instance._CarStatusRowResource.Instantiate();
                    var nameLabel = carStatusRowInstance.Q<Label>("StatusName");
                    var valueLabel = carStatusRowInstance.Q<Label>("StatusValue");

                    nameLabel.text = code;
                    valueLabel.text = $"{value}, {_instance._OBDDataHandler.Test_GetStatisticsString(code)}";

                    _instance._CarStatusScrollView.Add(carStatusRowInstance);
                    _instance.PIDUIDict[code] = carStatusRowInstance;
                }
            }
            catch (Exception ex)
            {

            }
            
            switch (code)
            {
                case "RPM":
                    {
                        try
                        {
                            float rpm = float.Parse(value);
                            _instance._LabelRpm.text = $"{rpm:0.}";

                            _instance._RPMGaugeCustomControl.Value = (rpm / 6000f);
                            if (rpm > 1500f)
                            {
                                _instance._RPMGaugeCustomControl.IsBlink = true;
                            }
                            else
                            {
                                _instance._RPMGaugeCustomControl.IsBlink = false;

                            }
                        }
                        catch
                        {

                        }

                        
                        break;
                    }
                case "VEHICLE_SPEED":
                    {
                        try
                        {
                            float speed = float.Parse(value);
                            _instance._LabelSpeed.text = $"{speed:0.}";

                            _instance._SpeedometerGaugeCustomControl.Value = (speed / 240);
                            CarViewVCamController.SetCameraNoise(speed / 240, speed / 60);
                        }
                        catch
                        {

                        }

                        break;
                    }
                case "COOLANT_TEMP":
                    {
                        try
                        {
                            float temperature = float.Parse(value);
                        }
                        catch
                        {

                        }
                        break;
                    }
                case "FUEL_LEVEL":
                    {
                        try
                        {
                            float fuelLevel = float.Parse(value);
                            _instance._FuelLevelGaugeCustomControl.Value = fuelLevel / 100f;
                            _instance._LabelFuelLevel.text = $"{fuelLevel:0.}";
                        }
                        catch
                        {

                        }
                        break;
                    }
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
