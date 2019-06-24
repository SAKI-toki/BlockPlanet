using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleBomb : MonoBehaviour
{

    /// <summary>
    /// タイトルシーンで落ちてくる爆弾
    /// </summary>

    //デストロイ
    private bool Destroyflg = false;
    private float Destroy_Timer = 0.2f;
    //爆発のパーティクル、子オブジェクト
    private ParticleSystem BOOM;
    //collision
    Collider[] BombColl;
    //rigidbody
    Rigidbody Rb;     


    // Use this for initialization
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

    // Update is called once per frame
    void Update()
    {
        //爆弾が消えるまで
        if (Destroyflg)
            Destroy_Timer -= Time.deltaTime;
        if (Destroy_Timer < 0)
        {
            Title.Instance.sounds.Play();
            Destroy(gameObject);
        }
    }

    //=====爆破処理=====
    void Explosion()
    {
        //フィールドにある破壊するキューブを検索
        GameObject[] tagobjs = GameObject.FindGameObjectsWithTag("Cube");

        //キューブを消す
        foreach (GameObject obj in tagobjs)
        {
            Destroy(obj);
        }

        //爆発音
        SoundManager.Instance.Bomb();
        //爆弾の見た目を消す
        transform.GetChild(0).gameObject.SetActive(false);
        //爆弾の位置を固定
        Rb.constraints = RigidbodyConstraints.FreezeAll;
        //パーティクル再生
        BOOM.Play();
        BOOM.transform.parent = null;
        //デストロイするためのフラグ
        Destroyflg = true;
    }
}
