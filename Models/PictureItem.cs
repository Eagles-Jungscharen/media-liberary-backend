using System;
using System.Collections.Generic;
namespace EaglesJungscharen.MediaLibrary.Models {
    public class PictureItem {
        public string Id {set;get;}
        public string PictureCollectionId {set;get;}
        public string Creator {set;get;}
        public DateTime Created {set;get;}
        public DateTime ItemDate {set;get;}
        public bool Published {set;get;}
        public string Titel {set;get;}
        public string Description {set;get;}
        public string Author {set;get;}
        public List<string> Keywords {set;get;}
        public string OrignalPictureURL {set;get;}
        public string AlbumPictureURL {set;get;}
        public string TumbNailPictureURL {set;get;}
    }
}