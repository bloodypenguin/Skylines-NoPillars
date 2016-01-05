using System;
using System.IO;
using System.Xml.Serialization;
using Debug = UnityEngine.Debug;

namespace NoPillars.Options
{
    public class Options
    {
        public Options()
        {
            alwaysVisible = false;
            resetOnHide = true;
        }

        [Checkbox("Panel always visible")]
        public bool alwaysVisible { set; get; }
        [Checkbox("Reset drop box positions on panel hiding")]
        public bool resetOnHide { set; get; }
    }

    public static class OptionsHolder
    {
        public static Options Options = new Options();
    }

    public static class OptionsLoader
    {
        private const string FileName = "CSL-NoPillars.xml";

        public static void LoadOptions()
        {
            try
            {
                try
                {
                    var xmlSerializer = new XmlSerializer(typeof(Options));
                    using (var streamReader = new StreamReader(FileName))
                    {
                        OptionsHolder.Options = (Options)xmlSerializer.Deserialize(streamReader);
                    }
                }
                catch (FileNotFoundException)
                {
                    // No options file yet
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Unexpected {0} while loading options: {1}\n{2}",
                    e.GetType().Name, e.Message, e.StackTrace);
            }
        }

        public static void SaveOptions()
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(Options));
                using (var streamWriter = new StreamWriter(FileName))
                {
                    xmlSerializer.Serialize(streamWriter, OptionsHolder.Options);
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Unexpected {0} while saving options: {1}\n{2}",
                    e.GetType().Name, e.Message, e.StackTrace);
            }
        }
    }
}