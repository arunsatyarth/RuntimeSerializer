using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuntimeSerializer
{

    [Serializable]
    public class DefaultCtorAbsentException : Exception
    {
        public DefaultCtorAbsentException(string err)
            : base(err)
        {

        }
    }
    [Serializable]
    public class SerializingHandlerException : Exception
    {
        public SerializingHandlerException(string message)
            : base(message)
        {

        }
    }
}

