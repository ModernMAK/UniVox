using System;
using System.Runtime.Serialization;

namespace UniVox.Types
{
    public class ObsoleteException : Exception
    {
        public ObsoleteException(SerializationInfo info, StreamingContext context) : base(
            info, context)
        {
        }

        public ObsoleteException(string original, string substitution, Exception innerException) : base(
            Format(original, substitution), innerException)
        {
        }

        public ObsoleteException(string substitution, Exception innerException) : base(
            Format(substitution), innerException)
        {
        }

        public ObsoleteException(string original, string substitution) : base(Format(original, substitution))
        {
        }

        public ObsoleteException(string substitution) : base(Format(substitution))
        {
        }
        
        public ObsoleteException() : base("This has been marked obsolete!")
        {
        }

        private static string Format(string substitution)
        {
            return $"'Please use '{substitution}' instead.";
        }

        private static string Format(string original, string substitution)
        {
            return $"'{original}' is obsolete, please use '{substitution}' instead.";
        }

//        public ObsoleteException(string message, string funcName) : 
//        {
//            
//        }
//        public ObsoleteException(string message, string funcName, string substitution)
    }
}