/***********************************************************************************************

Name		    :  clsLibrary.cs
Description	    :  Used for common functions 
Interaction with:  N/A
Created By	    :  Claudio Sciarrillo
Created Date	:  2017-05-15
-----------------------------------------------------------------------------------------------
Changes History:
Date		| Done By	| Change Description
-----------------------------------------------------------------------------------------------
2017-05-15	|Claudio	| Creation

***********************************************************************************************/
using System;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;

namespace OWPApplications
{
    public class clsLibrary
    {
        public clsLibrary() { }


        /// <summary>
        /// Convert an object to String (ignoring errors)
        /// </summary>
        /// <param name="strInt"></param>
        /// <returns></returns>
        public static DateTime WebReadDate(string value, DateTime defaultValue)
        {
            try
            {
                if (value != null)
                {
                    string[] strParts = value.Split('-');
                    if (strParts.Length > 2)
                    {
                        int year = dBReadInt(strParts[0]);
                        int month = dBReadInt(strParts[1]);
                        int day = dBReadInt(strParts[2]);
                        if (year > 0)
                            if ((month > 0) && (month < 13))
                                if ((day > 0) && (day < 32))
                                {
                                    return new DateTime(year, month, day);
                                }
                    }
                }
            }
            catch (Exception) { } /*Ignore any error*/

            return defaultValue;
        }
        /// <summary>
        /// Returns the first "size" characters from strign data.
        /// If data size is less than "size" then return data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string getStringLeft(string data, int size)
        {
            if (data != null && data.Length > size)
                return data.Substring(0, size);
            return data;
        }
        /// <summary>
        /// Convert an object to String (ignoring errors)
        /// </summary>
        /// <param name="strInt"></param>
        /// <returns></returns>
        public static string dBReadString(object objValue)
        {
            string stralue = "";
            try
            {
                stralue = objValue.ToString();
            }
            catch (Exception) { } /*Ignore any error*/

            return stralue;
        }
        /// <summary>
        /// Convert an object to UInt64 (ignoring errors)
        /// </summary>
        public static UInt64 dBReadUInt64(object objValue)
        {
            UInt64 intValue = 0;
            try
            {
                intValue = Convert.ToUInt64(objValue);
            }
            catch (Exception) { }

            return intValue;
        }
        /// <summary>
        /// Convert an object to UInt32 (ignoring errors)
        /// </summary>
        public static UInt32 dBReadUInt32(object objValue)
        {
            UInt32 intValue = 0;
            try
            {
                intValue = Convert.ToUInt32(objValue);
            }
            catch (Exception) { }

            return intValue;
        }
        /// <summary>
        /// Convert an object to UInt16 (ignoring errors)
        /// </summary>
        public static UInt16 dBReadUInt16(object objValue)
        {
            UInt16 intValue = 0;
            try
            {
                intValue = Convert.ToUInt16(objValue);
            }
            catch (Exception) { }

            return intValue;
        }
        /// <summary>
        /// Convert an object to Int (ignoring errors)
        /// </summary>
        /// <param name="strInt"></param>
        /// <returns></returns>
        public static int dBReadInt(object objValue)
        {
            int intValue = 0;
            try
            {
                intValue = Convert.ToInt32(objValue);
            }
            catch (Exception) { }

            return intValue;
        }
        /// <summary>
        /// Convert an object to Single (ignoring errors)
        /// </summary>
        public static Single dBReadSingle(object objValue)
        {
            Single sngValue = 0;
            try
            {
                sngValue = Convert.ToSingle(objValue);
            }
            catch (Exception) { }

            return sngValue;
        }
        /// <summary>
        /// Convert an object to Sbyte (ignoring errors)
        /// </summary>
        public static sbyte dBReadSByte(object objValue)
        {
            sbyte bytValue = 0;
            try
            {
                bytValue = Convert.ToSByte(objValue);
            }
            catch (Exception) { }

            return bytValue;
        }
        /// <summary>
        /// Convert an object to int64 (ignoring errors)
        /// </summary>
        public static Int64 dBReadInt64(object objValue)
        {
            Int64 intValue = 0;
            try
            {
                intValue = Convert.ToInt64(objValue);
            }
            catch (Exception) { }

            return intValue;
        }
        /// <summary>
        /// Convert an object to int16 (ignoring errors)
        /// </summary>
        public static Int16 dBReadInt16(object objValue)
        {
            Int16 intValue = 0;
            try
            {
                intValue = Convert.ToInt16(objValue);
            }
            catch (Exception) { }

            return intValue;
        }
        /// <summary>
        /// Convert an object to decimal (ignoring errors)
        /// </summary>
        public static decimal dBReadDecimal(object objValue)
        {
            decimal value = 0;
            try
            {
                value = Convert.ToDecimal(objValue);
            }
            catch (Exception) { }

            return value;
        }
        /// <summary>
        /// Convert an object to byte (ignoring errors)
        /// </summary>
        public static byte dBReadByte(object objValue)
        {
            byte bytValue = 0;
            try
            {
                bytValue = Convert.ToByte(objValue);
            }
            catch (Exception) { }

            return bytValue;
        }
        public static bool dBReadBoolean(object objValue)
        {
            bool booValue = false;
            try
            {
                booValue = Convert.ToBoolean(objValue);
            }
            catch (Exception) { }

            return booValue;
        }
        /// <summary>
        /// Convert an object to double (ignoring errors)
        /// </summary>
        /// <param name="strInt"></param>
        /// <returns></returns>
        public static double dBReadDouble(object objValue)
        {
            double douValue = 0;
            try
            {
                douValue = Convert.ToDouble(objValue);
            }
            catch (Exception) { }

            return douValue;
        }
        /// <summary>
        /// Convert an object to Date (ignoring errors)
        /// </summary>
        /// <param name="strInt"></param>
        /// <returns></returns>
        public static DateTime dBReadDate(object objValue)
        {
            DateTime datValue = new DateTime(0);
            try
            {
                datValue = Convert.ToDateTime(objValue);
            }
            catch (Exception) { } /*Ignore any error*/

            return datValue;
        }

