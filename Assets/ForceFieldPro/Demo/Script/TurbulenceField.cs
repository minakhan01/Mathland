using UnityEngine;
using System.Collections;

/// <summary>
/// This field applies some random forces and torques to make the water vivid.
/// </summary>
public class TurbulenceField : ForceField.CustomFieldFunction
{
    [FFToolTip("Size of the random torque.")]
    public float torqueSize = 1;

    [FFToolTip("Size of the random force.")]
    public float forceSize = 1;

    public override Vector3 GetForce(Vector3 position, Rigidbody rigidbody)
    {
        if (rigidbody == null)
        {
            return Vector3.zero;
        }
        Vector3 torque = new Vector3(Random.value * 2 - 1, Random.value * 2 - 1, Random.value * 2 - 1) * torqueSize;
        Vector3 force = new Vector3(Random.value * 2 - 1, Random.value * 2 - 1, Random.value * 2 - 1) * forceSize;
        rigidbody.AddTorque(torque);
        return force;
    }
}
