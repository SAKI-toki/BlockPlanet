using UnityEngine;
using UnityEngine.UI;

public class PlayerNumberUI : MonoBehaviour
{
    [SerializeField]
    int number;
    Player player;
    Transform playerTransform;
    RectTransform rectTransform;
    const float offsetY = 40.0f;
    Image image;
    float timeCount = 0.0f;
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
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (playerTransform == null)
        {
            Destroy(gameObject);
            return;
        }
        //L,Rを押すと再度表示される
        if (SwitchInput.GetButtonDown(number - 1, SwitchButton.SR) || SwitchInput.GetButtonDown(number - 1, SwitchButton.SL))
        {
            timeCount = 0.0f;
        }
        //表示時間(減少時も含む)
        const float DisplayTime = 3.0f;
        if (timeCount < DisplayTime)
        {
            timeCount += Time.deltaTime;
            Color color = image.color;
            //アルファ値の減少
            color.a = Mathf.Clamp(DisplayTime - timeCount, 0.0f, 1.0f);
            image.color = color;
            //追尾
            Vector2 position = RectTransformUtility.WorldToScreenPoint(Camera.main, playerTransform.position);
            position.y += offsetY;
            rectTransform.position = position;
        }
    }
}
