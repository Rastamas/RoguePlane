using System.Linq;
using UnityEngine;

public class UIPlatformSelector : MonoBehaviour
{
    public void Awake()
    {
        var userInterfaces = GetComponentsInChildren<Canvas>();
        var mobileUI = userInterfaces.FirstOrDefault(ui => ui.name.Contains("Mobile"));
        var desktopUI = userInterfaces.FirstOrDefault(ui => ui.name.Contains("Desktop"));

        if (mobileUI != null)
        {
            mobileUI.enabled = Application.isMobilePlatform || Application.isEditor;
        }

        if (desktopUI != null)
        {
            desktopUI.enabled = !Application.isMobilePlatform && !Application.isEditor;
        }
    }
}
