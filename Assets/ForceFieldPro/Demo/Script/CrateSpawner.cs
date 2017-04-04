using UnityEngine;
using System.Collections;

public class CrateSpawner : MonoBehaviour
{

    public Transform box;
    public Vector3 pos;

    public int objNum = 0;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            spwanBox();
        }
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                spwanBox();
            }
        }

    }

    void spwanBox()
    {
        Transform t = GameObject.Instantiate(box) as Transform;
        t.position = pos;
        t.GetComponent<Rigidbody>().AddTorque(Random.value * 2 - 1, Random.value * 2 - 1, Random.value * 2 - 1);
        objNum++;
    }
}
