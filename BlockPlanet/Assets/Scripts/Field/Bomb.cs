using UnityEngine;

/// <summary>
/// 爆弾
/// </summary>
public class Bomb : MonoBehaviour
{
    //デストロイ
    private bool Destroy_Flg = false;
    //プレイヤーに持たれているかどうか
    private bool Hold = false;

    //爆発のパーティクル、子オブジェクト
    private ParticleSystem BOOM = null;
    SphereCollider[] BombColl;
    Rigidbody rb = null;

    BlockMap block_map = null;
    const float bombInterval = 0.2f;
    float destroyCount = 0.0f;

    string collisionOtherTag;

    Vector3 collisionPosition;

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
        collisionOtherTag = collision.gameObject.tag;

        //for文で回していたがString型の結合が重く、一回で何回も実行されるため、
        //速度を上げるために直に書いた
        if (collisionOtherTag == "Player1" ||
            collisionOtherTag == "Player2" ||
            collisionOtherTag == "Player3" ||
            collisionOtherTag == "Player4")
        {
            if (!Hold)
            {
                Explosion(); //爆破処理
                transform.position = (collision.transform.position + transform.position) / 2;
            }
            return;
        }
        if (collisionOtherTag == "Cube" || collisionOtherTag == "StrongCube")
        {
            if (!Hold)
                Explosion(); //爆破処理
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        collisionOtherTag = other.tag;
        //キューブを破壊
        if (collisionOtherTag == "Cube")
        {
            block_map.BreakBlock(other.GetComponent<BlockNumber>());
            Destroy(other.gameObject);
        }

        //爆弾の威力をプレイヤーに伝える
        //for文で回していたがString型の結合が重く、一回で何回も実行されるため、
        //速度を上げるために直に書いた
        if (collisionOtherTag == "Player1" ||
            collisionOtherTag == "Player2" ||
            collisionOtherTag == "Player3" ||
            collisionOtherTag == "Player4")
        {
            other.GetComponent<Player>().HitBomb(collisionPosition);
        }

        //フィールド外に落ちた場合
        if (collisionOtherTag == "DustBox")
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
        if (Destroy_Flg)
        {
            destroyCount += Time.deltaTime;
            if (destroyCount >= bombInterval)
                Destroy(gameObject);
        }
    }

    //=====爆破処理=====
    void Explosion()
    {
        collisionPosition = transform.position;
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
