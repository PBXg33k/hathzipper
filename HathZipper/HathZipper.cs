using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ionic.Zip;
using Ionic.Crc;

namespace HathZipper
{
    class HathZipper
    {
        //public delegate void GalleryFoundHandler(object sender, EventArgs e);
        //public delegate void GalleryUpdateHandler(object sender, ProgressEventArgs e);
        //public event GalleryUpdateHandler OnGalleryUpdateStatus;

        public DirectoryInfo GalleriesDirectory { get; set; }
        public DirectoryInfo OutputDirectory { get; set; }
        public BindingList<Gallery> Galleries { get; set; }

        public HathZipper(string GalleriesDirectory)
        {
            DirectoryInfo gd = new DirectoryInfo(GalleriesDirectory);
            this.GalleriesDirectory = gd;
            this.Galleries = new BindingList<Gallery>();
        }

        public HathZipper(string GalleriesDirectory, string OutputDirectory)
        {
            DirectoryInfo gd = new DirectoryInfo(GalleriesDirectory);
            this.GalleriesDirectory = gd;
            DirectoryInfo od = new DirectoryInfo(OutputDirectory);
            this.OutputDirectory = od;
            this.Galleries = new BindingList<Gallery>();
        }

        public HathZipper(string GalleriesDirectory, string OutputDirectory, BindingList<Gallery> Galleries)
        {
            DirectoryInfo gd = new DirectoryInfo(GalleriesDirectory);
            this.GalleriesDirectory = gd;
            DirectoryInfo od = new DirectoryInfo(OutputDirectory);
            this.OutputDirectory = od;
            this.Galleries = Galleries;
        }
        public HathZipper(DirectoryInfo GalleriesDirectory)
        {
            this.GalleriesDirectory = GalleriesDirectory;
            this.Galleries = new BindingList<Gallery>();
        }
        public HathZipper(DirectoryInfo GalleriesDirectory, DirectoryInfo OutputDirectory)
        {
            this.GalleriesDirectory = GalleriesDirectory;
            this.OutputDirectory = OutputDirectory;
            this.Galleries = new BindingList<Gallery>();
        }
        public HathZipper(DirectoryInfo GalleriesDirectory, DirectoryInfo OutputDirectory, BindingList<Gallery> Galleries)
        {
            this.GalleriesDirectory = GalleriesDirectory;
            this.OutputDirectory = OutputDirectory;
            this.Galleries = Galleries;
        }

        public void ScanGalleries()
        {
            ScanGalleries(true, 3);
        }
        public void ScanGalleries(bool OnlyCompleted)
        {
            ScanGalleries(OnlyCompleted, 3);
        }
        public void ScanGalleries(bool OnlyCompleted, int method)
        {
            switch(method)
            {
                case 1:
                    ScanGalleriesMethodOne(OnlyCompleted);
                    break;
                case 2:
                    ScanGalleriesMethodTwo(OnlyCompleted);
                    break;
                case 3:
                default:
                    ScanGalleriesMethodThree(OnlyCompleted);
                    break;
            }
        }
        private void ScanGalleriesMethodOne(bool OnlyCompleted)
        {
            string[] gallerypaths = null;
            if(OnlyCompleted == true)
            {
                FileInfo[] fi = this.GalleriesDirectory.GetFiles("galleryinfo.txt", SearchOption.AllDirectories);
                gallerypaths = fi.Select(f => f.FullName).ToArray();
            }
            else
            {
                DirectoryInfo[] di = this.GalleriesDirectory.GetDirectories();
                gallerypaths = di.Select(d => d.FullName).ToArray();
            }

            foreach (string path in gallerypaths)
            {
                Gallery gallery = new Gallery(path);
                this.Galleries.Add(gallery);
            }
        }
        private void ScanGalleriesMethodTwo(bool OnlyCompleted)
        {
            DirectoryInfo[] directories = null;

            try
            {
                directories = this.GalleriesDirectory.GetDirectories();
            }
            catch (UnauthorizedAccessException e)
            {

            }
            catch (DirectoryNotFoundException e)
            {

            }
            if(directories != null)
            {
                foreach(DirectoryInfo di in directories)
                {
                    Gallery g = new Gallery(di);
                    

                }
            }
        }
        private void ScanGalleriesMethodThree(bool OnlyCompleted)
        {
            if(OnlyCompleted == true)
            {
                foreach(FileInfo gallery in this.GalleriesDirectory.EnumerateFiles("galleryinfo.txt",SearchOption.AllDirectories))
                {
                    string GalleryPath = gallery.DirectoryName;
                    Gallery g = new Gallery(GalleryPath);
                    this.Galleries.Add(g);
                }
            }
            else
            {
                foreach(DirectoryInfo gallery in this.GalleriesDirectory.EnumerateDirectories())
                {
                    string GalleryPath = gallery.Name;
                    Gallery g = new Gallery(GalleryPath);
                    this.Galleries.Add(g);
                }
            }
        }
        public void CompressGalleries(bool test)
        {
            foreach (Gallery gallery in this.Galleries)
            {
                CompressGallery(gallery, test);
            }
        }
        public void CompressGallery(Gallery gallery, bool test)
        {
            string TargetFile = this.OutputDirectory + "\\" + gallery.name + ".zip";
            using(ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(gallery.path);
                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestSpeed;
                zip.Comment = "Zip created with HathZipper at " + System.DateTime.Now.ToString("G");
                // TODO: Add zip.SaveProgress & zip.ZipError
                zip.Save(TargetFile);
            }

            if (test != false)
            {
                using(ZipFile zip = ZipFile.Read(TargetFile))
                {
                    foreach(ZipEntry e in zip)
                    {
                        e.Extract(System.IO.Stream.Null);
                    }
                }
            }
        }        
    }
}
