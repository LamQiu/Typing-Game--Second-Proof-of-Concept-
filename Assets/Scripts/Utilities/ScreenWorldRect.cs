using UnityEngine;

public class ScreenWorldRect : Singleton<ScreenWorldRect>
{
    public Camera cam;

    void OnDrawGizmos()
    {
        if (cam == null) cam = Camera.main;
        if (!cam.orthographic) return;

        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;

        Vector3 center = cam.transform.position;

        // The camera looks along its forward axis, so we offset corners accordingly
        Vector3 right = cam.transform.right * (width / 2f);
        Vector3 up = cam.transform.up * (height / 2f);

        // Get corners in world space
        Vector3 topLeft     = center - right + up;
        Vector3 topRight    = center + right + up;
        Vector3 bottomLeft  = center - right - up;
        Vector3 bottomRight = center + right - up;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }

    public Rect GetWorldRect2D()
    {
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;
        Vector3 center = cam.transform.position;
        return new Rect(center.x - width / 2f, center.y - height / 2f, width, height);
    }
}