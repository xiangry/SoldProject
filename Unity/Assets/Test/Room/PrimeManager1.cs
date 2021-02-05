using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sirenix.OdinInspector;
using Test.Room;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Random = System.Random;



public class PrimeManager1 : SerializedMonoBehaviour
{
    private const string MapRootName = "MapRoot";
    
    [PropertyRange(0, 20)]
    public int mapWidth = 10;
    
    [PropertyRange(0, 20)]
    public int mapHeight = 10;

    public MapData mapData;
    
    [PropertyRange(2, 5)]
    public int MaxLinkCnt = 3;

    private List<RoomTile> mapTiles = new List<RoomTile>();

//    private RoomTile[][] allMapTiles = new RoomTile[][]{};

    public Transform baseTile;
    
    private List<RoomTile> roomTiles = new List<RoomTile>();

    public int MaxTileCount { get => mapWidth * mapHeight;}
    
    [PropertyRange(0, "MaxTileCount")]
    [OnValueChanged("ResetMap")]
    public int RoomCount = 15;
    
    [PropertyRange(0, "RoomCount")]
    [OnValueChanged("ResetMap")]
    public int MainPathCount = 15;


    [PropertyRange(0, 1)]
    public float delayTime = 0.3f;

    private EditorCoroutine editorCoroutine;
    private Coroutine coroutine;


        [Button("重置地图")]
    private void ResetMap()
    {
        mapTiles.Clear();
        var map = GameObject.Find(MapRootName);
        if (map != null)
        {
            GameObject.DestroyImmediate(map);
        }
        map = new GameObject(MapRootName);
        map.transform.position = Vector3.zero;
        
        baseTile.gameObject.SetActive(true);
        mapData = new MapData(mapWidth, mapHeight);
        mapData.Walk(data =>
        {
            var tile = GameObject.Instantiate(baseTile, map.transform);
            var roomTile = tile.GetComponent<RoomTile>();
            roomTile.SetMapPosition(data.x, data.y);
            mapTiles.Add(roomTile);
        });
        baseTile.gameObject.SetActive(false);

        if (EditorApplication.isPlaying)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(GenerateMap());
        }
        else
        {
            if (editorCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(editorCoroutine);
            }
            editorCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(GenerateMap());
        }
    }

    private IEnumerator GenerateMap()
    {
        List<TileData> history = new List<TileData>();

        mapData.SetStartPoint(0, mapHeight/2);
        
        var beginBlcok = mapData.StartPoint;
        history.Add(beginBlcok);

        var genRandom = new Random((int)DateTime.Now.Millisecond);
        
        var current = history.Last();
        while (history.Count > 0)
        {
            if (mapData.RoomCount >= RoomCount)
            {
                break;
            }
            
            // 判断上下左右四方方向是否为路
            int roadCnt = mapData.NeighborRooms(current).Count;
            // 判断依据在于上下左右四个位置是否只有一个位置是路
            if (roadCnt <= 1)
            {
                mapData.MarkRoom(current);
                mapTiles[mapData.GetIndex(current)].IsRoom = true;
                if (current.step > MainPathCount)        // 主线
                {
                    history.Remove(current);
                    current = history[genRandom.Next(history.Count)];
                    continue;
                }
                
                {
                    var blocks = mapData.NeighborBlocks(current);
                    for (int i = 0; i < blocks.Count / 2; i++)
                    {
                        var tempIndex = genRandom.Next(i, blocks.Count);
                        var temp = blocks[i];
                        blocks[i] = blocks[tempIndex];
                        blocks[tempIndex] = temp;
                    }
                    history.AddRange(blocks);
                }
            }
            history.Remove(current);
            current = history.Last();
            
//            Debug.Log($"current history cnt {history.Count}");
            if (delayTime > 0)
            {
                if (EditorApplication.isPlaying)
                {
                    yield return new WaitForSeconds(delayTime);
                }
                else
                {
                    yield return new EditorWaitForSeconds(delayTime);
                }
            }
        }

        mapData.CheckPath();
        var paths = mapData.allPaths;
        foreach (var path in paths)
        {
            var last = path.EndPoint;
            mapTiles[mapData.GetIndex(last)].MarkEnd();
        }
        
        
        editorCoroutine = null;
        coroutine = null;
    }
}
