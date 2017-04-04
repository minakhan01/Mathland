using UnityEngine;
using System.Collections;

/// <summary>
/// A example of the CustomFieldFunction.
/// It returns a circular force.
/// </summary>
public class SprialField : ForceField.CustomFieldFunction
{
    [FFToolTip("Size of the force.")]
    public float size = 100;

    [FFToolTip("If not, the force will grow with the distance")]
    public bool normalizeDirection = true;

    public override Vector3 GetForce(Vector3 position, Rigidbody rigidbody)
    {
        Vector3 force = new Vector3(position.z, 0, -position.x);
        if (normalizeDirection)
        {
            force = force.normalized;
        }
        return force * size;
    }
}
