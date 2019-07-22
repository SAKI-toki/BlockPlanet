using UnityEngine;

public class TitleBomb : MonoBehaviour
{

    /// <summary>
    /// タイトルシーンで落ちてくる爆弾
    /// </summary>

    //デストロイ
    private bool destroyFlg = false;
    //爆発のパーティクル、子オブジェクト
    private ParticleSystem boomParticle;
    //collision
    Collider[] bombColl;
    //rigidbody
    Rigidbody rb;
    [SerializeField]
    Material rainbowMat;

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
            Destroy(other.gameObject);
    }

    private void FixedUpdate()
    {
        rb.AddForce(Vector3.down * 25.0f);
    }

    void Update()
    {
        if (destroyFlg && !boomParticle.isPlaying)
        {
            Title.Instance.bgmSound.Play();
            Destroy(gameObject);
        }
    }

    //=====爆破処理=====
    void Explosion()
    {
        Title.Instance.BombExplosion();
        //爆発音
        SoundManager.Instance.Bomb();
        //爆弾の見た目を消す
        transform.GetChild(0).gameObject.SetActive(false);
        //爆弾の位置を固定
        rb.constraints = RigidbodyConstraints.FreezeAll;
        //パーティクル再生
        boomParticle.Play();
        //デストロイするためのフラグ
        destroyFlg = true;
    }
}
