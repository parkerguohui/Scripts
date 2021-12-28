using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 修复网格间隙：合并网格临近点
/// 效率较低
/// </summary>
public class MergeVertices
{
    static List<Vector3> ReregisterVertices(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        List<Vector3> newVertices = new List<Vector3>();
        for (int i = 0; i < vertices.Length; i++)
        {
            if (FindClone(vertices[i], newVertices) == -1)
            {
                newVertices.Add(vertices[i]);
            }
        }
        return newVertices;
    }
    static List<int> RecalculateIndices(List<Vector3> newVertices, Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] indices = mesh.triangles;

        List<int> newIndices = new List<int>();
        for (int triangle = 0; triangle < indices.Length / 3; triangle++)
        {
            int[] index = new int[] { indices[triangle * 3 + 0], indices[triangle * 3 + 1], indices[triangle * 3 + 2] };
            Vector3[] vert = new Vector3[3];
            vert[0] = vertices[index[0]];
            vert[1] = vertices[index[1]];
            vert[2] = vertices[index[2]];

            int newIndexVert0 = FindClone(vert[0], newVertices);
            int newIndexVert1 = FindClone(vert[1], newVertices);
            int newIndexVert2 = FindClone(vert[2], newVertices);

            newIndices.Add(newIndexVert0);
            newIndices.Add(newIndexVert1);
            newIndices.Add(newIndexVert2);
        }
        return newIndices;
    }
    public static Mesh MergeCloseVertices(Mesh mesh)
    {
        List<Vector3> newVertices = ReregisterVertices(mesh);
        List<int> newIndices = RecalculateIndices(newVertices, mesh);

        Mesh mergedIcosahedron = new Mesh();
        mergedIcosahedron.vertices = newVertices.ToArray();
        mergedIcosahedron.triangles = newIndices.ToArray();

        mergedIcosahedron.RecalculateNormals();
        mergedIcosahedron.RecalculateBounds();

        return mergedIcosahedron;
    }

    static int FindClone(Vector3 vertex, List<Vector3> vertices)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            //if (IsSameVertex(vertex, vertices[i]))
            if (IsCloseVertex(vertex, vertices[i]))
            {
                return i;
            }
        }

        return -1;
    }
    static bool IsSameVertex(Vector3 vertA, Vector3 vertB)
    {
        return ((vertA - vertB).magnitude < 0.01f);
    }
    static bool IsCloseVertex(Vector3 vertA, Vector3 vertB)
    {
        return ((vertA - vertB).magnitude < 0.0001f);
    }
}