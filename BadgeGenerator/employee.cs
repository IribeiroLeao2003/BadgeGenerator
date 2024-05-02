using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BadgeGenerator
{
    public class Employee
    {

        private string empName;
        private string empNumber;
        private string empBarcode;
        private ImageSource empImage;


        public string EmpName
        {
            get { return empName; }
            set { empName = value; }  
        }

        
        public string EmpNumber
        {
            get { return empNumber; }
            set { empNumber = value; }  
        }

        public string EmpBarcode
        {
            get { return empBarcode; } 
            set { empBarcode = value; }
        }

      
        public ImageSource EmpImage
        {
            get { return empImage; }
            set { empImage = value; }
        }

        public Employee(string employeeName, string employeeNumber,string barcodeNumber, ImageSource image)
        {

            this.empName = employeeName;
            this.empNumber = employeeNumber;
            this.empBarcode = barcodeNumber;
            this.empImage = image;
        }
    }
}
