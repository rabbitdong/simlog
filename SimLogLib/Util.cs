using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace SimLogLib
{
    /// <summary>
    /// Simple Log Library tools method.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Get the string of the name/value list.
        /// </summary>
        /// <param name="parameters">The parameters</param>
        /// <returns>String expression for the name/value parameters</returns>
        public static string GetLogString(this NameValueCollection parameters)
        {
            StringBuilder str = new StringBuilder();

            string[] keys = parameters.AllKeys;
            foreach (string key in keys)
            {
                if (parameters[key] != null)
                {
                    str.AppendFormat("[{0}:{1}]&", key, parameters[key]);
                }
            }

            //Remove the last '&' char.
            if (str.Length > 0)
            {
                str.Remove(str.Length - 1, 1);
            }

            return str.ToString();
        }
    }
}
