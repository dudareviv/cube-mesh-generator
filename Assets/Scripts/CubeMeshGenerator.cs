using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeMeshGenerator : MonoBehaviour
{
    public Mesh CubeMesh;

    private List<Vector3> _vertices;
    private List<int> _triangles;

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = CubeMesh = new Mesh();
        CubeMesh.name = "Playground Chunk Mesh";

        _vertices = new List<Vector3>();
        _triangles = new List<int>();
    }

    private void Start()
    {
        CubeMesh.Clear();
        _vertices.Clear();
        _triangles.Clear();

        var x = (int) transform.position.x;
        var y = (int) transform.position.y;
        var z = (int) transform.position.z;

        var blockVertices = new Vector3[8];
        
        for (var i = 0; i < 8; i++)
        {
            /*
             * N| X Y Z
             * ========
             * 0| 0 0 0
             * 1| 0 0 1
             * 2| 0 1 0
             * 3| 0 1 1
             * 4| 1 0 0
             * 5| 1 0 1
             * 6| 1 1 0
             * 7| 1 1 1
             * _______
             * P: 2 1 0 -- сдвиг
             */

            // Сдвигаем на P бит
            var newX = x + (i >> 2 & 1);
            var newY = y + (i >> 1 & 1);
            var newZ = z + (i >> 0 & 1);
            blockVertices[i] = new Vector3(newX, newY, newZ);
        }
        
        // Пробегаем все стороны
        /*
         * N| D A |Side    | X | Y | Z |
         * =============================
         * 0| 0 0 |Back    | 0 | 0 |-1 |
         * 1| 1 1 |Up      | 0 | 1 | 0 |
         * 2| 0 2 |Left    |-1 | 0 | 0 |
         * 3| 1 0 |Forward | 0 | 0 | 1 |
         * 4| 0 1 |Down    | 0 |-1 | 0 |
         * 5| 1 2 |Right   | 1 | 0 | 0 |
         */
        for (var i = 0; i < 6; i++)
        {
            var sign = i % 2;
            var pos = i % 3;

            var vertices = new int[4];
            var a = (sign == 1 ? (1 << pos) : 0);

            vertices[0] = a + (0 << ((pos + 1) % 3)) + (0 << ((pos + 2) % 3));
            vertices[1] = a + (0 << ((pos + 1) % 3)) + (1 << ((pos + 2) % 3));
            vertices[2] = a + (1 << ((pos + 1) % 3)) + (0 << ((pos + 2) % 3));
            vertices[3] = a + (1 << ((pos + 1) % 3)) + (1 << ((pos + 2) % 3));

            Array.Sort(vertices);

            vertices[0] = AddVertex(blockVertices[vertices[0]]);
            vertices[1] = AddVertex(blockVertices[vertices[1]]);
            vertices[2] = AddVertex(blockVertices[vertices[2]]);
            vertices[3] = AddVertex(blockVertices[vertices[3]]);


            /**
             * В индексах вершин есть закономерность, которую можно использовать:
             *
             * 6---7
             * | 5 |
             * 4---5---7
             * | 4 | 3 |
             * 0---1---3---7
             *     | 2 | 1 |
             *     0---2---6
             *         | 0 |
             *         0---4
             * 
             * Минимальный и максимальный лежат по диагонали друг от друга, а индексы между ними
             * можно использовать следующим образом:
             *  для сторон 0-2
             *     0-1-3
             *     0-3-2
             *  для сторон 3-5
             *     0-2-3
             *     0-3-1
             */
            if (i < 3)
            {
                AddTriangle(vertices[0], vertices[1], vertices[3]);
                AddTriangle(vertices[0], vertices[3], vertices[2]);
            }
            else
            {
                AddTriangle(vertices[0], vertices[2], vertices[3]);
                AddTriangle(vertices[0], vertices[3], vertices[1]);
            }
        }

        CubeMesh.vertices = _vertices.ToArray();
        CubeMesh.triangles = _triangles.ToArray();
        CubeMesh.RecalculateNormals();
    }

    private int AddVertex(Vector3 value)
    {
        var vertexIndex = _vertices.Count;
        _vertices.Add(value);
        return vertexIndex;
    }

    private void AddTriangle(int v1, int v2, int v3)
    {
        _triangles.Add(v1);
        _triangles.Add(v2);
        _triangles.Add(v3);
    }
}