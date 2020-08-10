/**************************************/
//FileName: XTPencil.cs
//Author: wtx
//Data: 03/27/2019
//Describe:  选题模式使用的铅笔，主要用于划选汉字
/**************************************/
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Lean.Touch;
using UnityEngine.Events;

public class XTPencil: MonoBehaviour{

    [Tooltip("The conversion method used to find a world point from a screen point")]
    public LeanScreenDepth ScreenDepth;

    [Tooltip("The camera the translation will be calculated using (default = MainCamera)")]
    public Camera Camera;
    public GameObject _LeftBorder;
    public GameObject _RightBorder;
    public GameObject _TopBorder;
    public GameObject _BottomBorder;
    
    private bool _CanShowPencil;

    private Vector3 _OriPos;
    // Use this for initialization
    void Start () {
        _OriPos = transform.position;
    }

	void OnDestroy(){
		
	}
	void OnEnable(){
        // Hook events
        LeanTouch.OnFingerSet += FingerSet;
        LeanTouch.OnFingerUp += FingerUp;
    }
	void OnDisable(){
        // Unhook events
        LeanTouch.OnFingerSet -= FingerSet;
        LeanTouch.OnFingerUp -= FingerUp;
    }


	public void Update()
	{

	}

    [System.Serializable] public class OnPencilSelectEvent : UnityEvent<bool,int> { }
    public OnPencilSelectEvent OnPencilSelect;
    //当Is Trigger=true时，碰撞器被物理引擎所忽略，没有碰撞效果，可以调用OnTriggerEnter/Stay/Exit函数。
    void OnTriggerEnter(Collider c)
    {
        //进入触发器执行的代码
        XTHZ hz = c.gameObject.GetComponent<XTHZ>();
        if(hz != null){
            if (_CanShowPencil)
            {
                // 只有是可显示的时候，也就是游戏中的时候，才能执行
                //hz.SetIsSelect(); // 此处不执行选中，需要判断是否可以选中或者执行
                OnPencilSelect.Invoke(!hz.GetIsSelect(),hz.GetHZID());
            }
        }
    }

    //不会进入这里
    //当Is Trigger=false时，碰撞器根据物理引擎引发碰撞，产生碰撞的效果，可以调用OnCollisionEnter/Stay/Exit函数；
    void OnCollisionEnter(Collision collision)
    {
        //进入碰撞器执行的代码

        XTHZ hz = collision.gameObject.GetComponent<XTHZ>();
        if (hz != null)
        {
            if (_CanShowPencil)
            {
                // 只有是可显示的时候，也就是游戏中的时候，才能执行
            }
        }
    }

    private void FingerSet(LeanFinger finger)
    {
        if(_CanShowPencil){

            //铅笔不能超出边界
            if(finger.ScreenPosition.x < _LeftBorder.transform.position.x
               || finger.ScreenPosition.x > _RightBorder.transform.position.x
               || finger.ScreenPosition.y > _TopBorder.transform.position.y
               || finger.ScreenPosition.y < _BottomBorder.transform.position.y){
                return;
            }

            //不显示铅笔了，手指挡住也看不到，显示会有些奇怪反而
            //gameObject.GetComponent<Image>().DOFade(1.0f, 0.5f);
            //var worldPoint = ScreenDepth.Convert(finger.ScreenPosition, Camera, gameObject);
            transform.position = finger.ScreenPosition;
        }
    }

    public UnityEvent OnPencilSubmit;
    private void FingerUp(LeanFinger finger)
    {
        //gameObject.SetActive(false);
        gameObject.GetComponent<Image>().DOFade(0.0f,0.5f);
        if(_CanShowPencil){
            // 只有是可显示的时候，也就是游戏中的时候，才能执行
            OnPencilSubmit.Invoke();
        }
    }

    //-----------外部接口-----------------------------
    public void SetCanShowPencil(bool can){
        _CanShowPencil = can;
        if(!can){
            //禁用铅笔时，应该隐藏
            gameObject.GetComponent<Image>().DOFade(0.0f, 0.5f);
        }
    }

    public void DisablePencil()
    {
        transform.position = _OriPos;
        gameObject.SetActive(false);
    }
}
