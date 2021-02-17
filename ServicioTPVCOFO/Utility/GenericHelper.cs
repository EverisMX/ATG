using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace ServicioTPVAgenteLocal.Utility
{
    public class GenericHelper
    {
        #region Methods 

        /// <summary>
        /// Method to generate a security hash
        /// </summary>
        /// <param name="oHash"></param>
        /// <param name="sTimeStamp"></param>
        /// <returns></returns>
        internal static string GeneratedSignature(Object oHash, String sTimeStamp)
        {
            return GenerateSHA256(OrderObjectPropertiesValuesAlphabetically(oHash) + sTimeStamp + "IntegracionWSREST");
        }

        /// <summary>
        /// Orders alphabetically given object properties and return the concatenation of its values
        /// </summary>
        /// <param name="obj">Object to process</param>
        /// <returns>Resulting concatenation string</returns>
        internal static string OrderObjectPropertiesValuesAlphabetically(object obj)
        {
            StringBuilder result = new StringBuilder();
            if (obj != null)
            {
                List<PropertyInfo> props = obj.GetType().GetProperties().OrderBy(x => x.Name).ToList();
                foreach (PropertyInfo pi in props)
                {
                    if (pi.Name != "Signature")
                    {
                        Type type = pi.PropertyType;
                        if (pi.GetValue(obj, null) != null)
                        {
                            if (type.IsPrimitive || type.Equals(typeof(string)))
                            {
                                result.Append(pi.GetValue(obj, null));
                            }
                            else if (type.Equals(typeof(decimal)))
                            {
                                result.Append(pi.GetValue(obj, null).ToString().Replace(",", "."));
                            }
                            else if (type.IsArray)
                            {
                                foreach (var p in (Array)pi.GetValue(obj, null))
                                {
                                    Type type2 = p.GetType();

                                    if (type2.IsPrimitive || type2.Equals(typeof(string)))
                                    {
                                        result.Append(p);
                                    }
                                    else
                                    {
                                        result.Append(OrderObjectPropertiesValuesAlphabetically(p));
                                    }
                                }
                            }
                            else if (type.IsGenericType)
                            {
                                foreach (var g in (IEnumerable)pi.GetValue(obj, null))
                                {
                                    Type type2 = g.GetType();

                                    if (type2.IsPrimitive || type2.Equals(typeof(string)))
                                    {
                                        result.Append(g);
                                    }
                                    else
                                    {
                                        result.Append(OrderObjectPropertiesValuesAlphabetically(g));
                                    }
                                }
                            }
                            else if (type.IsEnum)
                            {
                                result.Append((int)pi.GetValue(obj, null));
                            }
                            else
                            {
                                result.Append(OrderObjectPropertiesValuesAlphabetically(pi.GetValue(obj, null)));
                            }
                        }
                        string strProcesoif = result.ToString();
                    }
                }
            }

            string strProceso = result.ToString();
            return result.ToString();
        }

        /// <summary>
        /// Generates a SHA256 string
        /// </summary>
        /// <param name="stringToHash">string to hash</param>
        /// <returns>hashed string</returns>
        internal static string GenerateSHA256(string stringToHash)
        {
            string hashedString = null;

            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] hashedSignature = mySHA256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(stringToHash));
            hashedString = ByteArrayToHex(hashedSignature);

            return hashedString;
        }

        /// <summary>
        /// Converts a byte array to hex string.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns>
        /// hex string
        /// </returns>
        internal static string ByteArrayToHex(byte[] array)
        {
            if (array != null)
            {
                return BitConverter.ToString(array).Replace("-", string.Empty);
            }

            return null;
        }
        #endregion
    }
}