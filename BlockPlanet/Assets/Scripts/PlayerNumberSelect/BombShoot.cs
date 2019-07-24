using UnityEngine;

public class BombShoot : MonoBehaviour
{
    [SerializeField]
    GameObject bombObject;

    GameObject bombInstance;

    /// <summary>
    /// 爆弾を発射する
    /// </summary>
    public void ShootBomb()
    {
        if (bombInstance) return;
        SoundManager.Instance.BombThrow();
        //爆弾の生成
        bombInstance = Instantiate(bombObject, transform.position, Quaternion.identity);
        Destroy(bombInstance, 1.0f);
    }
}