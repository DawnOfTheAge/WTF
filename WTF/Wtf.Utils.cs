using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WTF
{
    public static class Utils
    {
        #region Read / Write To Text Files

        public static bool ReadFileLines(string filename, out List<string> lines, out string result)
        {
            string method = ":{" + MethodBase.GetCurrentMethod().Name + "}: ";
            string line;

            lines = null;
            result = "";

            try
            {
                if (!File.Exists(filename))
                {
                    return false;
                }

                lines = new List<string>();

                StreamReader file = new StreamReader(filename);
                while ((line = file.ReadLine()) != null)
                {
                    lines.Add(line);
                }

                file.Close();

                return true;
            }
            catch (Exception e)
            {
                result = e.Message;

                return false;
            }
        }

        public static bool WriteLinesToFile(string filename, List<string> lines, out string result)
        {
            string method = ":{" + MethodBase.GetCurrentMethod().Name + "}: ";

            result = "";

            try
            {
                StreamWriter file = new StreamWriter(filename, true);
                foreach (string line in lines)
                {
                    file.WriteLine(line);
                }

                file.Close();

                return true;
            }
            catch (Exception e)
            {
                result = e.Message;

                return false;
            }
        }

        #endregion

        #region Read / Write To XML Files

        public static bool WriteToXmlFile<T>(string sFilePath, T objectToWrite, out string result, bool bAppend = false) where T : new()
        {
            TextWriter twTextWriter = null;

            result = "";

            try
            {
                var vSerializer = new XmlSerializer(typeof(T));
                twTextWriter = new StreamWriter(sFilePath, bAppend);
                vSerializer.Serialize(twTextWriter, objectToWrite);

                return true;
            }
            catch (Exception e)
            {
                result = e.Message;

                Console.WriteLine(result);

                return false;
            }
            finally
            {
                if (twTextWriter != null)
                {
                    twTextWriter.Close();
                }
            }
        }

        public static T ReadFromXmlFile<T>(string sFilePath) where T : new()
        {
            TextReader trTextReader = null;

            try
            {
                var vSerializer = new XmlSerializer(typeof(T));
                trTextReader = new StreamReader(sFilePath);

                return (T)vSerializer.Deserialize(trTextReader);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                return default(T);
            }
            finally
            {
                if (trTextReader != null)
                {
                    trTextReader.Close();
                }
            }
        }

        #endregion
    }
}
