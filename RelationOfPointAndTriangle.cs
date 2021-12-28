using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelationOfPointAndTriangle : MonoBehaviour {
    RelationOfPointAndTriangle(Vector3 p1,Vector3 p2,Vector3 p3) {
        this.top = p1;
        this.cw = p2;
        this.ccw = p3;
    }
    bool IsOnThePlane(Vector3 other)
    {
        Plane plane = new Plane(top, cw, ccw);
        if (plane.ClosestPointOnPlane(other) == other)
            return true;
        return false;
    }
    bool IsInTriangle(Vector3 other)
    {
        if (!IsOnThePlane(other))
            return false;
        Vector3 v0 = Vector3.Cross(cw - top, other - top);
        Vector3 v1 = Vector3.Cross(ccw - cw, other - cw);
        Vector3 v2 = Vector3.Cross(top - ccw, other - ccw);
        if (Vector3.Dot(v0, v1) < 0 || Vector3.Dot(v1, v2) < 0)//基于二维空间判断
            return false;
        return true;
    }
    Vector3 top, cw, ccw;//三角形三个点
    Vector3 other;
}
public class Intersection1 : MonoBehaviour
{
    /// <summary>
    /// 求直线与平面的交点
    /// </summary>
    /// <param name="plane">平面上任意一点</param>
    /// <param name="PointOnPlane"></param>
    /// <param name="head"></param>
    /// <param name="tail"></param>
    /// <returns></returns>
    private Vector3 GetTheIntersection(Plane plane,Vector3 PointOnPlane,Vector3  head ,Vector3 tail)
    {
        //Vector3 Intersection = Vector3.zero;
        Vector3 dir = head - tail;
        float scalar = Vector3.Dot(PointOnPlane - head, plane.normal) / Vector3.Dot(dir.normalized, plane.normal);//获得比例
        return scalar * dir.normalized + tail;
    }
    private void TranglesSegmentation(Vector3[] vertices, Vector3[] normals, int top, int cw, int ccw, out Vector3 Intersection1, out Vector3 Intersection2,
        Vector3 head1, Vector3 tail1, Vector3 head2, Vector3 tail2, Plane plane, Vector3 PointOnPlane)
    {
        Intersection1 = GetTheIntersection(plane, PointOnPlane, head1, tail1);
        Intersection2 = GetTheIntersection(plane, PointOnPlane, head2, tail2);

    }




    /// <summary>
    /// 计算直线与平面的交点
    /// </summary>
    /// <param name="point">直线上某一点</param>
    /// <param name="direct">直线的方向</param>
    /// <param name="planeNormal">垂直于平面的的向量</param>
    /// <param name="planePoint">平面上的任意一点</param>
    /// <returns></returns>
    private Vector3 GetIntersectWithLineAndPlane(Vector3 point, Vector3 direct, Vector3 planeNormal, Vector3 planePoint)
    {
        float d = Vector3.Dot(planePoint - point, planeNormal) / Vector3.Dot(direct.normalized, planeNormal);

        return d * direct.normalized + point;
    }
}
