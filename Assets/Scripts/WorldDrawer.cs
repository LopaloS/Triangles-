using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldDrawer : MonoBehaviour
{
    [SerializeField]
    GameObject levelRect;

    [SerializeField]
    GameObject objectPrototype;

    [SerializeField]
    Shader shader;

    Transform levelTransform;
    BrokenLineBasic brokenLine;
    Material material;
    Material bulletMaterial;
    Texture bulletTex;

    Vector2 rectSize = new Vector2(1000, 700);
    Vector2 spaceShipSize = new Vector2(10, 20);
    
    int pixelInUnits = 100;
    string userId;

    Rect scoreRect = new Rect(10, 10, 150, 25);
    float labelNameOffset = 20;

    Dictionary<string, PlayerData> players = new Dictionary<string, PlayerData>();
    Dictionary<string, Transform> bullets = new Dictionary<string, Transform>();

    // Use this for initialization
    void Start()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        material = new Material(shader);
        material.SetTexture("_MainTex", texture);
        material.color = Color.white;

        var texGenerator = new CircleTextureGenerator();
        bulletTex = texGenerator.GetTexture(16, 0, Color.white);
        bulletMaterial = new Material(shader);
        bulletMaterial.SetTexture("_MainTex", bulletTex);
        bulletMaterial.color = Color.white;

        var meshRenderer = levelRect.GetComponent<MeshRenderer>();
        meshRenderer.material = material;

        if (levelRect != null)
        {
            levelTransform = levelRect.transform;
            brokenLine = levelRect.GetComponent<BrokenLineBasic>();
        }

        //if (spaceShipPrototype != null)
            //CreateSpaceShip();

    }

    
    public void Init()
    {
        var data = NetworkController.Instance.Data;
        var dataDict = (Dictionary<string, object>)data["args"];

        userId = (string)dataDict["uid"];
        int[] levelSize = (int[])dataDict["level_size"];
        rectSize = new Vector2(levelSize[0], levelSize[1]);
        rectSize /= pixelInUnits;

        CreateLevelRect();
        var objectsDict = (Dictionary<string, object>)dataDict["objects"];
        AddObjects(objectsDict);
        StartCoroutine(UpdateScene());
    }

    void AddObjects(Dictionary<string, object> objectsDict)
    {
        foreach (var kvp in objectsDict)
        {
            if (!players.ContainsKey(kvp.Key))
            {
                var playerDataDict = (Dictionary<string, object>)kvp.Value;
                var tr = CreateSpaceShip().transform;
                var pData = new PlayerData((string)playerDataDict["name"], tr);
                players.Add(kvp.Key, pData);
            }
        }
    }

    void CreateLevelRect()
    {
        brokenLine.points = GetRect(rectSize, Vector2.zero);
        brokenLine.color = Color.white;
        brokenLine.CreateLine();

        if (objectPrototype != null)
            objectPrototype.SetActive(true);
    }

    List<Vector2> GetRect(Vector2 size, Vector2 center)
    {
        List<Vector2> verts = new List<Vector2>();
        verts.Add(center);
        verts.Add(new Vector2(verts[0].x, verts[0].y + size.y));
        verts.Add(verts[0] + size);
        verts.Add(new Vector2(verts[0].x + size.x, verts[0].y));
        verts.Add(verts[0] + Vector2.right * (brokenLine.widthLine / 2));
        return verts;
    }

    GameObject CreateSpaceShip()
    {
        Vector2 size = spaceShipSize / pixelInUnits;
        var spaceShipSize3 = (Vector3)size;
        var mesh = new Mesh();

        Vector3[] verts = new Vector3[3];
        verts[0] = Vector3.zero - spaceShipSize3 / 2;
        verts[1] = new Vector3(0, size.y / 2, 0);
        verts[2] = new Vector3(size.x / 2, verts[0].y, 0);

        Vector2[] uvs = new Vector2[3];
        uvs[0] = Vector2.zero;
        uvs[1] = Vector2.up;
        uvs[2] = Vector2.one;

        mesh.vertices = verts;
        mesh.triangles = new int[] { 0, 1, 2 };
        mesh.uv = uvs;

        var spaceShip = Instantiate(objectPrototype) as GameObject;
        spaceShip.transform.parent = levelRect.transform;
        spaceShip.GetComponent<MeshFilter>().sharedMesh = mesh;
        spaceShip.GetComponent<MeshRenderer>().material = material;
        return spaceShip;
    }

    GameObject CreateBullet()
    {
        var bullet = Instantiate(objectPrototype) as GameObject;
        var mesh = new Mesh();
        var bulletSize = Vector2.one * 0.03f;
        Vector2[] verts2 = GetRect(bulletSize, bulletSize * 0.5f).ToArray();
        Vector3[] verts3 = new Vector3[4];
        for (int i = 0; i < verts3.Length; i++)
        {
            verts3[i] = new Vector3(verts2[i].x, verts2[i].y, 0);
        }

        Vector2[] uvs = new Vector2[4];
        uvs[0] = Vector2.zero;
        uvs[1] = Vector2.up;
        uvs[2] = Vector2.one;
        uvs[3] = Vector2.right;

        mesh.vertices = verts3;
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.uv = uvs;

        bullet.transform.parent = levelRect.transform;
        bullet.GetComponent<MeshFilter>().sharedMesh = mesh;
        bullet.GetComponent<MeshRenderer>().material = bulletMaterial;
        return bullet;
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
            var args = (Dictionary<string, object>)data["args"];
            var cmd = (string)data["cmd"];

            if (cmd == "world.tick")
                UpdateObjTransforms(args);

            if (cmd == "scores.update")
                UpdateScores(args);
            
            if (cmd == "world.objects_info")
                AddPlayers(args);

            yield return null;
        }
    }

    void AddPlayers(Dictionary<string, object> data)
    {
        if (data.ContainsKey("objects") && data["objects"] is Dictionary<string, object>)
        {
            var objectsDict = (Dictionary<string, object>)data["objects"];
            AddObjects(objectsDict);
        }
    }

    void UpdateObjTransforms(Dictionary<string, object> data)
    {
        var unknownObjs = new List<string>();
        if (data.ContainsKey("objects"))
        {
            var objects = (Dictionary<string, object>)data["objects"];
            foreach (var kvp in objects)
            {
                if (!players.ContainsKey(kvp.Key))
                {
                    unknownObjs.Add(kvp.Key);
                    continue;
                }
                var tr = players[kvp.Key].Transform;
                var tempTransformData = (Dictionary<string, object>)kvp.Value;
                
                tr.localPosition = GetPosFromObj(tempTransformData["pos"]);
                if (kvp.Key == userId)
                    transform.position = new Vector3(tr.position.x, tr.position.y, transform.position.z);
                float angle = 0;
                if (tempTransformData["angle"] is double)
                    angle = (float)(double)tempTransformData["angle"];
                tr.up = Quaternion.Euler(new Vector3(0, 0, angle)) * Vector3.right;
            }
        }

        if (data.ContainsKey("bullets") && data["bullets"] is Dictionary<string, object>)
        {
            var bulletsDict = (Dictionary<string, object>)data["bullets"];
            var bulletsDictSet = new HashSet<string>(bulletsDict.Keys);
            var bulletsSet = new HashSet<string>(bullets.Keys);
            bulletsSet.ExceptWith(bulletsDictSet);

            foreach (var id in bulletsSet)
            {
                Destroy(bullets[id].gameObject);
                bullets.Remove(id);
            }

            foreach (var kvp in bulletsDict)
            {
                if (!bullets.ContainsKey(kvp.Key))
                    bullets.Add(kvp.Key, CreateBullet().transform);

                var pos = GetPosFromObj(kvp.Value);
                if(pos != Vector2.zero)
                    bullets[kvp.Key].localPosition = pos;
            }
        }
        NetworkController.Instance.RequestUnknownObjs(unknownObjs);
    }

    Vector2 GetPosFromObj(object obj)
    {
        if (obj is int[])
        {
            var tempPos = (int[])obj;
            return new Vector2(tempPos[0], tempPos[1]) / pixelInUnits;
        }
        return Vector2.zero;
    }

    void UpdateScores(Dictionary<string, object> data)
    {
        var scoresDict = (Dictionary<string, object>)data["scores"];
        foreach (var player in players.Values)
        {
            if(scoresDict.ContainsKey(player.Name))
                player.Score = (int)scoresDict[player.Name];
        }
    }

    void OnGUI()
    {
        Rect curRect = scoreRect;
        foreach (var val in players.Values)
        {
            GUI.Label(curRect, val.Name + ": " + val.Score);
            curRect.y += scoreRect.height;

            var labelNameRect = scoreRect;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(val.Transform.position);
            labelNameRect.center = new Vector2(screenPos.x , Screen.height - screenPos.y - labelNameOffset);
            GUI.Label(labelNameRect, val.Name);
        }
    }
}

public class PlayerData
{
    public string Name { get; private set; }
    public int Score { get; set; }
    public Transform Transform { get; private set; }

    public PlayerData(string name, Transform tr)
    {
        Name = name;
        Transform = tr;
    }
}
