using System;
using System.Collections.Generic;
using System.IO;

namespace HathZipper
{
    internal enum status
    {
        unprocessed,
        incomplete,
        complete
    }

    internal class Gallery
    {
        public string path { get; set; }

        public string name { get; set; }

        public string[] galleryInfo { get; set; }

        public bool processed { get; set; }

        /// <summary>
        /// Information parsed from galleryinfo.txt
        /// </summary>
        public string Title { get; set; }

        public DateTime UploadTime { get; set; }

        public string Uploader { get; set; }

        public DateTime Downloaded { get; set; }

        public string[] Tags { get; set; }

        public List<string> Comment { get; set; }

        public bool valid { get; set; }

        public Gallery(string path)
        {
            this.path = path;
            this.name = Path.GetFileName(path);
            this.processed = false;
            if (this.Comment == null)
                this.Comment = new List<string>();
            if (getGalleryStatus(path)) { ParseGalleryInfo(this.galleryInfo); }
        }

        public Gallery(DirectoryInfo path)
        {
            this.path = path.FullName;
            this.name = path.Name;
            this.processed = false;

            if (this.Comment == null)
                this.Comment = new List<string>();

            if (getGalleryStatus(this.path)) { ParseGalleryInfo(this.galleryInfo); }
        }

        private Boolean getGalleryStatus(string path)
        {
            FileAttributes fa = File.GetAttributes(path);
            if (Directory.Exists(path))
            {
                //Path is a directory, check if 'galleryinfo.txt' exists and update path
                if (File.Exists(path + "\\galleryinfo.txt"))
                    path = path + "\\galleryinfo.txt";
                else
                    return false; // 'galleryinfo.txt' doesn't exist
            }

            if (!File.Exists(path))
            {
                return false;
            }
            else
            {
                this.galleryInfo = File.ReadAllLines(path);
                ParseGalleryInfo(this.galleryInfo);
                return true;
            }
        }

        public void DeleteFiles()
        {
            DirectoryInfo di = new DirectoryInfo(this.path);
            di.Delete(true);
        }

        private void ParseGalleryInfo(string[] galleryinfo)
        {
            foreach (string line in galleryinfo)
            {
                if (line.StartsWith("Title:"))
                {
                    this.Title = line.Remove(0, "Title:".Length).TrimStart();
                }
                else if (line.StartsWith("Upload Time:"))
                {
                    this.UploadTime = DateTime.ParseExact(line.Remove(0, "Upload Time:".Length).Trim(), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (line.StartsWith("Uploaded By:"))
                {
                    this.Uploader = line.Remove(0, "Uploaded By:".Length).Trim();
                }
                else if (line.StartsWith("Downloaded:"))
                {
                    this.Downloaded = DateTime.ParseExact(line.Remove(0, "Downloaded:".Length).Trim(), "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (line.StartsWith("Tags:"))
                {
                    //Explode by ","
                    this.Tags = line.Remove(0, "Tags:".Length).Trim().Split(',');
                }
                else if (line.Trim().Length != 0 && !line.StartsWith("Uploader's Comments:"))
                {
                    //this.Comment = line;
                    this.Comment.Add(line);
                }
            }
        }

        /// <summary>
        /// TODO:
        /// 1. Login into exhentai
        /// 2. Search by Gallery name
        /// 3. Use gallery index to get all filenames in index (minimal 20 images per page)
        /// 4. Compare all filenames with local images
        /// </summary>
        private void CheckConsistencyOnlineByFilename()
        {
            // SUGGEST: Move this to a class that extends/inherits this class
            // Requires libraries. Including those here is a waste
        }
    }
}