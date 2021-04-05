using System;
namespace EaglesJungscharen.MediaLibrary.Utils {

    public class FunctionException:Exception {
        public int StatusCode {get;}

        public FunctionException(string message, int statusCode):base(message) {
            this.StatusCode = statusCode;
        }
        public FunctionException(string message, Exception inner, int statusCode):base(message,inner) {
            this.StatusCode = statusCode;
        }
    }
}