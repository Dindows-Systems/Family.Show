using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections;
using System.Globalization;
using Microsoft.FamilyShowLib;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for Histogram.xaml
    /// </summary>
    public partial class Histogram : System.Windows.Controls.UserControl
    {
        // This needs to be a static because the Histogram value convereter (see the bottom of this file)
        // uses the count of items in all groups to normalize the categories in the histogram.
        private static ListCollectionView lcv;

        /// <summary>
        /// Get the number of items in the current view.
        /// </summary>
        public static int Count
        {
            get { return lcv.Count; }
        }
        
        #region dependency properties

        public static readonly DependencyProperty SeriesCollectionProperty =
            DependencyProperty.Register("SeriesCollection", typeof(Object), typeof(Histogram),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(SeriesCollectionProperty_Changed)));

        /// <summary>
        /// The Collection that will be used to build the histogram
        /// </summary>
        public Object SeriesCollection
        {
            get { return (Object)GetValue(SeriesCollectionProperty); }
            set { SetValue(SeriesCollectionProperty, value); }
        }

        public static readonly DependencyProperty SortDescriptorProperty =
            DependencyProperty.Register("SortDescriptor", typeof(string), typeof(Histogram));

        /// <summary>
        /// The SortDescriptor is used for sorting the SeriesCollection
        /// </summary>
        public string SortDescriptor
        {
            get { return (string)GetValue(SortDescriptorProperty); }
            set { SetValue(SortDescriptorProperty, value); }
        }

        public static readonly DependencyProperty GroupDescriptorProperty =
            DependencyProperty.Register("GroupDescriptor", typeof(string), typeof(Histogram));

        /// <summary>
        /// The GroupDescriptor is used for goruping the collection into categories, or buckets
        /// </summary>
        public string GroupDescriptor
        {
            get { return (string)GetValue(GroupDescriptorProperty); }
            set { SetValue(GroupDescriptorProperty, value); }
        }

        #endregion

        #region routed events

        public static readonly RoutedEvent CategorySelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "CategorySelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Histogram));

        public event RoutedEventHandler CategorySelectionChanged
        {
            add { AddHandler(CategorySelectionChangedEvent, value); }
            remove { RemoveHandler(CategorySelectionChangedEvent, value); }
        }

        #endregion

        public Histogram()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Used as a filter predicate to see if the person should be included 
        /// </summary>
        /// <param name="o">Person object</param>
        /// <returns>True if the person should be included in the filter, otherwise false</returns>
        public static bool FilterPerson(object o)
        {
            Person p = o as Person;
            return (p.AgeGroup != AgeGroup.Unknown);
        }

        private static void SeriesCollectionProperty_Changed(DependencyObject sender, 
            DependencyPropertyChangedEventArgs args)
        {
            // The histogram will use a new collection view as its source. The ItemCount and Name properties
            // for the group will be used to display build the histogram (see Histogram.xaml).
            lcv = new ListCollectionView((IList)args.NewValue);
            lcv.Filter = new Predicate<object>(FilterPerson);

            Histogram histogram = (Histogram)sender;

            // Setup sorting for the view collection
            if (!string.IsNullOrEmpty(histogram.SortDescriptor))
                lcv.SortDescriptions.Add(new SortDescription(histogram.SortDescriptor,
                    ListSortDirection.Ascending));

            // Group the collection
            lcv.GroupDescriptions.Add(new PropertyGroupDescription(histogram.GroupDescriptor));

            // Use the new view w/group by as the items source for the list box
            histogram.HistogramListBox.ItemsSource = lcv.Groups;
        }

        private void HistogramListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CollectionViewGroup selected = (CollectionViewGroup)((ListBox)sender).SelectedItem;

            if (selected != null)
            {
                AgeGroupToHistogramLabelConverter cnv = new AgeGroupToHistogramLabelConverter();
                RaiseEvent(new RoutedEventArgs(CategorySelectionChangedEvent, 
                    cnv.Convert(selected.Name, null, null, null)));
            }
        }

        internal void Refresh()
        {
            lcv.Refresh();

            // Update the total count if items exist in the list view collection. Otherwise, if there
            // are no items, hide the histogram.
            if (lcv.Count == 0)
            {
                this.LayoutRoot.Visibility = Visibility.Hidden;
            }

            else
            {
                this.LayoutRoot.Visibility = Visibility.Visible;
                this.TotalCountLabel.Content = lcv.Count;
            }
        }

        internal void ClearSelection()
        {
            HistogramListBox.UnselectAll();
        }
    }


    /// <summary>
    /// Converts a category count to a value between 1 and 100.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    class HistogramValueToPercentageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double count = System.Convert.ToDouble(value, CultureInfo.CurrentCulture);

            // The count of all groups in the ListCollectionView is used to 'normalize' 
            // the values each category
            double total = System.Convert.ToDouble(Histogram.Count);

            if (total <= 0)
                return 0;
            else
                return System.Convert.ToInt32((count / total) * 100);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException(Properties.Resources.NotImplemented);
        }

        #endregion
    }

    /// <summary>
    /// Converts a person's age group to a text label that can be used on the histogram. Text is 
    /// retrieved from the resource file for the project.
    /// </summary>
    class AgeGroupToHistogramLabelConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string ageGroupLabel = Properties.Resources.AgeGroupUnknown;

            if (value != null)
            {
                AgeGroup ageGroup = (AgeGroup)value;
                if (ageGroup == AgeGroup.Youth)
                    ageGroupLabel = Properties.Resources.AgeGroupYouth;
                else if (ageGroup == AgeGroup.Adult)
                    ageGroupLabel = Properties.Resources.AgeGroupAdult;
                else if (ageGroup == AgeGroup.MiddleAge)
                    ageGroupLabel = Properties.Resources.AgeGroupMiddleAge;
                else if (ageGroup == AgeGroup.Senior)
                    ageGroupLabel = Properties.Resources.AgeGroupSenior;
            }

            return ageGroupLabel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException(Properties.Resources.NotImplemented);
        }

        #endregion
    }
}