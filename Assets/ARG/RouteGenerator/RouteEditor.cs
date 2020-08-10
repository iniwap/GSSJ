using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class RouteEditor : MonoBehaviour {

    [Serializable]
    public class ObjectState
    {
        public Vector3 position; //保存物体位置信息 the position of object
        public Vector3 rotation; //保存物体旋转信息 the rotation of object
		public Quaternion quaternion; //保存物体旋转信息 the quaternion of object
    }
    public List<ObjectState> stateList = new List<ObjectState>(); //保存输入点的状态信息 save the states of input points
    public float startTime = 0.0f; //运动开始时间 the movement start time
	public float endTime = 20.0f; //运动结束时间 the movement end time
    public ObjectState state = new ObjectState(); //物体当前状态 the current state of object
    public float stepLength = 0.01f; //步长 the length of each two points
    public bool isSave = false; //是否保存当前状态标记 if saving this state
    public int isWhere; //状态保存位置 the saving place in stateList
    public bool isMove = false; //是否移动到状态标记 if moving to the state that be selected

	void Update () {
        //实时更新编辑器状态 update the state of edior
        state.position = gameObject.transform.position; 
		state.quaternion = gameObject.transform.rotation;
		state.rotation = gameObject.transform.rotation.eulerAngles;
		//保证链表中至少有一个状态 ensure there are one state at least in the stateList
		if (stateList.Count == 0) 
		{
			stateList.Add(state);
		}
        //保存当前状态到选中位置 save the current state into stateList
        if (isSave)
        {
            stateList[isWhere].position = state.position;
            stateList[isWhere].rotation = state.rotation;
			stateList[isWhere].quaternion = state.quaternion;
            isSave = false;
        }
        //将物体移动到所选位置 move the object to the selected state
        if (isMove)
        {
            gameObject.transform.position = stateList[isWhere].position;
			gameObject.transform.rotation = stateList[isWhere].quaternion;
            isMove = false;
        }
	}
}
