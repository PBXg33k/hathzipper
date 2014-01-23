using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HathZipper
{
    class HathZipper
    {
        public delegate void GalleryUpdateHandler(object sender, ProgressEventArgs e);
        public event GalleryUpdateHandler OnGalleryUpdateStatus;
    }

    public class ProgressEventArgs : EventArgs
    {
        public string Status { get; private set; }

        public ProgressEventArgs(string status)
        {
            Status = status;
        }
    }
}
