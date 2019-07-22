using UnityEngine;

/// <summary>
/// リザルト画面に出てくる爆弾
/// </summary>
public class ResultBomb : MonoBehaviour
{
    //デストロイ
    private bool destroyFlg = false;
    //爆発のパーティクル、子オブジェクト
    private ParticleSystem boomParticle;
    //collision
    Collider[] bombColl;
    //rigidbody
    Rigidbody rb;

    void Start()
    {
        //パーティクル
        boomParticle = transform.GetChild(1).GetComponent<ParticleSystem>();
        //爆弾のコリジョン
        bombColl = GetComponents<Collider>();
        bombColl[1].enabled = false;
        //rigidbody
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explosion(); //爆破処理
    }

    private void OnTriggerEnter(Collider other)
    {
        //キューブを破壊
        if (other.tag == "Cube")
        {
            ResultManager.Instance.blockMap.BreakBlock(other.GetComponent<BlockNumber>());
            other.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        rb.AddForce(Vector3.down * 25.0f);
    }

    void Update()
    {
        if (destroyFlg && !boomParticle.isPlaying)
            Destroy(gameObject);
    }

    //=====爆破処理=====
    void Explosion()
    {
        SoundManager.Instance.Bomb();
        //爆弾の見た目を消す
        transform.GetChild(0).gameObject.SetActive(false);
        //爆弾の位置を固定
        rb.constraints = RigidbodyConstraints.FreezeAll;
        //パーティクル再生
        boomParticle.Play();
        //爆破の判定を出す
        bombColl[1].enabled = true;
        //デストロイするためのフラグ
        destroyFlg = true;
    }
}
