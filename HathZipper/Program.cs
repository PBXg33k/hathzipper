﻿using Ionic.Zip;
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

            int verbose = 0;

            var p = new OptionSet()
            {
                { "set-targetdir=", "Set and save the TargetDirectory to config", v => { targetdir = v; }},
                { "d|delete",       "Deletes sourcefiles after compression (implies zip test)", v => { delete = true; test = true; }},
                { "t|test",         "Test compressed files after compressing", v => { test = true; }},
                { "v",              "Increase verbosity",                         v => { ++verbose; }},
                { "h|?|help",       "Show this help message and exit",            v => help = v != null }
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
                TestGallery(zipper);
                Console.WriteLine("Scan completed.");
                Console.WriteLine("Found " + zipper.Galleries.Count + "  galleries.");
                Console.WriteLine("Press any key to start compression.");
                Console.ReadKey();
                zipper.OnSaveProgress += new EventHandler<SaveProgressEventArgs>(ZipProgress);
                zipper.OnZipError += new EventHandler<ZipErrorEventArgs>(ZipError);
                zipper.CompressGalleries(test);
            }

            if (help)
                ShowHelp(p);
        }

        private static void TestGallery(HathZipper zipper)
        {
            Console.WriteLine("Starting scan in a second. This might take a few minutes.");
            Console.Write("Scanning...");
            zipper.ScanGalleries(); //Loads all (completed) galleries, TODO: Test performance
        }

        private static void GalleryFound(object sender, ScanProgressEventArgs e)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine("Found (" + e.Galleries.Count + "):" + e.Gallery.name);
            Console.Write("Scanning...");
        }

        /// <summary>
        /// Looks up galleries that are completed by the HatH client.
        /// These galleries contain a galleryinfo.txt file.
        /// </summary>
        /// <param name="path">{hath_client_path}/downloaded</param>
        /// <returns>Lists of gallerypaths</returns>
        private static List<string> GetCompletedHatHGalleries(string path)
        {
            Console.WriteLine("Looking for completed HatH galleries in " + path);
            string[] Files = Directory.GetFiles(path, "galleryinfo.txt", SearchOption.AllDirectories);
            List<string> CompletedDirectories = new List<string>();

            foreach (string file in Files)
            {
                CompletedDirectories.Add(Directory.GetParent(file).ToString());
            }

            return CompletedDirectories;
        }

        /// <summary>
        /// Checks if the given Gallery is already compressed to a file in the output directory
        /// </summary>
        /// <param name="path">Path to the galllery</param>
        /// <returns>TRUE if zip exists, FALSE if not</returns>
        private static bool CheckGalleryStatus(string path)
        {
            return false;
        }

        /// <summary>
        /// Compresses the given gallery to a zipfile
        /// </summary>
        /// <param name="path">Path to the gallery</param>
        /// <param name="test">Test the zip right after compression</param>
        /// <param name="target">Output directory to place the zipfile</param>
        /// <returns></returns>
        private static bool CompressGallery(string path, bool test, string target)
        {
            string galleryName = new DirectoryInfo(path).Name;
            using (ZipFile zip = new ZipFile())
            {
                Console.WriteLine("Compressing gallery: " + galleryName);
                zip.AddDirectory(path);
                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestSpeed;
                zip.Comment = "Zip created with HathZipper v0.0.1_prealpha at " + System.DateTime.Now.ToString("G");
                //zip.AddProgress += new EventHandler<AddProgressEventArgs>(ZipProgress);
                zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(ZipProgress);
                zip.ZipError += new EventHandler<ZipErrorEventArgs>(ZipError);
                zip.Save(target + "\\" + galleryName + ".zip");
                Console.WriteLine();
            }
            return false;
        }

        private static void DeleteGallery(string path)
        {
            Directory.Delete(path, true);
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
            //int NewCursorPosition = (Console.CursorLeft - line.Length < 0) ? 0 : Console.CursorLeft - line.Length;
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