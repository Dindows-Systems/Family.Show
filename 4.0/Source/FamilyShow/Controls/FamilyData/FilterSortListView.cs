/*
 * A base class that sorts the data in a ListView control.
*/

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Threading;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Class that parses the filter text.
    /// </summary>
    public class Filter
    {
        // Parsed data from the filter string.
        private string filterText;
        private int? maximumAge;
        private int? minimumAge;
        private DateTime? filterDate;
        private bool photos = false;
        private bool restrictions = false;
        private bool attachments = false;
        private bool notes = false;
        private bool images = false;
        private bool living = false;
        private bool citations = false;

        private bool nophotos = false;
        private bool norestrictions = false;
        private bool noattachments = false;
        private bool nonotes = false;
        private bool noimages = false;
        private bool noliving = false;
        private bool nocitations = false;
        private string gender;

        /// <summary>
        /// Indicates if the filter is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(this.filterText); }
        }

        /// <summary>
        /// Return true if the filter contains the specified text.
        /// </summary>
        public bool Matches(string text)
        {
            return (this.filterText != null && text != null &&
                text.ToLower(CultureInfo.CurrentCulture).Contains(this.filterText));
        }

        /// <summary>
        /// Return true if the filter contains the specified date.
        /// </summary>
        public bool Matches(DateTime? date)
        {
            return (date != null && date.Value.ToShortDateString().Contains(this.filterText));
        }

        /// <summary>
        /// Return true if the filter contains the year in the specified date.
        /// </summary>
        public bool MatchesYear(DateTime? date)
        {
            return (date != null && date.Value.Year.ToString(CultureInfo.CurrentCulture).Contains(this.filterText));
        }

        public bool MatchesPhotos(bool photo)
        {
            if (photo == true && photos ==true)
                return true;
            if (nophotos == true && photo==false)
                return true;
            
            return false;
        }

        public bool MatchesRestrictions(bool restriction)
        {
            if (restriction==true && restrictions==true)
                return true;

            if (norestrictions == true && restriction==false)
                return true;
            
            return false;
        }

        public bool MatchesCitations(bool citation)
        {
            if (citation == true && citations == true)
                return true;

            if (nocitations == true && citation == false)
                return true;

            return false;
        }

        public bool MatchesAttachments(bool attachment)
        {
            if (attachment == true && attachments == true)
                return true;

            if (noattachments == true && attachment == false)
                return true;

            return false;
        }

        public bool MatchesNotes(bool note)
        {
            if (note == true && notes == true)
                return true;

            if (nonotes == true && note == false)
                return true;

            return false;
        }

        public bool MatchesLiving(bool isliving)
        {
            if (isliving == true && living == true)
                return true;

            if (noliving == true && isliving == false)
                return true;

            return false;
        }

        public bool MatchesImages(bool image)
        {
            if (image == true && images == true)
                return true;

            if (noimages == true && image == false)
                return true;

            return false;
        }

        public bool MatchesGender(string genders)
        {
            if (genders==gender)
                return true;

            return false;
        }

        /// <summary>
        /// Return true if the filter contains the month in the specified date.
        /// </summary>
        public bool MatchesMonth(DateTime? date)
        {
            return (date != null && this.filterDate != null &&
                date.Value.Month == this.filterDate.Value.Month);
        }

        /// <summary>
        /// Return true if the filter contains the day in the specified date.
        /// </summary>
        public bool MatchesDay(DateTime? date)
        {
            return (date != null && this.filterDate != null &&
                date.Value.Day == this.filterDate.Value.Day);
        }

        /// <summary>
        /// Return true if the filter contains the specified age. The filter can 
        /// represent a single age (10), a range (10-20), or an ending (10+).
        /// </summary>
        public bool Matches(int? age)
        {
            if (age == null)
                return false;

            // Check single age.
            if (this.minimumAge != null && age.Value == this.minimumAge.Value)
                return true;

            // Check for a range.
            if (this.minimumAge != null && this.maximumAge != null &&
                age.Value >= this.minimumAge && age <= this.maximumAge)
                return true;

            // Check for an ending age.
            if (this.minimumAge == null && this.maximumAge != null && age.Value >= this.maximumAge)
                return true;

            return false;
        }

        /// <summary>
        /// Parse the specified filter text.
        /// </summary>
        public void Parse(string text)
        {
            // Initialize fields.
            this.filterText = "";
            this.gender = "";
            this.filterDate = null;
            this.minimumAge = null;
            this.maximumAge = null;

            this.photos = false;
            this.restrictions = false;
            this.attachments = false;
            this.notes = false;
            this.images = false;
            this.living = false;
            this.citations = false;

            this.nophotos = false;
            this.norestrictions = false;
            this.noattachments = false;
            this.nonotes = false;
            this.noimages = false;
            this.noliving = false;
            this.nocitations = false;

            // Store the filter text.
            this.filterText = string.IsNullOrEmpty(text) ? "" : text.ToLower(CultureInfo.CurrentCulture).Trim();

            // Parse date and age.
            ParseDate();
            ParseAge();
            ParsePhotos();
            ParseRestrictions();
            ParseNotes();
            ParseAttachments();
            ParseImages();
            ParseLiving();
            ParseCitations();
            ParseGender();

        }

        /// <summary>
        /// Parse the filter date.
        /// </summary>
        private void ParseDate()
        {
            DateTime date;
            if (DateTime.TryParse(this.filterText, out date))
                this.filterDate = date;
        }

        /// <summary>
        /// Parse photos.
        /// </summary>
        private void ParsePhotos()
        {
            if (this.filterText == (Properties.Resources.Photos.ToLower()))
                this.photos = true;
            if (this.filterText == ("!" + Properties.Resources.Photos.ToLower()))
                this.nophotos = true;
        }

        /// <summary>
        /// Parse genders.
        /// </summary>
        private void ParseGender()
        {
            if (this.filterText == Properties.Resources.Female.ToLower())
                this.gender = Properties.Resources.Female.ToLower();
            if (this.filterText == Properties.Resources.Male.ToLower())
                this.gender = Properties.Resources.Male.ToLower();
        }

        /// <summary>
        /// Parse restrictions.
        /// </summary>
        private void ParseRestrictions()
        {
            if (this.filterText == (Properties.Resources.Restriction.ToLower()))
                this.restrictions = true;
            if (this.filterText == ("!" + Properties.Resources.Restriction.ToLower()))
                this.norestrictions = true;
        }

        /// <summary>
        /// Parse images.
        /// </summary>
        private void ParseImages()
        {
            if (this.filterText == (Properties.Resources.Image.ToLower()))
                this.images = true;
            if (this.filterText == ("!" +Properties.Resources.Image.ToLower()))
                this.noimages = true;
        }

        /// <summary>
        /// Parse notes.
        /// </summary>
        private void ParseNotes()
        {
            if (this.filterText == (Properties.Resources.Note.ToLower()))
                this.notes = true;
            if (this.filterText == ("!"+ Properties.Resources.Note.ToLower()))
                this.nonotes = true;
        }

        /// <summary>
        /// Parse attachments.
        /// </summary>
        private void ParseAttachments()
        {
            if (this.filterText == (Properties.Resources.Attachment.ToLower()))
                this.attachments = true;
            if (this.filterText == ("!"+ Properties.Resources.Attachment.ToLower()))
                this.noattachments = true;
        }

        /// <summary>
        /// Parse living.
        /// </summary>
        private void ParseLiving()
        {
            if (this.filterText == (Properties.Resources.Living.ToLower()))
                this.living = true;
            if (this.filterText == (Properties.Resources.Deceased.ToLower()))
                this.noliving = true;
        }

        /// <summary>
        /// Parse citations.
        /// </summary>
        private void ParseCitations()
        {
            if (this.filterText == (Properties.Resources.Citations.ToLower()))
                this.citations = true;
            if (this.filterText == ("!"+Properties.Resources.Citations.ToLower()))
                this.nocitations = true;
        }

        /// <summary>
        /// Parse the filter age. The filter can represent a
        /// single age (10), a range (10-20), or an ending (10+).
        /// </summary>
        private void ParseAge()
        {
            int age;

            // Single age.
            if (Int32.TryParse(this.filterText, out age))
                this.minimumAge = age;

            // Age range.
            if (this.filterText.Contains("-"))
            {
                string[] list = this.filterText.Split('-');

                if (Int32.TryParse(list[0], out age))
                    this.minimumAge = age;

                if (Int32.TryParse(list[1], out age))
                    this.maximumAge = age;
            }

            // Ending age.
            if (this.filterText.EndsWith("+"))
            {
                if (Int32.TryParse(this.filterText.Substring(0, this.filterText.Length - 1), out age))
                    this.maximumAge = age;
            }
        }
    }


    /// <summary>
    /// ??
    /// </summary>
    public class FilterSortListView : SortListView
    {
        private delegate void FilterDelegate();
        private Filter filter = new Filter();

        /// <summary>
        /// Get the filter for this control.
        /// </summary>
        protected Filter Filter
        {
            get { return this.filter; }
        }

        /// <summary>
        /// Filter the data using the specified filter text.
        /// </summary>
        public void FilterList(string text)
        {
            // Setup the filter object.
            filter.Parse(text);

            // Start an async operation that filters the list.
            this.Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                new FilterDelegate(FilterWorker));
        }

        /// <summary>
        /// Worker method that filters the list.
        /// </summary>
        private void FilterWorker()
        {
            // Get the data the ListView is bound to.
            ICollectionView view = CollectionViewSource.GetDefaultView(this.ItemsSource);
           
            // Clear the list if the filter is empty, otherwise filter the list.
            view.Filter = filter.IsEmpty ? null :
                new Predicate<object>(FilterCallback);
        }

        /// <summary>
        /// This is called for each item in the list. The derived classes 
        /// override this method.
        /// </summary>
        virtual protected bool FilterCallback(object item)
        {
            return false;
        }
    }
}
