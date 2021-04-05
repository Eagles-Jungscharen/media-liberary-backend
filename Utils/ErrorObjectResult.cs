using System;
using Microsoft.AspNetCore.Mvc;

namespace EaglesJungscharen.MediaLibrary.Utils
{
    public class ErrorObjectResult:ObjectResult {

        public ErrorObjectResult(int status, object payload):base(payload) {
            this.StatusCode = status;
        }

        public static ErrorObjectResult Build(FunctionException e) {
            string trace = e.StackTrace;
            var payload = new {error=e.Message, trace=trace};
            return new ErrorObjectResult(e.StatusCode, payload);
        }

        public static ErrorObjectResult BuildError500(Exception e) {
            string trace = e.StackTrace;
            var payload = new {error=e.Message, trace=trace};
            return new ErrorObjectResult(500, payload);
        }

    }
}