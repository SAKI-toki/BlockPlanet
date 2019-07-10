using UnityEngine;

public class BombEffect : MonoBehaviour
{
    ParticleSystem ps;
    bool prevIsPlaying = false;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (prevIsPlaying && !ps.isPlaying)
        {
            Destroy(gameObject);
        }
        prevIsPlaying = ps.isPlaying;
    }
}