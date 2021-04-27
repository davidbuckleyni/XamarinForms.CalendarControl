using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinForms.CalendarComponent.Models
{
   public  class SpecialDays
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Colour { get; set; }

        public bool ShowSpecialDays { get; set; }
    }
}
