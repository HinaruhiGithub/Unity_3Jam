using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : SingletonMonoBehaviour<BallManager>
{

    //GameManagerからgetcomponentで弾の状況を受け取る。
    public GameObject gamemaster;


    //判定がすんだかどうか
    private bool judgeF;
    //判定を下すためのインターバル時間(判定エリア直撃後にバットに当たったことを考慮して)
    private float startTimes;
    //判定内容
    private string judges;
    //棒に当たったかどうか
    private bool attackF;

    [SerializeField] private float flyingBaseRadian = 45.0f;







    //定数群

    //ボールの投げるスピード
    [SerializeField] float throwspeed = 40f;

    //ボールの曲げ具合
    private readonly Vector3 ballAccel = new Vector3(0.2f, 0.4f, 0.2f);

    //飛んでるときの曲げ具合
    private readonly Vector3 ballFlyAccel = new Vector3(0.01f, 0, 0.01f);

    //ボールの初期位置
    private readonly Vector3 ballInitPos = new Vector3(0, 10, 0);



    // Start is called before the first frame update
    void Start()
    {
        gamemaster = GameObject.Find("GameMaster");
        BallInit();
    }

    // Update is called once per frame
    void Update()
    {
        //判断中...
        if (judgeF && !attackF)
        {
            if(Time.time - startTimes > 0.5f)
            {
                gamemaster.GetComponent<GameManager>().BallJudge(judges);
                judgeF = false;
            }
        }
    }






    //判定の壁への衝突の判定について
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);



        if (judgeF) return;


        switch (other.gameObject.name)
        {
            case "ThrowArea":

                judgeF = true;
                judges = "Strike";
                startTimes = Time.time;

                break;

            case "BallArea":

                judgeF = true;
                judges = "Ball";
                startTimes = Time.time;

                break;
        }
    }



    //バットとの衝突判定
    private void OnCollisionEnter(Collision collision)
    {

        for(int i = 1; i <= 4; i++)
        {
            if (collision.gameObject.name == "bat" + i.ToString())
            {
                attackF = true;

                //重さを十倍に増やし、基本的に上に飛ぶようにする。
                Rigidbody rd = this.GetComponent<Rigidbody>();
                rd.mass = 10;

                Vector3 hitPoint = Vector3.zero;

                foreach(ContactPoint point in collision.contacts){
                    hitPoint = point.point;
                }

                rd.velocity = (rd.position - hitPoint).normalized * rd.velocity.magnitude;
                
                //zベクトルとyベクトルによる角度を調整することにより、基本上に飛ばすようにする
                Vector3 _v = rd.velocity;
                float mag = Mathf.Sqrt(Mathf.Pow(rd.velocity.y, 2) + Mathf.Pow(rd.velocity.z, 2));
                float rad = Mathf.Rad2Deg * Mathf.Atan2(_v.y, -_v.z);
                rad = Mathf.Pow(rad - flyingBaseRadian, 3.0f) * Mathf.PI / 2.0f / 729000.0f + flyingBaseRadian * Mathf.Deg2Rad;
                rd.velocity = new Vector3(rd.velocity.x, mag * Mathf.Sin(rad), -mag * Mathf.Cos(rad));

            }
        }

        if (attackF)
        {
            gamemaster.GetComponent<GameManager>().BallHit();
        }
    }

    //ボールの初期化
    public void BallInit()
    {
        Rigidbody rd = this.GetComponent<Rigidbody>();

        this.transform.position = ballInitPos;
        rd.velocity = Vector3.zero;
        rd.useGravity = false;
        judgeF = false;
        attackF = false;
    }
    public void ThrowInit(Vector2 controll)
    {
        Rigidbody rd = this.GetComponent<Rigidbody>();

        rd.velocity = new Vector3(controll.x, controll.y, throwspeed);
        rd.useGravity = true;

    }


    //ここからはボールの状況によって処理が変更されます。
    public void Throwing()
    {

        Rigidbody rd = this.GetComponent<Rigidbody>();
        Vector3 accel = new Vector3(0, 0, 0);




        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5)
        {
            accel.x = ballAccel.x * Mathf.Sign(Input.GetAxisRaw("Horizontal"));
            //Debug.Log("x");
        }

        if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.5)
        {
            accel.y = ballAccel.y * Mathf.Sign(Input.GetAxisRaw("Vertical"));
            //Debug.Log("y");
        }



        //vの更新
        rd.velocity += accel;


        //位置の更新
        //this.transform.position += velocity;

    }


    public void Fly()
    {

        Rigidbody rd = this.GetComponent<Rigidbody>();
        Vector3 accel = new Vector3(0, 0, 0);


        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5)
        {
            //最初の時と比べてカメラの向きが180度変化することに注意する。
            accel.x = ballFlyAccel.x * -Mathf.Sign(Input.GetAxisRaw("Horizontal"));
            //Debug.Log("x");
        }

        if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.5)
        {
            accel.z = ballFlyAccel.z * -Mathf.Sign(Input.GetAxisRaw("Vertical"));
            //Debug.Log("y");
        }


        //vの更新
        rd.velocity += accel;
    }

    public Vector3 CameraDirectionAtFlying()
    {
        Vector3 vectors = this.GetComponent<Rigidbody>().velocity;

        //基準
        Vector3 answer = new Vector3(0, 180, 0);

        //実際の計算
        //Vector3 answer = new vector3(Mathf.Atan2(vectors.z, vectors.y),Mathf.Atan2(vectors.x, vectors.z),Mathf.Atan2(vectors.y, vectors.x));

        return answer;
    }
}
