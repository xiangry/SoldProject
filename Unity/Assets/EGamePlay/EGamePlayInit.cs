using EGamePlay;
using ET;
using UnityEngine;


public class EGamePlayInit : MonoBehaviour
{
    private void Awake()
    {
        EntityFactory.Global = new GlobalEntity();
        EntityFactory.CreateWithParent<TimerComponent>(EntityFactory.Global);
    }

    private void Start()
    {
//        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        EntityFactory.Global.Update();
        TimerComponent.Instance.Update();
    }

    private void OnApplicationQuit()
    {
        Entity.Destroy(EntityFactory.Global);
    }
}
