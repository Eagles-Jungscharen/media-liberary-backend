using System;
using System.Collections.Generic;
using System.Linq;
namespace EaglesJungscharen.MediaLibrary.Utils{

    public static class ValidationExtension {
        public static void IsNotNullOrEmpty(this string toTest, string errorMessage) {
            if (string.IsNullOrEmpty(toTest)) {
                throw new FunctionException(errorMessage,400);
            }
        }
        public static void IsNotNullOrEmpty(this IEnumerable<object> toTest, string errorMessage) {
            if (toTest == null || toTest.Count() ==0) {
                throw new FunctionException(errorMessage,400);
            }
        }
        public static void IsNotNull(this DateTime toTest, string errorMessage) {
            if(toTest == null) {
                throw new FunctionException(errorMessage,400);
            }
        }
        public static void IsBeforOtherDate(this DateTime toTest, DateTime after, string errorMessage) {
            if(toTest == null || after == null) {
                throw new FunctionException("Please provide correct Dates",400);
            }
            if (after < toTest) {
                throw new FunctionException(errorMessage,400);
            }
        }
    }
}