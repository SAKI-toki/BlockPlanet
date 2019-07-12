using UnityEngine.UI;
using UnityEngine;

public class TitleImageBlink : MonoBehaviour
{
    Image image;

    float time = 0.0f;

    void Start()
    {
        image = GetComponent<Image>();
        UpdateColor();
    }

    void Update()
    {
        time += Time.deltaTime * 3;
        UpdateColor();
    }

    void UpdateColor()
    {
        const float MinAlpha = 0.3f;
        Color color = image.color;
        color.a = (Mathf.Sin(time) + 1) / 2 * (1 - MinAlpha) + MinAlpha;
        image.color = color;
    }
}
