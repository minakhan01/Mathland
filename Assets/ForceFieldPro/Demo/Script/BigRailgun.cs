using UnityEngine;
using System.Collections;

public class BigRailgun : MonoBehaviour
{
    [FFToolTip("Rotate speed.")]
    public float rotateSpeed = 0.1f;

    [FFToolTip("Lower bound of the rotation.")]
    public float lowAngle = 0;

    [FFToolTip("Upper bound of the rotation.")]
    public float highAngle = 10;

    [FFToolTip("Cannon ball prefab.")]
    public Transform ball;

    [FFToolTip("Cool down time of the gun.")]
    public float cd = 0.5f;

    [FFToolTip("Bullet life time.")]
    public float bulletLife = 60;

    bool cdFlag = true;

    float angle;

    // Use this for initialization
    void Start()
    {
        angle = transform.rotation.eulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        MoveControl();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (cdFlag)
            {
                shot();
            }
        }
    }

    void MoveControl()
    {

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            angle += rotateSpeed;
            if (angle > highAngle)
            {
                angle = highAngle;
            }
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            angle -= rotateSpeed;
            if (angle < lowAngle)
            {
                angle = lowAngle;
            }
        }
        transform.rotation = Quaternion.AngleAxis(angle, -Vector3.right);
    }

    IEnumerator cooldown()
    {
        yield return new WaitForSeconds(cd);
        cdFlag = true;
    }

    IEnumerator SetLifeTime(float t, GameObject go)
    {
        yield return new WaitForSeconds(t);
        Destroy(go);
    }

    void shot()
    {
        cdFlag = !cdFlag;
        StartCoroutine("cooldown");
        Transform t = GameObject.Instantiate(ball) as Transform;
        t.position = new Vector3(5, 9.5f, -13);
        StartCoroutine(SetLifeTime(bulletLife, t.gameObject));
    }
}
