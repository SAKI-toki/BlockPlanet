using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Tratest : MonoBehaviour {

    [SerializeField]
    private Material _transitionIn;

    [SerializeField]
    private Material _transitionOut;

    void Start()
    {
       StartCoroutine(BeginTransition());
    }

    private void Update()
    {
        StartCoroutine("BeginTransition");
    }

    IEnumerator BeginTransition()
    {

        yield return Fade(_transitionOut, 1);

        yield return new WaitForEndOfFrame();

        yield return Fade(_transitionIn, 1);


    }

    /// <summary>
    /// time秒かけてトランジションを行う
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator Fade(Material material, float time)
    {
        GetComponent<Image>().material = material;
        float current = 0;
        while (current < time)
        {
            material.SetFloat("_Alpha", current / time);
            yield return new WaitForEndOfFrame();
            current += Time.deltaTime;
        }
        material.SetFloat("_Alpha", 1);
    }

}
