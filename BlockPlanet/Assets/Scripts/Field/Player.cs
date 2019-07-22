using UnityEngine;

public class Player : MonoBehaviour
{

    /// <summary>
    /// プレイヤー
    /// </summary>
    [SerializeField, Header("プレイヤーの番号")]
    int playerNumber = 0;
    //歩くスピード
    private float walkSpeed = 15;
    //投げる力
    private float shootPow = 5.0f;
    private float bodyScale = 1.0f;
    float vibrationTimer = 0.5f;
    [SerializeField]
    public bool isGameStart = false;

    //プレイヤーの頭上
    private Vector3 holdPosition;
    private Rigidbody rb;

    [SerializeField]
    //=====爆弾=====
    GameObject bombObject;

    //爆弾を持っている状態
    private bool isHold = false;
    float bombimpactVertical, bombimpactHorizontal;

    //投げる場所
    private Vector3 bombTargetPosition;
    //自分の位置
    private Transform shootPoint = null;
    [System.NonSerialized]
    //投げる物
    public GameObject ShootObject = null;

    private ParticleSystem chargeParticle;

    GameObject throwBombObject = null;

    bool IsHitBomb = false;
    Vector3 bombPosition = new Vector3();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        chargeParticle = transform.GetChild(0).GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "DustBox")
        {
            //フラグ関連
            isHold = false;
            FieldManeger.Instance.playerGameOvers[playerNumber] = true;
            //爆弾を持っていたら消す
            Destroy(ShootObject);
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SwitchVibration.LowVibration(playerNumber, 0.0f);
    }

    private void FixedUpdate()
    {
        if (isGameStart && !FieldManeger.Instance.isPause && !FieldManeger.Instance.isGameOver)
        {
            float vertical = SwitchInput.GetVertical(playerNumber);
            float horizontal = SwitchInput.GetHorizontal(playerNumber);
            if (Mathf.Abs(vertical) >= 0.0f ||
            Mathf.Abs(horizontal) >= 0.0f)
            {
                float sqrt = Mathf.Sqrt(Mathf.Pow(vertical, 2) + Mathf.Pow(horizontal, 2));
                //=====移動=====
                this.transform.Translate(Vector3.forward * walkSpeed * Time.deltaTime * sqrt);
                //=====回転=====
                this.transform.rotation = Quaternion.Slerp
                    (this.transform.rotation,
                     Quaternion.Euler(0, Mathf.Atan2(-vertical, horizontal) * Mathf.Rad2Deg + 90, 0),
                      Mathf.Sqrt(sqrt) / 2);
            }
        }

        if (IsHitBomb)
        {
            IsHitBomb = false;
            //吹っ飛ぶ力を設定 bombimpactは爆弾側で設定している
            Vector3 force = transform.position - bombPosition;
            force.y = 0;
            force.Normalize();
            //後ろに吹っ飛ぶ
            rb.AddForce(force * bombimpactHorizontal, ForceMode.Impulse);
            //上に吹っ飛ぶ
            rb.AddForce(Vector3.up * bombimpactVertical, ForceMode.Impulse);
            //振動
            vibrationTimer = 0.5f;
            SwitchVibration.LowVibration(playerNumber, 0.3f);
            CameraShake.Instance.Shake();
        }

        //重力
        rb.AddForce(Vector3.down * 60f);
    }

    void Update()
    {
        VibrationControl();

        if (isGameStart && !FieldManeger.Instance.isPause && !FieldManeger.Instance.isGameOver)
        {
            RaycastHit hit;

            if (SwitchInput.GetButtonDown(playerNumber, SwitchButton.Jump) &&
            //ジャンプ                 自分の中心　　　　　　　レイの幅　　　　　　　　　　方向　　　　長さ
            Physics.SphereCast(transform.position, 1.5f, Vector3.down, out hit, 1.7f))
            {
                if (hit.collider.tag == "Cube" || hit.collider.tag == "StrongCube")
                {
                    //力を加える
                    rb.AddForce(Vector3.up * 1000f);
                    //かかっている力をリセット
                    rb.velocity = Vector3.zero;
                    SoundManager.Instance.Jump();
                }
            }

            //プレイヤーの頭上に移動
            holdPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            //===爆弾関連===
            PlayerBomb();
        }
    }

    //===プレイヤーが持つ爆弾関連===
    void PlayerBomb()
    {
        //体を縮める
        transform.GetChild(1).transform.localScale = new Vector3(bodyScale, 1.0f, 1.0f);

        //爆弾を生成
        if (SwitchInput.GetButton(playerNumber, SwitchButton.Bomb) && !Namecheck() && !isHold)
            isHold = true;

        if (isHold)
        {
            //投げる力
            if (shootPow <= 15.0f)
                shootPow += 10.0f * Time.deltaTime;
            if (bodyScale >= 0.5f)
                bodyScale -= 0.5f * Time.deltaTime;

            chargeParticle.Play();
        }

        //大きくするのを止めて投げる
        if (SwitchInput.GetButtonUp(playerNumber, SwitchButton.Bomb) && isHold)
        {
            SoundManager.Instance.BombThrow();
            chargeParticle.Stop();
            ShootObject = Instantiate(bombObject, holdPosition, Quaternion.identity);
            throwBombObject = ShootObject;
            //ターゲットの位置
            bombTargetPosition = transform.Find("Circle").transform.position;
            //投げるよ (投げる場所)入ってるよ
            Shoot(bombTargetPosition);
            //持っている物はなし
            ShootObject = null;
            //数値を元に戻す
            shootPow = 5.0f;
            bodyScale = 1;
            isHold = false;
        }
    }

    //爆弾が存在するかのチェック
    bool Namecheck()
    {
        return throwBombObject != null;
    }

    void VibrationControl()
    {
        if (vibrationTimer == 0) return;
        vibrationTimer -= Time.deltaTime;
        if (vibrationTimer <= 0)
        {
            SwitchVibration.LowVibration(playerNumber, 0.0f);
            vibrationTimer = 0;
        }
    }

    private void Shoot(Vector3 TargetPosition)
    {
        //投げる場所,角度
        ShootFixedAngle(TargetPosition, 45.0f);
    }

    private void ShootFixedAngle(Vector3 TargetPosition, float angle)
    {
        //投げる場所,角度
        float speedVec = ComputeVectorFromAngle(TargetPosition, angle);

        //投げれない条件
        if (speedVec <= 0.0f)
        {
            Debug.Log("むり");
            return;
        }
        //対象との距離,角度,投げる場所
        Vector3 vec = ConvertVectorToVector3(speedVec, angle, TargetPosition);
        InstantiateShootObject(vec);
    }

    private float ComputeVectorFromAngle(Vector3 TargetPosition, float angle)
    {
        //自分の位置
        shootPoint = ShootObject.transform;
        //自分の位置
        Vector2 startPos = new Vector2(shootPoint.transform.position.x, shootPoint.transform.position.z);
        //投げる場所
        Vector2 targetPos = new Vector2(TargetPosition.x, TargetPosition.z);
        //投げた瞬間の距離
        float distance = Vector2.Distance(targetPos, startPos);

        //距離
        float x = distance;
        //重力
        float g = Physics.gravity.y;
        //自分の位置のｙ
        float y0 = shootPoint.transform.position.y;
        //投げる場所
        float y = TargetPosition.y;
        //角度*Mathf.Deg2Radでラジアンに変換
        float rad = angle * Mathf.Deg2Rad;

        //コサイン
        float cos = Mathf.Cos(rad);
        //タンジェント
        float tan = Mathf.Tan(rad);
        //投げる力*重力*距離*距離/(2*コサイン*コサイン*(投げる場所-自分の位置のｙ-距離*タンジェント))
        float v0Square = shootPow * g * x * x / (2 * cos * cos * (y - y0 - x * tan));

        //虚数になる計算は打ち切る
        if (v0Square <= 0.0f)
            return 0.0f;

        //平方根にする
        float v0 = Mathf.Sqrt(v0Square);
        return v0;
    }

    //対象との距離,角度(60度),投げる場所
    private Vector3 ConvertVectorToVector3(float iv0, float angle, Vector3 iTargetPosition)
    {

        //投げる場所
        Vector3 startPos = shootPoint.transform.position;
        //自分の位置
        Vector3 targetPos = iTargetPosition;
        startPos.y = 0.0f;
        targetPos.y = 0.0f;

        //正規化
        Vector3 dir = (targetPos - startPos).normalized;
        //ある方向から(from)ある方向へ(to)と回転させる
        Quaternion yawRot = Quaternion.FromToRotation(Vector3.right, dir);
        //対象との距離*Vector3(1,0,0)
        Vector3 vec = iv0 * Vector3.right;

        vec = yawRot * Quaternion.AngleAxis(angle, Vector3.forward) * vec;
        return vec;
    }

    private void InstantiateShootObject(Vector3 iShootVector)
    {
        //投げる物に付いているrigidbody取得
        var rigidbody = ShootObject.GetComponent<Rigidbody>();
        //値を0に
        rigidbody.velocity = Vector3.zero;
        //速さ
        Vector3 force = iShootVector * rigidbody.mass;
        //速さ*重さ(瞬時に速度変化)
        rigidbody.AddForce(force, ForceMode.Impulse);
    }

    public void HitBomb(Vector3 position)
    {
        bombPosition = position;
        bombimpactHorizontal = 120 / Mathf.Max(Vector3.Distance(this.transform.position, position), 0.5f);
        bombimpactVertical = 40 / Mathf.Max(Vector3.Distance(this.transform.position, position), 0.5f);
        IsHitBomb = true;
    }
}



