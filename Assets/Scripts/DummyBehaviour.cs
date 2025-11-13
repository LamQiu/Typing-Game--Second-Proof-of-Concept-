using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

public class DummyBehaviour : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 1f);
    public int pressTime = 3;
    private int _currentPressTime;
    
    public GameObject dummyIndicator;
    public float indicatorSpacing = 0.5f;
    private Stack<GameObject> _indicators;

    public void Initialize(Vector3 letterPos)
    {
        transform.SetPositionAndRotation(letterPos + offset, Quaternion.identity);

        _indicators = new Stack<GameObject>();
        for (int i = 0; i < pressTime; i++)
        {
            var indicator = Instantiate(dummyIndicator, transform.position + new Vector3(0, indicatorSpacing * i, 0), Quaternion.identity);
            _indicators.Push(indicator);
        }
    }
    public bool TryKill()
    {
        _currentPressTime++;
        if (_indicators.Count > 0)
        {
            Destroy(_indicators.Pop());
        }
        if (_currentPressTime > pressTime)
        {
            Destroy(gameObject);
            return true;
        }
        return false;
    }
}
