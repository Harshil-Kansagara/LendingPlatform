using Microsoft.AspNetCore.Http;
using System.IO;

namespace LendingPlatform.Utils.Utils
{
    public interface IFileOperationsUtility
    {
        /// <summary>
        /// Method to read contents of file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string ReadFileContent(string path);

        /// <summary>
        /// Method to read stream content.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        Stream OpenReadStreamContent(IFormFile file);

        /// <summary>
        /// Method to calculate the file bytes.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        byte[] ReadStreamBytes(Stream stream, IFormFile file);

        /// <summary>
        /// Method to read memory stream.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="writable"></param>
        /// <returns></returns>
        MemoryStream ReadMemoryStream(byte[] bytes, bool writable);
    }
}
