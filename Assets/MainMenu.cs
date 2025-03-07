using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private bool GameIsPaused;
    public float timer;
    [SerializeField]
    private float timeElapsed = 0;
    [SerializeField]
    private bool gameStarting = false;
    public GameObject canvas;
    public LevelLoader levelLoader;

    private void Update()
    {
        if (gameStarting)
        {
            timeElapsed += Time.deltaTime;
            if(timeElapsed >= timer)
            {
                levelLoader.LoadLevel(1);
            }
        }
    }
    public void PlayGame()
    {
        gameStarting = true;
        canvas.SetActive(true);
        
    }
    public void QuitGame()
    {
        Debug.Log("Quitting");
        Application.Quit();
    }
}
