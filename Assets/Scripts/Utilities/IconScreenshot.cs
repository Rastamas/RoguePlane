using System.IO;
using System.Linq;
using UnityEngine;

public class IconScreenshot : MonoBehaviour
{
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeScreenshot();
        }
    }

    private void TakeScreenshot()
    {
        var pathParts = Application.dataPath.Split('/').ToList();
        var path = string.Join('/', pathParts.Take(pathParts.Count - 2)) + "/Screenshots/" + pathParts.Skip(pathParts.Count - 2).First();

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        };

        var filename = path + "/Screenshot_" + System.DateTime.UtcNow.ToString("MMddyyyyHHmmss") + ".png";
        ScreenCapture.CaptureScreenshot(filename);

        Debug.Log(filename);
    }
}