        public static string dbReadDateAsStringFormat(object objValue, string format)
        {
            string dateValue = "";
            DateTime date = clsLibrary.dBReadDate(objValue);
            if (!date.Equals(new DateTime(0)))
            {
                dateValue = date.ToString(format);
            }
            return dateValue;

        }

        public static string iif(bool condition, string trueValue, string falseValue)
        {
            if (condition) return trueValue;
            return falseValue;
        }
        public static DateTime stringToDate(string strDate, string format)
        {
            return DateTime.ParseExact(strDate, format, null);
        }

        public static string xmlGetAttributeOrNodeValue(XmlNode objNode, string nodeOrAttributeName, string defaultValue)
        {
            //try to read from attribute
            //if doesn't exist attribute, then try to reda from node
            if (objNode != null)
            {
                XmlAttribute objAttribute = (XmlAttribute)objNode.Attributes.GetNamedItem(nodeOrAttributeName);
                if (objAttribute != null)
                {
                    return clsLibrary.dBReadString(objAttribute.Value);
                }
                else
                {
                    XmlNode objChildNode = objNode.SelectSingleNode(nodeOrAttributeName);
                    if (objChildNode != null)
                        return clsLibrary.dBReadString(objChildNode.InnerText);
                }
            }
            //if the attribute doesn't exist return the default value
            return defaultValue;
        }
        public static int xmlGetAttributeOrNodeValue(XmlNode objNode, string nodeOrAttributeName, int defaultValue)
        {
            //try to read from attribute
            //if doesn't exist attribute, then try to reda from node
            if (objNode != null)
            {
                XmlAttribute objAttribute = (XmlAttribute)objNode.Attributes.GetNamedItem(nodeOrAttributeName);
                if (objAttribute != null)
                {
                    return clsLibrary.dBReadInt(objAttribute.Value);
                }
                else
                {
                    XmlNode objChildNode = objNode.SelectSingleNode(nodeOrAttributeName);
                    if (objChildNode != null)
                        return clsLibrary.dBReadInt(objChildNode.InnerText);
                }
            }
            //if the attribute doesn't exist return the default value
            return defaultValue;
        }
        public static bool xmlGetAttributeOrNodeValue(XmlNode objNode, string nodeOrAttributeName, bool defaultValue)
        {
            //try to read from attribute
            //if doesn't exist attribute, then try to reda from node
            if (objNode != null)
            {
                XmlAttribute objAttribute = (XmlAttribute)objNode.Attributes.GetNamedItem(nodeOrAttributeName);
                if (objAttribute != null)
                {
                    string strVal = clsLibrary.dBReadString(objAttribute.Value).Trim();
                    if (strVal == "1") return true;
                    if (strVal.IndexOf("true", StringComparison.OrdinalIgnoreCase) == 0) return true;
                    //if (clsLibrary.dBReadInt(strVal) == 1) return true;
                    //if (clsLibrary.dBReadString(strVal).ToLower().Trim() == "true") return true;
                    return false;
                }
                else
                {
                    XmlNode objChildNode = objNode.SelectSingleNode(nodeOrAttributeName);
                    if (objChildNode != null)
                    {
                        string strVal = clsLibrary.dBReadString(objChildNode.InnerText).Trim();
                        if (strVal == "1") return true;
                        if (strVal.IndexOf("true", StringComparison.OrdinalIgnoreCase) == 0) return true;
                        //if (clsLibrary.dBReadInt(strVal) == 1) return true;
                        //if (clsLibrary.dBReadString(strVal).ToLower().Trim() == "true") return true;
                        return false;
                    }
                }
            }
            //if the attribute doesn't exist return the default value
            return defaultValue;
        }
        public static decimal xmlGetAttributeOrNodeValue(XmlNode objNode, string nodeOrAttributeName, decimal defaultValue)
        {
            //try to read from attribute
            //if doesn't exist attribute, then try to reda from node
            if (objNode != null)
            {
                XmlAttribute objAttribute = (XmlAttribute)objNode.Attributes.GetNamedItem(nodeOrAttributeName);
                if (objAttribute != null)
                {
                    return clsLibrary.dBReadDecimal(objAttribute.Value);
                }
                else
                {
                    XmlNode objChildNode = objNode.SelectSingleNode(nodeOrAttributeName);
                    if (objChildNode != null)
                    {
                        return clsLibrary.dBReadDecimal(objChildNode.InnerText);
                    }
                }
            }
            //if the attribute doesn't exist return the default value
            return defaultValue;
        }
        public static DateTime xmlGetAttributeOrNodeValue(XmlNode objNode, string nodeOrAttributeName, DateTime defaultValue)
        {
            //try to read from attribute
            //if doesn't exist attribute, then try to reda from node
            if (objNode != null)
            {
                XmlAttribute objAttribute = (XmlAttribute)objNode.Attributes.GetNamedItem(nodeOrAttributeName);
                if (objAttribute != null)
                {
                    return clsLibrary.dBReadDate(objAttribute.Value);
                }
                else
                {
                    XmlNode objChildNode = objNode.SelectSingleNode(nodeOrAttributeName);
                    if (objChildNode != null)
                    {
                        return clsLibrary.dBReadDate(objChildNode.InnerText);
                    }
                }
            }
            //if the attribute doesn't exist return the default value
            return defaultValue;
        }
        /// <summary>
        /// Return the string value of an Attribute into a XML node
        /// </summary>
        /// <param name="oNode"></param>
        /// <param name="AttributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string xmlAttributeGetValue(XmlNode objNode, string attributeName, string defaultValue)
        {
            if (objNode != null)
            {
                XmlAttribute objAttribute = (XmlAttribute)objNode.Attributes.GetNamedItem(attributeName);
                if (objAttribute != null)
                    return clsLibrary.dBReadString(objAttribute.Value);
            }
            //if the attribute doesn't exist return the default value
            return defaultValue;
        }
        /// <summary>
        /// Return the Integer value if an attribute into a XML Node
        /// </summary>
        /// <param name="oNode"></param>
        /// <param name="AttributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int xmlAttributeGetValue(XmlNode objNode, string attributeName, int defaultValue)
        {
            XmlAttribute objAttribute = (XmlAttribute)objNode.Attributes.GetNamedItem(attributeName);
            if (objAttribute != null)
                return clsLibrary.dBReadInt(objAttribute.Value);
            //if the attribute doesn't exist return the default value
            return defaultValue;
        }
        /// <summary>
        /// Return teh decimal value of an attribute into an XML node
        /// </summary>
        /// <param name="oNode"></param>
        /// <param name="AttributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static decimal xmlAttributeGetValue(XmlNode objNode, string attributeName, decimal defaultValue)
        {
            XmlAttribute objAttribute = (XmlAttribute)objNode.Attributes.GetNamedItem(attributeName);
            if (objAttribute != null)
                return clsLibrary.dBReadDecimal(objAttribute.Value);
            //if the attribute doesn't exist return the default value
            return defaultValue;
        }
        /// <summary>
        /// Return a DateTime value of an attribute into an XML node
        /// </summary>
        /// <param name="oNode"></param>
        /// <param name="AttributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static DateTime xmlAttributeGetValue(XmlNode objNode, string attributeName, DateTime defaultValue)
        {
            XmlAttribute objAttribute = (XmlAttribute)objNode.Attributes.GetNamedItem(attributeName);
            if (objAttribute != null)
                return clsLibrary.dBReadDate(objAttribute.Value);
            //if the attribute doesn't exist return the default value
            return defaultValue;
        }
        /// <summary>
        /// Return a boolean valult of an attribute into a XML Node
        /// </summary>
        /// <param name="oNode"></param>
        /// <param name="AttributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool xmlAttributeGetValue(XmlNode objNode, string attributeName, bool defaultValue)
        {
            XmlAttribute objAttribute = (XmlAttribute)objNode.Attributes.GetNamedItem(attributeName);
            if (objAttribute != null)
            {
                string strVal = clsLibrary.dBReadString(objAttribute.Value).Trim();
                if (strVal == "1") return true;
                if (strVal.IndexOf("true", StringComparison.OrdinalIgnoreCase) == 0) return true;
                //if (clsLibrary.dBReadInt(strVal) == 1) return true;
                //if (clsLibrary.dBReadString(strVal).ToLower().Trim() == "true") return true;
                return false;
            }
            //if the attribute doesn't exist return the default value
            return defaultValue;
        }
        public static string xmlNodeGetValue(XmlNode objParentNode, string nodeName, string defaultValue)
        {
            if (objParentNode != null)
            {
                XmlNode objNode = objParentNode.SelectSingleNode(nodeName);
                if (objNode != null)
                    return clsLibrary.dBReadString(objNode.InnerText);
                //if the node doesn't exist return the default value
            }
            return defaultValue;
        }
        public static bool xmlNodeGetValue(XmlNode objParentNode, string nodeName, bool defaultValue)
        {
            if (objParentNode != null)
            {
                XmlNode objNode = objParentNode.SelectSingleNode(nodeName);
                if (objNode != null)
                {
                    string strVal = clsLibrary.dBReadString(objNode.InnerText).Trim();
                    if (strVal == "1") return true;
                    if (strVal.IndexOf("true", StringComparison.OrdinalIgnoreCase) == 0) return true;
                    //if (clsLibrary.dBReadInt(strVal) == 1) return true;
                    //if (clsLibrary.dBReadString(strVal).ToLower().Trim() == "true") return true;
                    return false;
                }
            }
            return defaultValue;
        }
        public static XmlAttribute xmlAddAttribute(XmlNode objNode, string attributeName, string attributeValue)
        {
            XmlAttribute oAttribute = objNode.OwnerDocument.CreateAttribute(attributeName);
            oAttribute.Value = attributeValue;
            objNode.Attributes.Append(oAttribute);
            return oAttribute;
        }
        public static XmlNode xmlAddChildnode(XmlNode objParentNode, string nodeName, string strValue)
        {
            XmlNode oNewNode = xmlCreateChildnode(objParentNode, nodeName);
            oNewNode.InnerText = strValue;
            objParentNode.AppendChild(oNewNode);
            return oNewNode;
        }
        public static XmlNode xmlAddChildCData(XmlNode objParentNode, string nodeName, string strValue)
        {
            XmlNode oNewNode = xmlCreateChildnode(objParentNode, nodeName);
            xmlCreateChildCData(oNewNode, strValue);
            objParentNode.AppendChild(oNewNode);
            return oNewNode;
        }
        public static XmlCDataSection xmlCreateChildCData(XmlNode objParentNode, string nodeValue)
        {
            XmlCDataSection objNewNode = objParentNode.OwnerDocument.CreateCDataSection(nodeValue);
            objParentNode.AppendChild(objNewNode);
            return objNewNode;
        }
        public static XmlNode xmlCreateChildnode(XmlNode objNode, string nodeName)
        {
            XmlNode objNewNode = objNode.OwnerDocument.CreateNode(XmlNodeType.Element, nodeName, "");
            return objNewNode;
        }
        /// <summary>
        /// Create a Directory ignoring errors
        /// </summary>
        /// <param name="strPath"></param>
        public static void createDirectory(string strPath)
        {
            try
            {
                System.IO.Directory.CreateDirectory(strPath);
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// Encrypts a string using a lateral bit-rotation scheme.
        /// Not secure, but keeps casual snoopers out.
        /// </summary>
        /// <param name="strPlain">String to be encrypted.</param>
        /// <returns>Encrypted version of the input string.</returns>
        public static string UpcEncrypt(string strPlain)
        {
            if (strPlain.Length == 0)
                return string.Empty;

            int pbase = 0, cbase = 0, p, c;

            int nPlainLen = ((strPlain.Length + 4) / 5) * 5;
            byte[] abyPlain = new byte[nPlainLen];
            for (p = 0; p < strPlain.Length; p++)
                abyPlain[p] = Convert.ToByte(strPlain[p] & 0xFF);

            int nCryptLen = (nPlainLen * 8) / 5;
            byte[] abyCrypt = new byte[nCryptLen];
            byte[] abyMask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };

            while (pbase < strPlain.Length)
            {
                for (p = 0; p < 5; p++)
                {
                    for (c = 0; c < 8; c++)
                    {
                        if ((abyPlain[pbase + p] & abyMask[c]) != 0)
                            abyCrypt[cbase + c] |= abyMask[p];
                    }
                }

                for (c = 0; c < 8; c++)
                {
                    byte by = Convert.ToByte(abyCrypt[cbase + c] + '0');

                    if (by > Convert.ToByte('9'))
                        by += Convert.ToByte('A' - '9' - 1);

                    if (by >= Convert.ToByte('I'))
                        by++;
                    if (by >= Convert.ToByte('O'))
                        by++;

                    abyCrypt[cbase + c] = by;
                }

                pbase += 5;
                cbase += 8;
            }

            string strCrypt = string.Empty;
            for (c = 0; c < nCryptLen; c++)
                strCrypt += Convert.ToChar(abyCrypt[c]);

            return strCrypt;
        }

        /// <summary>
        /// Decrypts a string using a lateral bit-rotation scheme.
        /// Not secure, but keeps casual snoopers out.
        /// </summary>
        /// <param name="strCrypt">String to be decrypted.</param>
        /// <returns>Plaintext version of the input string.</returns>
        public static string UpcDecrypt(string strCrypt)
        {
            int nCryptLen = strCrypt.Length;
            if (nCryptLen == 0 || (nCryptLen % 8) != 0)
                return string.Empty;

            int cbase = 0, pbase = 0, c, p;

            byte[] abyCrypt = new byte[nCryptLen];
            for (c = 0; c < strCrypt.Length; c++)
                abyCrypt[c] = Convert.ToByte(strCrypt[c] & 0xFF);

            int nPlainLen = (nCryptLen * 5) / 8;
            byte[] abyPlain = new byte[nPlainLen];
            byte[] abyMask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };

            while (cbase < strCrypt.Length)
            {
                for (c = 0; c < 8; c++)
                {
                    byte by = Convert.ToByte(abyCrypt[cbase + c] - '0');

                    if (by > Convert.ToByte(9))
                        by -= Convert.ToByte('A' - '9' - 1);

                    if (by >= Convert.ToByte('O' - 'A' + 10))
                        by--;
                    if (by >= Convert.ToByte('I' - 'A' + 10))
                        by--;

                    abyCrypt[cbase + c] = by;

                    for (p = 0; p < 5; p++)
                    {
                        if ((abyCrypt[cbase + c] & abyMask[p]) != 0)
                            abyPlain[pbase + p] |= abyMask[c];
                    }
                }

                pbase += 5;
                cbase += 8;
            }

            string strPlain = string.Empty;
            for (p = 0; p < nPlainLen; p++)
            {
                if (abyPlain[p] != 0)
                    strPlain += Convert.ToChar(abyPlain[p]);
                else
                    break;
            }

            return strPlain;
        }

