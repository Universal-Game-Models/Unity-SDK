using UGM.Core;
using UGM.Examples.ThirdPersonController;
using UnityEngine;

namespace UGM.Examples.WeaponController
{
    public enum WeaponType
    {
        Melee,
        Gun
    }
    public enum MeleeWeaponType
    {
        Axe,
        Sword,
        Hammer
    }
    public enum FireType
    {
        Automatic,
        Burst,
        Single
    }
    public enum GunType
    {
        Pistol,
        Rifle
    }
    public enum WeaponTier
    {
        Common,
        Rare,
        Unique,
        Legendary
    }

    public class WeaponController : MonoBehaviour
    {
        [SerializeField]
        private GameObject bulletPrefab;

        private UGMDownloader ugmDownloader;

        private WeaponType weaponType;
        private MeleeWeaponType meleeWeaponType;
        private FireType fireType;
        private GunType gunType;
        private WeaponTier weaponTier;

        private MeleeWeapon meleeWeapon;
        private GunWeapon gunWeapon;
        private int hand = 0;

        private void Awake()
        {
            ugmDownloader = GetComponent<UGMDownloader>();
            if (ugmDownloader != null)
            {
                ugmDownloader.onMetadataSuccess.AddListener(HandleMetadataSuccess);
            }
            else
            {
                Debug.LogError("UGMDownloader component not found!");
            }
        }

        private void OnDestroy()
        {
            if (ugmDownloader != null)
            {
                ugmDownloader.onMetadataSuccess.RemoveListener(HandleMetadataSuccess);
            }
        }

        private void HandleMetadataSuccess(UGMDataTypes.Metadata metadata)
        {
            DetermineWeaponStats(metadata);
        }

        private void DetermineWeaponStats(UGMDataTypes.Metadata metadata)
        {
            // Check if the equipment is a weapon
            if (!HasTrait(metadata, "Equipment", "Weapon"))
                return;

            // Retrieve the weapon type attribute
            UGMDataTypes.Attribute weaponTypeAttribute = GetAttribute(metadata, "Weapon Type");
            if (weaponTypeAttribute == null)
                return;
            // Set the weapon type based on the attribute value
            if (System.Enum.TryParse(weaponTypeAttribute.value.ToString(), out WeaponType type))
                weaponType = type;
            else
                return;

            // Determine the stats based on the weapon type
            if (weaponType == WeaponType.Melee)
            {
                // Retrieve the melee weapon type attribute
                UGMDataTypes.Attribute meleeWeaponTypeAttribute = GetAttribute(metadata, "Melee Weapon");
                if (meleeWeaponTypeAttribute == null)
                    return;

                // Set the melee weapon type based on the attribute value
                if (System.Enum.TryParse(meleeWeaponTypeAttribute.value.ToString(), out MeleeWeaponType meleeType))
                    meleeWeaponType = meleeType;
                else
                    return;
            }
            else if (weaponType == WeaponType.Gun)
            {
                // Retrieve the fire type attribute
                UGMDataTypes.Attribute fireTypeAttribute = GetAttribute(metadata, "Fire Type");
                if (fireTypeAttribute == null)
                    return;
                // Set the fire type based on the attribute value
                if (System.Enum.TryParse(fireTypeAttribute.value.ToString(), out FireType ftype))
                    fireType = ftype;
                else
                    return;

                // Retrieve the gun type attribute
                UGMDataTypes.Attribute gunTypeAttribute = GetAttribute(metadata, "Gun Type");
                if (gunTypeAttribute == null)
                    return;
                // Set the gun type based on the attribute value
                if (System.Enum.TryParse(gunTypeAttribute.value.ToString(), out GunType gtype))
                    gunType = gtype;
                else
                    return;
            }

            // Retrieve the weapon tier attribute
            UGMDataTypes.Attribute weaponTierAttribute = GetAttribute(metadata, "Tier");
            if (weaponTierAttribute == null)
                return;

            // Set the weapon tier based on the attribute value
            if (System.Enum.TryParse(weaponTierAttribute.value.ToString(), out WeaponTier tier))
                weaponTier = tier;
            else
                return;
            Init();
        }

        private void Init()
        {
            // Set Avatar animation for the weapon type

            // Setup the weapon logic
            switch (weaponType)
            {
                case WeaponType.Melee:
                {
                    if (meleeWeapon == null)
                    {
                        //Add a MeleeWeapon component
                        meleeWeapon = ugmDownloader.InstantiatedGO.AddComponent<MeleeWeapon>();
                    }
                    var meleeDamage = 1;
                    switch (weaponTier)
                    {
                        case WeaponTier.Common:
                            meleeDamage = 10;
                            break;
                        case WeaponTier.Rare:
                            meleeDamage = 20;
                            break;
                        case WeaponTier.Unique:
                            meleeDamage = 30;
                            break;
                        case WeaponTier.Legendary:
                            meleeDamage = 40;
                            break;
                        default:
                            break;
                    }
                    meleeWeapon.Init(meleeDamage, meleeWeaponType);
                    break;
                }
                case WeaponType.Gun:
                {
                    if (gunWeapon == null)
                    {
                        //Add a GunWeapon component
                        gunWeapon = ugmDownloader.InstantiatedGO.AddComponent<GunWeapon>();
                    }
                    var gunDamage = 1;
                    switch (weaponTier)
                    {
                        case WeaponTier.Common:
                            gunDamage = 1;
                            break;
                        case WeaponTier.Rare:
                            gunDamage = 2;
                            break;
                        case WeaponTier.Unique:
                            gunDamage = 3;
                            break;
                        case WeaponTier.Legendary:
                            gunDamage = 4;
                            break;
                        default:
                            break;
                    }
                    if (ugmDownloader is HumanoidEquipmentLoader) {
                        var bone = ((HumanoidEquipmentLoader)ugmDownloader).humanoidBone;
                        if(bone == HumanBodyBones.RightHand)
                        {
                            hand = 0;
                        }
                        else if(bone == HumanBodyBones.LeftHand)
                        {
                            hand = 1;
                        }
                    }
                    gunWeapon.Init(gunDamage, fireType, gunType, hand, bulletPrefab);
                    break;
                }
            }
        }

        private UGMDataTypes.Attribute GetAttribute(UGMDataTypes.Metadata metadata, string traitType)
        {
            foreach (UGMDataTypes.Attribute attribute in metadata.attributes)
            {
                if (attribute.trait_type == traitType)
                    return attribute;
            }
            return null;
        }

        private bool HasTrait(UGMDataTypes.Metadata metadata, string traitType, string value)
        {
            foreach (UGMDataTypes.Attribute attribute in metadata.attributes)
            {
                if (attribute.trait_type == traitType && attribute.value.ToString() == value)
                    return true;
            }
            return false;
        }

        public void DestroyWeapon(int dontDestroyHand)
        {
            if (ugmDownloader && hand != dontDestroyHand)
            {
                DestroyImmediate(ugmDownloader.InstantiatedGO);
            }
        }
    }
}