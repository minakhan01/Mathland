using UnityEngine;
using System.Collections;

public class GUIScript : MonoBehaviour
{
    public string helpInfo = "";

    bool flag = true;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 20), "Reset Scene"))
        {
            Application.LoadLevel(Application.loadedLevelName);
        }
        flag = GUI.Toggle(new Rect(10, 35, 100, 20), flag, "Show Info");
        if (flag)
        {
            GUI.Box(new Rect(10, 60, 300, 300), "Help Info");
            GUI.Label(new Rect(15, 80, 295, 280), helpInfo);
        }
    }
}
