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


    [SerializeField] private GameObject ballObj;
    [SerializeField] private GameObject canvasObj;
    [SerializeField] private List<GameObject> bats;



    Camera mainCamera;



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

                GameReady();
                break;

            case GameStatus.fighting:

                BallUpdate();
                CameraUpdate();

                break;

            default:

                Debug.LogError("readyでもfightingでもない状態です。");

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

    private void GameReady()
    {
        ballstatus = BallStatus.waiting;
        mainCamera.transform.position = new Vector3(0, 10, -10);
        canvasObj.GetComponent<UIManager>().WaitInit();
        gameStatus = GameStatus.fighting;
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
                foreach (GameObject bat in bats)
                {
                    bat.GetComponent<Swing>().BatInit();
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
                    Debug.LogError("ボールの状態がわかりません");
                    break;

            }
        }
    }


    public void BallJudge(string judge)
    {
        canvasObj.GetComponent<UIManager>().judging(judge);
        ballstatus = BallStatus.avoided;
        StartCoroutine(FadeOut());

    }


    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(fadeoutTime);

        if (ballstatus == BallStatus.avoided)
        {
            //フェードアウト処理をする。
            ballObj.GetComponent<BallManager>().BallInit();
            foreach (GameObject bat in bats)
            {
                bat.GetComponent<Swing>().BatInit();
            }
            ballstatus = BallStatus.waiting;
            gameStatus = GameStatus.ready;

        }
    }


    public void BallHit()
    {
        ballstatus = BallStatus.flying;
    }

}
