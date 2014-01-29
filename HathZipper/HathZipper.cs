﻿using Ionic.Zip;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HathZipper
{
    internal class HathZipper
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

        private void Construct()
        {
            this.Galleries.AllowRemove = false;
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
            // Set listchanged event to galleryfound notification

            switch (method)
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
            if (OnlyCompleted == true)
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
            if (directories != null)
            {
                foreach (DirectoryInfo di in directories)
                {
                    Gallery g = new Gallery(di);
                }
            }
        }

        private void ScanGalleriesMethodThree(bool OnlyCompleted)
        {
            if (OnlyCompleted == true)
            {
                foreach (FileInfo gallery in this.GalleriesDirectory.EnumerateFiles("galleryinfo.txt", SearchOption.AllDirectories))
                {
                    string GalleryPath = gallery.DirectoryName;
                    Gallery g = new Gallery(GalleryPath);
                    this.Galleries.Add(g);
                    ScanStatusUpdate(1, this.Galleries);
                }
                ScanStatusUpdate(10, this.Galleries);
            }
            else
            {
                foreach (DirectoryInfo gallery in this.GalleriesDirectory.EnumerateDirectories())
                {
                    string GalleryPath = gallery.Name;
                    Gallery g = new Gallery(GalleryPath);
                    this.Galleries.Add(g);
                    ScanStatusUpdate(1, this.Galleries);
                }
                ScanStatusUpdate(10, this.Galleries);
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
            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(gallery.path);
                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestSpeed;
                zip.Comment = "Zip created with HathZipper at " + System.DateTime.Now.ToString("G");
                // TODO: Add zip.SaveProgress & zip.ZipError
                zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(OnSaveProgress);
                zip.ZipError += new EventHandler<ZipErrorEventArgs>(OnZipError);
                zip.AddProgress += new EventHandler<AddProgressEventArgs>(OnAddProgress);
                zip.Save(TargetFile);
            }

            if (test != false)
            {
                using (ZipFile zip = ZipFile.Read(TargetFile))
                {
                    foreach (ZipEntry e in zip)
                    {
                        e.Extract(System.IO.Stream.Null);
                    }
                }
            }
        }
        
        /// EVENTS
        public delegate void ScanStatusUpdateHandler(object sender, ScanProgressEventArgs e);
        public event ScanStatusUpdateHandler OnUpdateStatus;

        private void ScanStatusUpdate(int status, Gallery g)
        {
            // Make sure something is listening to this event
            // If not, return nothing and save cycles
            if (OnUpdateStatus == null) return;

            ScanProgressEventArgs args = new ScanProgressEventArgs(status, g);
            OnUpdateStatus(this, args);
        }
        private void ScanStatusUpdate(int status, BindingList<Gallery> g)
        {
            // Make sure something is listening to this event
            // If not, return nothing and save cycles
            if (OnUpdateStatus == null) return;

            ScanProgressEventArgs args = new ScanProgressEventArgs(status, g);
            OnUpdateStatus(this, args);
        }

        //ZIP EVENTPROXIES
        public event EventHandler<SaveProgressEventArgs> OnSaveProgress;
        public event EventHandler<ZipErrorEventArgs> OnZipError;
        public event EventHandler<AddProgressEventArgs> OnAddProgress;
    }

    public class ScanProgressEventArgs : EventArgs
    {
        public enum ScanStatus
        {
            Not_started = 0,
            Scanning = 1,
            Paused = 2,
            Interrupted = 3,
            Compressing = 4, //For zip
            Completed = 10
        }

        public Gallery Gallery;
        public BindingList<Gallery> Galleries;
        public ScanStatus Status { get; private set; }

        public ScanProgressEventArgs(int status, Gallery g)
        {
            switch (status)
            {
                case 0:
                default:
                    Status = ScanStatus.Not_started;
                    break;
                case 1:
                    Status = ScanStatus.Scanning;
                    break;
                case 2:
                    Status = ScanStatus.Paused;
                    break;
                case 3:
                    Status = ScanStatus.Interrupted;
                    break;
                case 4:
                    Status = ScanStatus.Compressing;
                    break;
                case 10:
                    Status = ScanStatus.Completed;
                    break;
            }

            Gallery = g;
        }
        public ScanProgressEventArgs(int status, BindingList<Gallery> g)
        {
            switch (status)
            {
                case 0:
                default:
                    Status = ScanStatus.Not_started;
                    break;
                case 1:
                    Status = ScanStatus.Scanning;
                    break;
                case 2:
                    Status = ScanStatus.Paused;
                    break;
                case 3:
                    Status = ScanStatus.Interrupted;
                    break;
                case 4:
                    Status = ScanStatus.Compressing;
                    break;
                case 10:
                    Status = ScanStatus.Completed;
                    break;
            }
            Gallery = g.Last();
            Galleries = g;
        }
    }
}