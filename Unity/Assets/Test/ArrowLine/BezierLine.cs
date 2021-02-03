using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class BezierLine : SerializedMonoBehaviour
{
    private List<Vector2> ControlPoints = new List<Vector2>();
    private List<Vector2> ControlFactor = new List<Vector2>();
    private List<Transform> PointObjs = new List<Transform>();

    public GameObject PointObject;
    

    public Transform StartPoint;
    public Transform EndPoint;

    public Transform ControlFactorPoint1;
    public Transform ControlFactorPoint2;
    
    [OnValueChanged("OnControlFactorChanged")]
    public Vector2 ControlFactor1 = new Vector2(-0.3f, 0.8f);

    [OnValueChanged("OnControlFactorChanged")]
    public Vector2 ControlFactor2 = new Vector2(-0.1f, 1.4f);

    private void Start()
    {
        DoInit();
    }

    // Start is called before the first frame update
   
    [Button("Test", ButtonSizes.Large)]
    private void DoInit()
    {
        if (ControlPoints.Count < 4)
        {
            ControlPoints.Add(Vector2.zero);
            ControlPoints.Add(Vector2.zero);
            ControlPoints.Add(Vector2.zero);
            ControlPoints.Add(Vector2.zero);
        }
        
        foreach (var point in PointObjs)
        {
            GameObject.DestroyImmediate(point.gameObject);
        }
        PointObjs.Clear();
        
        PointObject.SetActive(true);
        for (int i = 0; i < 15; i++)
        {
            PointObjs.Add(GameObject.Instantiate(PointObject,  PointObject.transform.parent).transform);
        }
        PointObject.SetActive(false);
    }

    [Button("RefreshLine", ButtonSizes.Large)]
    void DoRefresu()
    {
        Update();
    }

    private void OnControlFactorChanged()
    {
        DoRefresu();
    }

//    [OnInspectorGUI]
//    private void OnGUIRefresu()
//    {
//        DoRefresu();
//    }

    // Update is called once per frame
    void Update()
    {
        this.ControlPoints[0] = StartPoint.position;
        this.ControlPoints[3] = EndPoint.position;
        this.ControlPoints[1] = this.ControlPoints[0] + (this.ControlPoints[3] - this.ControlPoints[0]) * ControlFactor1;
        this.ControlPoints[2] = this.ControlPoints[0] + (this.ControlPoints[3] - this.ControlPoints[0]) * ControlFactor2;


        ControlFactorPoint1.position = this.ControlPoints[1];
        ControlFactorPoint2.position = this.ControlPoints[2];
        
        // B(t) = (1-t)^3*P0 + 3*(1-t)^2*t*P1 + 3(1-t)*t^2*P2 + t^3*P3
        var cnt = PointObjs.Count;
        for (int i = 0; i < cnt; i++)
        {
            var t = Mathf.Log(1f * i / (cnt - 1) + 1f, 2f);

            PointObjs[i].position =
                Mathf.Pow(1 - t, 3) * ControlPoints[0] +
                3 * Mathf.Pow(1 - t, 2) * t * ControlPoints[1] +
                3 * (1 - t) * t * t * ControlPoints[2] +
                Mathf.Pow(t, 3) * t * ControlPoints[3];

//            if (i > 0)
//            {
//                PointObjs[i].rotation = Vector2.SignedAngle(Vector2.up, PointObjs[i].position - PointObjs[i-1].position);
//            }
//
//            var scale = this.scaleFactor * (1f - 0.03f * (cnt - 1 - i));
//            PointObjs[i].scale = new Vector2(scale, scale);
        }
    }
}
