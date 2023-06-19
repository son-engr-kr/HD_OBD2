using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingUI : MonoBehaviour
{
    static SettingUI _instance;
    VisualElement _SettingFrame;
    ScrollView _ScrollViewSettingValue;
    // Start is called before the first frame update
    void Start()
    {
        _instance = this;
        var root = GetComponent<UIDocument>().rootVisualElement;
        var settingSliderRowResource = Resources.Load<VisualTreeAsset>("_s_SettingSliderRow");
        _SettingFrame = root.Q<VisualElement>("SettingFrame");
        _ScrollViewSettingValue = root.Q<ScrollView>("ScrollViewSettingValue");
        {
            //PlayerPrefs.SetFloat("", 3);
            var settingRow = settingSliderRowResource.Instantiate();
            var slider = settingRow.Q<Slider>("SliderDefault");
            var valueLabel = settingRow.Q<Label>("LabelValue");
            slider.RegisterValueChangedCallback((ChangeEvent<float> evt) =>
            {
                valueLabel.text = $"{evt.previousValue}->{evt.newValue}";
            });
            Debug.Log($"_ScrollViewSettingValue:{_ScrollViewSettingValue}");
            _ScrollViewSettingValue.Add(settingRow);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
