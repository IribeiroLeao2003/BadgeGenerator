using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BadgeGenerator
{
    internal class Employee
    {

        private string empName;
        private string empNumber;
        private ImageSource empImage;

        public Employee(string employeeName, string employeeNumber, ImageSource image)
        {
            this.empName = employeeName;
            this.empNumber = employeeNumber;
            this.empImage = image;
        }
    }
}
