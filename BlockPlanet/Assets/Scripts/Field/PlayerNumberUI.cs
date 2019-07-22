using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNumberUI : MonoBehaviour
{
    [SerializeField]
    int number;
    Player player;
    Transform playerTransform;
    RectTransform rectTransform;
    const float offsetY = 40.0f;
    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player" + number);
        if (playerObject == null)
        {
            Destroy(gameObject);
            return;
        }
        player = playerObject.GetComponent<Player>();
        playerTransform = player.transform;
        rectTransform = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (playerTransform == null)
        {
            Destroy(gameObject);
            return;
        }
        Vector2 position = RectTransformUtility.WorldToScreenPoint(Camera.main, playerTransform.position);
        position.y += offsetY;
        rectTransform.position = position;
    }
}
