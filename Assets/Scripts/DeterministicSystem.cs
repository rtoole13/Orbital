using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class DeterministicSystem : MonoBehaviour
{
    private static DeterministicSystem instance;
    public static DeterministicSystem Instance
    {
        get
        {
            if (!instance)
            {
                DeterministicSystem[] systems = FindObjectsOfType<DeterministicSystem>();
                if (systems.Length > 1)
                {
                    string systemParentObjects = systems[0].name;
                    for (int i = 1; i < systems.Length; i++)
                    {
                        systemParentObjects = string.Format("{0}, {1}", systemParentObjects, systems[i].name);
                    }
                    throw new UnityException(string.Format("{0} all have DeterministicSystems in the scene! Must have just one.", systemParentObjects));
                }
                else if (systems.Length == 0)
                {
                    // Impossible to hit this..
                    throw new UnityException(string.Format("No DeterministicSystem found in the scene! Add a DeterministicSystem to a game object."));
                }
                instance = systems[0];
            }
            return instance;
        }
    }

    [SerializeField]
    private GravitySource _primarySource;
    public GravitySource PrimarySource
    {
        get { return _primarySource; }
    }

    private List<GravityAffected> gravityAffecteds = new List<GravityAffected>();

    private void Awake()
    {
        if (_primarySource.CurrentGravitySource != null)
        {
            throw new UnityException(string.Format("System's root gravity source, '{0}', must not have a CurrentGravitySource!", _primarySource.name));
        }
        _primarySource.InitializeSystem(null);
    }

    void Start()
    {
        
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        _primarySource.UpdateSystem();
    }
}
