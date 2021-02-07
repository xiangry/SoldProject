/*
* @Author: xiangry
* @LastEditors: xiangry
* @Description: 
* @Date: 2021-02-07-11
*/

using System;
using EGamePlay;
using Sirenix.OdinInspector;

namespace Test.Room
{
    public class MapContext : Entity
    {
        [LabelText("Width")]
        public int Width;

        [LabelText("Height")]
        public int Height;
    }
}