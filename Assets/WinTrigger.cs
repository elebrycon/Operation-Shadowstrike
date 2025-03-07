using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Device;
using UnityEngine.Diagnostics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinTrigger : MonoBehaviour
{
    public AudioSource audioSource;
    public GameObject WinScreen;
    public GameObject player;
    public GameObject black;
    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();

        black.SetActive(true);
        
        Color fixedColor = black.GetComponent<Image>().color;
        fixedColor.a = 1;
        black.GetComponent<Image>().color = fixedColor;
        black.GetComponent<Image>().CrossFadeAlpha(0f, 0f, true);
        

        if (player != null)
        {

            StartCoroutine(endGame());

        }
    }
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    IEnumerator endGame()
    {   
        //TEST PART
        player.GetComponent<PlayerMovement>().enabled = false;
        black.GetComponent<Image>().CrossFadeAlpha(1.0f, 1.5f, true); 
        //END OF TEST PART
        yield return new WaitForSeconds(2);
        audioSource.Play();
        black.SetActive(false);
        WinScreen.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;

    }
}
