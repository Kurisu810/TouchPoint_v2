using UnityEngine;

public class DebugTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Logger.Instance.LogInfo("Test start");
    }

    // Update is called once per frame
    void Update()
    {
        Logger.Instance.LogInfo("Test update");
    }
}
