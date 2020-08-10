using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RouteEditor))]
public class RouteInspector : Editor
{
    private static GUIContent
        insertContent = new GUIContent("+", "creat a new state"), //添加状态按钮 add button
        deleteContent = new GUIContent("-", "delete this state"), //删除状态按钮 delete button
        saveContent = new GUIContent("S", "save current state"), //保存状态按钮 save button
        moveContent = new GUIContent("M", "move to state"); //移动到当前状态按钮 move button
    private static GUILayoutOption
        buttonWidth = GUILayout.MaxWidth(30f); //按钮宽度 button width

    private SerializedObject obj; //编辑器对象 editor object
	private SerializedProperty //编辑器属性 editor property
        startTime,
        endTime,
        stepLength,
        state,
        stateList,
        isSave,
        isWhere,
        isMove;

	//获取编辑器属性 get the property from editor
    void OnEnable()
    {
        obj = new SerializedObject(target);
        startTime = obj.FindProperty("startTime");
        endTime = obj.FindProperty("endTime");
        stepLength = obj.FindProperty("stepLength");
        stateList = obj.FindProperty("stateList");
        state = obj.FindProperty("state");
        isSave = obj.FindProperty("isSave");
        isWhere = obj.FindProperty("isWhere");
        isMove = obj.FindProperty("isMove");
    }

	//重载编辑器 override the editor
    public override void OnInspectorGUI()
    {
        obj.Update();
        EditorGUILayout.PropertyField(startTime);
        EditorGUILayout.PropertyField(endTime);
        EditorGUILayout.PropertyField(stepLength);
        EditorGUILayout.PropertyField(state, true);

        GUILayout.Label("StateList(Request at least 6 state)");
        for (int i = 0; i < stateList.arraySize; i++)
        {
            string text = "———— State" + i.ToString() + " ————";
			SerializedProperty s = stateList.GetArrayElementAtIndex(i); //获取状态链表第i个状态 get the state i from stateList
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(text);
            //显示移动按钮 display the move button
            if (GUILayout.Button(moveContent, EditorStyles.miniButtonMid, buttonWidth))
            {
                isMove.boolValue = true;
                isWhere.intValue = i;
            }
			//显示保存按钮 display the save button
            if (GUILayout.Button(saveContent, EditorStyles.miniButtonLeft, buttonWidth))
            {
                isSave.boolValue = true;
                isWhere.intValue = i;
            }
			//显示添加按钮 display the add button
            if (GUILayout.Button(insertContent, EditorStyles.miniButtonMid, buttonWidth))
            {
                stateList.InsertArrayElementAtIndex(i);
            }
			//显示删除按钮 display the delete button
            if (GUILayout.Button(deleteContent, EditorStyles.miniButtonRight, buttonWidth))
            {
                stateList.DeleteArrayElementAtIndex(i);
            }
            EditorGUILayout.EndHorizontal();

			//显示该状态位置信息 display the position
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(s.FindPropertyRelative("position"));
            EditorGUILayout.EndHorizontal();
			//显示该状态旋转信息 display the rotation
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(s.FindPropertyRelative("rotation"));
            EditorGUILayout.EndHorizontal();
        }
        obj.ApplyModifiedProperties();
    }
}
