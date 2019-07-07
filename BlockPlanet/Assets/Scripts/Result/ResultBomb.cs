using UnityEngine;

/// <summary>
/// リザルト画面に出てくる爆弾
/// </summary>
public class ResultBomb : MonoBehaviour
{
    //デストロイ
    private bool Destroy_Flg = false;
    //爆発のパーティクル、子オブジェクト
    private ParticleSystem BOOM;
    //collision
    Collider[] BombColl;
    //rigidbody
    Rigidbody rb;

    void Start()
    {
        //パーティクル
        BOOM = transform.GetChild(1).GetComponent<ParticleSystem>();
        //爆弾のコリジョン
        BombColl = GetComponents<Collider>();
        BombColl[1].enabled = false;
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
        if (Destroy_Flg && !BOOM.isPlaying)
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
        BOOM.Play();
        //爆破の判定を出す
        BombColl[1].enabled = true;
        //デストロイするためのフラグ
        Destroy_Flg = true;
    }
}
