using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NaughtyAttributes;
using UGM.Core;
using UnityEngine;
using static UGM.Core.UGMDataTypes;

namespace UGM.Examples.Inventory
{
    /// <summary>
    /// The GetNftsOwned class retrieves NFTs owned by a specific address.
    /// It manages pagination, storing pages and cursors, and provides methods to get the next or previous page of NFTs.
    /// </summary>
    /// <remarks>
    /// This class is attached to a GameObject and requires the NaughtyAttributes and Newtonsoft.Json namespaces.
    /// It uses a dictionary to store the pages and their respective cursors. The MaxPageNumber property determines
    /// the maximum page number for GetNftsByAddress and is used to determine how many page buttons to show.
    /// The GetNftsByAddress method retrieves a list of TokenInfo objects asynchronously. It accepts parameters for the
    /// page number, wallet address, and a flag to retrieve all pages recursively. The method retrieves a maximum of
    /// 100 results per request. The GetNextPage and GetPreviousPage methods are button functions to navigate through the pages.
    /// </remarks>
    public class GetNftsOwned : MonoBehaviour
    {
        [Tooltip("Variable to store the wallet address.")]
        [SerializeField]
        private string address;

        [Tooltip("Variable to store the current page number.")]
        [SerializeField]
        private int currentPage = 0;

        /// <summary>
        /// Dictionary to store the pages and their respective cursors.
        /// </summary>
        private Dictionary<int, string> pages = new Dictionary<int, string>();

        [Tooltip("Use this property to determine how many page buttons to show and what the maximum pageNumber is for GetNftsByAddress.")]
        public int MaxPageNumber { get => pages.Count - 1; }

        /// <summary>
        /// Retrieves a list of TokenInfo objects representing NFTs owned by a specific address.
        /// </summary>
        /// <param name="walletAddress">The address for which to retrieve the NFTs. Defaults to an empty string.</param>
        /// <param name="pageNumber">The page number of the NFTs to retrieve. Defaults to 0.</param>
        /// <param name="getAllPagesRecursive">Flag indicating whether to retrieve all pages recursively. 
        /// WARNING: This is recursive as long as the response returns a cursor.
        /// Suggested approach is to only use this with pageNumber 0 during initial loading.
        /// To check for new pages during gameplay, you can use this with pageNumber = MaxPageNumber.
        /// Each request gets a maximum of 100 results.</param>    
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of TokenInfo objects.</returns>
        [Button]
        public async Task<List<TokenInfo>> GetNftsByAddress(int pageNumber = 0, string walletAddress = "",  bool getAllPagesRecursive = false)
        {
            if(!string.IsNullOrEmpty(walletAddress) && address != walletAddress)
            {
                address = walletAddress;
                pages.Clear();
                currentPage = 0;
            }
            if(pageNumber == 0 && !pages.ContainsKey(0))
            {
                pages.Add(0, "");
            }
            if (!pages.ContainsKey(pageNumber))
            {
                Debug.LogError("Their is no cursor for this page number yet");
                return null;
            }
            string cursor = pages[pageNumber];
            // Retrieve the requested page
            var response = await UGMManager.GetNftsOwned(address, cursor);
            if (response == null)
            {
                Debug.LogWarning("NULL response");
                return null;
            }
            var result = response.result;
            //Their is a cursor for the next page and it has not been added yet
            if (!string.IsNullOrEmpty(response.cursor) && !pages.ContainsKey(response.page))
            {
                Debug.Log(response.cursor);
                pages.Add(response.page, response.cursor);
                if (getAllPagesRecursive)
                {
                    result = result.Concat(await GetNftsByAddress(pageNumber + 1, address, getAllPagesRecursive)).ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// Button function to get the next page of NFTs.
        /// </summary>
        [Button]
        public void GetNextPage()
        {
            int nextPageNumber = currentPage + 1;

            if (!pages.ContainsKey(nextPageNumber))
            {
                Debug.LogWarning("There are no more pages");
                return;
            }
            currentPage = nextPageNumber;
            GetNftsByAddress(nextPageNumber);
        }

        /// <summary>
        /// Button function to get the previous page of NFTs.
        /// </summary>
        [Button]
        public void GetPreviousPage()
        {
            int previousPageNumber = currentPage - 1;

            if (previousPageNumber < 0)
            {
                Debug.LogWarning("This is the first page");
                return;
            }
            currentPage = previousPageNumber;
            GetNftsByAddress(previousPageNumber);
        }
    }
}
