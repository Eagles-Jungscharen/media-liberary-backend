using System.Collections.Generic;
namespace EaglesJungscharen.MediaLibrary.Models {

    public class User {
        public string FirstName {get;set;}
        public string LastName {get;set;}
        public string EMail {get;set;}
        public List<string> Scopes {set;get;}
        public bool IsAdmin {set;get;}
        public bool IsContributor {set;get;}
        public bool IsPictureAdmin {set;get;}
        public bool IsPictureContributor {set;get;}
    }
}