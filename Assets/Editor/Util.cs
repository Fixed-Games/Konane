using UnityEditor;
using UnityEngine;

public static class Util
{
    [MenuItem("Custom/Screenshot")]
    public static void Screenshot()
    {
        string filename = string.Format("Screenshot {0:s}.png", System.DateTime.Now).Replace(":", string.Empty);
        Debug.LogWarningFormat("{0}/{1}", Application.persistentDataPath, filename);
        ScreenCapture.CaptureScreenshot(filename);
    }
}
