using System;
using System.Windows;

namespace Microsoft.FamilyShow
{
    /// <summary>
    /// Interaction logic for DateCalculator.xaml
    /// </summary>
    public partial class DateCalculator : System.Windows.Controls.UserControl
    {

        public DateCalculator()
        {
            InitializeComponent();
            AddSubtractComboBox.SelectedIndex = 0;
        }

        #region routed events

        public static readonly RoutedEvent CancelButtonClickEvent = EventManager.RegisterRoutedEvent(
            "CancelButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DateCalculator));

        public event RoutedEventHandler CancelButtonClick
        {
            add { AddHandler(CancelButtonClickEvent, value); }
            remove { RemoveHandler(CancelButtonClickEvent, value); }
        }

        #endregion

        #region methods

        /// <summary>
        /// Handler for the age, birthdate, deathdate handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset the results
            DeathResult.Content = string.Empty;
            BirthResult.Content = string.Empty;
            AgeResult.Content = string.Empty;

            // Hide errors if showing
            HideErrors123();

            try
            {

                // Get any input dates
                DateTime s1 = App.StringToDate(Date1TextBox.Text);
                DateTime s2 = App.StringToDate(Date2TextBox.Text);

                if (!string.IsNullOrEmpty(Date1TextBox.Text))
                    Date1TextBox.Text = s1.ToShortDateString();
                if (!string.IsNullOrEmpty(Date2TextBox.Text))
                    Date2TextBox.Text = s2.ToShortDateString();

                int age = 0;

                // Get any input age
                if (!string.IsNullOrEmpty(AgeTextBox.Text))
                    age = int.Parse(AgeTextBox.Text);

                // If a birth date and death date are specified, calculate an age
                if (!string.IsNullOrEmpty(Date1TextBox.Text) && !string.IsNullOrEmpty(Date2TextBox.Text))
                {
                    AgeTextBox.Text = string.Empty;
                    age = -1;

                    TimeSpan span = s2.Subtract(s1);

                    DeathResult.Content = s2.ToShortDateString();
                    BirthResult.Content = s1.ToShortDateString();
                    AgeResult.Content = Math.Round(span.Days / 365.25, 0, MidpointRounding.AwayFromZero) + " " + Properties.Resources.years;

                }

                // If death data and age are specified, calculate a birth date
                if (string.IsNullOrEmpty(Date1TextBox.Text) && !string.IsNullOrEmpty(Date2TextBox.Text) && age != -1)
                {

                    int year = s2.Year;
                    int day = s2.Day;
                    int month = s2.Month;

                    year -= age;

                    DeathResult.Content = s2.ToShortDateString();
                    BirthResult.Content = new DateTime(year, month, day).ToShortDateString();
                    AgeResult.Content = age + " " + Properties.Resources.years;

                }

                // If birth data and age are specified, calculate a death date
                if (!string.IsNullOrEmpty(Date1TextBox.Text) && string.IsNullOrEmpty(Date2TextBox.Text) && age != -1)
                {

                    int year = s1.Year;
                    int month = s1.Month;
                    int day = s1.Day;

                    year += age;

                    BirthResult.Content = s1.ToShortDateString();
                    DeathResult.Content = new DateTime(year, month, day).ToShortDateString();
                    AgeResult.Content = age + " " + Properties.Resources.years;

                }
            }
            catch
            {
                // An error with the user input.  
                // Clear all results and show error icons.
                DeathResult.Content = string.Empty;
                BirthResult.Content = string.Empty;
                AgeResult.Content = string.Empty;
                ShowErrors123();
            }

        }

        /// <summary>
        /// Handler for the add/subtract operation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Calculate2Button_Click(object sender, RoutedEventArgs e)
        {
            // Reset the results
            Result2.Content = "";

            // Hide errors if showing
            HideErrors4();

            if (!string.IsNullOrEmpty(ToBox.Text.ToString()))
            {
                //Get the date input and try to add/subtract the specified number of days, months and years.
                DateTime s = App.StringToDate(ToBox.Text);
                if (!string.IsNullOrEmpty(ToBox.Text))
                    ToBox.Text = s.ToShortDateString();

                int i = 1;

                if (AddSubtractComboBox.SelectedIndex == 0)
                    i = 1;
                if ((AddSubtractComboBox.SelectedIndex == 1))
                    i = -1;

                double days = 0;
                int months = 0;
                int years = 0;

                try { days = double.Parse(DayBox.Text); }
                catch { }

                try { months = int.Parse(MonthBox.Text); }
                catch { }

                try { years = int.Parse(YearBox.Text); }
                catch { }

                try
                {
                    s = s.AddDays(days * i);
                    s = s.AddMonths(months * i);
                    s = s.AddYears(years * i);
                    Result2.Content = s.ToShortDateString();
                }
                catch { ShowErrors4(); }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            clear();
            RaiseEvent(new RoutedEventArgs(CancelButtonClickEvent));
        }

        #endregion

        #region helper methods

        private void HideErrors123()
        {
            Error1.Visibility = Visibility.Hidden;
            Error2.Visibility = Visibility.Hidden;
            Error3.Visibility = Visibility.Hidden;
        }

        private void ShowErrors123()
        {
            Error1.Visibility = Visibility.Visible;
            Error2.Visibility = Visibility.Visible;
            Error3.Visibility = Visibility.Visible;

        }

        private void ShowErrors4()
        {
            Error4.Visibility = Visibility.Visible;
        }

        private void HideErrors4()
        {
            Error4.Visibility = Visibility.Hidden;
        }

        private void clear()
        {
            //clear all the results and text input boxes
            Date1TextBox.Text = "";
            Date2TextBox.Text = "";
            AgeTextBox.Text = "";
            YearBox.Text = "";
            MonthBox.Text = "";
            DayBox.Text = "";
            ToBox.Text = "";

            DeathResult.Content = "";
            BirthResult.Content = "";
            AgeResult.Content = "";
            Result2.Content = "";

            //reset the error icons
            HideErrors123();
            HideErrors4();

        }

        #endregion

        private void AddSubtractComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //When the user changes the add/subtract selection, update the descriptor to/from
            if (AddSubtractComboBox.SelectedItem!=null)
            {
                if (AddSubtractComboBox.SelectedIndex == 0)
                    DateTo.Content = Properties.Resources.To.ToLower();
                else
                    DateTo.Content = Properties.Resources.From.ToLower();
            }
        }

    }
}