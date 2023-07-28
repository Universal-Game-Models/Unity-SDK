using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UGM.Core;
using UnityEngine;

namespace UGM.Examples.GlobalLoaderController
{
    /// <summary>
    /// This class manage all the UGMDownloader objects
    /// Add or Remove UGMDownloader by using the event called UGMManager.OnStartUGMDownloader and UGMManager.OnDestroyUGMDownloader
    /// </summary>
    public class GlobalLoaderManager : MonoBehaviour
    {
        /// <summary>
        ///  List of UGMDownloader in the scene. All the UGMDownloader here will be use for enable or disable when the player is within range.
        /// </summary>
        private List<UGMDownloader> ugmDownloaderList = new List<UGMDownloader>();
        [Required("This is required to reference the player position")]
        public Transform playerTransform;
        [Tooltip("The distance between the model and the player")]
        public float rangeToEnableModelFromPlayer = 25f;
        
        #region ------------------------------------- Main -----------------------------------------------------
        private void Start()
        {
            List<UGMDownloader> existingUgmDownloaderList = FindAllUGMDownloaderInTheScene();
            AddUGMDownloaderToTheList(existingUgmDownloaderList);
        }
        /// <summary>
        /// Hook up the events from the UGMManager for add or remove UGMDownloader 
        /// </summary>
        private void OnEnable()
        {
            UGMManager.OnStartUGMDownloader.AddListener(AddUGMDownloaderToTheList);
            UGMManager.OnDestroyUGMDownloader.AddListener(RemoveUGMDownloaderToTheList);
        }

        private void OnDisable()
        {
            UGMManager.OnStartUGMDownloader.RemoveListener(AddUGMDownloaderToTheList);
            UGMManager.OnDestroyUGMDownloader.RemoveListener(RemoveUGMDownloaderToTheList);
        }
        /// <summary>
        /// Check the range of the player from all the models
        /// </summary>
        private void FixedUpdate()
        {
            UpdateRangeOfPlayerFromUGMObjectList();
        }

        /// <summary>
        /// Disable or Enable the Model when the player is within range
        /// </summary>
        private void UpdateRangeOfPlayerFromUGMObjectList()
        {
            int numOfUGMDownloaderInTheList = ugmDownloaderList.Count;
            
            for (int i = 0; i < numOfUGMDownloaderInTheList; i++)
            {
                Transform ugmTransform = ugmDownloaderList[i].transform;
                if (IsPlayerInRange(ugmTransform))
                {
                    EnableGameObject(ugmTransform.gameObject);
                }
                else
                {
                    DisableGameObject(ugmTransform.gameObject);
                }
            }
        }

        /// <summary>
        /// Add the UgmDownloader model to the list 
        /// </summary>
        /// <param name="ugmDownloader"></param>
        private void AddUGMDownloaderToTheList(UGMDownloader ugmDownloader)
        {
            if (IsUGMExistInTheList(ugmDownloader)) return;
            ugmDownloaderList.Add(ugmDownloader);
        }
        

        /// <summary>
        /// Remove the UgmDownloader model to the list 
        /// </summary>
        /// <param name="ugmDownloader"></param>
        private void RemoveUGMDownloaderToTheList(UGMDownloader ugmDownloader)
        {
            ugmDownloaderList.Remove(ugmDownloader);
        }
        
        #endregion ------------------------------------- End Main -----------------------------------------------------
        
        /// <summary>
        /// Check if the ugmDownloader already inside the list
        /// </summary>
        /// <param name="ugmDownloader"></param>
        /// <returns></returns>
        private bool IsUGMExistInTheList(UGMDownloader ugmDownloader)
        {
            return ugmDownloaderList.Contains(ugmDownloader) == true;
        }
        
        /// <summary>
        /// Get all the UGMDownloader in the Scene
        /// </summary>
        /// <returns></returns>
        private List<UGMDownloader> FindAllUGMDownloaderInTheScene()
        {
            return FindObjectsOfType<UGMDownloader>().ToList();
        }
        /// <summary>
        /// Enable the Gameobject of the UGMDownloader Model
        /// </summary>
        /// <param name="ugmObject"></param>
        private void EnableGameObject(GameObject ugmObject)
        {
            if (ugmObject.activeSelf == true) return;
            ugmObject.SetActive(true);
        }
        
        /// <summary>
        /// Disable the Gameobject of the UGMDownloader Model
        /// </summary>
        /// <param name="ugmObject"></param>
        private void DisableGameObject(GameObject ugmObject)
        {
            if (ugmObject.activeSelf == false) return;
            ugmObject.SetActive(false);
        }
        /// <summary>
        /// Returns if the player is within the range of ugm Game object
        /// </summary>
        /// <param name="ugmObject"></param>
        /// <returns></returns>
        private bool IsPlayerInRange(Transform ugmObject)
        {
            return (ugmObject.position - playerTransform.position).sqrMagnitude < rangeToEnableModelFromPlayer;
        }
        /// <summary>
        /// Add a list of ugmdownloader to the list of ugmdownloader that GlobalLoaderManager use for enabling and disabling game object.
        /// </summary>
        /// <param name="existingUGMDownloaderList"></param>
        private void AddUGMDownloaderToTheList(List<UGMDownloader> newUGMDownloaderList)
        {
            foreach (UGMDownloader ugmDownloader in newUGMDownloaderList)
            {
                if (ugmDownloader.CompareTag("Player")) continue;
                AddUGMDownloaderToTheList(ugmDownloader);
            }
        }
    }
}