using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer
{
    public class ConfigurationFileReader
    {
        string _configFile;
        StreamReader reader = null;

        public Dictionary<string, Dictionary<string, string>> ConfigItems = new Dictionary<string, Dictionary<string, string>>();

        public ConfigurationFileReader(string configFilePath)
        {
            _configFile = configFilePath;
            try
            {
                reader = new StreamReader(configFilePath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void ReadConfigFile()
        {
            string line = null;
            string currentHeader = null;
            try
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("[") && line.Contains("]"))
                    {
                        int fIdx = line.IndexOf("[");
                        int rIdx = line.IndexOf("]");
                        string header = line.Substring((fIdx + 1), (rIdx - (fIdx + 1)));
                        if (!ConfigItems.ContainsKey(header))
                        {
                            ConfigItems.Add(header, new Dictionary<string, string>());
                            currentHeader = header;
                        }
                        else
                        {
                            throw new Exception("Already a header here: " + header);
                        }
                    }
                    else if (line.Contains("="))
                    {
                        string[] keyValue = line.Split('=');
                        if (keyValue.Count() == 2)
                        {
                            Dictionary<string, string> items = ConfigItems[currentHeader];
                            if (items != null)
                            {
                                items.Add(keyValue[0], keyValue[1]);
                            }
                        }
                        else
                        {
                            throw new Exception("Malformed Key-Value Pair under Header: " + currentHeader);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
