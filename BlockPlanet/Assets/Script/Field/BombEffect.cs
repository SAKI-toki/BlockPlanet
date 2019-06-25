using UnityEngine;

public class BombEffect : MonoBehaviour
{


    /// <summary>
    /// パーティクルが終わったら消すだけ
    /// </summary>

    float timer = 5;

    void Start()
    {

    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer < 0)
            Destroy(gameObject);
    }
}
