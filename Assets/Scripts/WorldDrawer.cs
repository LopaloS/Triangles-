using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldDrawer : MonoBehaviour
{
    [SerializeField]
    GameObject levelRect;

    [SerializeField]
    GameObject spaceShipPrototype;

    [SerializeField]
    Shader shader;

    Transform levelTransform;
    BrokenLineBasic brokenLine;
    Material material;

    Vector2 rectSize = new Vector2(1000, 700);
    Vector2 spaceShipSize = new Vector2(10, 20);
    
    int pixelInUnits = 100;
    bool init = false;
    string userId;

    Dictionary<string, Transform> gameTransforms = new Dictionary<string, Transform>();
    Dictionary<string, string> players = new Dictionary<string, string>();

    // Use this for initialization
    void Start()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        material = new Material(shader);
        material.SetTexture("_MainTex", texture);
        material.color = Color.white;
        var meshRenderer = levelRect.GetComponent<MeshRenderer>();
        meshRenderer.material = material;

        if (levelRect != null)
        {
            levelTransform = levelRect.transform;
            brokenLine = levelRect.GetComponent<BrokenLineBasic>();
        }

        //if (spaceShipPrototype != null)
            //CreateSpaceShip();

        StartCoroutine(WaitInit());
    }

    IEnumerator WaitInit()
    {
        NetworkController.Instance.OnInit += Init;
        while (!init)
            yield return null;

        var data = NetworkController.Instance.Data;
        userId = (string)data["uid"];
        int[] levelSize = (int[])data["level_size"];
        print(levelSize[0].GetType());
        rectSize = new Vector2(levelSize[0], levelSize[1]);
        rectSize /= pixelInUnits;

        CreateLevelRect();
        var objectsDict = (Dictionary<string, object>)data["objects"];
        foreach (var key in objectsDict.Keys)
        {
            if (!gameTransforms.ContainsKey(key))
                gameTransforms[key] = CreateSpaceShip().transform;
        }
        StartCoroutine(UpdateScene());
    }

    void Init()
    {
        init = true;
    }

    void CreateLevelRect()
    {
        List<Vector2> verts = new List<Vector2>();
        verts.Add(Vector2.zero);
        verts.Add(new Vector2(verts[0].x, verts[0].y + rectSize.y));
        verts.Add(verts[0] + rectSize);
        verts.Add(new Vector2(verts[0].x + rectSize.x, verts[0].y));
        verts.Add(verts[0] + Vector2.right * (brokenLine.widthLine / 2));

        brokenLine.points = verts;
        brokenLine.color = Color.white;
        brokenLine.CreateLine();

        if (spaceShipPrototype != null)
            spaceShipPrototype.SetActive(true);
    }

    GameObject CreateSpaceShip()
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

        var spaceShip = Instantiate(spaceShipPrototype) as GameObject;
        spaceShip.transform.parent = levelRect.transform;
        spaceShip.GetComponent<MeshFilter>().sharedMesh = mesh;
        spaceShip.GetComponent<MeshRenderer>().material = material;
        return spaceShip;
    }


    IEnumerator UpdateScene()
    {
        while (true)
        {
            Dictionary<string, object> data;
            do
            {
                data = NetworkController.Instance.Data;
                yield return null;
            }
            while (data == null);
            
            print(data);
            var unknownObjs = new List<string>();
            print(gameTransforms.Count);
            if (data.ContainsKey("objects"))
            {
                var objects = (Dictionary<string, object>)data["objects"];
                foreach (var kvp in objects)
                {
                    if (!gameTransforms.ContainsKey(kvp.Key))
                    {
                        unknownObjs.Add(kvp.Key);
                        continue;
                    }
                    var tr = gameTransforms[kvp.Key];
                    var tempTransformData = (Dictionary<string, object>)kvp.Value;
                    Vector2 pos = Vector2.zero;
                    
                    if (tempTransformData["pos"] is int[])
                    {
                        var tempPos = (int[])tempTransformData["pos"];
                        pos = new Vector2(tempPos[0], tempPos[1]) / pixelInUnits;
                    }
                    tr.localPosition = pos;
                    if (kvp.Key == userId)
                        transform.position = new Vector3(tr.position.x, tr.position.y, transform.position.z);
                    float angle = 0;
                    if (tempTransformData["angle"] is double)
                        angle = (float)(double)tempTransformData["angle"];
                    tr.up = Quaternion.Euler(new Vector3(0, 0, angle)) * Vector3.right;
                }
            }
            RequestUnknownObjs(unknownObjs);
            yield return null;
        }
    }

    void RequestUnknownObjs(List<string> ids)
    {

    }
}
