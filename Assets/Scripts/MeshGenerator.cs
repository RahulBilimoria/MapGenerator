using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

    public SquareGrid squareGrid;
    List<Vector3> vertices;
    List<int> triangles;

    public void GenerateMesh(int[,] map, float squareSize) {
        squareGrid = new SquareGrid(map, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++) {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++) {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void TriangulateSquare(Square square) {
        switch (square.configuration) {
            // no points
            case 0:
                break;
            // one point
            case 1:
                MeshFromPoints(square.bottom, square.bottomLeft, square.left);
                break;
            case 2:
                MeshFromPoints(square.right, square.bottomRight, square.bottom);
                break;
            case 4:
                MeshFromPoints(square.top, square.topRight, square.right);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.top, square.left);
                break;
            // two points
            case 3:
                MeshFromPoints(square.right, square.bottomRight, square.bottomLeft, square.left);
                break;
            case 6:
                MeshFromPoints(square.top, square.topRight, square.bottomRight, square.bottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.top, square.bottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.right, square.left);
                break;
            // diagonal two points
            case 5:
                MeshFromPoints(square.top, square.topRight, square.right, square.bottom, square.bottomLeft, square.left);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.top, square.right, square.bottomRight, square.bottom, square.left);
                break;
            // three points
            case 7:
                MeshFromPoints(square.top, square.topRight, square.bottomRight, square.bottomLeft, square.left);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.top, square.right, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.right, square.bottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottom, square.left);
                break;
            // four points
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                break;
        }
    }

    void MeshFromPoints(params Node[] points) {
        AssignVertices(points);

        if (points.Length >= 3) {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if (points.Length >= 4) {
            CreateTriangle(points[0], points[2], points[3]);
        }
        if (points.Length >= 5) {
            CreateTriangle(points[0], points[3], points[4]);
        }
        if (points.Length >= 6) {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }

    void AssignVertices(Node[] points) {
        for (int i = 0; i < points.Length; i++) {
            if (points[i].vertexIndex == -1) {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c) {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);
    }

    private void OnDrawGizmos() {
        /*if (squareGrid != null) {
            for (int x = 0; x < squareGrid.squares.GetLength(0); x++) {
                for (int y = 0; y < squareGrid.squares.GetLength(1); y++) {
                    Gizmos.color = (squareGrid.squares[x, y].topLeft.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].topLeft.position, Vector3.one * 0.4f);

                    Gizmos.color = (squareGrid.squares[x, y].topRight.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].topRight.position, Vector3.one * 0.4f);

                    Gizmos.color = (squareGrid.squares[x, y].bottomRight.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].bottomRight.position, Vector3.one * 0.4f);
                                                             
                    Gizmos.color = (squareGrid.squares[x, y].bottomLeft.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].bottomLeft.position, Vector3.one * 0.4f);

                    Gizmos.color = Color.gray;
                    Gizmos.DrawCube(squareGrid.squares[x, y].top.position, Vector3.one * 0.15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].right.position, Vector3.one * 0.15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].bottom.position, Vector3.one * 0.15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].left.position, Vector3.one * 0.15f);
                }
            }
        }*/
    }

    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize) {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);

            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++) {
                for (int y = 0; y < nodeCountY; y++) {
                    Vector3 position = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                    controlNodes[x, y] = new ControlNode(position, map[x, y] == 1, squareSize);
                }
            }
            squares = new Square[nodeCountX - 1, nodeCountY - 1];

            for (int x = 0; x < nodeCountX - 1; x++) {
                for (int y = 0; y < nodeCountY - 1; y++) {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);

                }
            }
        }
    }

    public class Square
    {
        public ControlNode topLeft, topRight, bottomLeft, bottomRight;
        public Node top, bottom, left, right;
        public int configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            top = topLeft.right;
            right = bottomRight.above;
            bottom = bottomLeft.right;
            left = bottomLeft.above;

            if (topLeft.active) {
                configuration += 8;
            } if (topRight.active) {
                configuration += 4;
            } if (bottomRight.active) {
                configuration += 2;
            } if (bottomLeft.active) {
                configuration += 1;
            }
        }
    }

	public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _position)
        {
            position = _position;
        } 
    }

    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _position, bool _active, float squareSize) : base(_position)
        {
            active = _active;
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);

        }
    }
}
