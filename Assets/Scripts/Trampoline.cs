using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
    //private AudioManager audioManager;

    //public string TrampoSound;
    public AudioSource audioSource;
    public AudioClip TrampSound;

    [SerializeField] private float force;
    [SerializeField] private float forceXamp = 3.0f;
    [SerializeField] private float forceYamp = 3.0f;

    private void Start()
    {
        // audioManager = AudioManager.instance;
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {   
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(CalculateForceAngle());
            collision.gameObject.GetComponent<PlatformerMovement>().AddExternalForceX(CalculateForceAngle().x);
            //audioManager.PlaySound(TrampoSound);
            audioSource.PlayOneShot(TrampSound);
        }
    }

    private Vector2 CalculateForceAngle()
    {
        Vector2 currentForce = transform.right * force;
        currentForce.x = currentForce.x * forceXamp;
        currentForce.y = currentForce.y * forceYamp;
        return currentForce;
    }
}
