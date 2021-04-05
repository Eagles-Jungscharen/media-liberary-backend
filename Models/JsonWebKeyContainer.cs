using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace EaglesJungscharen.MediaLibrary.Models {
    public class JsonWebKeyContainer {
        public List<JsonWebKey> Keys {get;set;}
    }
}