using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinForms.CalendarComponent.Models;

namespace App1.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalPage : ContentPage
    {
        public CalPage()
        {
            InitializeComponent();
         
            List<SpecialDays> list = new List<SpecialDays>();
            Calendar.ShowSpecialDays = true;
            
             
            SpecialDays day = new SpecialDays();
            day.Date = DateTime.Now;
            list.Add(day);
            Calendar.SpecialDays = list;
            Calendar.Date = DateTime.Now;


        }

        private void Calendar_DayTapped(object sender, XamarinForms.CalendarComponent.Components.CalendarDayTappedEventArgs e)
        {

        }
    }
}