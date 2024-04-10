using System;
using UnityEngine;
using UnityEngine.Events;

public class PositionController : Singleton<PositionController>
{
    public UnityEvent<Vector3> positionUpdated = new UnityEvent<Vector3>();
    public Vector3 position {
        get {
            return _position;
        }
        set {
            _position = value;
            positionUpdated.Invoke(value);
        }
    }

    public GameObject indicator;

    [SerializeField]
    private Vector3 _position;
    
    public PositionController() : base() {
        indicator = GameObject.Find("Indicator");
        position = indicator.transform.position;
        positionUpdated.AddListener((Vector3 vec) => {
            if(indicator) {
                indicator.transform.position = vec;
            }
        });
    }
}
