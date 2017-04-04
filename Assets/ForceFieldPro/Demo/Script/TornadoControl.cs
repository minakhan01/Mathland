using UnityEngine;
using System.Collections;

public class TornadoControl : MonoBehaviour
{
    [FFToolTip("The moving speed.")]
    float moveSpeed = 5;

    Transform thisTransform;
    // Use this for initialization
    void Start()
    {
        thisTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        MoveControl();
    }

    void MoveControl()
    {

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            thisTransform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            thisTransform.Translate(Vector3.back * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            thisTransform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            thisTransform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }

    }
}
