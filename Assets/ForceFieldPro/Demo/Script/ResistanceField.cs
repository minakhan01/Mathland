using UnityEngine;
using System.Collections;

public class ResistanceField : ForceField.CustomFieldFunction
{
    [Range(0, 1)]
    public float speedDamp = 0.95f;
    [Range(0, 1)]
    public float angularSpeedDamp = 0.99f;

    public override Vector3 GetForce(Vector3 position, Rigidbody rigidbody)
    {
        if (rigidbody == null)
        {
            return Vector3.zero;
        }
        rigidbody.velocity *= speedDamp;
        rigidbody.angularVelocity *= angularSpeedDamp;
        return Vector3.zero;
    }
}
