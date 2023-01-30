using UnityEngine;

public static class ScreenUtil
{
    public static Vector3 TouchedPoint => Application.isMobilePlatform
            ? new Vector3(Input.touches[0].position.x, Input.touches[0].position.y, 0)
            : Input.mousePosition;

    public static bool TouchingScreen => Application.isMobilePlatform
            ? Input.touchCount > 0
            : Input.GetMouseButton(0);

    public static bool StartingTouch => Application.isMobilePlatform
        ? Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began
        : Input.GetMouseButtonDown(0);
}
