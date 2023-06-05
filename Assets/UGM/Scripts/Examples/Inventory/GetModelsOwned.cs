using NaughtyAttributes;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static UGMAssetManager;

public class GetModelsOwned : MonoBehaviour
{
    [SerializeField]
    private string address;
    [SerializeField]
    private int currentPage = 0;
    private Dictionary<int, string> pages = new Dictionary<int, string>();

    [Button]
    public async Task<List<ModelsOwnedTokenInfo>> GetModelsOwnedByAddress(int pageNumber = 0)
    {
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
        var response = await UGMAssetManager.GetModelsOwned(address, cursor);
        if (response == null)
        {
            Debug.LogWarning("NULL response");
            return null;
        }
        //Their is a cursor for the next page and it has not been added yet
        if (!string.IsNullOrEmpty(response.cursor) && !pages.ContainsKey(response.page))
        {
            Debug.Log(response.cursor);
            pages.Add(response.page, response.cursor);
        }

        return response.result;
    }

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
        GetModelsOwnedByAddress(nextPageNumber);
    }

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
        GetModelsOwnedByAddress(previousPageNumber);
    }
}
