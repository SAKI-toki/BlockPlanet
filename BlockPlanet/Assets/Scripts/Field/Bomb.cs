using UnityEngine;
using System.Collections;

/// <summary>
/// 爆弾
/// </summary>
public class Bomb : MonoBehaviour
{
    //プレイヤーに持たれているかどうか
    private bool hold = false;

    //爆発のパーティクル、子オブジェクト
    private ParticleSystem boomParticle = null;
    SphereCollider[] bombColl;
    Rigidbody rb = null;

    BlockMap blockMap = null;
    const float BombInterval = 0.2f;

    string collisionOtherTag;

    Vector3 collisionPosition;

    void Start()
    {
        //パーティクル
        boomParticle = transform.GetChild(1).GetComponent<ParticleSystem>();
        //爆弾のコリジョン
        bombColl = GetComponents<SphereCollider>();
        bombColl[1].enabled = false;
        //rigidbody
        rb = this.GetComponent<Rigidbody>();
        blockMap = GameObject.Find("StageCreation").GetComponent<StageCreation>().blockMap;
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
            if (!hold)
            {
                Explosion(); //爆破処理
                transform.position = (collision.transform.position + transform.position) / 2;
            }
            return;
        }
        if (collisionOtherTag == "Cube" || collisionOtherTag == "StrongCube")
        {
            if (!hold)
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
            blockMap.BreakBlock(other.GetComponent<BlockNumber>());
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
            if (player == null) continue;
            if (gameObject == player.ShootObject)
            {
                holdFlg = true;
                break;
            }
        }
        if (holdFlg)
        {
            hold = true;
            //プレイヤーが持っている状態なら重力はいらない
            rb.useGravity = false;
        }
        else
        {
            hold = false;
            rb.useGravity = true;
            rb.AddForce(Vector3.down * 50.0f);
        }
    }


    /// <summary>
    /// 爆破処理
    /// </summary>
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
        boomParticle.Play();
        boomParticle.transform.parent = null;
        //爆破の判定を出す
        bombColl[0].enabled = false;
        bombColl[1].enabled = true;
        StartCoroutine(DestroyCoroutine());
    }

    /// <summary>
    /// 破棄するコルーチン
    /// </summary>
    IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(BombInterval);
        Destroy(gameObject);
    }
}
