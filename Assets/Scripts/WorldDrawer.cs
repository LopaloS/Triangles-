using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldDrawer : MonoBehaviour 
{
    [SerializeField]
    GameObject levelRect;

    [SerializeField]
    GameObject spaceShip;

    [SerializeField]
    Shader shader;

    Transform levelTransform;
    BrokenLineBasic brokenLine;
    Material material;

    Vector2 rectSize = new Vector2(1000, 700);
    Vector2 spaceShipSize = new Vector2(10, 20);
    int pixelInUnits = 100;

	// Use this for initialization
    void Start()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        material = new Material(shader);
        material.SetTexture("_MainTex", texture);
        material.color = Color.white;

        if (levelRect != null)
        {
            levelTransform = levelRect.transform;
            brokenLine = levelRect.GetComponent<BrokenLineBasic>();
        }
        CreateLevelRect();

        if (spaceShip != null)
            CreateSpacheShipMesh();
    }

    void CreateLevelRect()
    {
        var meshRenderer = levelRect.GetComponent<MeshRenderer>();
        meshRenderer.material = material;
        rectSize /= pixelInUnits;

        List<Vector2> verts = new List<Vector2>();
        verts.Add(- rectSize / 2);
        verts.Add(new Vector2(verts[0].x, verts[0].y + rectSize.y));
        verts.Add(verts[0] + rectSize);
        verts.Add(new Vector2(verts[0].x + rectSize.x, verts[0].y));
        verts.Add(verts[0] + Vector2.right * (brokenLine.widthLine / 2));

        brokenLine.points = verts;
        brokenLine.color = Color.white;
        brokenLine.CreateLine();
    }

    void CreateSpacheShipMesh()
    {
        spaceShipSize /= pixelInUnits;
        var spaceShipSize3 = (Vector3)spaceShipSize;
        var mesh = new Mesh();

        Vector3[] verts = new Vector3[3];
        verts[0] = Vector3.zero - spaceShipSize3 / 2;
        verts[1] = new Vector3(0, spaceShipSize.y / 2, 0);
        verts[2] = new Vector3(spaceShipSize.x / 2, verts[0].y, 0);

        Vector2[] uvs = new Vector2[3];
        uvs[0] = Vector2.zero;
        uvs[1] = Vector2.up;
        uvs[2] = Vector2.one;

        mesh.vertices = verts;
        mesh.triangles = new int[] { 0, 1, 2 };
        mesh.uv = uvs;

        spaceShip.GetComponent<MeshFilter>().sharedMesh = mesh;
        spaceShip.GetComponent<MeshRenderer>().material = material;
    }
	
	// Update is called once per frame
    //void Update () {
	
    //}
}
