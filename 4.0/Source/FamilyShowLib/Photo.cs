using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Microsoft.FamilyShowLib
{
    /// <summary>
    /// Simple representation of a serializable photo associated with the Person class
    /// </summary>
    [Serializable]
    public class Photo : INotifyPropertyChanged
    {
        #region Fields and Constants

        public const string PhotosFolderName = "Images";
        private string relativePath;
        private bool isAvatar;
        
        #endregion

        #region Properties

        /// <summary>
        /// The relative path to the photo.
        /// </summary>
        public string RelativePath
        {
            get { return relativePath; }
            set
            {
                if (relativePath != value)
                {
                    relativePath = value;
                    OnPropertyChanged("relativePath");
                }
            }
        }

        /// <summary>
        /// The fully qualified path to the photo.
        /// </summary>
        [XmlIgnore]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public string FullyQualifiedPath
        {
            get
            {

                string tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                App.ApplicationFolderName);
                tempFolder = Path.Combine(tempFolder, App.AppDataFolderName);

                return Path.Combine(tempFolder, relativePath);
            }
            set
            {
                // This empty setter is needed for serialization.
            }
        }

        /// <summary>
        /// Whether the photo is the avatar photo or not.
        /// </summary>
        public bool IsAvatar
        {
            get { return isAvatar; }
            set
            {
                if (isAvatar != value)
                {
                    isAvatar = value;
                    OnPropertyChanged("IsAvatar");
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Empty constructor is needed for serialization
        /// </summary>
        public Photo() { }

        /// <summary>
        /// Constructor for Photo. Copies the photoPath to the images folder
        /// </summary>
        public Photo(string photoPath)
        {

            if (!string.IsNullOrEmpty(photoPath))
                // Copy the photo to the images folder
                this.relativePath = Copy(photoPath);
        }

        #endregion

        #region Methods
        
        public override string ToString()
        {
            return FullyQualifiedPath;
        }

        /// <summary>
        /// Copies the photo file to the application photos folder. 
        /// Returns the relative path to the copied photo.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static string Copy(string fileName)
        {
		
            // The photo file being copied
            FileInfo fi = new FileInfo(fileName);

            // Absolute path to the application folder
            string appLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                App.ApplicationFolderName);
            appLocation = Path.Combine(appLocation, App.AppDataFolderName);

            // Absolute path to the photos folder
            string photoLocation = Path.Combine(appLocation, PhotosFolderName);

            // Fully qualified path to the new photo file
            string photoFullPath = Path.Combine(photoLocation, App.ReplaceEncodedCharacters(fi.Name));

            // Relative path to the new photo file
		
			string photoRelLocation = Path.Combine(PhotosFolderName, App.ReplaceEncodedCharacters(fi.Name));

            // Create the appLocation directory if it doesn't exist
            if (!Directory.Exists(appLocation))
                Directory.CreateDirectory(appLocation);

            // Create the photos directory if it doesn't exist
            if (!Directory.Exists(photoLocation))
                Directory.CreateDirectory(photoLocation);
            
            // Copy the photo.
            try
            {
                string photoName = Path.GetFileName(photoFullPath);
                string photoNameNoExt = Path.GetFileNameWithoutExtension(photoFullPath);
                string photoNameExt = Path.GetExtension(photoFullPath);

                int i = 1;

                if (File.Exists(photoFullPath))
                {
                    do
                    {
                        photoName = photoNameNoExt + "(" + i + ")" + photoNameExt;  //don't overwrite existing files, append (#) to file if exists.
                        photoRelLocation = Path.Combine(PhotosFolderName, photoName);
                        photoFullPath = Path.Combine(photoLocation, photoName);
                        i++;
                    }
                    while (File.Exists(photoFullPath));
                    
                }
                fi.CopyTo(photoFullPath, true);

            }
            catch
            {
                // Could not copy the photo. Handle all exceptions 
                // the same, ignore and continue.
            }

            return photoRelLocation;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// Collection for photos.
    /// </summary>
    [Serializable]
    public class PhotoCollection : ObservableCollection<Photo>
    {
    }
}
