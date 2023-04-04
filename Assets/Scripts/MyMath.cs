using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���w�֐�
/// </summary>
public static class MyMath
{
    /// <summary>
    /// �x�N�g���̂���������̔����v���̊p�x(0-360)���Z�o
    /// </summary>
    public static float GetAngular(Vector2 vector)
    {
        Quaternion q = Quaternion.FromToRotation(Vector2.right, vector);

        float z = q.eulerAngles.z;

        if (Mathf.Approximately(z, 0f))
        {
            //180�x��]�̏ꍇ�Ax, y��]�Ƃ݂Ȃ����z���O�ɂȂ��Ă���\��������
            if ((q.eulerAngles.x > 90f) || (q.eulerAngles.y > 90f))
            {
                z = 180f;
            }
        }

        return z;
    }

    /// <summary>
    /// �Q�̃x�N�g���̊p�x�i���v���j���Z�o
    /// </summary>
    public static float GetAngularDifference(Vector2 fromVec, Vector2 toVec)
    {
        Quaternion q = Quaternion.FromToRotation(fromVec, toVec);

        float z = q.eulerAngles.z;

        if (Mathf.Approximately(z, 0f))
        {
            //180�x��]�̏ꍇ�Ax, y��]�Ƃ݂Ȃ����z���O�ɂȂ��Ă���\��������
            if ((q.eulerAngles.x > 90f) || (q.eulerAngles.y > 90f))
            {
                z = 180f;
            }
        }

        return z;
    }

    /// <summary>
    /// �^����ꂽ�x�N�g���ɑ΂��A�^����ꂽ�_���E�ɂ��邩��Ԃ�
    /// </summary>
    public static bool IsRightFromVector(Vector2 point, Vector2 linePoint, Vector2 lineVector)
    {
        float angularDiference = Quaternion.FromToRotation(lineVector, point - linePoint).eulerAngles.z;

        if (angularDiference < 180f)
        {
            //���ɂ���
            return false;
        }
        else
        {
            //�E�ɂ���
            return true;
        }
    }

    /// <summary>
    /// �@���x�N�g�������߂�
    /// </summary>
    public static Vector2 GetPerpendicular(Vector2 vec)
    {
        return new Vector2(vec.y, -vec.x);
    }

    /// <summary>
    /// �p�̓񓙕����x�N�g��
    /// </summary>
    public static Vector2 GetBisector(Vector2 vec0, Vector2 vec1)
    {
        Vector2 u0 = vec0.normalized;
        Vector2 u1 = vec1.normalized;

        return (u0 + u1).normalized;
    }

    /// <summary>
    /// �_�ƒ����̋��������߂�
    /// </summary>
    public static float GetDistance(Vector2 point, Vector2 linePoint, Vector2 lineVector)
    {
        //point�̑��΍��W
        Vector2 pointToLineStart = point - linePoint;

        //point����̐��ˉe�_
        float dotProduct = Vector2.Dot(pointToLineStart, lineVector);
        Vector2 projection = linePoint + lineVector * dotProduct;

        //���΍��W�Ɛ��ˉe�_�̋��������߂�Ηǂ�
        return Vector2.Distance(point, projection);
    }

    /// <summary>
    /// �덷�������ē���l��Ԃ�
    /// </summary>
    public static bool IsSame(float v0, float v1, float threshold)
    {
        return (Mathf.Abs(v0 - v1) <= threshold);
    }

    /// <summary>
    /// ��̃x�N�g�������s���i臒l�ȉ��j���Ԃ�
    /// </summary>
    public static bool IsParallel(Vector2 vec0, Vector2 vec1, float threshold)
    {

        float angle = Vector2.Angle(vec0, vec1);

        if ((angle <= threshold)
            || (Mathf.Abs(angle - 180f) <= threshold)
            || (Mathf.Abs(angle - 360f) <= threshold))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// �ɍ��W���畽�ʍ��W�ɕϊ�
    /// </summary>
    public static Vector2 GetPositionFromPolar(Vector2 pole, float radius, float angular)
    {
        return pole + (Vector2)(Quaternion.Euler(0f, 0f, angular) * Vector2.right) * radius;
    }

    /// <summary>
    /// �_��������ɂ��邩���肷��
    /// </summary>
    public static bool CheckOnLine(Vector3 point, Vector3 linePoint, Vector3 lineVector, float threshold)
    {
        Vector3 difference = point - linePoint;

        //�O�ς̑傫�������߂�
        float outer = Vector3.Cross(lineVector, difference).magnitude;

        //0�Ȃ璼����
        return IsSame(outer, 0f, threshold);
    }

    /// <summary>
    /// �����̑������߂�
    /// </summary>
    public static Vector2 GetFootOfPerpendicular(Vector2 point, Vector2 linePoint, Vector2 lineVector)
    {
        Vector2 v = point - linePoint;
        float t = Vector2.Dot(v, lineVector) / lineVector.sqrMagnitude;
        Vector2 foot = linePoint + lineVector * t;

        return foot;
    }

    /// <summary>
    /// ��̐����̌�_�����߂�
    /// </summary>
    public static Vector2 GetIntersection(Vector2 line0Point, Vector2 line0Vector, Vector2 line1Point, Vector2 line1Vector)
    {
        // �O�ς����߂�
        float cross = line0Vector.x * line1Vector.y - line0Vector.y * line1Vector.x;

        // ���������s�ł���ꍇ
        if (Mathf.Approximately(cross, 0f))
        {
            Debug.LogError("���s");
            return Vector2.zero;
        }

        // ��_�����߂�
        float t = ((line1Point.x - line0Point.x) * line1Vector.y - (line1Point.y - line0Point.y) * line1Vector.x) / cross;
        Vector2 intersectionPoint = line0Point + line0Vector * t;

        return intersectionPoint;
    }
}
