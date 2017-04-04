using UnityEngine;
using System.Collections;

public class InitialPush : MonoBehaviour
{
    public float size = 100;
    // Use this for initialization
    void Start()
    {
        GetComponent<Rigidbody>().AddForce(new Vector3(Random.value * 2 - 1, Random.value * 2 - 1, Random.value * 2 - 1).normalized * size);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
