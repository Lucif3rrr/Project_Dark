using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class PlayerAttack : NetworkBehaviour
{

    private PlayerStats stats;

    [SerializeField] private Transform attackCircle;
    [SerializeField] private float attackRate;
    private bool canAttack = true;

    [SerializeField] private float attackRadius;
    private Collider2D[] cirCol;
    [SerializeField] private LayerMask attackLayer;

    [SerializeField] private Light2D attackEffects;

    public AudioSource audioSource;
    public AudioClip[] clipArrays;

    private void Start()
    {
        stats = GetComponent<PlayerStats>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        PlayerDetection();
    }

    private void PlayerDetection()
    {
        
        cirCol = Physics2D.OverlapCircleAll(attackCircle.position, attackRadius, attackLayer);

        if (cirCol.Length > 0)
        {
            for (int i = 0; i < cirCol.Length; i++)
            {
                if (cirCol[i].transform.parent.gameObject != gameObject)
                {
                    //Another Player within range
                    PlayerStats ph = cirCol[i].gameObject.GetComponentInParent<PlayerStats>();
                    Attack(ph);
                }
            }
        }

        AttackEffects();
    }

    private void Attack(PlayerStats target)
    {
        if (canAttack && !stats.grace)
        {
            if (!isClientOnly) target.ModifyHealth(-1);
            attackEffects.intensity = Random.Range(0.8f, 1.0f);
            StartCoroutine("AttackCooldown");
            audioSource.PlayOneShot(RandomClip());
        }
    }

    private void AttackEffects()
    {
        //Remove when better attack effects are here
        if (attackEffects.intensity > 0) attackEffects.intensity = Mathf.Lerp(attackEffects.intensity, 0, 10 * Time.deltaTime);
        if (attackEffects.intensity < 0.01) attackEffects.intensity = 0;
    }

    [Command(ignoreAuthority = true)]
    private void CmdAttackEffects()
    {
        RpcAttackEffects();
    }

    [ClientRpc]
    private void RpcAttackEffects()
    {
        AttackEffects();
    }

    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackRate);
        canAttack = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    AudioClip RandomClip()
    {
        return clipArrays[Random.Range(0, clipArrays.Length)];
    }
}
