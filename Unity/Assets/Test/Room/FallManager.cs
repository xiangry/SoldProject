using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Test.Room;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Random = System.Random;
using MxGame;



public class FallManager : SerializedMonoBehaviour
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

        mapData.SetStartPoint(0, mapHeight/2);

        var beginPoint = mapData.StartPoint;
        mapData.MarkRoom(beginPoint);
        mapTiles[mapData.GetIndex(beginPoint)].IsRoom = true;

        var allTiles = new List<TileData>();


        var obstacleCount = MaxTileCount - RoomCount;
        bool[,] obstacleFlags = new bool[mapWidth, mapHeight];
        var currentBlockCnt = 0;
        var allOpenTiles = new List<TileData>();
        mapData.Walk(tile => allOpenTiles.Add(tile));

        Debug.Log($"目标障碍数量 {obstacleCount}");

        var cnt = 0;
        for (int i = 0; i < obstacleCount; i++)
        {
            cnt++;
            if (cnt > 1000)
            {
                break;
            }

            if (allTiles.Count == 0)
            {
                allTiles.AddRange(allOpenTiles);
                allTiles.Remove(beginPoint);
                MxGame.Utilities.ShuffleList(ref allTiles, DateTime.Now.Millisecond);
            }
            var tileData = allTiles.First();
            allTiles.Remove(tileData);
            
            obstacleFlags[tileData.x, tileData.y] = true;    // 暂时标记为障碍
            currentBlockCnt++;

            if (tileData != beginPoint && MapIsFullyAccessible(obstacleFlags, currentBlockCnt))
            {
                // 可以生成障碍
                allOpenTiles.Remove(tileData);
            }
            else     // 不能生成障碍
            {
                obstacleFlags[tileData.x, tileData.y] = false;
                currentBlockCnt--;
                obstacleCount++;
            }
            
        }

        foreach (var tileData in allOpenTiles)
        {
            mapData.MarkRoom(tileData);
            mapTiles[mapData.GetIndex(tileData)].IsRoom = true;
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
        
        editorCoroutine = null;
        coroutine = null;
    }

    public bool MapIsFullyAccessible(bool[,] obstacleFlags, int obstacleCnt)
    {
        var tiles = mapData.allTiles;
        var start = mapData.StartPoint;

        var width = obstacleFlags.GetLength(0);
        var height = obstacleFlags.GetLength(1);
        
        bool[,] mapFlags = new bool[width, height];
        var checkQueue = new Queue<TileData>();        // 检查列表
        checkQueue.Enqueue(start);

        int accessibleTileCount = 0;//不含障碍物的瓦片的数量，一开始为1因为中心点肯定不是障碍物
        
        TileData current;
        while (checkQueue.Count > 0)
        {
            current = checkQueue.Dequeue();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 || y == 0)        // 排除对角线
                    {
                        // 遍历周边8个地块
                        int neightbourX = current.x + x;
                        int neightbourY = current.y + y;
                        if(neightbourX >= 0 && neightbourX < width && neightbourY >=0 && neightbourY < height)    // 检查越界
                        {
                            // 检查这个tile是否已经检测过了 && 相邻的瓦片没有障碍
                            if (mapFlags[neightbourX, neightbourY] == false &&
                                obstacleFlags[neightbourX, neightbourY] == false)
                            {
                                mapFlags[neightbourX, neightbourY] = true;
                                checkQueue.Enqueue(tiles[neightbourX, neightbourY]);
//                                Debug.Log($"check {neightbourX}, {neightbourY}   accessibleTileCount:{accessibleTileCount}");

                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int)(width* height - obstacleCnt);
        return targetAccessibleTileCount == accessibleTileCount;
    }
}
