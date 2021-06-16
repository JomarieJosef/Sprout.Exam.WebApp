using System;
using System.Collections.Generic;
using System.Text;

namespace Sprout.Exam.Business.DataTransferObjects
{
    public class Calculatedto
    {
        public int Id { get; set; }
        public decimal absentDays { get; set; }
        public decimal workedDays { get; set; }
    }
}