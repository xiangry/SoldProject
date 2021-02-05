using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomTile : MonoBehaviour
{
    public Text TileName;
    public GameObject TileObject;
    
//    public Vector2 Point = new Vector2(0, 0);

    public int PointX = 0;
    public int PointY = 0;

    private bool _isRoom;
    public bool IsRoom
    {
        set
        {
            _isRoom = value;
            MarkRoom(_isRoom);
        }
        get { return _isRoom; }
    }

    public Material roomMaterial;
    public Material blockMaterial;
    public Material startMaterial;
    public Material endMaterial;
    public Material mainPathMaterial;

//    public void SetMapPosition(Vector2 pos)
//    {
//        Point = pos;
//        this.transform.position = Point;
//        TileName.text = $"{pos.x + 1}.{pos.y + 1}";
//        this.name = TileName.text;
//    }

    public void SetMapPosition(int x, int y)
    {
        PointX = x;
        PointY = y;
        this.transform.position = new Vector3(x, y);
        TileName.text = $"{x}.{y}";
        this.name = TileName.text;
    }

    public void MarkRoom(bool isRoom = true)
    {
        if (isRoom)
        {
            TileObject.GetComponent<Renderer>().material = roomMaterial;
        }
        else
        {
            TileObject.GetComponent<Renderer>().material = blockMaterial;
        }
    }

    public void SetColor(int i)
    {
        TileObject.GetComponent<Renderer>().material.color = new Color(80, 255 - i * 10, 80);
    }


    public void MarkEnd()
    {
        TileObject.GetComponent<Renderer>().material = endMaterial;
    }
}
