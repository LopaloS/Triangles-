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

    float serverTick = 200;
    Vector2 rectSize = new Vector2(1000, 700);
    Vector2 spaceShipSize = new Vector2(10, 20);
    
    int pixelInUnits = 100;
    string userId;

    Rect scoreRect = new Rect(10, 10, 150, 25);
    float labelNameOffset = 20;

    GUIStyle playerLabelStyle;

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
    }

    
    public void Init()
    {
        var data = NetworkController.Instance.Data;
        var dataDict = (Dictionary<string, object>)data["args"];

        userId = (string)dataDict["uid"];
        int[] levelSize = (int[])dataDict["level_size"];
        int serverTickInt = (int)dataDict["server_tick"];
        serverTick = (float)serverTickInt / 1000;
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
        Dictionary<string, object> data;
        Dictionary<string, object> curObjectsData = null;
        Dictionary<string, object> curBulletsData = null;
        Dictionary<string, object> prevObjectsData = null;
        Dictionary<string, object> prevBulletsData = null;
        float interpolateStep = 0.2f;
        float curLerpPos = 0;
        bool updateDicts = true;

        while (true)
        {
            data = NetworkController.Instance.Data;

            if (data != null)
            {
                var args = (Dictionary<string, object>)data["args"];
                var cmd = (string)data["cmd"];

                if (cmd == "world.tick")
                {
                    interpolateStep = 1f /(serverTick / Time.deltaTime);
                    curLerpPos = 0;
                    updateDicts = true;
                    if (args.ContainsKey("objects"))
                    {
                        prevObjectsData = curObjectsData;
                        curObjectsData = (Dictionary<string, object>)args["objects"];

                        var unknownObjs = new List<string>();
                        foreach (var kvp in curObjectsData)
                        {
                            if (!players.ContainsKey(kvp.Key))
                                unknownObjs.Add(kvp.Key); 
                        }
                        NetworkController.Instance.RequestUnknownObjs(unknownObjs);
                    }
                    if (args.ContainsKey("bullets") && args["bullets"] is Dictionary<string, object>)
                    {
                        prevBulletsData = curBulletsData;
                        curBulletsData = (Dictionary<string, object>)args["bullets"];
                    }
                    else curBulletsData = null;
                }

                if (cmd == "scores.update")
                    UpdateScores(args);

                if (cmd == "world.objects_info")
                    AddPlayers(args);
            }

            curLerpPos += interpolateStep;
            if (curObjectsData != null)
            {
                
                UpdateObjTransforms(prevObjectsData, prevBulletsData, curObjectsData, curBulletsData, curLerpPos, updateDicts);
                updateDicts = false;
            }

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

    void UpdateObjTransforms(Dictionary<string, object> prevObjectsData, Dictionary<string, object> prevBulletsData,
        Dictionary<string, object> objectsData, Dictionary<string, object> bulletsData, float t, bool updateDicts)
    {
        LerpObjects(prevObjectsData, objectsData, t);
        if (updateDicts)
        {
            foreach (var playerKey in GetDiffKeys(players.Keys, objectsData.Keys))
            {
                if (!objectsData.ContainsKey(playerKey))
                {
                    Destroy(players[playerKey].Transform.gameObject);
                    players.Remove(playerKey);
                }
            }
        }

        if (bulletsData != null && bulletsData.Count > 0)
            LerpBullets(prevBulletsData, bulletsData, t);
        else if (updateDicts)
        {
            foreach (var bullet in bullets.Values)
            {
                Destroy(bullet.gameObject);
            }
            bullets.Clear();
        }
    }

    void LerpObjects(Dictionary<string, object> prevObjectsData, Dictionary<string, object> objectsData, float t)
    {
        foreach (var kvp in objectsData)
        {
            if (!players.ContainsKey(kvp.Key))
                continue;
            var tr = players[kvp.Key].Transform;
            var tempTransformData = (Dictionary<string, object>)kvp.Value;
            Dictionary<string, object> tempPrevTransformData = null;
            
            if (prevObjectsData != null && prevObjectsData.ContainsKey(kvp.Key))
            {
                tempPrevTransformData = (Dictionary<string, object>)prevObjectsData[kvp.Key];

                var prevPos = GetPosFromObj(tempPrevTransformData["pos"]);
                var curPos = GetPosFromObj(tempTransformData["pos"]);
                tr.localPosition = Vector3.Lerp(prevPos, curPos, t);
            }
            else
                tr.localPosition = GetPosFromObj(tempTransformData["pos"]);

            if (kvp.Key == userId)
                transform.position = new Vector3(tr.position.x, tr.position.y, transform.position.z);
            float angle = 0;
            float prevAngle = 0;
            Quaternion curRot = Quaternion.identity;
            if (tempTransformData["angle"] is double)
            {
                angle = (float)(double)tempTransformData["angle"];
                if (prevObjectsData != null)
                {
                    if (tempPrevTransformData["angle"] is double)
                        prevAngle = (float)(double)tempPrevTransformData["angle"];
                    else
                        prevAngle = angle;
                    angle = Mathf.LerpAngle(prevAngle, angle, t);
                }
            }
            curRot = Quaternion.Euler(new Vector3(0, 0, angle));            
            tr.up = curRot * Vector3.right;
        }
    }

    void LerpBullets(Dictionary<string, object> prevBulletsData, Dictionary<string, object> bulletsData, float t)
    {
        foreach (var kvp in bulletsData)
        {
            if (!bullets.ContainsKey(kvp.Key))
                bullets.Add(kvp.Key, CreateBullet().transform);

            var pos = GetPosFromObj(kvp.Value);
            
            if (pos != Vector2.zero)
            {
                if (prevBulletsData != null && prevBulletsData.ContainsKey(kvp.Key))
                {
                    var prevPos = GetPosFromObj(prevBulletsData[kvp.Key]);
                    bullets[kvp.Key].localPosition = Vector3.Lerp(prevPos, pos, t);
                }
                else
                    bullets[kvp.Key].localPosition = pos;
            }
        }
        if (prevBulletsData != null)
        {
            foreach (var id in GetDiffKeys(bullets.Keys, prevBulletsData.Keys))
            {
                Destroy(bullets[id].gameObject);
                bullets.Remove(id);
            }
        }
    }

    HashSet<string> GetDiffKeys(IEnumerable<string> firstDict, IEnumerable<string> secondDict)
    {
        var firstDictSet = new HashSet<string>(firstDict);
        var secondDictSet = new HashSet<string>(secondDict);
        firstDictSet.ExceptWith(secondDictSet);
        return firstDictSet;
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
        if (playerLabelStyle == null)
        {
            playerLabelStyle = new GUIStyle(GUI.skin.label);
            playerLabelStyle.alignment = TextAnchor.MiddleCenter;
        }

        Rect curRect = scoreRect;
        foreach (var val in players.Values)
        {
            GUI.Label(curRect, val.Name + ": " + val.Score);
            curRect.y += scoreRect.height;

            var labelNameRect = scoreRect;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(val.Transform.position);
            labelNameRect.center = new Vector2(screenPos.x , Screen.height - screenPos.y - labelNameOffset);
            GUI.Label(labelNameRect, val.Name, playerLabelStyle);
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
