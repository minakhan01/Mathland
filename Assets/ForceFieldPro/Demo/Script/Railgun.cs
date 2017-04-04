using UnityEngine;
using System.Collections;

public class Railgun : MonoBehaviour
{
    [FFToolTip("Bullet prefab.")]
    public Transform bullet;

    [FFToolTip("Cool down time of the gun.")]
    public float cd = 0.5f;

    [FFToolTip("The life time of bullets.")]
    public float bulletLife = 5;

    [FFToolTip("The force field.")]
    public ForceField field;

    [FFToolTip("The force applied on the bullets.")]
    public float force = 1000;

    [FFToolTip("Should bullets use gravity?")]
    public bool bulletGarvity = true;

    bool cdFlag = true;

    Vector3 pos = new Vector3(0, 1.5f, -0.3f);

    // Use this for initialization
    void Start()
    {
        Physics.IgnoreLayerCollision(8, 8);
    }

    // Update is called once per frame
    void Update()
    {
        field.generalMultiplier = force;
        if (Input.GetMouseButton(0))
        {
            if (cdFlag)
            {
                shot();
            }
        }
    }

    void shot()
    {
        cdFlag = !cdFlag;
        StartCoroutine("cooldown");
        Transform t = GameObject.Instantiate(bullet) as Transform;
        t.position = transform.TransformPoint(pos);
        t.rotation = transform.rotation;
        t.GetComponent<Rigidbody>().useGravity = bulletGarvity;
        StartCoroutine(SetLifeTime(bulletLife, t.gameObject));
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
}
