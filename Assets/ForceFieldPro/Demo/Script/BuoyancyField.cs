using UnityEngine;
using System.Collections;

/// <summary>
/// This is a buoyancy field.
/// It will estimate the underwater volume of the target,
/// and calculate the buoyancy force.
/// </summary>
public class BuoyancyField : ForceField.CustomFieldFunction
{
    [FFToolTip("Y value of the water level.")]
    public float waterLevel = 0;

    [FFToolTip("Coefficient that controls the size of the buoyancy.")]
    public float coefficient = 1;

    [FFToolTip("The sample size used to estimate the underwater volume.\nThe smaller the better, the slower.")]
    public float sampleSize = 0.5f;

    [FFToolTip("The sample number per side is bound by this value.\nThe bigger the better, the slower.")]
    public int maxSampleNum = 4;

    [FFToolTip("The sample number per side is bound by this value.\nThe bigger the better, the slower.")]
    public int minSampleNum = 2;

    public override Vector3 GetForce(Vector3 position, Rigidbody rigidbody)
    {
        //always check null if you use rigidbody
        if (rigidbody == null)
        {
            return Vector3.zero;
        }
        return -coefficient * Physics.gravity * EstimateSubmergedVolume(rigidbody);
    }

    //this method is not accurate for concave collider
    //basically it cut the bounds of the rigidbody in to pieces, and then use raycast to get the true volume that occupied by the collider
    float EstimateSubmergedVolume(Rigidbody rigidbody)
    {
        Ray r;
        RaycastHit hit;
        Collider collider = rigidbody.GetComponent<Collider>();
        Bounds b = collider.bounds;
        float height = b.size.y + 2;
        float dh = (waterLevel - b.min.y);
        if (dh <= 0)
        {
            return 0;
        }
        int countX = Mathf.CeilToInt(b.size.x / sampleSize);
        countX = countX > maxSampleNum ? maxSampleNum : countX;
        countX = countX < minSampleNum ? minSampleNum : countX;
        int countZ = Mathf.CeilToInt(b.size.z / sampleSize);
        countZ = countZ > maxSampleNum ? maxSampleNum : countZ;
        countZ = countZ < minSampleNum ? minSampleNum : countZ;
        float dx = b.size.x / countX;
        float dz = b.size.z / countZ;

        float total = 0;
        for (int i = 0; i < countX; i++)
        {
            for (int j = 0; j < countZ; j++)
            {
                float top = 0;
                float bottom = 0;
                r = new Ray(new Vector3(b.min.x + dx * (0.5f + i), waterLevel + 1, b.min.z + dz * (0.5f + j)), Vector3.down);
                if (collider.Raycast(r, out hit, height))
                {
                    top = hit.distance - 1;
                    if (top < 0)
                    {
                        top = 0;
                    }
                }
                r = new Ray(new Vector3(b.min.x + dx * (0.5f + i), b.min.y - 1, b.min.z + dz * (0.5f + j)), Vector3.up);
                if (collider.Raycast(r, out hit, height))
                {
                    bottom = hit.distance - 1;
                    if (bottom < 0)
                    {
                        bottom = 0;
                    }
                }
                if (top != 0 || bottom != 0)
                {
                    total += dh;
                    total -= top;
                    total -= bottom;
                }
            }
        }
        return total * dx * dz;
    }
}
