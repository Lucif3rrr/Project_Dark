using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LightBulbs : MonoBehaviour, ISelectable
{
    [SerializeField] private float timer;
    [SerializeField] private Light2D lightBulb;

    public AudioSource audioSource;
    public AudioClip[] clipArrays;

    private void Awake()
    {
        lightBulb.gameObject.SetActive(false);
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

        public void Trigger()
    {
        LightsOn();
        StartCoroutine(Timer());
    }

    void LightsOn()
    {
        lightBulb.gameObject.SetActive(true);
        audioSource.PlayOneShot(RandomClip());
    }

    void LightsOff()
    {
        lightBulb.gameObject.SetActive(false);
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(timer);
        LightsOff();
    }

    AudioClip RandomClip()
    {
        return clipArrays[Random.Range(0, clipArrays.Length)];
    }
}
