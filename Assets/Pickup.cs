using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pickup : MonoBehaviour
{
    public AudioSource AudioSource;
   
    public GameObject img;

    private void Update()
    {
        transform.Rotate(0f, 40* Time.deltaTime, 0f, Space.Self);
    }
    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player != null)
        {
            player.PickUps();
            img.SetActive(true);
            AudioSource.Play();
            gameObject.SetActive(false);
            

        }
    }
}
