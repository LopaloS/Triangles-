using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WebSocketSharp;
using JsonFx.Json;
using System;
using WebSocketSharp;

public class FakeConection : MonoBehaviour
{
    public event EventHandler<CloseEventArgs> OnClose;
    public event EventHandler<ErrorEventArgs> OnError;
    public event EventHandler<MessageEventArgs> OnMessage;
    public event EventHandler OnOpen;

    BlackBox blackBox;
    public void Start()
    {
        blackBox = new BlackBox();
    }

    public void Send(string message)
    {
        if (blackBox != null)
        {
            var r = new JsonReader();
            print(blackBox.Handle(message));
        }
    }
}



public class BlackBox
{
    public float speedFactor = 0.1f;
    public float serverTick = 200f;

    public string uid = "user:123456";
    public Vector2 pos = new Vector2(100, 100);
    public float angle = 0;
    public Vector2 speed = Vector2.zero;
    public string idents = string.Empty;

    public string Handle(string inputData)
    {
        var reader = new JsonReader();
        Dictionary<string, object> inputDict = reader.Read(inputData) as Dictionary<string, object>;
        string cmd = (string)inputDict["cmd"];
        Dictionary<string, object> args = inputDict["args"] as Dictionary<string, object>;

        var message = new Dictionary<string, object>();

        switch (cmd)
        {
            case "world.start":
                var temArgs = new Dictionary<string, object>();
                temArgs.Add("uid", uid);
                temArgs.Add("server_tick", serverTick);
                temArgs.Add("level_size", new JsonVector2(1000, 700));

                message.Add("cmd", "world.init");
                message.Add("args", temArgs);
                break;

            case "user.commands":
                var dict = (Dictionary<string, object>)args;

                float[] vec = new float[2];

                Debug.Log(dict["move_vector"].GetType());
                Debug.Log("is float[]: " + (dict["move_vector"] is object[]));
                if (dict["move_vector"] is double[])
                {
                    var tempVec = (double[])dict["move_vector"];
                    vec[0] = (float)tempVec[0];
                    vec[1] = (float)tempVec[1];

                }
                else
                    vec = new float[] { 0, 0 };
                
                angle = vec[0];
                float length = vec[1];
                speed = (Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.up) * length * speedFactor;
                pos += speed * serverTick;

                var data = new Dictionary<string, Dictionary<string, object>>();
                var userData = new Dictionary<string, object>();
                userData.Add("pos", new JsonVector2(pos.x, pos.y));
                userData.Add("angle", angle);
                data.Add("user:123456", userData);

                var secondUserData = new Dictionary<string, object>();
                secondUserData.Add("pos",new JsonVector2(100,100));
                secondUserData.Add("angle", 70);
                data.Add("user:55", secondUserData);

                var dataArgs = new Dictionary<string, object>();
                dataArgs.Add("tick_data", data);
                message.Add("cmd", "world.tick");
                message.Add("args", dataArgs);
                break;
            case "world.get_objects_info":
                idents = (string)args["idents"];

                data = new Dictionary<string, Dictionary<string, object>>();
                userData = new Dictionary<string, object>();
                userData.Add("pos", new JsonVector2(100, 100));
                userData.Add("angle", 70);
                data.Add("user:55", userData);

                dataArgs = new Dictionary<string, object>();
                dataArgs.Add("objects", data);
                message.Add("cmd", "world.objects_info");
                message.Add("args", dataArgs);
                break;
        }
        JsonWriter writer = new JsonWriter();
        return writer.Write(message);
    }
}
