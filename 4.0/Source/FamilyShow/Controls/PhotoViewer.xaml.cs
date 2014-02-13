using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for PhotoViewer.xaml
    /// </summary>
    public partial class PhotoViewer : System.Windows.Controls.UserControl
    {
        public PhotoViewer()
        {
            InitializeComponent();
        }

        #region routed events

        public static readonly RoutedEvent CloseButtonClickEvent = EventManager.RegisterRoutedEvent(
            "CloseButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PhotoViewer));

        // Expose the PhotoViewer Close Button click event
        public event RoutedEventHandler CloseButtonClick
        {
            add { AddHandler(CloseButtonClickEvent, value); }
            remove { RemoveHandler(CloseButtonClickEvent, value); }
        }

        #endregion

        #region methods

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {            
                // Hide the photo tags and photo edit buttons if there is no main photo.
                if (DisplayPhoto.Source == null)
                {
                    TagsStackPanel.Visibility = Visibility.Hidden;
                    CaptionTextBlock.Visibility = Visibility.Collapsed;
                }
            }
        }
        
        /// <summary>
        /// Handler for the CloseButton click event
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Raise the CloseButtonClickEvent to notify the container to close this control
            RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent));
        }
	
        private void PhotosListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox photosListBox = sender as ListBox;
            if (photosListBox.SelectedIndex != -1)
            {
                // Get the path to the selected photo
                String path = photosListBox.SelectedItem.ToString();

                // Make sure that the file exists
                FileInfo fi = new FileInfo(path);
                if (fi.Exists)
                    SetDisplayPhoto(path);

            }
            else
            {
                // Clear the display photo
                DisplayPhoto.Source = null;

                // Hide the photos and tags

                TagsStackPanel.Visibility = Visibility.Collapsed;
                CaptionTextBlock.Visibility = Visibility.Collapsed;

                //Clear tags and caption
                TagsListBox.ItemsSource = null;
                CaptionTextBlock.Text = string.Empty;
                CaptionTextBlock.ToolTip = null; ;
            }
        }

        private void OpenPhotoButton_Click(object sender, RoutedEventArgs e)
        {

            Person person = (Person)this.DataContext;
            Photo photo = (Photo)PhotosListBox.SelectedItem;
            string path = photo.FullyQualifiedPath;

            string appLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    App.ApplicationFolderName);
            appLocation = Path.Combine(appLocation, App.AppDataFolderName);

            string fileExtension = Path.GetExtension(path);
            string newFileName = Path.GetFileNameWithoutExtension(path) + Guid.NewGuid().ToString() + fileExtension;
            string tempFilePath = Path.Combine(appLocation, newFileName);

            FileInfo ofi = new FileInfo(path);
            ofi.CopyTo(tempFilePath, true);

            try
            {
                System.Diagnostics.Process.Start(tempFilePath);
            }
            catch { }
        }

        #endregion

        #region helper methods

        /// <summary>
        /// Set the display photo
        /// </summary>
        private void SetDisplayPhoto(String path)
        {
            //This code must be used to create the bitmap
            //otherwise the program locks the image.
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelHeight = 280;  //max height of photo in viewer
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();

            DisplayPhoto.Source = bitmap;

            // Make sure the photo supports meta data before retrieving and displaying it
            if (HasMetaData(path))
            {
                // Extract the photo's metadata
                BitmapMetadata metadata = (BitmapMetadata)BitmapFrame.Create(new Uri(path)).Metadata;

                // Display the photo's tags
                if (metadata.Keywords != null)
                {
                    TagsStackPanel.Visibility = Visibility.Visible;
                    TagsListBox.ItemsSource = metadata.Keywords;
                }
                else
                {
                    TagsStackPanel.Visibility = Visibility.Hidden;
                    TagsListBox.ItemsSource = null;
                }

                // Display the photo's comment
                if (metadata.Title != null)
                {
                    CaptionTextBlock.Visibility = Visibility.Visible;
                    CaptionTextBlock.Text = metadata.Title;
                    CaptionTextBlock.ToolTip = metadata.Title;  //displays the full title if the title won't fit in the box
                }
                else
                {
                    CaptionTextBlock.Visibility = Visibility.Collapsed;
                    CaptionTextBlock.Text = string.Empty;
                    CaptionTextBlock.ToolTip = null;
                }
            }
            else
            {
                // Clear tags and caption
                TagsStackPanel.Visibility = Visibility.Hidden;
                TagsListBox.ItemsSource = null;
                CaptionTextBlock.Text = string.Empty;
                CaptionTextBlock.ToolTip = null;
            }
            
        }
	
        /// <summary>
        /// Only JPEG photos support metadata.
        /// </summary>
        private static bool HasMetaData(string fileName)
        {
            string extension = System.IO.Path.GetExtension(fileName);

            if (string.Compare(extension, ".jpg", true, CultureInfo.InvariantCulture) == 0 ||
                string.Compare(extension, ".jpeg", true, CultureInfo.InvariantCulture) == 0)
                return true;
                
            return false;
        }

        public void SetDefaultFocus()
        {
            CloseButton.Focus();
        }

        public void LoadPhotos(PeopleCollection people)
        {

            PhotosListBox.Items.Clear();

            PhotoCollection allPhotos = new PhotoCollection();

            foreach (Person p in people)
            {
                foreach (Photo photo in p.Photos)
                {

                    bool add = true;

                    foreach (Photo existingPhoto in allPhotos)
                    {

                        if (string.IsNullOrEmpty(photo.RelativePath))
                            add = false;

                        if (existingPhoto.RelativePath == photo.RelativePath)
                        {
                            add = false;
                            break;
                        }

                    }
                    if (add == true)
                        allPhotos.Add(photo);

                }

                if (allPhotos.Count == 0)
                    View.Visibility = Visibility.Collapsed;
                else
                    View.Visibility = Visibility.Visible;

            }


            foreach (Photo photo in allPhotos)
                PhotosListBox.Items.Add(photo);

            if (PhotosListBox.Items.Count > 0)
                PhotosListBox.SelectedIndex = 0;
        }

        #endregion

    }
}