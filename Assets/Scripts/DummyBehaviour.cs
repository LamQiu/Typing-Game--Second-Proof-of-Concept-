using Sirenix.OdinInspector.Editor;
using UnityEngine;

public class DummyBehaviour : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 1f);
    public int pressTime = 3;
    private int _currentPressTime;

    public void Initialize(Vector3 letterPos)
    {
        transform.SetPositionAndRotation(letterPos + offset, Quaternion.identity);
    }
    public bool TryKill()
    {
        _currentPressTime++;
        if (_currentPressTime >= pressTime)
        {
            Destroy(gameObject);
            return true;
        }
        return false;
    }
}
