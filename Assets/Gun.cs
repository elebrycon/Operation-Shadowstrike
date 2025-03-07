using UnityEngine;
using System.Collections;
using TMPro;

public class Gun : MonoBehaviour
{

    public int damage = 10;
    public float range = 100f;

    public int maxAmmo = 10;
    private int currentAmmo=10;
    public float reloadTime = 1f;
    private bool isReloading = false;
    //public int PickedUpAmmo = 0;

    public Camera fpsCam;
    public ParticleSystem muzzleflash;

    public Animator animator;

    public TMP_Text  text;
    public TMP_Text text2;

    public AudioSource shoot;
    public AudioSource reload;



    private void Start()
    {
        if (currentAmmo == -1)
        {
            currentAmmo = maxAmmo;
        }
        updateUIAmmo();
    }

    // Update is called once per frame
    void Update()
    {
        if (isReloading)
        {
            return;
        }

        if (currentAmmo <= 0 && maxAmmo <= 0)
        {
            return;
        }

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
        

        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading");
        reload.Play();
        yield return new WaitForSeconds(0.5f);

        animator.SetBool("Reload", true);

        yield return new WaitForSeconds(reloadTime);

        animator.SetBool("Reload", false);
        if (maxAmmo >= 10)
        {
            currentAmmo = 10;
            maxAmmo = maxAmmo - 10;
        }
        isReloading = false;
        updateUIAmmo();
    }
    void Shoot()
    {
        muzzleflash.Play();
        shoot.Play();
        currentAmmo--;
        updateUIAmmo();

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);

            AIControl target = hit.transform.GetComponent<AIControl>();

            if(target != null)
            {
                target.TakeDamage(damage);
            }

        }
    }
    public void PickUpAmmo()
    {
        maxAmmo += 10;
        Debug.Log("Ammo + 10");
        updateUIAmmo();
    }
    void updateUIAmmo()
    {
        text.text = currentAmmo.ToString();
        text2.text = maxAmmo.ToString();
    }
}
