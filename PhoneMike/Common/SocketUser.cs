using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhoneMike.Common
{
    public class SocketUserCollection:System.Collections.IEnumerable//,System.Collections.ICollection,System.Collections.IList
    {
        public System.Collections.IEnumerator GetEnumerator()
        {

            return new SockUser();
        }
    }

    public class SockUser:System.Collections.IEnumerator
    {
        public object Current
        {     
            get
            {
                return new object();
            }
        }

        public bool MoveNext()
        {
            return true;
        }

        public void Reset()
        {

        }
    }
}