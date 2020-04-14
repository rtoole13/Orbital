using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeterministicSystem : MonoBehaviour
{
    [SerializeField]
    private GravitySource primarySource;

    private void Awake()
    {
        if (primarySource.CurrentGravitySource != null)
        {
            throw new UnityException(string.Format("System's root gravity source, '{0}', must not have a CurrentGravitySource!", primarySource.name));
        }
        primarySource.InitializeSystem(null);
    }

    void Start()
    {
        
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        primarySource.UpdateSystem();
    }
}
