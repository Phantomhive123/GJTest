using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public Text text;
    public Slider slider;

    private SceneLoader instance = null;
    public SceneLoader Instance
    {
        get{ return instance; }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (instance != null)
            DestroyImmediate(this);
        else
            instance = this;
    }

    public void LoadSceneAsync(int target)
    {
        StartCoroutine(LoadSceneAsyncCoroutine(target));
    }

    IEnumerator LoadSceneAsyncCoroutine(int target)
    {
        float value = 0;
        AsyncOperation async = SceneManager.LoadSceneAsync(target);
        async.allowSceneActivation = false;
        while(!async.isDone)
        {
            value = async.progress < 0.9 ? async.progress : 1;
            slider.value = 1;
            text.text = (int)(value * 100) + "%";

            if (value >= 0.9f)
            {
                text.text = "点击任意键继续";
                if (Input.anyKeyDown)
                    async.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
