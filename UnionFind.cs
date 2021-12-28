using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnionFind  {

    private int[] id;    //连通分量，用分量中的某个点作为索引
    private int count;    //分量的数量
    private int[] size;     //每个分量的节点数量

    public UnionFind(int n)
    {
        id = new int[n];
        size = new int[n];
        count = n;
        for (int i = 0; i < n; i++) {
            id[i] = i;
            size[i] = 1;
        }
    }

    public int getCount()
    {
        return count;
    }

    public bool isConnected(int p, int q)
    {
        return find(p) == find(q);
    }

    public int find(int p) {
        while (id[p] != p) p = id[p];
        return p;
    }

    public void union(int p, int q) {
        int pId = find(p);
        int qId = find(q);
        if (isConnected(p, q)) return;
        if (size[pId] < size[qId])
        {
            id[pId] = qId;
            size[qId] += size[pId];
        }
        else
        {
            id[qId] = pId;
            size[pId] += size[qId];
        }
        count--;
    }

    public List<int>[] getResult()
    {
        List<int>[] arrays = new List<int>[count];
        for (int i = 0; i < count; i++) //初始化
        {
            arrays[i] = new List<int>();
        }
        int index = 0;
        Dictionary<int, int> keyValuePairs = new Dictionary<int, int>(); // key是连通分量号（如：5,11,120），value是第几组（如：0,1,2）

        for (int i = 0; i<id.Length;i++)
        {
            int pId = find(i);
            if (!keyValuePairs.ContainsKey(pId))
            {
                keyValuePairs.Add(pId,index++);
            }
            arrays[keyValuePairs[pId]].Add(i);
        }

        return arrays;

    }
}
