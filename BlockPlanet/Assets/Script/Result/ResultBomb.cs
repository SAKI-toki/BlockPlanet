using UnityEngine;

public class ResultBomb : MonoBehaviour
{

    /// <summary>
    /// リザルト画面に出てくる爆弾
    /// </summary>

    //デストロイ
    private bool Destroy_Flg = false;
    private float Destroy_Timer = 0.2f;
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
            Destroy(other.gameObject);
    }

    private void FixedUpdate()
    {
        rb.AddForce(Vector3.down * 25.0f);
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
        SoundManager.Instance.Bomb();
        //爆弾の見た目を消す
        transform.GetChild(0).gameObject.SetActive(false);
        //爆弾の位置を固定
        rb.constraints = RigidbodyConstraints.FreezeAll;
        //パーティクル再生
        BOOM.Play();
        BOOM.transform.parent = null;
        //爆破の判定を出す
        BombColl[1].enabled = true;
        //デストロイするためのフラグ
        Destroy_Flg = true;
    }
}
