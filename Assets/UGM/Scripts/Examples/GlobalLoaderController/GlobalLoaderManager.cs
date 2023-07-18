using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UGM.Core;
using UnityEngine;

namespace UGM.Examples.GlobalLoaderController
{
    public class GlobalLoaderManager : MonoBehaviour
    {
        private List<UGMDownloader> ugmDownloaderList = new List<UGMDownloader>();
        [Required("This is required to reference the player position")]
        public Transform playerTransform;

        public float rangeToEnableModelFromPlayer = 25f;
        
        private void Start()
        {
            List<UGMDownloader> existingUgmDownloaderList = FindObjectsOfType<UGMDownloader>().ToList();
            AddUgmDownloaderToTheList(existingUgmDownloaderList);
        }

        private void FixedUpdate()
        {
            UpdateRangeOfPlayerFromUGMObjectList();
        }

        private void UpdateRangeOfPlayerFromUGMObjectList()
        {
            for (int i = 0; i < ugmDownloaderList.Count; i++)
            {
                Transform ugmObject = ugmDownloaderList[i].transform;
                if (IsPlayerInRange(ugmObject))
                {
                    EnableGameObject(ugmObject.gameObject);
                }
                else
                {
                    DisableGameObject(ugmObject.gameObject);
                }
            }
        }
        
        private void EnableGameObject(GameObject ugmObject)
        {
            if (ugmObject.activeSelf == true) return;
            ugmObject.SetActive(true);
        }
        
        
        private void DisableGameObject(GameObject ugmObject)
        {
            if (ugmObject.activeSelf == false) return;
            ugmObject.SetActive(false);
        }

        private bool IsPlayerInRange(Transform ugmObject)
        {
            return (ugmObject.position - playerTransform.position).sqrMagnitude < rangeToEnableModelFromPlayer;
        }

        private void AddUgmDownloaderToTheList(List<UGMDownloader> existingUGMDownloaderList)
        {
            foreach (UGMDownloader ugmDownloader in existingUGMDownloaderList)
            {
                if (ugmDownloader.CompareTag("Player")) continue;
                ugmDownloaderList.Add(ugmDownloader);
            }
        }
    }
}