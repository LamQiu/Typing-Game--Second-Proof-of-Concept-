using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [Header("Facing Options")]
    [SerializeField] private bool faceCameraForward = true;
    [SerializeField] private bool reverseYAxis = false;

    [Header("Axis Locks")]
    [SerializeField] private bool lockXRotation = false;
    [SerializeField] private bool lockYRotation = false;
    [SerializeField] private bool lockZRotation = false;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        Vector3 direction = faceCameraForward
            ? cam.transform.forward
            : transform.position - cam.transform.position;

        // Flip Y axis if needed
        if (reverseYAxis)
            direction.y *= -1;

        transform.rotation = Quaternion.LookRotation(direction);

        // Apply axis locks
        Vector3 euler = transform.rotation.eulerAngles;
        if (lockXRotation) euler.x = 0;
        if (lockYRotation) euler.y = 0;
        if (lockZRotation) euler.z = 0;

        transform.rotation = Quaternion.Euler(euler);
    }
}