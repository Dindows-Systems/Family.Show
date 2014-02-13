using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for AttachmentViewer.xaml
    /// </summary>
    public partial class AttachmentViewer : System.Windows.Controls.UserControl
    {
        public AttachmentViewer()
        {
            InitializeComponent();
        }

        #region routed events

        public static readonly RoutedEvent CloseButtonClickEvent = EventManager.RegisterRoutedEvent(
            "CloseButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AttachmentViewer));

        // Expose the AttachmentViewer Close Button click event
        public event RoutedEventHandler CloseButtonClick
        {
            add { AddHandler(CloseButtonClickEvent, value); }
            remove { RemoveHandler(CloseButtonClickEvent, value); }
        }

        #endregion

        #region methods

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
        
        /// <summary>
        /// Handler for the CloseButton click event
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Raise the CloseButtonClickEvent to notify the container to close this control
            RaiseEvent(new RoutedEventArgs(CloseButtonClickEvent));
        }

        /// <summary>
        /// Creates a temporary copy of the file in the family data directory with a GUID and then opens that file.
        /// This prevents problems such as:
        /// 1. On opening a familyx file, the process may fail if another program
        ///    has locked a file in the Attachments directory if the new file has an attachment of the same name.
        /// Any temp files will be cleaned up on application open or close once the other program has been closed.   
        /// </summary>
        private void LoadSelectedAttachment(object sender, RoutedEventArgs e)
        {
            if (AttachmentsListBox.Items.Count > 0 && AttachmentsListBox.SelectedItem != null)
            {

                string appLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    App.ApplicationFolderName);
                appLocation = Path.Combine(appLocation, App.AppDataFolderName);

                string fullFilePath = AttachmentsListBox.SelectedItem.ToString();

                string fileExtension = Path.GetExtension(fullFilePath);
                string newFileName = Path.GetFileNameWithoutExtension(fullFilePath) + Guid.NewGuid().ToString() + fileExtension;
                string tempFilePath = Path.Combine(appLocation, newFileName);

                FileInfo ofi = new FileInfo(fullFilePath);
                ofi.CopyTo(tempFilePath, true);

                try
                {
                    System.Diagnostics.Process.Start(tempFilePath);
                }
                catch { }
            }
        }

        #endregion

        #region helper methods

        public void SetDefaultFocus()
        {
            CloseButton.Focus();
        }

        public void LoadAttachments(PeopleCollection people)
        {

            AttachmentsListBox.Items.Clear();

            AttachmentCollection allAttachments = new AttachmentCollection();

            foreach (Person p in people)
            {
                foreach (Attachment Attachment in p.Attachments)
                {

                    bool add = true;

                    foreach (Attachment existingAttachment in allAttachments)
                    {

                        if (string.IsNullOrEmpty(Attachment.RelativePath))
                            add = false;

                        if (existingAttachment.RelativePath == Attachment.RelativePath)
                        {
                            add = false;
                            break;
                        }

                    }
                    if (add == true)
                        allAttachments.Add(Attachment);

                    

                }
            }


            foreach (Attachment Attachment in allAttachments)
                AttachmentsListBox.Items.Add(Attachment);

            if (AttachmentsListBox.Items.Count > 0)
                AttachmentsListBox.SelectedIndex = 0;
        }

        #endregion

    }
}