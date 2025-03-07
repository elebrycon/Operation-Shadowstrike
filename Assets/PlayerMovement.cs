using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    [Header("Movement")]
    public float speed = 9f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public bool sneak = false;
    [Header("HP")]
    public float Maxhealth = 100;
    public float health = 100;
    public Slider healthSlider;
    [Header("PickUp")]
    public int pickUps = 0;
    public TMP_Text ObjText;

    [Header("Audio")]
    public AudioSource hitSound;
    public AudioSource endScreenSound;

    [Header("Random")]
    public GameObject winTrigger;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public GameObject loseScreen;

    Vector3 velocity;
    bool isGrounded;

    void Start()
    {
        UpdateUI();
        /*Time.timeScale = 1f;
        pauseMenu.GetComponent<PauseMenu>().Resume();*/
    }
    void UpdateUI()
    {
        healthSlider.value = health / Maxhealth;
    }
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance,groundMask);

        if(isGrounded&&velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        //Ulazi u sneak
        if (Input.GetKeyDown(KeyCode.LeftShift) && sneak == false)
        {
            speed = 4f;
            sneak = true;
        }
        else if(Input.GetKeyDown(KeyCode.LeftShift) && sneak == true)
        {
            speed = 9f;
            sneak = false;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        /*if (Input.GetKeyDown(KeyCode.K))
        {
            hitDamage(10);
        }*/
        if(health <= 0)
        {
            StartCoroutine(endGame());
        }
        
    }
    public void hitDamage(float dmg)
    {
        health -= dmg;
        hitSound.Play();
        UpdateUI();
    }
    public void PickUps()
    {
        pickUps += 1;
        Debug.Log("Dimaonds:"+pickUps);
        if(pickUps == 5)
        {
            ObjText.text = "Escape  via  ship!";
            winTrigger.SetActive(true);

        }
    }
    IEnumerator endGame()
    {
        speed = 0;
        jumpHeight = 0;
        yield return new WaitForSeconds(0.2f);
        endScreenSound.Play();
        loseScreen.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;

    }
}
