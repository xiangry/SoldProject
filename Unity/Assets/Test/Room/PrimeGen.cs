/*
* @Author: xiangry
* @LastEditors: xiangry
* @Description: 
* @Date: 2021-02-04-32
*/

using System;
using System.Collections.Generic;
using UnityEditor;

namespace Test.Room
{
    public class PrimeGen
    {
        private int width;
        private int height;

        public TileData[][] MapData;

        public PrimeGen(int _width, int _height)
        {
            width = _width;
            height = _height;
            ResetMap();
        }

        public void ResetMap()
        {
            MapData = new TileData[width][];
            for (int i = 0; i < width; i++)
            {
                MapData[i] = new TileData[height];
                for (int j = 0; j < height; j++)
                {
                    MapData[i][j] = new TileData(i, j);
                }
            }
        }

        public void Generate()
        {
            List<TileData> history = new List<TileData>();

            var beginBlcok = MapData[0][0];
            
            history.Add(beginBlcok);

            var genRandom = new Random();
            while (history.Count > 0)
            {
                var index = genRandom.Next(history.Count);
                var current = history[index];
                // 判断上下左右四方方向是否为路
                int roadCnt = 0;
                if(IsRoom(current.x - 1, current.y)) roadCnt++;
                if(IsRoom(current.x + 1, current.y)) roadCnt++;
                if(IsRoom(current.x, current.y - 1)) roadCnt++;
                if(IsRoom(current.x, current.y + 1)) roadCnt++;

                // 判断依据在于上下左右四个位置是否只有一个位置是路
                if (roadCnt <= 1)
                {
                    current.isRoom = true;
                    
                    //在墙队列中插入新的墙
                    if(IsBlock(current.x - 1, current.y)) history.Add(MapData[current.x - 1][current.y]);
                    if(IsBlock(current.x + 1, current.y)) history.Add(MapData[current.x + 1][current.y]);
                    if(IsBlock(current.x, current.y + 1)) history.Add(MapData[current.x][current.y + 1]);
                    if(IsBlock(current.x, current.y - 1)) history.Add(MapData[current.x][current.y - 1]);
                }

                history.Remove(current);
            }
        }

        public bool IsRoom(int x, int y)
        {
            if (x < 0 || x >= width)
            {
                return false;
            }

            if (y < 0 || y >= height)
            {
                return false;
            }

            var block = MapData[x][y];
            return block.isRoom == true;
        }
        
        
        public bool IsBlock(int x, int y)
        {
            if (x < 0 || x >= width)
            {
                return false;
            }

            if (y < 0 || y >= height)
            {
                return false;
            }

            var block = MapData[x][y];
            return block.isRoom == false;
        }
    }
}