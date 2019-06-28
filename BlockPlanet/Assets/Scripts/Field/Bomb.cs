using UnityEngine;

public class Bomb : MonoBehaviour
{

    /// <summary>
    /// 爆弾💣
    /// </summary>

    //デストロイ
    private bool Destroy_Flg = false;
    //爆弾が消滅するまでの時間
    private float Destroy_Timer = 0.2f;
    //プレイヤーに持たれているかどうか
    private bool Hold = false;
    //爆弾の威力
    const float Bombimpact = 60.0f;

    //爆発のパーティクル、子オブジェクト
    private ParticleSystem BOOM = null;
    SphereCollider[] BombColl;
    Rigidbody rb = null;

    BlockMap block_map = null;

    void Start()
    {
        //パーティクル
        BOOM = transform.GetChild(1).GetComponent<ParticleSystem>();
        //爆弾のコリジョン
        BombColl = GetComponents<SphereCollider>();
        BombColl[1].enabled = false;
        //rigidbody
        rb = this.GetComponent<Rigidbody>();
        block_map = GameObject.Find("StageCreation").GetComponent<StageCreation>().blockMap;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //接触判定
        for (int i = 1; i <= 4; ++i)
        {
            if (collision.gameObject.CompareTag("Player" + i))
            {
                if (!Hold)
                {
                    Explosion(); //爆破処理
                    BombColl[1].center = Vector3.up * -1;
                }
                return;
            }
        }
        if (collision.gameObject.tag == "Cube" || collision.gameObject.tag == "StrongCube")
        {
            if (!Hold)
                Explosion(); //爆破処理
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //キューブを破壊
        if (other.tag == "Cube")
        {
            block_map.BreakBlock(other.GetComponent<BlockNumber>());
            Destroy(other.gameObject);
        }

        //爆弾の威力をプレイヤーに伝える
        for (int i = 1; i <= 4; ++i)
        {
            if (other.CompareTag("Player" + i))
            {
                other.GetComponent<Player>().HitBomb(Bombimpact, Vector3.Distance(other.transform.position, this.transform.position));
            }
        }

        //フィールド外に落ちた場合
        if (other.tag == "DustBox")
            Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        //プレイヤーに持たれていたら フラグをtrueにする、重力を解除する
        bool holdFlg = false;
        foreach (var player in FieldManeger.Instance.players)
        {
            if (gameObject == player.ShootObject)
            {
                holdFlg = true;
                break;
            }
        }
        if (holdFlg)
        {
            Hold = true;
            //プレイヤーが持っている状態なら重力はいらない
            rb.useGravity = false;
        }
        else
        {
            Hold = false;
            rb.useGravity = true;
            rb.AddForce(Vector3.down * 50.0f);
        }
    }

    void Update()
    {
        //爆弾が消えるまで
        if (Destroy_Flg)
            Destroy_Timer -= Time.deltaTime;
        if (Destroy_Timer < 0)
            Destroy(gameObject);
    }

    //=====爆破処理=====
    void Explosion()
    {
        //爆発音
        SoundManager.Instance.Bomb();
        //爆弾の見た目を消す
        transform.GetChild(0).gameObject.SetActive(false);
        //爆弾の位置を固定
        rb.constraints = RigidbodyConstraints.FreezeAll;
        //パーティクル再生
        BOOM.Play();
        BOOM.transform.parent = null;
        //爆破の判定を出す
        BombColl[0].enabled = false;
        BombColl[1].enabled = true;
        //デストロイするためのフラグ
        Destroy_Flg = true;
    }
}
