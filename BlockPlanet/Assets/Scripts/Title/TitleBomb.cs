using UnityEngine;

public class TitleBomb : MonoBehaviour
{

    /// <summary>
    /// タイトルシーンで落ちてくる爆弾
    /// </summary>

    //デストロイ
    private bool Destroyflg = false;
    //爆発のパーティクル、子オブジェクト
    private ParticleSystem BOOM;
    //collision
    Collider[] BombColl;
    //rigidbody
    Rigidbody Rb;
    [SerializeField]
    Material rainbowMat;

    void Start()
    {
        //パーティクル
        BOOM = transform.GetChild(1).GetComponent<ParticleSystem>();
        //爆弾のコリジョン
        BombColl = GetComponents<Collider>();
        BombColl[1].enabled = false;
        //rigidbody
        Rb = GetComponent<Rigidbody>();
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
        Rb.AddForce(Vector3.down * 25.0f);
    }

    void Update()
    {
        if (Destroyflg && !BOOM.isPlaying)
        {
            Title.Instance.sounds.Play();
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
        Rb.constraints = RigidbodyConstraints.FreezeAll;
        //パーティクル再生
        BOOM.Play();
        //デストロイするためのフラグ
        Destroyflg = true;
    }
}
