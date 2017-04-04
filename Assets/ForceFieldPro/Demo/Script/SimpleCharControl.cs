using UnityEngine;
using System.Collections;

/// <summary>
/// A simple FPS controller that works with the planetary gravity.
/// </summary>
public class SimpleCharControl : MonoBehaviour
{
    [FFToolTip("Anchor of the center of the gravity.")]
    public Transform center;

    [FFToolTip("Move speed.")]
    public float moveSpeed = 5;

    [FFToolTip("The force that push up.")]
    public float jumpForce = 500;

    [FFToolTip("Character camera.")]
    public Camera cam;

    [FFToolTip("Lower bound of the camera rotation.")]
    public float lowAngle = -40;

    [FFToolTip("Upper bound of the camera rotation.")]
    public float highAngle = 70;

    [FFToolTip("Horizontal rotation speed.")]
    public float hRotSpeed = 20;

    [FFToolTip("Vertical rotation speed.")]
    public float vRotSpeed = 5;

    float camAngle = 0;


    Transform thisTransform;

    // Use this for initialization
    void Start()
    {
        thisTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        StandUp();
        MoveControl();
        UpdateCamera();

    }

    //make the character always stand vertical on the surface.
    void StandUp()
    {
        Vector3 d = thisTransform.position - center.position;
        Vector3 view = thisTransform.forward;
        Vector3.OrthoNormalize(ref d, ref view);
        thisTransform.rotation = Quaternion.LookRotation(view, d);
    }

    //rotate the camera based on the mouse motions
    void UpdateCamera()
    {
        float vRot = vRotSpeed * Input.GetAxis("Mouse Y");
        camAngle += vRot;
        if (camAngle > highAngle)
        {
            camAngle = highAngle;
        }
        if (camAngle < lowAngle)
        {
            camAngle = lowAngle;
        }
        cam.transform.rotation = Quaternion.AngleAxis(camAngle, -thisTransform.right) * thisTransform.rotation;
    }

    //move the character
    void MoveControl()
    {
        float hRot = hRotSpeed * Input.GetAxis("Mouse X");
        thisTransform.rotation = Quaternion.AngleAxis(hRot, thisTransform.up) * thisTransform.rotation;

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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<Rigidbody>().AddForce(thisTransform.up * jumpForce);
        }
    }
}
