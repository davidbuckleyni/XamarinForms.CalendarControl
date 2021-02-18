using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace XamarinForms.CalendarComponent.Components
{
    public partial class CalendarControl : ContentView
    {
        public List<DayControl> Days { get; } = new List<DayControl>();
        
        public event EventHandler<DayControlTappedEventArgs> DayTapped;
        public event EventHandler<DayControlAddedEventArgs> DayAdded;

        #region WeekDayHeaderControlTemplateProperty

        public static readonly BindableProperty WeekDayControlTemplateProperty =
            BindableProperty.Create(
                propertyName: nameof(WeekDayControlTemplateProperty),
                returnType: typeof(ControlTemplate),
                declaringType: typeof(CalendarControl),
                propertyChanged: OnWeekDayControlTemplateChanged);

        private static void OnWeekDayControlTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var calendarControl = bindable as CalendarControl;
            calendarControl.InitializeWeekDayHeaders();
        }
        
        public ControlTemplate WeekDayHeaderControlTemplate
        {
            get => (ControlTemplate) GetValue(WeekDayControlTemplateProperty);
            set => SetValue(WeekDayControlTemplateProperty, value);
        }
        
        #endregion

        #region DayControlTemplateProperty

        public static readonly BindableProperty DayControlTemplateProperty =
            BindableProperty.Create(
                propertyName: nameof(DayControlTemplate),
                returnType: typeof(ControlTemplate),
                declaringType: typeof(CalendarControl),
                propertyChanged: OnDayControlTemplateChanged);

        private static void OnDayControlTemplateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var calendarControl = bindable as CalendarControl;
            calendarControl.InitializeView();
        }

        public ControlTemplate DayControlTemplate
        {
            get => (ControlTemplate) GetValue(DayControlTemplateProperty);
            set => SetValue(DayControlTemplateProperty, value);
        }
            
        #endregion
        
        #region SelectedDaysProperty
        
        public static readonly BindableProperty SelectedDaysProperty =
            BindableProperty.Create(
                propertyName: nameof(SelectedDays),
                returnType: typeof(IReadOnlyCollection<DateTime>),
                declaringType: typeof(CalendarControl),
                defaultValue: new List<DateTime>().AsReadOnly(),
                propertyChanged: OnSelectedDaysChanged);

        private static void OnSelectedDaysChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var daysToSelect = newValue as IList<DateTime>;
            var calendarControl = bindable as CalendarControl;
            
            if (calendarControl.SelectionMode == CalendarControlSelectionMode.SingleSelect)
            {
                if (daysToSelect != null && 
                    daysToSelect.Count > 1)
                {
                    throw new InvalidOperationException(
                        "Trying to select more than one day when working in SingleSelect mode");
                }
            }

            calendarControl.SelectDayControls(daysToSelect);
        }

        public IReadOnlyCollection<DateTime> SelectedDays
        {
            get => (IReadOnlyCollection<DateTime>) GetValue(SelectedDaysProperty);
            set => SetValue(SelectedDaysProperty, value);
        }

        #endregion
        
        #region DateProperty

        public static readonly BindableProperty DateProperty =
            BindableProperty.Create(
                propertyName: nameof(Date),
                returnType: typeof(DateTime),
                declaringType: typeof(CalendarControl),
                propertyChanged: OnDateChanged);

        private static void OnDateChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var calendarControl = bindable as CalendarControl;
            calendarControl.InitializeView();
        }

        public DateTime Date
        {
            get => (DateTime) GetValue(DateProperty);
            set => SetValue(DateProperty, value);
        }

        #endregion
        
        #region SelectionModeProperty

        public static readonly BindableProperty SelectionModeProperty =
            BindableProperty.Create(
                propertyName: nameof(SelectionMode),
                returnType: typeof(CalendarControlSelectionMode),
                declaringType: typeof(CalendarControl));

        public CalendarControlSelectionMode SelectionMode
        {
            get => (CalendarControlSelectionMode) GetValue(SelectionModeProperty);
            set => SetValue(SelectionModeProperty, value);
        }

        #endregion

        public CalendarControl()
        {
            InitializeComponent();
        }

        private void InitializeView()
        {
            if (DayControlTemplate == null)
            {
                return;
            }

            InitializeGridForCalendarDays();
            InitializeCalendarDays();
            
            SelectDayControls(SelectedDays);
        }
        
        private void SelectDayControls(IEnumerable<DateTime> daysToSelect)
        {
            Days.ForEach(dayControl => dayControl.IsSelected = false);

            foreach (var dayToSelect in daysToSelect)
            {
                var dayControl = Days.FirstOrDefault(x => x.Date == dayToSelect.Date);
                if (dayControl != null)
                {
                    dayControl.IsSelected = true;
                }
            }
        }

        private void InitializeGridForCalendarDays()
        {
            GridDays.Children.Clear();
            
            var gridRows = new RowDefinitionCollection();

            for (var i = 1; i <= Date.WeeksInMonth(); i++)
            {
                gridRows.Add(new RowDefinition());
            }

            GridDays.RowDefinitions = gridRows;
        }

        private void InitializeCalendarDays()
        {
            if (Days.Count > 0)
            {
                foreach (var dayControl in Days)
                {
                    dayControl.Tapped -= DayComponent_OnTapped;
                }
                
                Days.Clear();
            }

            for (var i = 1; i <= Date.DaysInMonth(); i++)
            {
                var date = new DateTime(Date.Year, Date.Month, i);

                var dayControl = new DayControl
                {
                    Date = date,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    VerticalOptions =  LayoutOptions.CenterAndExpand,
                };

                dayControl.ControlTemplate = DayControlTemplate;

                Grid.SetColumn(dayControl, date.DayOfWeek() - 1);
                Grid.SetRow(dayControl, date.WeekOfMonth() - 1);
                
                dayControl.Tapped += DayComponent_OnTapped;
                
                Days.Add(dayControl);
                GridDays.Children.Add(dayControl);

                DayAdded?.Invoke(this, new DayControlAddedEventArgs(dayControl));
            }
            
            if (SelectedDays?.Count > 0)
            {
                SelectDayControls(SelectedDays);
            }
        }
        
        private void DayComponent_OnTapped(object sender, EventArgs e)
        {
            var dayControl = sender as DayControl;

            if (dayControl.IsSelectable)
            {
                if (SelectionMode == CalendarControlSelectionMode.SingleSelect)
                {
                    SelectedDays = new[] {dayControl.Date};
                }
                else if (SelectionMode == CalendarControlSelectionMode.MultiSelect)
                {
                    var newSelectedDays = SelectedDays.ToList();

                    if (dayControl.IsSelected)
                    {
                        newSelectedDays.Remove(dayControl.Date);
                    }
                    else
                    {
                        newSelectedDays.Add(dayControl.Date);
                    }

                    SelectedDays = new ReadOnlyCollection<DateTime>(newSelectedDays);
                }
            }

            DayTapped?.Invoke(this, new DayControlTappedEventArgs(dayControl));
        }

        private void InitializeWeekDayHeaders()
        {
            if (WeekDayHeaderControlTemplate == null)
            {
                return;
            }

            void AddWeekDayHeaderControl(DayOfWeek dayOfWeek, int weekDayNumber)
            {
                var weekDayControl = new WeekDayHeaderControl();
                weekDayControl.ControlTemplate = WeekDayHeaderControlTemplate;
                weekDayControl.DayOfWeek = dayOfWeek;

                Grid.SetColumn(weekDayControl, weekDayNumber - 1);

                GridWeekDayHeaders.Children.Add(weekDayControl);
            }

            AddWeekDayHeaderControl(DayOfWeek.Monday, weekDayNumber: 1);
            AddWeekDayHeaderControl(DayOfWeek.Tuesday, weekDayNumber: 2);
            AddWeekDayHeaderControl(DayOfWeek.Wednesday, weekDayNumber: 3);
            AddWeekDayHeaderControl(DayOfWeek.Thursday, weekDayNumber: 4);
            AddWeekDayHeaderControl(DayOfWeek.Friday, weekDayNumber: 5);
            AddWeekDayHeaderControl(DayOfWeek.Saturday, weekDayNumber: 6);
            AddWeekDayHeaderControl(DayOfWeek.Sunday, weekDayNumber: 7);
        }
    }
    
    public class DayControlAddedEventArgs : EventArgs
    {
        public DayControl DayControl { get; }
        
        public DayControlAddedEventArgs(DayControl dayControl)
        {
            DayControl = dayControl;
        }
    }
    
    public class DayControlTappedEventArgs : EventArgs
    {
        public DayControl DayControl { get; }

        public DayControlTappedEventArgs(DayControl dayControl)
        {
            DayControl = dayControl;
        }
    }

    public enum CalendarControlSelectionMode
    {
        SingleSelect,
        MultiSelect
    }
}