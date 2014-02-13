
using System.Globalization;
namespace Microsoft.FamilyShowLib
{
    /// <summary>
    /// Stores the properties specific to this class library.
    /// </summary>
    public class App
    {
        // The name of the application folder.  This folder is used to save the 
        // files for this application such as the photos, stories and family data.
        public const string ApplicationFolderName = "Family.Show";
        public const string AppDataFolderName = "Family Data";

        internal static string ReplaceEncodedCharacters(string fileName)
        {
            fileName = fileName.Replace(" ", "");
            fileName = fileName = fileName.Replace("{", "");
            fileName = fileName.Replace("}", "");
            return fileName;
        }

        /// <summary>
        /// Determines if an image file is supported based on its extension.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static bool IsPhotoFileSupported(string fileName)
        {
            string extension = System.IO.Path.GetExtension(fileName);

            if (string.Compare(extension, ".jpg", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".jpeg", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".png", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".gif", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".tiff", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".tif", true, CultureInfo.InvariantCulture) == 0)
                return true;

            return false;
        }

        /// <summary>
        /// Determines if an attachment file is supported based on its extension.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static bool IsAttachmentFileSupported(string fileName)
        {

            string extension = System.IO.Path.GetExtension(fileName);

            // Only allow certain file types
            if (string.Compare(extension, ".docx", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".xlsx", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".pptx", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".odt", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".ods", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".odp", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".doc", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".xls", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".ppt", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".txt", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".htm", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".html", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".pdf", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".xps", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".rtf", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".kml", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".kmz", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".jpg", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".jpeg", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".png", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".gif", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".tiff", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".tif", true, CultureInfo.InvariantCulture) == 0)
                return true;

            return false;
        }
    }
}
