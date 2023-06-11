using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunWeapon : Weapon
{
    private static readonly int GunHandsHash = Animator.StringToHash("GunHands");
    private static readonly int ShootHash = Animator.StringToHash("Shoot");

    private GameObject bulletPrefab;
    private FireType fireType;
    private GunType gunType;
    private Coroutine shootingRoutine;
    private int burstAmount = 3;
    private int fireRate = 25;
    private float bulletSpeed = 100;
    private float bulletDistance = 2;
    private float maxRange = 100;
    private List<GameObject> bullets = new List<GameObject>();

    public void Init(int damage, FireType fireType, GunType gunType, GameObject bulletPrefab)
    {
        this.damage = damage;
        this.fireType = fireType;
        this.gunType = gunType;
        this.bulletPrefab = bulletPrefab;
        //No rifle animation setup that works
        SetGunHands(true);//gunType == GunType.Pistol
    }

    private void SetGunHands(bool isPistol)
    {
        animator.SetInteger(GunHandsHash, isPistol?1:2);
    }

    public override void Attack()
    {
        base.Attack();
        animator.SetBool(ShootHash, true);
        switch (fireType)
        {
            case FireType.Automatic:
                //Start a couroutine that continuously shoots
                shootingRoutine = StartCoroutine(ContinuousShooting());
                break;
            case FireType.Burst:
                //Start a couroutine that shoots a short burst of bullets
                shootingRoutine = StartCoroutine(BurstShooting());
                break;
            case FireType.Single:
                //Shoot a single bullet
                StartCoroutine(SingleShot());
                break;
            default:
                break;
        }
    }
    private void OnDestroy()
    {
        animator.SetInteger("GunHands", 0);
    }

    public override void StopAttacking()
    {
        //Stop the coroutine if their is one
        if (shootingRoutine != null) StopCoroutine(shootingRoutine);
        shootingRoutine = null;
        animator.SetBool(ShootHash, false);
        base.StopAttacking();
    }

    private IEnumerator ContinuousShooting()
    {
        while (isAttacking)
        {
            Shoot();
            yield return new WaitForSeconds(1 / fireRate);
        }
        StopAttacking();
    }
    private IEnumerator BurstShooting()
    {
        for (int i = 0; i < burstAmount; i++)
        {
            Shoot();
            yield return new WaitForSeconds(1 / fireRate);
        }
        StopAttacking();
    }
    private IEnumerator SingleShot()
    {
        Shoot();
        animator.SetBool(ShootHash, false);
        yield return new WaitForSeconds(1 / fireRate);
        StopAttacking();
    }
    private void Shoot()
    {
        // Instantiate a bullet flying in the forward direction
        if(bulletPrefab == null)
        {
            Debug.LogError("GunWeapon was not Initialized with a bullet prefab");
            return;
        }
        Quaternion rot = animator.transform.rotation;

        GameObject bulletInstance = Instantiate(bulletPrefab, transform.position, rot);
        // Add the bullet to the list
        bullets.Add(bulletInstance);
    }

    protected override void Update()
    {
        base.Update();
        // Perform raycasting from the bullets
        UpdateBullets();
    }

    private void UpdateBullets()
    {
        int layerMask = ~LayerMask.GetMask("Player"); // Exclude the "Player" layer
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            GameObject bullet = bullets[i];
            if (bullet == null)
            {
                // Remove destroyed bullets from the list
                bullets.RemoveAt(i);
                continue;
            }

            //Move the bullet
            bullet.transform.position += bullet.transform.forward * bulletSpeed * Time.deltaTime;

            // Perform a raycast from the bullet's position and forward direction
            RaycastHit hit;
            if (Physics.Raycast(bullet.transform.position, bullet.transform.forward, out hit, bulletDistance, layerMask))
            {
                if (hit.collider.gameObject != this.gameObject)
                {
                    // Handle the hit object
                    OnHit(hit.collider.gameObject);

                    Destroy(bullet.gameObject);
                    bullets.RemoveAt(i);
                }
            }
            else if (Vector3.Distance(bullet.transform.position, transform.position) > maxRange)
            {
                // Destroy the bullet if it travels beyond the bullet distance without hitting anything
                Destroy(bullet);
                bullets.RemoveAt(i);
            }
        }
    }
}
