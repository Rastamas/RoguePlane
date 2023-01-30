using UnityEngine;

public class ObjectEnabler : MonoBehaviour
{
    public GameObject button;
    public Collider triggerCollider;
    public int touchLimit;
    private int touchCount = 0;

    public void Update()
    {
        if (!ScreenUtil.StartingTouch || button.activeInHierarchy)
        {
            return;
        }

        var ray = Camera.main.ScreenPointToRay(ScreenUtil.TouchedPoint);

        if (!triggerCollider.Raycast(ray, out var _, 5000))
        {
            return;
        }

        touchCount++;

        if (touchCount >= touchLimit)
        {
            button.SetActive(true);
        }
    }
}
