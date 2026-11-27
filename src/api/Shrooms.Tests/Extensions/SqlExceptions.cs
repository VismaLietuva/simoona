using System;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace Shrooms.Tests.Extensions
{
    /// <summary>
    /// SqlException has no public constructor; builds one the way the driver does.
    /// </summary>
    public static class SqlExceptions
    {
        public static SqlException UniqueViolation() => WithNumber(2627, "Violation of UNIQUE KEY constraint");

        public static SqlException WithNumber(int number, string message)
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

            var error = (SqlError)typeof(SqlError)
                .GetConstructor(flags, null,
                    new[] { typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int), typeof(Exception) },
                    null)
                .Invoke(new object[] { number, (byte)1, (byte)14, "server", message, "", 0, null });

            var errors = (SqlErrorCollection)typeof(SqlErrorCollection)
                .GetConstructor(flags, null, Type.EmptyTypes, null)
                .Invoke(null);
            typeof(SqlErrorCollection).GetMethod("Add", flags).Invoke(errors, new object[] { error });

            return (SqlException)typeof(SqlException)
                .GetMethod("CreateException", flags, null, new[] { typeof(SqlErrorCollection), typeof(string) }, null)
                .Invoke(null, new object[] { errors, "16.0" });
        }
    }
}
