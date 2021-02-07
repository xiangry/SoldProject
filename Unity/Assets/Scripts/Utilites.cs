/*
* @Author: xiangry
* @LastEditors: xiangry
* @Description: 
* @Date: 2021-02-07-46
*/

using System.Collections.Generic;

namespace MxGame
{
    public class Utilities 
    {
        public static T[] ShuffleArray<T>(T[] _dataArray, int _seed)
        {
            System.Random prng = new System.Random(_seed);

            T temp;
            for(int i = 0; i < _dataArray.Length - 1; i++)
            {
                int randomIndex = prng.Next(i, _dataArray.Length);

                temp = _dataArray[randomIndex];
                _dataArray[randomIndex] = _dataArray[i];
                _dataArray[i] = temp;
            }

            return _dataArray;
        }

        public static void ShuffleList<T>(ref List<T> _dataList, int _seed)
        {
            System.Random prng = new System.Random(_seed);

            for(int i = 0; i < _dataList.Count - 1; i++)
            {
                int randomIndex = prng.Next(i, _dataList.Count);

                T temp = _dataList[randomIndex];
                _dataList[randomIndex] = _dataList[i];
                _dataList[i] = temp;
            }
        }
    }
}


