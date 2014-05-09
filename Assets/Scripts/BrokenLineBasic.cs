using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class BrokenLineBasic : MonoBehaviour 
{
    public List<Vector2> points = new List<Vector2>();
    List<Vector3> vert = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> triangles = new List<int>();

    public Color color;

    float halfWidthLine = 0.0f;
    public float widthLine = 0.1f;        

    public Mesh newMesh;
    MeshFilter mesh;
    MeshRenderer meshRend;

    void Awake()
    {
        mesh = GetComponent<MeshFilter>();
        meshRend = GetComponent<MeshRenderer>();
        meshRend.material.color = color;
    }

	public void CreateLine () 
    {
        vert.Clear();
        uvs.Clear();
        triangles.Clear();

        halfWidthLine = widthLine / 2.0f;
        GenerateBrokenLine();
	}
	

    void GenerateBrokenLine()
    {
        foreach (Vector2 p in points)
        {
            Vector3 v0 = new Vector3(p.x, p.y, 0);
            Vector3 v1 = new Vector3(p.x, p.y, 0);
            Vector3 v2 = new Vector3(p.x, p.y, 0);
            if (p == points[0])
            {
                Vector2 dirLine = points[points.IndexOf(p) + 1] - p;
                CreateEndLine(-dirLine, p);
                v1 += new Vector3(-dirLine.y, dirLine.x, 0).normalized * halfWidthLine;
                v2 += new Vector3(dirLine.y, -dirLine.x, 0).normalized * halfWidthLine;
                GenerateUV(new Vector2(0, 0.5f));
                GenerateUV(new Vector2(0, 1.0f));
                GenerateUV(new Vector2(0, 0));
                GenerateUV(new Vector2(0.5f, 0.5f));
                GenerateUV(new Vector2(0.5f, 1.0f));
                GenerateUV(new Vector2(0.5f, 0));
            }
            else if(p == points[points.Count - 1])
            {
                Vector2 dirLine = points[points.IndexOf(p) - 1] - p;
                CreateEndLine(dirLine, p);
                v1 += new Vector3(dirLine.y, -dirLine.x, 0).normalized * halfWidthLine;
                v2 += new Vector3(-dirLine.y, dirLine.x, 0).normalized * halfWidthLine;
                GenerateUV(new Vector2(0.5f, 0.5f));
                GenerateUV(new Vector2(0.5f, 1.0f));
                GenerateUV(new Vector2(0.5f, 0));
                GenerateUV(new Vector2(1.0f, 0.5f));
                GenerateUV(new Vector2(1.0f, 1.0f));
                GenerateUV(new Vector2(1.0f, 0));                
            }
            
            else
            {
                Vector2 dirLine1 = points[points.IndexOf(p) - 1] - p;
                dirLine1.Normalize();
                Vector2 dirLine2 = points[points.IndexOf(p) + 1] - p;
                dirLine2.Normalize();
                Vector2 norDirLine1 = new Vector2(dirLine1.y, -dirLine1.x).normalized;
                float halfAngle = Vector2.Angle(dirLine1, dirLine2) / 2.0f;
                halfAngle *= Mathf.Deg2Rad;
                float coefWidth = Mathf.Sin(halfAngle);
                Vector2 dirVert = (dirLine1 + dirLine2).normalized;
                Vector2 distVert1 = (dirVert * halfWidthLine) / coefWidth;
                Vector2 distVert2 = (-dirVert * halfWidthLine) / coefWidth;
                GenerateUV(new Vector2(0.5f, 0.5f));

                if (Vector3.Angle(norDirLine1, dirLine2) < 90.0f)
                {
                    v1 += new Vector3(distVert1.x, distVert1.y, 0);
                    v2 += new Vector3(distVert2.x, distVert2.y, 0);

                }

                else if (Vector3.Angle(norDirLine1, dirLine2) > 90.001f)
                {
                    v1 += new Vector3(distVert2.x, distVert2.y, 0);
                    v2 += new Vector3(distVert1.x, distVert1.y, 0);
                }
                else
                {
                    Vector2 dirLine = points[points.IndexOf(p) + 1] - p;
                    v1 += new Vector3(-dirLine.y, dirLine.x, 0).normalized * halfWidthLine;
                    v2 += new Vector3(dirLine.y, -dirLine.x, 0).normalized * halfWidthLine;
                }
                GenerateUV(new Vector2(0.5f, 1.0f));
                GenerateUV(new Vector2(0.5f, 0));
            }

            vert.Add(v0);
            vert.Add(v1);
            vert.Add(v2);
        }

        GeneratePolygons();
        CreateMesh();
    }

    void CreateEndLine(Vector2 dir,Vector2 point)
    {
        dir.Normalize();
        Vector3 v0 = new Vector3(dir.x, dir.y, 0) * halfWidthLine + new Vector3 (point.x, point.y, 0);
        Vector3 norDirR = new Vector3(dir.y, -dir.x, 0).normalized;
        Vector3 v1 = norDirR * halfWidthLine + v0;
        Vector3 norDirL = new Vector3(-dir.y, dir.x, 0).normalized;
        Vector3 v2 = norDirL * halfWidthLine + v0;
        vert.Add(v0);
        vert.Add(v1);
        vert.Add(v2);
    }

    void GeneratePolygons()
    {
        for (int i = 0; i < vert.Count - 1; i += 3)
        {
            if (i < vert.Count - 3)
            {
                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + 4);
                triangles.Add(i);
                triangles.Add(i + 4);
                triangles.Add(i + 3);
                triangles.Add(i);
                triangles.Add(i + 3);
                triangles.Add(i + 2);
                triangles.Add(i + 2);
                triangles.Add(i + 3);
                triangles.Add(i + 5);
            }
            else
                break;
        }
    }

    void GenerateUV(Vector2 uv)
    {
        uvs.Add(uv);

    }

    void CreateMesh()
    {
        newMesh = new Mesh();
        newMesh.Clear();
        Vector3[] newVert = vert.ToArray();
        int[] newTriangles = triangles.ToArray();
        Vector2[] newUVs = uvs.ToArray();
        newMesh.vertices = newVert;
        newMesh.triangles = newTriangles;
        newMesh.uv = newUVs;
        
        mesh.sharedMesh = newMesh;
    }
}
