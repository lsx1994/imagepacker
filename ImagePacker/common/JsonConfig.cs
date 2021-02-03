using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace ImagePacker
{
    internal static class JsonConfig
    {
        public static void writeToFile(string fileName, object target, bool IsHide = false)
        {
            var jss = new JavaScriptSerializer();
            var content = jss.Serialize(target);
            var filePath = Path.Combine(Util.WorkPath, fileName);
            File.WriteAllBytes(filePath, Encoding.UTF8.GetBytes(content));
            /*FileInfo fileInfo = new FileInfo(filePath);
            var fileData = fileInfo.Open(FileMode.OpenOrCreate);
            fileData.Write(Encoding.UTF8.GetBytes(content), 0, content.Length);
            fileData.Close();
            if (IsHide)
            {
                if (!fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    fileInfo.Attributes = FileAttributes.Hidden;
                }
            }*/
        }

        public static T readFromFile<T>(string fileName)
        {
            try
            {
                var filePath = Path.Combine(Util.WorkPath, fileName);
                var fileContent = File.ReadAllBytes(filePath);
                var content = Encoding.UTF8.GetString(fileContent);
                var jss = new JavaScriptSerializer();
                return jss.Deserialize<T>(content);
            }
            catch (System.Exception)
            {
            }

            return default(T);
        }
    }
}
