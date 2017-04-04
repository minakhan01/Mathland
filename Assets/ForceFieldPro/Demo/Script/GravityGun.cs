using UnityEngine;
using System.Collections;

/// <summary>
/// Gravity gun!
/// </summary>
public class GravityGun : MonoBehaviour
{
    [FFToolTip("Force field.")]
    public ForceField ff;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            ff.generalMultiplier = 1;
        }
        else if (Input.GetMouseButton(1))
        {
            ff.generalMultiplier = -1;
        }
        else
        {
            ff.generalMultiplier = 0;
        }
    }
}
