using Ionic.Zip;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;

namespace HathZipper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "HathZipper Alpha " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " (" + RetrieveLinkerTimestamp() + ")";
            Console.WindowWidth = 140;
            string targetdir = general.Default.TargetDirecory;
            bool help = false;
            bool error = false;
            bool delete = false;
            bool test = true;
            bool skipConfirmation = false;
            bool exitImmediatly = false;
            int verbose = 0;

            var p = new OptionSet()
            {
                { "set-targetdir=", "Set and save the TargetDirectory to config", v => { targetdir = v; }},
                { "d|delete",       "Deletes sourcefiles after compression (implies zip test)", v => { delete = true; test = true; }},
                { "t|test",         "Test compressed files after compressing", v => { test = true; }},
                { "v",              "Increase verbosity",                         v => { ++verbose; }},
                { "h|?|help",       "Show this help message and exit",            v => help = v != null },
                { "s|skip-confirmation", "Skips confirmation before starting compression", v => { skipConfirmation = true; }},
                { "x|exit-immediatly", "Exits the application without waiting for user input", v => { exitImmediatly = true; }}
            };
            List<string> Extra = p.Parse(args);

            if (general.Default.TargetDirecory == "" && targetdir == "")
            {
                ConsoleError("TargetDirectory isn't set, please update the config file or use --set-targetdir=foo to update the config file");
                help = true;
                error = true;
            }
            else if (general.Default.TargetDirecory != targetdir) // targetdir is updated, update settings
            {
                general.Default.TargetDirecory = targetdir;
            }
            else if (Extra.Count == 0)
            {
                ConsoleError("Missing arguments");
                help = true;
                error = true;
            }

            if (!help && !Directory.Exists(Extra[0]))
            {
                ConsoleError("Invalid or non-existing path given.");
                error = true;
            }

            if (!error)
            {
                HathZipper zipper = new HathZipper(Extra[0], targetdir);
                zipper.OnUpdateStatus += new HathZipper.ScanStatusUpdateHandler(GalleryFound);
                Console.WriteLine("Starting scan in a second. This might take a few minutes.");
                Console.Write("Scanning...");
                zipper.ScanGalleries(); //Loads all (completed) galleries, TODO: Test performance
                Console.WriteLine("Scan completed.");
                if (zipper.Galleries.Count > 0)
                {
                    Console.WriteLine("Found " + zipper.Galleries.Count + "  galleries.");
                    if(!skipConfirmation) {
                        Console.WriteLine("Press any key to start compression.");
                        Console.ReadKey();
                    }
                    zipper.OnSaveProgress += new EventHandler<SaveProgressEventArgs>(ZipProgress);
                    zipper.OnZipError += new EventHandler<ZipErrorEventArgs>(ZipError);
                    zipper.OnGalleryDeleted += new EventHandler<GalleryEventArgs>(GalleryDeleted);
                    zipper.OnGalleryChange += new EventHandler<GalleryEventArgs>(GalleryChange);
                    zipper.CompressGalleries(test, delete);
                    Console.WriteLine("Finished all work.");
                    Console.WriteLine("Compressed " + zipper.Galleries.Count + " galleries.");
                }
                else
                {
                    Console.WriteLine("No galleries found.");
                }
                if (!exitImmediatly)
                    Console.ReadKey();
            }

            if (help)
                ShowHelp(p);
        }

        private static void GalleryDeleted(object sender, GalleryEventArgs e)
        {
            //Console.WriteLine(e.Message);
            Console.WriteLine("Deleted: " + e.Gallery.name);
        }

        private static void GalleryChange(object sender,GalleryEventArgs e)
        {
            if(e.Type == GalleryEventArgs.EventType.Gallery_changed)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(e.Message);
                Console.SetCursorPosition(0, Console.CursorTop);
            }
        }

        private static void GalleryFound(object sender, ScanProgressEventArgs e)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            if(e.Galleries.Count > 0)
                Console.WriteLine("Found (" + e.Galleries.Count + "):" + e.Gallery.name);
            Console.Write("Scanning...");
        }

        private static void ZipError(object sender, ZipErrorEventArgs e)
        {
            ConsoleError("ZIPERROR: An error occured while compressing a gallery. The following exception was thrown: " + e.Exception.Message);
        }

        private static void ZipProgress(object sender, SaveProgressEventArgs args)
        {
            string filename = new DirectoryInfo(args.ArchiveName).Name;
            FileInfo fi = new FileInfo(filename);
            string line = fi.Name + ": " + args.BytesTransferred.ToString() + "/" + args.TotalBytesToTransfer.ToString();
            Console.SetCursorPosition(0, Console.CursorTop);
            if (args.EventType == ZipProgressEventType.Saving_Completed)
                Console.WriteLine("Complete: " + fi.Name + "   ");
            else
                Console.Write("Compressing: " + fi.Name);
        }

        /// <summary>
        /// Dumps formatted error message in red to console
        /// </summary>
        /// <param name="error">Error message as a string</param>
        private static void ConsoleError(string error)
        {
            Console.WriteLine("ERROR: " + error, Console.ForegroundColor = ConsoleColor.Red);
            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Dumps all arguments to console in case invalid arguments are given or "?" is used
        /// </summary>
        /// <param name="p"></param>
        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: HathZipper [OPTIONS]+ Source directory");
            Console.WriteLine("Compress completed HatH galleries to target directory");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        private static DateTime RetrieveLinkerTimestamp()
        {
            string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            System.IO.Stream s = null;

            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            int i = System.BitConverter.ToInt32(b, c_PeHeaderOffset);
            int secondsSince1970 = System.BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }
    }
}