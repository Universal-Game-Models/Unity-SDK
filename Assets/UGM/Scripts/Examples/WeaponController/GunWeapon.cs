﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunWeapon : Weapon
{
    private static readonly int LeftItemHash = Animator.StringToHash("LeftItem");
    private static readonly int RightItemHash = Animator.StringToHash("RightItem");
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
    private int hand;

    public void Init(int damage, FireType fireType, GunType gunType, int hand, GameObject bulletPrefab)
    {
        this.damage = damage;
        this.fireType = fireType;
        this.gunType = gunType;
        this.bulletPrefab = bulletPrefab;
        this.hand = hand;
        SetGunHands();
    }

    private void SetGunHands()
    {
        if(gunType == GunType.Pistol)
        {
            animator.SetInteger(hand == 0 ? RightItemHash : LeftItemHash, 1);
        }
        else if(gunType == GunType.Rifle)
        {
            //Unequip all other hand items
            if (animator) {
                var weaponControllers = animator.GetComponentsInChildren<WeaponController>();
                foreach (var weaponController in weaponControllers)
                {
                    weaponController.DestroyWeapon(hand);
                }
            }
            animator.SetInteger(RightItemHash, 2);
            animator.SetInteger(LeftItemHash, 2);
        }
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
        var handAnimHash = hand == 0 ? RightItemHash : LeftItemHash;
        var offhandAnimHash = hand == 0 ? LeftItemHash : RightItemHash;
        if (gunType == GunType.Pistol)
        {
            animator.SetInteger(handAnimHash, -1);
        }
        else if (gunType == GunType.Rifle)
        {
            var offhandAnimInt = animator.GetInteger(offhandAnimHash);
            if(offhandAnimInt == 2)
            {
                animator.SetInteger(offhandAnimHash, -1);
            }
            animator.SetInteger(handAnimHash, -1);
        }
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
        // Instantiate a bullet flying towards the center of the screen
        if (bulletPrefab == null)
        {
            Debug.LogError("GunWeapon was not initialized with a bullet prefab");
            return;
        }

        // Calculate the center of the screen
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);

        // Perform a raycast from the camera's position through the center of the screen
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // Calculate the direction from the player to the hit point
            Vector3 playerToHit = hit.point - transform.position;

            // Calculate the rotation to aim the bullet towards the hit point
            Quaternion rotation = Quaternion.LookRotation(playerToHit);

            GameObject bulletInstance = Instantiate(bulletPrefab, transform.position, rotation);
            // Add the bullet to the list
            bullets.Add(bulletInstance);
        }
        else
        {
            // If the raycast didn't hit anything, shoot in the camera's forward direction
            // This is not great as the bullets aren't shooting exactly the right direction
            Quaternion rotation = Quaternion.LookRotation(Camera.main.transform.forward);

            GameObject bulletInstance = Instantiate(bulletPrefab, transform.position, rotation);
            // Add the bullet to the list
            bullets.Add(bulletInstance);
        }
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