        /// <summary>
        /// Convert an string to byte[] (ignoring errors)
        /// </summary>
        /// 
        public static byte[] dBReadByteArray(string value)
        {
            byte[] bytes = new byte[0];
            try
            {
                bytes = Convert.FromBase64String(value.ToString());
            }
            catch (Exception) { } /*Ignore any error*/

            return bytes;
        }

        public static DateTime GetWorkingDay(DateTime date, int step) {

            do
            {
                date = date.AddDays(step);
            } while ((date.DayOfWeek == DayOfWeek.Saturday)
                    || (date.DayOfWeek == DayOfWeek.Sunday));

            return date;
        }



        public class clsWebLib
        {
            public static string RequestContent(string strUrl, string strReferer, string strHost, string postData, CookieContainer cookieContainer, string Method, string UserName, string UserPassword, string UserDomain)
            {

                //clsLog.Write("RequestContent:" + strUrl);
                try
                {
                    // Create a request using a URL that can receive a post. 
                    WebRequest request = WebRequest.Create(strUrl);
                    HttpWebRequest oHttpWebRequest = (HttpWebRequest)request;

                    //if there is username , authenticate=>
                    if (UserDomain != "")
                    {
                        oHttpWebRequest.UseDefaultCredentials = false;
                        oHttpWebRequest.PreAuthenticate = true;
                        oHttpWebRequest.Credentials = new NetworkCredential(UserName, UserPassword, UserDomain);

                    }

                    // Set the Method property of the request to POST.
                    request.Method = Method;
                    oHttpWebRequest.Accept = "text/html, application/xhtml+xml, */*";
                    oHttpWebRequest.Referer = strReferer;
                    oHttpWebRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "es-AR");
                    oHttpWebRequest.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
                    //oHttpWebRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                    //if (strHost != "") oHttpWebRequest.Host = strHost;

                    oHttpWebRequest.Headers.Add(HttpRequestHeader.Pragma, "no-cache");
                    oHttpWebRequest.CookieContainer = cookieContainer;
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                    // Set the ContentType property of the WebRequest.
                    request.ContentType = "application/x-www-form-urlencoded";
                    Stream dataStream = null;
                    if (Method.ToUpper() == "POST")
                    {
                        // Set the ContentLength property of the WebRequest.
                        request.ContentLength = byteArray.Length;
                        // Get the request stream.
                        dataStream = request.GetRequestStream();
                        // Write the data to the request stream.
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        // Close the Stream object.
                        dataStream.Close();
                    }

                    // Get the response.
                    WebResponse response = request.GetResponse();
                    // Display the status.
                    string strTemp = ((HttpWebResponse)response).StatusDescription;
                    // Get the stream containing content returned by the server.
                    dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    string responseFromServer = reader.ReadToEnd();

                    //clsLog.Write("RequestContent - responseFromServer:" + responseFromServer);
                    // Display the content.
                    //Console.WriteLine(responseFromServer);
                    // Clean up the streams.
                    reader.Close();
                    dataStream.Close();
                    response.Close();
                    return responseFromServer;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            public static byte[] RequestBinaryContent(string strUrl, string strReferer, string strHost, string postData, CookieContainer cookieContainer, string Method, string UserName, string UserPassword, string UserDomain)
            {
                // Create a request using a URL that can receive a post. 
                WebRequest request = WebRequest.Create(strUrl);
                HttpWebRequest oHttpWebRequest = (HttpWebRequest)request;

                //if there is username , authenticate=>
                if (UserDomain != "")
                {
                    oHttpWebRequest.UseDefaultCredentials = false;
                    oHttpWebRequest.PreAuthenticate = true;
                    oHttpWebRequest.Credentials = new NetworkCredential(UserName, UserPassword, UserDomain);

                }

                // Set the Method property of the request to POST.
                request.Method = Method;
                oHttpWebRequest.Accept = "text/html, application/xhtml+xml, */*";
                oHttpWebRequest.Referer = strReferer;
                oHttpWebRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "es-AR");
                oHttpWebRequest.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
                //oHttpWebRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                //if (strHost != "") oHttpWebRequest.Host = strHost;

                oHttpWebRequest.Headers.Add(HttpRequestHeader.Pragma, "no-cache");
                oHttpWebRequest.CookieContainer = cookieContainer;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/x-www-form-urlencoded";
                Stream dataStream = null;
                if (Method.ToUpper() == "POST")
                {
                    // Set the ContentLength property of the WebRequest.
                    request.ContentLength = byteArray.Length;
                    // Get the request stream.
                    dataStream = request.GetRequestStream();
                    // Write the data to the request stream.
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    // Close the Stream object.
                    dataStream.Close();
                }


                // Get the response.
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                MemoryStream memoryStream = new MemoryStream();
                byte[] buffer = new byte[4097];
                int count = 0;
                do
                {
                    count = responseStream.Read(buffer, 0, buffer.Length);
                    memoryStream.Write(buffer, 0, count);

                    if (count == 0)
                    {
                        break;
                    }
                }
                while (true);
                return memoryStream.ToArray();
            }

        }

    }
}