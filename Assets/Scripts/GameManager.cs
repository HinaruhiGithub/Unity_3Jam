using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    //ゲームモードの状況
    public enum GameStatus
    {
        //投げる準備
        ready,
        //勝負中
        fighting
    }
    //ボールの状態
    public enum BallStatus
    {
        //待機中
        waiting,
        //投げられた
        throwed,
        //当たるか当たらないかの瀬戸際の時
        dodging,
        //バットから逃れた
        avoided,
        //打たれて飛ばされているとき
        flying
    };
    GameStatus gameStatus;
    public BallStatus ballstatus;


    GameObject ballObj;
    GameObject canvasObj;
    GameObject[] bat = new GameObject[4];



    Camera mainCamera;


    //時間計測用の変数
    private float startTime;


    //デバッグモードかどうか
    public bool IsDebugging = false;



    //定数群

    //フェードアウトまでにようする時間
    float fadeoutTime = 3.0f;
    //投げる際のボールの方向の振れ幅
    [SerializeField] Vector2 throwSize;


    // Start is called before the first frame update
    void Start()
    {
       

        ballObj = GameObject.Find("Ball");
        canvasObj = GameObject.Find("Canvas");
        bat[0] = GameObject.Find("bat0");
        bat[1] = GameObject.Find("bat1");
        bat[2] = GameObject.Find("bat2");
        bat[3] = GameObject.Find("bat3");

        mainCamera = Camera.main;

        IsDebugging = false;

        gameStatus = GameStatus.ready;


    }

    // Update is called once per frame
    void Update()
    {

        //投げる準備～収集までの流れ
        switch (gameStatus)
        {
            case GameStatus.ready:

                ballstatus = BallStatus.waiting;
                mainCamera.transform.position = new Vector3(0, 10, -10);
                canvasObj.GetComponent<UIManager>().WaitInit();
                gameStatus = GameStatus.fighting;
                break;

            case GameStatus.fighting:

                GameUpdate();
                BallUpdate();
                CameraUpdate();

                break;

            default:

                Debug.Log("You are an idiot!");

                break;
        }




        //デバッグモードの操作
        if (Input.GetButtonDown("DebugButton"))
        {
            IsDebugging = !IsDebugging;
            Debug.Log("debug change!");
            if (IsDebugging)
            {
                Debug.Log("debug now!");
            }
        }
    }



    //ゲーム進行での更新
    public void GameUpdate()
    {
        switch (ballstatus)
        {
            //判定が決まってからは数秒後フェードアウトする
            case BallStatus.avoided:

                //Debug.Log(Time.time - startTime);
                if(Time.time - startTime > fadeoutTime)
                {
                    //フェードアウト処理をする。
                    ballObj.GetComponent<BallManager>().BallInit();
                    for (int i = 0; i < 4; i++)
                    {
                        bat[i].GetComponent<Swing>().BatInit();
                    }
                    ballstatus = BallStatus.waiting;
                    gameStatus = GameStatus.ready;
                }
                break;
        }
    }

    //ボールの情報更新
    public void BallUpdate()
    {
        
        switch (ballstatus)
        {
            case BallStatus.waiting:
                if (Input.GetButtonDown("Throw"))
                {
                    ballstatus = BallStatus.throwed;
                    ballObj.GetComponent<BallManager>().ThrowInit(new Vector2(Random.Range(-throwSize.x / 2, throwSize.x / 2), Random.Range(-throwSize.y / 2, throwSize.y / 2)));
                }
                break;

            case BallStatus.throwed:
                ballObj.GetComponent<BallManager>().Throwing();
                break;

            case BallStatus.avoided:

                //特に何もしない
                break;

            case BallStatus.flying:
                ballObj.GetComponent<BallManager>().Fly();

                break;
            default:
                Debug.Log("You are an idiot!");
                break;
        }

    }



    //カメラ情報の更新
    public void CameraUpdate()
    {
        //デバッグモード中
        if (IsDebugging)
        {
            //カメラについて
            //mainCamera.transform.eulerAngles += new Vector3(0, 1, 0);

            if (Input.GetButtonDown("DirectionX"))
            {
                mainCamera.transform.eulerAngles = new Vector3(0, -90, 0);
                mainCamera.transform.position = new Vector3(100, 10, 30);
            }
            if (Input.GetButtonDown("DirectionY"))
            {
                mainCamera.transform.eulerAngles = new Vector3(90, 0, 0);
                mainCamera.transform.position = new Vector3(0, 65, 30);
            }
            if (Input.GetButtonDown("DirectionZ"))
            {
                mainCamera.transform.eulerAngles = new Vector3(0, 180, 0);
                mainCamera.transform.position = new Vector3(0, 10, 100);
            }
            //リセットだ
            if (Input.GetButtonDown("Reset"))
            {
                ballObj.GetComponent<BallManager>().BallInit();
                for (int i = 0; i < 4; i++)
                {
                    bat[i].GetComponent<Swing>().BatInit();
                }
                ballstatus = BallStatus.waiting;
            }
        }
        else
        {
            Vector3 ballPos = ballObj.transform.position;

            switch (ballstatus)
            {
                case BallStatus.waiting:

                    break;

                case BallStatus.throwed:
                    mainCamera.transform.position = ballPos;
                    mainCamera.transform.eulerAngles = new Vector3(0, 0, 0);

                    break;

                case BallStatus.flying:
                    mainCamera.transform.position = ballPos;
                    mainCamera.transform.eulerAngles = ballObj.GetComponent<BallManager>().CameraDirectionAtFlying();

                    break;

                default:
                    Debug.Log("You are an idiot!");
                    break;

            }
        }
    }


    public void BallJudge(string judge)
    {
        canvasObj.GetComponent<UIManager>().judging(judge);
        ballstatus = BallStatus.avoided;
        startTime = Time.time;

    }


    public void BallHit()
    {
        ballstatus = BallStatus.flying;
    }

}
