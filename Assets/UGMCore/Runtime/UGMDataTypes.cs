using System.Collections.Generic;
using Newtonsoft.Json;

namespace UGM.Core
{
    public static class UGMDataTypes
    {
        public class Metadata
        {
            public string name;
            public string description;
            public string token_id;
            public string image;
            public Attribute[] attributes;
        }
        public class Attribute
        {
            public string trait_type;
            public object value;
        }

        //Classes for Models Owned Response
        [System.Serializable]
        public class TokenInfo
        {
            public string token_address;
            public string token_id;
            public string owner_of;
            public string block_number;
            public string block_number_minted;
            public string token_hash;
            public string amount;
            public string contract_type;
            public string name;
            public string symbol;
            public string token_uri;
            [JsonProperty("metadata")]
            private string metadataString;
            private Metadata _metadata;
            [JsonProperty("")]
            public Metadata metadata
            {
                get
                {
                    if (_metadata == null && !string.IsNullOrEmpty(metadataString))
                    {
                        // Deserialize the metadata string into a Metadata object
                        _metadata = JsonConvert.DeserializeObject<Metadata>(metadataString);
                    }
                    return _metadata;
                }
            }
            public string last_token_uri_sync;
            public string last_metadata_sync;
            public string minter_address;
            public bool possible_spam;
        }
        [System.Serializable]
        public class NFTsOwnedResult
        {
            public string total;
            public int page;
            public int page_size;
            public string cursor;
            public List<TokenInfo> result;
            public string status;
        }
    }
}
