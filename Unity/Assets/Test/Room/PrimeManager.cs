using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Test.Room;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = System.Random;

public class PrimeManager : SerializedMonoBehaviour
{
    private const string MapRootName = "MapRoot";
    
    [PropertyRange(0, 20)]
    public int mapWidth = 10;
    
    [PropertyRange(0, 20)]
    public int mapHeight = 10;

    [PropertyRange(2, 5)]
    public int MaxLinkCnt = 3;

    private List<RoomTile> mapTiles = new List<RoomTile>();

    public Transform baseTile;
    
    private List<RoomTile> roomTiles = new List<RoomTile>();

    public int MaxTileCount { get => mapWidth * mapHeight;}
    
    [PropertyRange(0, "MaxTileCount")]
    [OnValueChanged("ResetMap")]
    public int RoomCount = 15;

    [PropertyRange(0, 1)]
    public float delayTime = 0.3f;

    private EditorCoroutine editorCoroutine;

    
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
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                var tile = GameObject.Instantiate(baseTile, map.transform);
                var roomTile = tile.GetComponent<RoomTile>();
                roomTile.SetMapPosition(i, j);
                mapTiles.Add(roomTile);
            }
        }
        baseTile.gameObject.SetActive(false);

        if (editorCoroutine != null)
        {
            EditorCoroutineUtility.StopCoroutine(editorCoroutine);
        }
        editorCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(GenerateMap());
    }

//    [Button("随机房间")]
//    private void FillRandomRoom()
//    {
////        var tiles = new List<RoomTile>();
////        tiles.AddRange(mapTiles);
//
////        var random = new Random();
////        int x;
////        RoomTile temp;
////        for (int i = 0; i < tiles.Count / 2; i++)
////        {
////            x = random.Next(i, tiles.Count);
////            temp = tiles[i];
////            tiles[i] = tiles[x];
////            tiles[x] = temp;
////        }
//
//        if (!IsGenerating)
//        {
//            StartCoroutine(GenerateMap());
//        }
////        var mazeMap = new PrimeGen(mapWidth, mapHeight);
////        mazeMap.Generate();
////        for (int i = 0; i < tiles.Count; i++)
////        {
////            var tile = tiles[i];
////            var isRoom = mazeMap.IsRoom((int) tile.Point.x, (int) tile.Point.y);
////            tiles[i].MarkRoom(isRoom);
////        }
//    }

    private IEnumerator GenerateMap()
    {
        List<RoomTile> history = new List<RoomTile>();

        var beginBlcok = mapTiles[0];
            
        history.Add(beginBlcok);

        var genRandom = new Random();
        while (history.Count > 0)
        {
            var randomIndex = genRandom.Next(history.Count);
            var current = history[randomIndex];
            // 判断上下左右四方方向是否为路
            int roadCnt = 0;
            RoomTile tile;
            tile = GetRoom(current.PointX - 1, current.PointY); if(tile != null) roadCnt++;
            tile = GetRoom(current.PointX + 1, current.PointY); if(tile != null) roadCnt++;
            tile = GetRoom(current.PointX, current.PointY - 1); if(tile != null) roadCnt++;
            tile = GetRoom(current.PointX, current.PointY + 1); if(tile != null) roadCnt++;

            // 判断依据在于上下左右四个位置是否只有一个位置是路
            if (roadCnt <= 1)
            {
                current.IsRoom = true;
                
                
                //在墙队列中插入新的墙
                tile = GetBlock(current.PointX - 1, current.PointY); if(tile != null) history.Add(tile);
                tile = GetBlock(current.PointX + 1, current.PointY); if(tile != null) history.Add(tile);
                tile = GetBlock(current.PointX, current.PointY - 1); if(tile != null) history.Add(tile);
                tile = GetBlock(current.PointX, current.PointY + 1); if(tile != null) history.Add(tile);
                
            }
            history.Remove(current);
            Debug.Log($"current history cnt {history.Count}");
            if (delayTime > 0)
            {
                yield return new EditorWaitForSeconds(delayTime);
            }
        }

        editorCoroutine = null;
    }

    private RoomTile GetRoom(int x, int y)
    {
        foreach (var tile in mapTiles)
        {
            if (tile.PointX == x && tile.PointY == y && tile.IsRoom) return tile;
        }
        return null;
    }
    
    private RoomTile GetBlock(int x, int y)
    {
        foreach (var tile in mapTiles)
        {
            if (tile.PointX == x && tile.PointY == y && tile.IsRoom == false) return tile;
        }
        return null;
    }
}
