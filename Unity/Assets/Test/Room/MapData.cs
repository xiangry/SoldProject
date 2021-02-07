/*
* @Author: xiangry
* @LastEditors: xiangry
* @Description: 
* @Date: 2021-02-05-22
*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Test.Room
{
    public class TileData
    {
        public int x;
        public int y;
        public bool isRoom;
        public int step;

        public TileData(int _x, int _y)
        {
            x = _x;
            y = _y;
            isRoom = false;
            step = 0;
        }
    }

    public class PathData
    {
        public List<TileData> path = new List<TileData>();
        
        public int Length { get => path.Count; }
        public TileData StartPoint { get => path.Count > 0 ? path[0] : null; }
        public TileData EndPoint { get => path.Count > 0 ? path[path.Count - 1] : null; }
        public TileData EndBackPoint { get => path.Count > 1 ? path[path.Count - 2] : null; }

        
        public PathData(TileData tile)
        {
            path.Add(tile);
        }

        public PathData(PathData _path, TileData tile)
        {
            path.AddRange(_path.path);
            path.Add(tile);
        }
        
        public void AddTile(TileData data)
        {
            path.Add(data);
        }
        
        public bool IsInPath(TileData data)
        {
            return path.Contains(data);
        }
    }


    public class MapData
    {
        public int width;
        public int height;

        public TileData[,] allTiles;

        public List<PathData> allPaths = new List<PathData>();

        public TileData StartPoint;

        public int RoomCount
        {
            get
            {
                var cnt = 0;
                Walk(tile =>
                {
                    if (tile.isRoom)
                        cnt++;
                });
                return cnt;
            }
        }

        public MapData(int _width, int _height)
        {
            width = _width;
            height = _height;
            ResetMapData();
        }

        private void ResetMapData()
        {
            allTiles = new TileData[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    allTiles[i,j] = new TileData(i, j);
                }
            }
            allPaths.Clear();
        }

        public List<TileData> NeighborRooms(TileData tile)
        {
            return NeighborRooms(tile.x, tile.y);
        }
        
        public List<TileData> NeighborRooms(int x, int y)
        {
            var tiles = new List<TileData>();
            if (IsRoom(x + 1, y)) tiles.Add(allTiles[x + 1,y]);
            if (IsRoom(x - 1, y)) tiles.Add(allTiles[x - 1,y]);
            if (IsRoom(x, y + 1)) tiles.Add(allTiles[x,y + 1]);
            if (IsRoom(x, y - 1)) tiles.Add(allTiles[x,y - 1]);
            return tiles;
        }

        public List<TileData> NeighborBlocks(TileData tile)
        {
            return NeighborBlocks(tile.x, tile.y);
        }
        
        public List<TileData> NeighborBlocks(int x, int y)
        {
            var tiles = new List<TileData>();
            if (IsBlock(x, y + 1)) tiles.Add(allTiles[x, y + 1]);
            if (IsBlock(x, y - 1)) tiles.Add(allTiles[x,y - 1]);
            if (IsBlock(x - 1, y)) tiles.Add(allTiles[x - 1, y]);
            if (IsBlock(x + 1, y)) tiles.Add(allTiles[x + 1, y]);
            return tiles;
        }

        public bool IsValidTile(int x, int y)
        {
            if (x < 0 || x >= width)
            {
                return false;
            }
            if (y < 0 || y >= height)
            {
                return false;
            }
            return true;
        }
        
        public bool IsBlock(int x, int y)
        {
            return IsValidTile(x, y) && !allTiles[x,y].isRoom;
        }

        public bool IsRoom(int x, int y)
        {
            return IsValidTile(x, y) && allTiles[x,y].isRoom;
        }

        public void Walk(Action<TileData> call)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    call.Invoke(allTiles[i,j]);
                }
            }
        }

        public TileData DataAt(int x, int y)
        {
            if (IsValidTile(x, y))
                return allTiles[x,y];
            return null;
        }

        public int GetIndex(TileData tile)
        {
            return tile.y + tile.x * height;
        }

        public void SetStartPoint(int x, int y)
        {
            StartPoint = allTiles[x,y];
            StartPoint.step = 0;
            allPaths.Clear();
            allPaths.Add(new PathData(StartPoint));
        }

        // 标记房间
        public void MarkRoom(TileData tile)
        {
            tile.isRoom = true;

            var checkedTiles = new List<TileData>();
            {
                var rooms = this.NeighborRooms(tile);

                if (rooms.Count == 0)
                {
                    tile.step = 0;
                    return;
                }
                
                if (rooms.Count > 1)
                {
                    Debug.LogError("新房间有间相邻房间");
                    return;
                }

                var room = rooms[0];
                if (room.step < 0)
                {
                    Debug.LogError("之前的房间没有标记step");                    
                    return;
                }
                else
                {
                    tile.step = room.step + 1;
                    return;
                }
            } 
        }

        public void CheckPath()
        {
            var cnt = 0;
            var index = 0;
            allPaths.Clear();
            allPaths.Add(new PathData(StartPoint));
            while (allPaths.Count > index)
            {
                cnt++;
                if (cnt > 100)
                {
                    break;
                }
                var path = allPaths[index];
                var endPoint = path.EndPoint;

                // 检查通路
                var endBackPoint = path.EndBackPoint;
                var rooms = NeighborRooms(endPoint);
                if (endBackPoint != null)
                {
                    rooms.Remove(endBackPoint);
                }

                if (rooms.Count > 0)
                {
                    for (int i = 1; i < rooms.Count; i++)
                    {
                        allPaths.Add(new PathData(path, rooms[i]));
                    }

                    path.AddTile(rooms[0]);
                    rooms[0].step = path.Length;
                }
                else
                {
                    index++;
                }
            }
            
            allPaths.Sort((a, b) => { return b.Length.CompareTo(a.Length); });

//            Debug.Log("");
//            Debug.Log(">>>===============================================================");
//            for (int i = 0; i < allPaths.Count; i++)
//            {
//                Debug.Log($"查找到了路径 {i} {allPaths[i].Length}");
//            }
//            Debug.Log("<<<===============================================================");
            
            
//            Walk(tile =>
//            {
//                if (tile.isRoom)
//                {
//                    Debug.Log($"tile step ==== {tile.x},{tile.y}  {tile.step}");
//                }
//            });
        }
    }
    
}