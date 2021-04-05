using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;
namespace EaglesJungscharen.MediaLibrary.Models {
    public class MediaCollectionDefinition {
        public string Id {set;get;}
        public string Title {set;get;}
        public string Description {set;get;}
        public List<MediaItemDefinition> Items {set;get;}
    }

    public class MediaItemDefinition {
        public string Key {set;get;}
        public string Title {set;get;}
        public string Description {set;get;}
        public string Type {set;get;}
        public string Status {set;get;}
    }
}

    