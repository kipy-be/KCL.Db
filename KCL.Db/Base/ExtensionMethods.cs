using System;
using System.Data;

namespace KCL.Db
{
    internal static class ExtensionMethods
    {
        public static void CloseAndDispose(this IDataReader reader)
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
                reader = null;
            }
        }
    }
}
