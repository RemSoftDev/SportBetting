using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Collections.ObjectModel;
using SportRadar.Common.Logs;

//using NLogger;

namespace SportRadar.Common.Windows
{
	public class TextUtil
	{
	    private static ILog m_logger = LogFactory.CreateLog(typeof (TextUtil));

        public const string _SCHEMA = "_schema";
        private static bool _DOTICKETRECALCULATION = false;
        private static bool _USENEWCALCULATION = false;

        public static bool UseNewCalculation
        {
            get { return TextUtil._USENEWCALCULATION; }
            set { TextUtil._USENEWCALCULATION = value; }
        }

        public static bool DoTicketRecalculation
        {
            get { return TextUtil._DOTICKETRECALCULATION; }
            set { TextUtil._DOTICKETRECALCULATION = value; }
        }       

        public static string Shorten(string text, int len, string end)
        {
            if (text != null && text.Length > len)
                return text.Substring(0, len - end.Length).Trim() + end;
            return text;
        }

		public static string ShortenWithShortDots(string text, int len, string end) {
			if (text != null  &&  text.Length > len - 1 && len > 0)//GMU 2011-07-14 do not shorten if new length should by < 0
				return text.Substring(0, len-end.Length-1).Trim() + end;
			return text;
		}

        public static string LoadFile(string path, string fileName)
        {
            if (!path.EndsWith("\\")) path = path + "\\";
            byte[] bArray = null;
            if (Directory.Exists(path) && File.Exists(path + fileName))
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(path + fileName, FileMode.Open, FileAccess.Read);
                    bArray = new byte[fs.Length];
                    int count = fs.Read(bArray, 0, Convert.ToInt32(fs.Length));
                    if (count != bArray.Length)
                    {
                        fs.Close();
                        return String.Empty;
                    }
                }
                catch (Exception e)
                {
                    //TODO do something in else case and exception
                    if (fs != null) fs.Close();
                    m_logger.Excp(e, "file: \"" + path + fileName + "\" does not exist or is unreadable"); //logger useless within IIS
                }

            }
            else
            {
                m_logger.Warn("File or folder : \"" + path + fileName + "\" was not found");
                return String.Empty;

            }
            if (bArray != null)
            {
                return Convert.ToBase64String(bArray);
            }
            return String.Empty;
        }

        public static byte[] Compress(string s)
        {
            byte[] bArray = new System.Text.UTF8Encoding().GetBytes(s);
            MemoryStream ms = new MemoryStream();
            Stream stream = new GZipStream(ms, CompressionMode.Compress);
            stream.Write(bArray, 0, bArray.Length);
            stream.Close();
            byte[] compressedData = (byte[])ms.ToArray();

            return compressedData;
        }

        public static string Decompress(byte[] b)
        {
            //string s1 = System.Text.Encoding.ASCII.GetString(b, 0, b.Length);
            string s1 = Convert.ToBase64String(b);
            byte[] bArray1 = Convert.FromBase64String(s1);//new System.Text.ASCIIEncoding().GetBytes(s1);

            b = bArray1;

            MemoryStream ms = new MemoryStream(b);
            GZipStream zipStream = new GZipStream(ms, CompressionMode.Decompress);
            //Console.WriteLine("Decompression");
            byte[] decompressedBuffer = new byte[b.Length + 100];
            // Use the ReadAllBytesFromStream to read the stream.
            int totalCount = ReadAllBytesFromStream(zipStream, decompressedBuffer);
            zipStream.Close();
            string s = System.Text.Encoding.ASCII.GetString(decompressedBuffer, 0, decompressedBuffer.Length);
            return s;
        }

        public static string DecompressBase64String(string pBase64Str)
        {
            System.Text.Encoding encoder = Encoding.UTF8;
            //byte[] compressedData = encoder.GetBytes(xmlFile);

            byte[] compressedData = Convert.FromBase64String(pBase64Str);
            byte[] writeData = new byte[1024*1024];
            System.IO.Stream zipStream = new System.IO.Compression.GZipStream(new System.IO.MemoryStream(compressedData), System.IO.Compression.CompressionMode.Decompress);
            int totalLength = 0;
            int size;

            string sResult = string.Empty;

            while ((size = zipStream.Read(writeData, 0, writeData.Length)) > 0)
            {
                totalLength += size;
                sResult += encoder.GetString(writeData, 0, size);
            }
            zipStream.Close();
            // _Logger.Debug("compressed Lenght: " + compressedData.Length + " de compressed Lenght: " + totalLength);
            return sResult;
        }

        public static int ReadAllBytesFromStream(Stream stream, byte[] buffer)
        {
            // Use this method is used to read all bytes from a stream.
            int offset = 0;
            int totalCount = 0;
            while (true)
            {
                int bytesRead = stream.Read(buffer, offset, 1024*1024);
                if (bytesRead == 0)
                {
                    break;
                }
                offset += bytesRead;
                totalCount += bytesRead;
            }
            return totalCount;
        }

        public static ObservableCollection<String> LoadFileToStrings(String path, DateTime date)
        {
            if (date.Date == DateTime.Today)
            {
                return LoadFileToStrings(path, "NBT_StationStarter.log");
            }
            else
            {
                return LoadFileToStrings(path, "NBT_StationStarter.log." + date.Year + "-" + date.Month.ToString("0#") + "-" + date.Day.ToString("0#"));
            }
        }

        public static ObservableCollection<String> LoadFileToStrings(string path, string fileName)
        {
            if (Directory.Exists(path))
            {
                FileStream fs = null;
                ObservableCollection<String> compressedStrings = new ObservableCollection<String>();
                try
                {
                    if (!File.Exists(path + "\\" + fileName))
                    {
                        compressedStrings.Add(Convert.ToBase64String(Compress("File not found: " + path + "\\" + fileName)));
                        return compressedStrings;
                    }
                    
                    FileInfo fi = new FileInfo(path + "\\" + fileName);
                    if (fi.Length >= (100 << 20))  // 100 MB
                    {
                        compressedStrings.Add(Convert.ToBase64String(Compress("File too big (> 100MB): " + path + "\\" + fileName  + "  Size: " + (fi.Length >> 20) + " MB")));
                        return compressedStrings;
                    }
                    
                    String[] allLines = File.ReadAllLines(path + "\\" + fileName);
                    //double d = (allLines.Length/100);
                    StringBuilder build = new StringBuilder();
                    for (int i = 0; i < allLines.Length; i++)
                    {
                        build.Append(allLines[i] + "\r");
                        if (i % 50 == 49)
                        {
                            compressedStrings.Add(Convert.ToBase64String(Compress(build.ToString())));
                            build.Length = 0;
                        }
                    }
                    if (build.Length != 0)
                    {
                        compressedStrings.Add(Convert.ToBase64String(Compress(build.ToString())));
                    }
                    return compressedStrings;
                }
                catch (Exception e)
                {
                    if (fs != null) fs.Close();
                    m_logger.Excp(e, "file: " + path + fileName + " does not exist or is unreadable");  //logger useless within IIS
                }
            }
            return null;
        }


        public static string CompressDBFile(string DBFile)
        {

            string compressedString = "";
            try
            {
                if (DBFile == null || DBFile.Length == 0)
                {
                    return null;
                }
                compressedString = (Convert.ToBase64String(Compress(DBFile)));
                // compressedStrings.Add(strBuilder.ToString());


                return compressedString;
            }
            catch (Exception e)
            {
                m_logger.Excp(e, "Error converting string to compressed string");
            }
            
            return null;
        }

        public static ObservableCollection<String> LoadFileArrayToString(string[] StrArray)
        {

            ObservableCollection<String> compressedStrings = new ObservableCollection<String>();
            try
            {
                if (StrArray == null)
                {
                    return null;
                }               

                StringBuilder strBuilder = new StringBuilder();
                
                int linecount = 0;
                foreach(string line in StrArray)
                {
                    if (line.Length > 1)
                    {
                        strBuilder.Append(line + "/>\r");
                    }

                    if (linecount % 30 == 29)
                    {
                        compressedStrings.Add(Convert.ToBase64String(Compress(strBuilder.ToString())));
                        // compressedStrings.Add(strBuilder.ToString());
                        strBuilder.Length = 0;
                    }
                    linecount++;

                }
                //append rest of file
                if (strBuilder.Length != 0)
                {
                    string str = strBuilder.ToString();
                    if (str.EndsWith("/"))
                    {
                        str = str.Substring(0, str.Length - 1);
                    }
                    compressedStrings.Add(Convert.ToBase64String(Compress(str)));
                    // compressedStrings.Add(str);
                    strBuilder.Length = 0;
                }
                return compressedStrings;
            }
            catch (Exception e)
            {
                m_logger.Excp(e, "Error converting string[] to compressed ObservableCollection<String>");
            }
            
            return null;
        }

        /// <summary>
        /// To convert a Byte Array of Unicode values (UTF-8 encoded) to a complete String.
        /// </summary>
        /// <param name="characters">Unicode Byte Array to be converted to String</param>
        /// <returns>String converted from Unicode Byte Array</returns>
        public static String UTF8ByteArrayToString(Byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            String constructedString = encoding.GetString(characters);
            return (constructedString);
        }



        /// <summary>
        /// Converts the String to UTF8 Byte array and is used in De serialization
        /// </summary>
        /// <param name="pXmlString"></param>
        /// <returns></returns>
        public static Byte[] StringToUTF8ByteArray(String pXmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }

        public static bool WriteFile(string FilePath, string FileData)
        {
            try
            {
                string path = FilePath.Substring(0, FilePath.LastIndexOf("\\"));
                if(!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
               
                using (StreamWriter sw = File.CreateText(FilePath))
                {
                    sw.Write(FileData);
                    sw.Close();
                }
            }
            catch (Exception Ex)
            {
                m_logger.Excp(Ex, "In WriteFile");
                return false;
            }
            return true;
        }

        public static string ReplaceChar(string RepStr)
        {
            RepStr = RepStr.Replace(":", "_");
            RepStr = RepStr.Replace(" ", "_");
            RepStr = RepStr.Replace(".", "_");
            return RepStr;
        }

    }
}
