using Microsoft.AspNetCore.Http;
using System.IO;

namespace LendingPlatform.Utils.Utils
{
    public class FileOperationsUtility : IFileOperationsUtility
    {
        public FileOperationsUtility()
        {

        }

        /// <summary>
        /// Method to read contents of file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string ReadFileContent(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                return json;
            }
        }

        /// <summary>
        /// Method to read stream content.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public Stream OpenReadStreamContent(IFormFile file)
        {
            Stream result = file.OpenReadStream();
            return result;
        }

        /// <summary>
        /// Method to calculate the file bytes.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public byte[] ReadStreamBytes(Stream stream, IFormFile file)
        {
            using (BinaryReader b = new BinaryReader(stream))
            {
                byte[] fileByte = b.ReadBytes((int)file.Length);
                return fileByte;
            }
        }

        /// <summary>
        /// Method to read memory stream.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="writable"></param>
        /// <returns></returns>
        public MemoryStream ReadMemoryStream(byte[] bytes, bool writable)
        {
            MemoryStream ms = new MemoryStream(bytes, writable);

            return ms;
        }
    }
}
