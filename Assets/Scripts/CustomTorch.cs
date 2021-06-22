using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class CustomTorch : MonoBehaviour
{
    [SerializeField] private GameObject selfLight;
    [SerializeField] private Light2D torch;
    [SerializeField] private float torchRange;

    [Range(0.1f, 3.0f)]
    [SerializeField] private float torchRangeOffset;
    [SerializeField] private LayerMask torchBlockLayer;

    public AudioSource audioSource;
    public AudioClip TorchSound;

    public bool torchOn;
    private float radius;

    public TorchBattery battery;
    public bool outOfBattery = false;

    [SerializeField] private PlatformerMovement move;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        battery = GetComponent<TorchBattery>() ? GetComponent<TorchBattery>() : null;
    }

    // Update is called once per frame
    void Update()
    {
        if (torchOn)
        {
            TorchRange();
        }
    }

    public void Torch()
    {
        if (torchOn) {

            TorchOff();

        }
        else
        {
            TorchOn();
        }
    }

    public void TorchOn()
    {
        selfLight.SetActive(true);
        torch.enabled = true;
        torchOn = true;
        audioSource.PlayOneShot(TorchSound);
    }

    public void TorchOff()
    {
        selfLight.SetActive(false);
        torch.enabled = false;
        torchOn = false;
        audioSource.PlayOneShot(TorchSound);
    }

    private void TorchRange()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, torchRange, torchBlockLayer);

        if (hit)
        {
            torch.pointLightOuterRadius = hit.distance + torchRangeOffset;
        }
        else
        {
            torch.pointLightOuterRadius = torchRange;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.up * torchRange);
    }
}
