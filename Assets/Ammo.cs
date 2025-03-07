using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour
{
    public AudioSource AudioSource;
    private void OnTriggerEnter(Collider other)
    {
        Gun GunObj;

       
        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if(player != null)
        {
            GunObj = FindObjectOfType<Gun>();
            GunObj.PickUpAmmo();
            AudioSource.Play();
            gameObject.SetActive(false);
        }
    }
}
