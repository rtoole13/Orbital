using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    private float currentMultiplier;
    private int currentMultiplierIndex = 0;

    [SerializeField]
    private int[] timeMultipliers = new int[] { 1, 2, 3, 4, 5 };

    public delegate void ChangingTimeScale(float newTimeScale);
    public static event ChangingTimeScale TimeScaleChangeEvent;
    private void Awake()
    {
        currentMultiplier = (float)timeMultipliers[currentMultiplierIndex];
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Period) && IncrementMultiplier())
        {
            TimeScaleChangeEvent(currentMultiplier);
            ChangeFixedUpdateTime();
            return;
        }
        if (Input.GetKeyUp(KeyCode.Comma) && DecrementMultiplier())
        {
            TimeScaleChangeEvent(currentMultiplier);
            ChangeFixedUpdateTime();
        }
    }

    private void ChangeFixedUpdateTime()
    {
        float currentTimeScale = currentMultiplier;
        currentMultiplier = (float)timeMultipliers[currentMultiplierIndex];
        Time.timeScale = currentMultiplier;
        Debug.LogFormat("Updating time warp from {0}x to {1}x.", currentTimeScale, Time.timeScale);
    }

    private bool DecrementMultiplier()
    {
        if (currentMultiplierIndex == 0)
            return false;

        currentMultiplierIndex -= 1;
        return true;
    }

    private bool IncrementMultiplier()
    {
        if (currentMultiplierIndex == timeMultipliers.Length - 1)
            return false;

        currentMultiplierIndex += 1;
        return true;
    }

    public void Pause()
    {
        Time.timeScale = 0f;
    }

    public void UnPause()
    {
        Time.timeScale = currentMultiplier;
    }
}
