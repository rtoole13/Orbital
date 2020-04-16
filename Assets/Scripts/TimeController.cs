using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    private float currentMultiplier;
    private int currentMultiplierIndex = 0;

    [SerializeField]
    private float[] timeMultipliers = new float[] { 1f, 2f, 3f, 4f, 5f };

    private void Awake()
    {
        currentMultiplier = timeMultipliers[currentMultiplierIndex];
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Period)){
            Debug.Log("Up");
        }
        if (Input.GetKeyUp(KeyCode.Comma)){
            Debug.Log("Down");
        }
        if (Input.GetKeyUp(KeyCode.Period) && IncrementMultiplier())
        {
            ChangeFixedUpdateTime();
            return;
        }
        if (Input.GetKeyUp(KeyCode.Comma) && DecrementMultiplier())
        {
            ChangeFixedUpdateTime();
        }
    }

    private void ChangeFixedUpdateTime()
    {
        float currentTimeScale = currentMultiplier;
        currentMultiplier = timeMultipliers[currentMultiplierIndex];
        Time.timeScale = currentMultiplier;
        Time.fixedDeltaTime *= Time.timeScale;
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
}
