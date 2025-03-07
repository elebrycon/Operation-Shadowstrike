using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public GameObject loadScreen;
    public Slider slider;
    //public TMPro.TextMeshPro progressNmbr;

    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsync(sceneIndex));
        
    }

    IEnumerator LoadAsync(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        loadScreen.SetActive(true);

        while (!operation.isDone)
        {


            float progress = Mathf.Clamp01(operation.progress / .9f);
            slider.value = progress;

            //progressNmbr.text = progress * 100 + "%";

           
            yield return null;  
        }
    }
    
}
