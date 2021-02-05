using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Test.Room;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = System.Random;

public class RoomManager : SerializedMonoBehaviour
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

    private IEnumerator GenerateMap()
    {
//        List<RoomTile> history = new List<RoomTile>();
        var current = mapTiles[0];
        current.IsRoom = true;
            
//        history.Add(beginBlcok);

        var genRandom = new Random();
        var roomCnt = RoomCount - 1;
        var count = MaxLinkCnt;
        var dir = 1;
        while (roomCnt > 0)
        {
            var isChange = genRandom.Next(count) < 1;
            if (isChange)
            {
                count = MaxLinkCnt;
                dir = dir == 0 ? 1 : 0;
            }
            else
            {
                count--;
            }

            if (dir == 0) // 水平
            {
                var tile = GetBlock(current.PointX + 1, current.PointY);
                if (tile != null)
                {
                    tile.IsRoom = true;
                    current = tile;
                }
            }
            else
            {
                var tile = GetBlock(current.PointX, current.PointY + 1);
                if (tile != null)
                {
                    tile.IsRoom = true;
                    current = tile;
                }
            }
            
            if (delayTime > 0)
            {
                yield return new EditorWaitForSeconds(delayTime);
            }
            roomCnt--;
//            Debug.Log($"---------- {roomCnt}");
        }
        editorCoroutine = null;
//        
//        
//
//        var genRandom = new Random();
//        while (history.Count > 0)
//        {
//            var randomIndex = genRandom.Next(history.Count);
//            var current = history[randomIndex];
//            // 判断上下左右四方方向是否为路
//            int roadCnt = 0;
//            RoomTile tile;
//            tile = GetRoom(current.PointX - 1, current.PointY); if(tile != null) roadCnt++;
//            tile = GetRoom(current.PointX + 1, current.PointY); if(tile != null) roadCnt++;
//            tile = GetRoom(current.PointX, current.PointY - 1); if(tile != null) roadCnt++;
//            tile = GetRoom(current.PointX, current.PointY + 1); if(tile != null) roadCnt++;
//
//            // 判断依据在于上下左右四个位置是否只有一个位置是路
//            if (roadCnt <= 1)
//            {
//                current.IsRoom = true;
//                
//                //在墙队列中插入新的墙
//                tile = GetBlock(current.PointX - 1, current.PointY); if(tile != null) history.Add(tile);
//                tile = GetBlock(current.PointX + 1, current.PointY); if(tile != null) history.Add(tile);
//                tile = GetBlock(current.PointX, current.PointY - 1); if(tile != null) history.Add(tile);
//                tile = GetBlock(current.PointX, current.PointY + 1); if(tile != null) history.Add(tile);
//                
//            }
//            history.Remove(current);
//            Debug.Log($"current history cnt {history.Count}");
//            if (delayTime > 0)
//            {
//                yield return new EditorWaitForSeconds(delayTime);
//            }
//        }
//
//        editorCoroutine = null;
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
