using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
{
    [Header("Slider setup")] [SerializeField, Range(0, 1f)]
    private float sliderValue;
    
    public bool CurrentValue {get; private set;}
    
    private Slider _slider;
    
    [Header("Animation")]
    [SerializeField, Range(0, 1f)] private float animationDuration = 0.5f;
    [SerializeField] private AnimationCurve slideEase = AnimationCurve.EaseInOut(0,0,1,1);
    
    private Coroutine _animateSliderCoroutine;
    
    [Header("Events")]
    [SerializeField] private UnityEvent onToggleOn;
    [SerializeField] private UnityEvent onToggleOff;

    protected void OnValidate()
    {
        SetupToggleComponents();
        
        _slider.value = sliderValue;
    }

    private void SetupToggleComponents()
    {
        if (_slider != null)
            return;
        
        SetupSliderComponent();
    }

    private void SetupSliderComponent()
    {
        _slider = GetComponent<Slider>();

        if (_slider == null)
        {
            Debug.LogWarning("No slider found!", this);
            return;
        }
        
        _slider.interactable = false;
        var sliderColors = _slider.colors;
        sliderColors.disabledColor = Color.white;
        _slider.colors = sliderColors;
        _slider.transition = Selectable.Transition.None;
    }

    private void Awake()
    {
        SetupSliderComponent();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Toggle();
    }

    private void Toggle()
    {
        SetStateAndStartAnimation(!CurrentValue);
    }

    private void SetStateAndStartAnimation(bool state)
    {
        CurrentValue = state;
        
        if(CurrentValue)
            onToggleOn?.Invoke();
        else
            onToggleOff?.Invoke();
        
        if(_animateSliderCoroutine != null)
            StopCoroutine(_animateSliderCoroutine);

        _animateSliderCoroutine = StartCoroutine(AnimationSlider());
    }

    private IEnumerator AnimationSlider()
    {
        float startValue = _slider.value;
        float endValue = CurrentValue? 1f : 0f;

        float time = 0;
        if (animationDuration > 0)
        {
            while (time < animationDuration)
            {
                time += Time.deltaTime;
                float lerpFactor = slideEase.Evaluate(time / animationDuration);
                _slider.value = sliderValue+Mathf.Lerp(startValue, endValue, lerpFactor);
                
                yield return null;
            }
        }
        
        _slider.value = endValue;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
