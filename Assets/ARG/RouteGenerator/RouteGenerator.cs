using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RouteGenerator : MonoBehaviour 
{

	private float startTime; //运动开始时间 the movement start time
	private float endTime; //运动结束时间 the movement end time

	private List<Vector3> positionInput = new List<Vector3>(); //储存输入点位置 the positions of input points
	private List<Vector3> positionGet = new List<Vector3>(); //储存输出点位置 the positions of output points

	private List<Quaternion> rotationInput = new List<Quaternion>(); //储存输入点旋转信息 the Quaternions of input points
	private List<Quaternion> rotationGet = new List<Quaternion>(); //储存输出点旋转信息 the Quaternions of output points

	private float stepLength; //步长 the length of each two points
	private float slot; //每步所用时间 the time of move the object from one state to next state
	private float timeControl = 0.0f; //计时器 the timer
    private int count = 0; //计数器 the counter

    void Start()
    {
		//获取编辑器中的数据 get the datas from RouteEditor
        startTime = gameObject.GetComponent<RouteEditor>().startTime;
        endTime = gameObject.GetComponent<RouteEditor>().endTime;
        stepLength = gameObject.GetComponent<RouteEditor>().stepLength;
        List<RouteEditor.ObjectState> stateList = gameObject.GetComponent<RouteEditor>().stateList;
		//判断状态数是否大于6 if the states number is more than 6
		if (stateList.Count >= 6) {
			//转换为计算要求数据 get the position and rotation from stateList
			for (int i = 0; i < stateList.Count; i++) {
					positionInput.Add (stateList [i].position);
					rotationInput.Add (stateList [i].quaternion);
			}
			slot = (endTime - startTime) / (1 / stepLength + 1) / (stateList.Count - 4); //计算时隙（即每步移动所用时间） calculate the slot

			positionGet = PositionChange.GetPositionRoute (positionInput, stepLength); //获取位置结果 get the Positions of output points
			rotationGet = RotationChange.GetRotationRoute (rotationInput, stepLength); //获取旋转结果 get the Quaternions of output points
		} 
		else {
			Debug.Log ("You need at least 6 state!");
		}
    }
    void FixedUpdate()
    {
		//显示物体飞行路线 draw the route in Scene
		for (int i = 0; i < positionGet.Count - 1; i++) 
		{
			Debug.DrawLine(positionGet[i],positionGet[i+1],Color.red);
		}
        //在开始和结束时间内移动物体 move the object between start time and end time
        if (Time.time >= startTime)
        {
            if (count < positionGet.Count)
            {
                if (Time.time - timeControl > slot)
                {
					//if the got states are Available,then move the object to this state
					if(!Single.IsNaN(positionGet[count].x) && !Single.IsNaN(positionGet[count].y) && !Single.IsNaN(positionGet[count].z))
                    	gameObject.transform.position = positionGet[count];
					if(!Single.IsNaN(rotationGet[count].x) && !Single.IsNaN(rotationGet[count].y) && !Single.IsNaN(rotationGet[count].z) && !Single.IsNaN(rotationGet[count].w))
                    	gameObject.transform.rotation = rotationGet[count];
                    timeControl = Time.time;
                    count++;
                }
            }
        }
    }
}
