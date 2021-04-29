using System;
using System.Collections.Generic;

namespace EaglesJungscharen.MediaLibrary.Models {
    public class MediaItem {
        public string Id {set;get;}
        public string MediaCollectionId {set;get;}
        public string Creator {set;get;}
        public DateTime Created {set;get;}
        public DateTime ItemDate {set;get;}
        public bool Published {set;get;}
        public string Titel {set;get;}
        public string Description {set;get;}
        public string Author {set;get;}
        public List<string> Keywords {set;get;}
        public List<MediaItemEntry> Entries{set;get;}
    }


    public class MediaItemEntry {
        public string MediaItemId {set;get;}
        public string CollectionItemKey {set;get;}
        public string Value {set;get;}
        public string DownloadUrl {set;get;}
    }
}