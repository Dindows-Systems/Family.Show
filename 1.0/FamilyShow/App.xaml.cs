using System;
using System.Windows;
using System.Windows.Input;
using System.Data;
using System.Xml;
using System.Configuration;
using Microsoft.FamilyShowLib;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml.Serialization;
using System.IO;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : System.Windows.Application
    {
        // The name of the application folder.  This folder is used to save the files 
        // for this application such as the photos, stories and family data.
        private const string ApplicationFolderName = "Family.Show";

        // The main list of family members that is shared for the entire application.
        // The FamilyCollection and Family fields are accessed from the same thread,
        // so suppressing the CA2211 code analysis warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static People FamilyCollection = new People();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static PeopleCollection Family = FamilyCollection.PeopleCollection;

        // The number of recent files to keep track of.
        private const int NumberOfRecentFiles = 5;

        // The path to the recent files file.
        private readonly static string RecentFilesFilePath = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
            Path.Combine(App.ApplicationFolderName, "RecentFiles.xml"));

        // The global list of recent files.
        private static StringCollection recentFiles = new StringCollection();

        /// <summary>
        /// Return list of recent files.
        /// </summary>
        public static StringCollection RecentFiles
        {
            get { return recentFiles; }
        }

        /// <summary>
        /// Occurs when the application starts.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected override void OnStartup(StartupEventArgs e)
        {
            // Full path to the document file location.
            string location = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments), ApplicationFolderName);

            // Create the directory and sample files if necessary.
            if (!Directory.Exists(location))
            {
                try
                {
                    // Document file location.
                    Directory.CreateDirectory(location);

                    // Sample files.
                    CreateSampleFile(location, "Windsor.family", FamilyShow.Properties.Resources.WindsorSampleFile);
                    CreateSampleFile(location, "Kennedy.ged", FamilyShow.Properties.Resources.KennedySampleFile);
                }
                catch
                {
                    // Could not install the sample files, handle all exceptions the
                    // same, ignore and continue without installing the sample files.
                }
            }

            // Load the collection of recent files.
            LoadRecentFiles();

            base.OnStartup(e);
        }

        /// <summary>
        /// Occurs when the application exits.
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            // Save the file automatically
            if (Family.IsDirty && !string.IsNullOrEmpty(FamilyCollection.FullyQualifiedFilename))
                FamilyCollection.Save();

            SaveRecentFiles();
            base.OnExit(e);
        }
        
        /// <summary>
        /// Return the animation duration. The duration is extended
        /// if special keys are currently pressed (for demo purposes)  
        /// otherwise the specified duration is returned. 
        /// </summary>
        public static TimeSpan GetAnimationDuration(double milliseconds)
        {
            return TimeSpan.FromMilliseconds(
                Keyboard.IsKeyDown(Key.F12) ?
                milliseconds * 5 : milliseconds);
        }

        /// <summary>
        /// Load the list of recent files from disk.
        /// </summary>
        public static void LoadRecentFiles()
        {
            if (File.Exists(RecentFilesFilePath))
            {
                // Load the Recent Files from disk
                XmlSerializer ser = new XmlSerializer(typeof(StringCollection));
                using (TextReader reader = new StreamReader(RecentFilesFilePath))
                {
                    recentFiles = (StringCollection)ser.Deserialize(reader);
                }

                // Remove files from the Recent Files list that no longer exists.
                for (int i = 0; i < recentFiles.Count; i++)
                {
                    if (!File.Exists(recentFiles[i]))
                        recentFiles.RemoveAt(i);
                }

                // Only keep the 5 most recent files, trim the rest.
                while (recentFiles.Count > NumberOfRecentFiles)
                    recentFiles.RemoveAt(NumberOfRecentFiles);
            }
        }

        /// <summary>
        /// Save the list of recent files to disk.
        /// </summary>
        public static void SaveRecentFiles()
        {
            XmlSerializer ser = new XmlSerializer(typeof(StringCollection));
            using (TextWriter writer = new StreamWriter(RecentFilesFilePath))
            {
                ser.Serialize(writer, recentFiles);
            }
        }

	    /// <summary>
	    /// Extract the sample family files from the executable and write it to the file system.
	    /// </summary>
        private static void CreateSampleFile(string location, string fileName, byte[] fileContent)
        {
            // Full path to the sample file.
            string path = Path.Combine(location, fileName);

            // Return right away if the file already exists.
            if (File.Exists(path))
                return;

            // Create the file.
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                writer.Write(fileContent);
            }
        }

        /// <summary>
        /// Converts string to date time object using DateTime.TryParse.  
        /// Also accepts just the year for dates. 1977 = 1/1/1977.
        /// </summary>
        internal static DateTime StringToDate(string dateString)
        {
            // Append first month and day if just the year was entered.
            if (dateString.Length == 4)
                dateString = "1/1/" + dateString;

            DateTime date;
            DateTime.TryParse(dateString, out date);

            return date;
        }
    }
}