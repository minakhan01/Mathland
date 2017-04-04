using UnityEngine;
using System.Collections;

public class SolarSystemGUI : MonoBehaviour
{
    CrateSpawner c;
    void Start()
    {
        c = GetComponent<CrateSpawner>();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(120, 10, 100, 20), "FPS: " + 1 / Time.deltaTime);
        GUI.Label(new Rect(220, 10, 100, 20), "Num: " + c.objNum);
    }
}
