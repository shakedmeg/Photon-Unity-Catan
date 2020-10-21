using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class JsonLayout
{
    public List<JsonTile> tiles;
    public List<JsonVertex> vertexes;
    public List<JsonEdge> edges;
}
[System.Serializable] 
public class JsonTile
{
    public float x;
    public float y;
    public float z;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public Vector3 pos;
    public List<int> vertexes = new List<int>();
}

[System.Serializable] 
public class JsonVertex
{
    public float x;
    public float y;
    public float z;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public Vector3 pos;
    public List<int> neighbors = new List<int>();
    public List<int> edges = new List<int>();
    public List<int> tiles = new List<int>();
}

[System.Serializable] 
public class JsonEdge
{
    public int id;
    public Vector3 pos;
    public Vector3 angle;
    public int layerID = 0;
    public string layerName = "Default";
    public List<int> neighbors = new List<int>();
    public List<int> vertexes = new List<int>();
}

public class Layout : MonoBehaviour
{
    public List<JsonTile> tiles;
    public List<JsonVertex> vertexes;
    public List<JsonEdge> edges;

    public void ReadLayout(string json){
        JsonLayout root = JsonUtility.FromJson<JsonLayout>(json);
        foreach(JsonTile tile in root.tiles){
            tile.pos = new Vector3(tile.x, tile.y, tile.z);
        }
        tiles = root.tiles;
        foreach(JsonVertex vertex in root.vertexes){
            vertex.pos = new Vector3(vertex.x, vertex.y, vertex.z);
        }
        vertexes = root.vertexes;
        edges = root.edges;
    }
}
