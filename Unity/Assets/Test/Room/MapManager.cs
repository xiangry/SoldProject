/*
* @Author: xiangry
* @LastEditors: xiangry
* @Description: 
* @Date: 2021-02-07-12
*/

using System;
using EGamePlay;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Test.Room
{
    public class MapManager : SerializedMonoBehaviour
    {
        public MapContext MapContext;

        [Button("InitMap", ButtonSizes.Large)]
        private void DoInit()
        {
            MapContext = EntityFactory.Create<MapContext>();
        }
    }
}