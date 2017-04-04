using UnityEngine;
using System.Collections;

public class FFGizmos
{
    public static void DrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f, float threshold = 0.1f)
    {
        if (float.IsNaN(direction.x) || float.IsNaN(direction.y) || float.IsNaN(direction.z) || direction.magnitude < threshold || direction == Vector3.zero)
        {
            return;
        }
        Gizmos.DrawRay(pos, direction);
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * Vector3.back;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * Vector3.back;
        Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back;
        Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back;
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, up * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, down * arrowHeadLength);
    }

    public static void DrawCapsule(Vector3 point1, Vector3 point2, float radius, int laNum = 16, int halfLoNum = 4)
    {
        DrawCylinder(point1, point2, radius, laNum);
        if (radius == 0)
        {
            return;
        }
        DrawSemisphere(point1, point1 - point2, radius, laNum, halfLoNum);
        DrawSemisphere(point2, point2 - point1, radius, laNum, halfLoNum);
    }

    public static void DrawCapsuleByHeight(Vector3 point, Vector3 height, float radius, int laNum = 16, int halfLoNum = 4)
    {
        DrawCapsule(point, point + height, radius, laNum, halfLoNum);
    }

    public static void DrawCylinder(Vector3 point1, Vector3 point2, float r, int pointNum = 16, bool drawBase = false)
    {
        if (r == 0)
        {
            Gizmos.DrawLine(point1, point2);
        }
        Vector3 dir = point2 - point1;
        if (dir == Vector3.zero)
        {
            return;
        }
        Vector3 radiusVector = Vector3.Cross(Vector3.Cross(dir, (dir.normalized == Vector3.up || dir.normalized == Vector3.down) ? Vector3.right : Vector3.up), dir).normalized * r;
        float delta = 360f / pointNum;
        Vector3 p1;
        Vector3 p2;
        Vector3 p3;
        Vector3 p4;
        p1 = point1 + radiusVector;
        p2 = point2 + radiusVector;
        for (int i = 0; i < pointNum; i++)
        {
            Gizmos.DrawLine(p1, p2);
            if (drawBase)
            {
                Gizmos.DrawLine(point1, p1);
                Gizmos.DrawLine(point2, p2);
            }
            radiusVector = Quaternion.AngleAxis(delta, dir) * radiusVector;
            p3 = point1 + radiusVector;
            p4 = point2 + radiusVector;
            Gizmos.DrawLine(p1, p3);
            Gizmos.DrawLine(p2, p4);
            p1 = p3;
            p2 = p4;
        }
    }

    public static void DrawSemisphere(Vector3 center, Vector3 direction, float radius, int laNum = 16, int halfLoNum = 4, bool drawBase = false)
    {
        if (radius == 0 || laNum < 4 || halfLoNum < 1)
        {
            return;
        }
        direction = direction.normalized;
        Vector3 top = center + direction * radius;
        float laDeg = 360f / laNum;
        float loDeg = 90f / halfLoNum;
        Vector3 rotAxis = Vector3.Cross(direction, (direction == Vector3.up || direction == Vector3.down) ? Vector3.right : Vector3.up);
        Vector3[] vecs1 = new Vector3[halfLoNum];
        Vector3[] vecs2 = new Vector3[halfLoNum];
        for (int i = 0; i < halfLoNum; i++)
        {
            vecs1[i] = Quaternion.AngleAxis(loDeg * (i + 1), rotAxis) * direction;
        }
        for (int j = 0; j < laNum; j++)
        {
            vecs1.CopyTo(vecs2, 0);
            Gizmos.DrawLine(top, vecs1[0] * radius + center);
            for (int i = 0; i < halfLoNum; i++)
            {
                if (i + 1 < halfLoNum)
                {
                    Gizmos.DrawLine(vecs1[i] * radius + center, vecs1[i + 1] * radius + center);
                }
                else if (drawBase)
                {
                    Gizmos.DrawLine(vecs1[i] * radius + center, center);
                }
                vecs1[i] = Quaternion.AngleAxis(laDeg, direction) * vecs2[i];
                Gizmos.DrawLine(vecs1[i] * radius + center, vecs2[i] * radius + center);
            }
        }
    }

    public static void DrawSphere(Vector3 center, float radius, int laNum = 16, int loNum = 7)
    {
        if (radius == 0 || laNum < 4 || loNum < 2)
        {
            return;
        }
        Vector3 top = center + Vector3.up * radius;
        Vector3 bottom = center + Vector3.down * radius;
        float laDeg = 360f / laNum;
        float loDeg = 180f / (loNum + 1);
        Vector3[] vecs1 = new Vector3[loNum];
        Vector3[] vecs2 = new Vector3[loNum];
        for (int i = 0; i < loNum; i++)
        {
            vecs1[i] = Quaternion.AngleAxis(loDeg * (i + 1), Vector3.right) * Vector3.up;
        }
        for (int j = 0; j < laNum; j++)
        {
            vecs1.CopyTo(vecs2, 0);
            Gizmos.DrawLine(top, vecs1[0] * radius + center);
            for (int i = 0; i < loNum; i++)
            {
                if (i + 1 < loNum)
                {
                    Gizmos.DrawLine(vecs1[i] * radius + center, vecs1[i + 1] * radius + center);
                }
                else
                {
                    Gizmos.DrawLine(vecs1[i] * radius + center, bottom);
                }
                vecs1[i] = Quaternion.AngleAxis(laDeg, Vector3.up) * vecs2[i];
                Gizmos.DrawLine(vecs1[i] * radius + center, vecs2[i] * radius + center);
            }
        }
    }

    public static void DrawPolygon(params Vector3[] points)
    {
        int length = points.Length;
        if (length <= 1)
        {
            return;
        }
        for (int i = 0; i < length; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1 < length ? i + 1 : 0]);
        }
    }

    public static void DrawCircle(Vector3 center, Vector3 normal, float radius, int points = 32)
    {
        Vector3 radiusVec = (normal.normalized == Vector3.up || normal.normalized == Vector3.down) ? Vector3.right : Vector3.up;
        Vector3.OrthoNormalize(ref normal, ref radiusVec);
        Vector3 point1 = center + radiusVec * radius;
        Vector3 point2;
        float angle = 360f / points;
        for (int i = 0; i < points; i++)
        {
            point2 = point1;
            point1 = Quaternion.AngleAxis(angle * (i + 1), normal) * radiusVec * radius + center;
            Gizmos.DrawLine(point1, point2);
        }
    }
}
