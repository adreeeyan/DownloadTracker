using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DownloadTracker.Controllers
{
    [Route("/api/[controller]")]
    public class DownloadController : Controller
    {
        // This should be in a db
        static List<DownloadUser> OngoingDownloads = new List<DownloadUser>();

        [HttpGet("{id}")]
        public void Download(string id)
        {
            var fileName = id;
            var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var FileName = new System.IO.FileInfo(fileName);
            var _BinaryReader = new BinaryReader(file);

            // Add download to the ongoing downloads list
            // Get logged in user (in this case we just generate something)
            var user = new DownloadUser()
            {
                DownloadFile = id,
                UserId = "TestUser"
            };
            if (!OngoingDownloads.Contains(user))
            {
                OngoingDownloads.Add(user);
            }

            // Set the response headers manually
            var startBytes = 0;
            Response.Clear();
            Response.Headers.Add("Accept-Ranges", "bytes");
            Response.ContentType = "application/octet-stream";
            Response.Headers.Add("Content-Disposition", "attachment;filename=" + FileName.Name);
            Response.Headers.Add("Content-Length", (FileName.Length - startBytes).ToString());
            Response.Headers.Add("Connection", "Keep-Alive");

            // Send data
            _BinaryReader.BaseStream.Seek(startBytes, SeekOrigin.Begin);

            // Dividing the data in 1024 bytes package
            var maxCount = (int)Math.Ceiling((FileName.Length - startBytes + 0.0) / 1024);

            //Download in block of 1024 bytes
            int readCount;
            for (readCount = 0; readCount < maxCount && !Response.HttpContext.RequestAborted.IsCancellationRequested; readCount++)
            {
                byte[] part = _BinaryReader.ReadBytes(1024);
                Response.Body.WriteAsync(part, 0, part.Length);
                Response.Body.Flush();
            }

            // If blocks transfered is equals total number of blocks, then it is finished
            if (readCount == maxCount)
            {
                // Delete the file here

                // Check if all the users that are currently downloading this file have finished the download
                var haveActiveDownloaders = OngoingDownloads.Find(u => u.DownloadFile == id);
                if (haveActiveDownloaders != null)
                {
                    // There are still people downloading this file, what to do?
                    Console.WriteLine("There are others that are still downloading.");
                }
                else
                {
                    // No one is downloading this, delete
                    Console.WriteLine("Delete the file.");
                }
            }
            else
            {
                Console.WriteLine("The transfer was interrupted, keeping the file.");
            }

            // Delete the record in the OngoingDownloads list
            OngoingDownloads.Remove(user);
        }

        [HttpGet("{id}/status")]
        public IActionResult GetDownloadStatus(string id)
        {
            // Get logged in user (in this case we just generate something)
            var user = new DownloadUser()
            {
                DownloadFile = id,
                UserId = "TestUser"
            };
            return Ok(new
            {
                Exists = OngoingDownloads.Contains(user)
            });
        }
    }

    public class DownloadUser : IEquatable<DownloadUser>
    {
        public string DownloadFile { get; set; }
        public string UserId { get; set; }

        public bool Equals(DownloadUser other)
        {
            return this.UserId == other.UserId && this.DownloadFile == other.DownloadFile;
        }
    }
}
