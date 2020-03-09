using KursProject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UZNRKT.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddApplication.xaml
    /// </summary>
    public partial class AddApplication : Window
    {
        /// <summary>
        /// Упрощенное взаимодействие с БД Access
        /// </summary>
        private UsingAccess UsAc;

        public AddApplication(UsingAccess UsAc)
        {
            InitializeComponent();

            this.UsAc = UsAc;
        }

        private void ButtonClick_add(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        private void ButtonClick_cancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public string DateApplication
        {
            get
            {
                return F_DateApplication.Text;
            }
            set
            {
                F_DateApplication.Text = value;
            }
        }
        public string Client
        {
            get
            {
                return F_Client.Text;
            }
            set
            {
                F_Client.Text = value;
            }
        }
        public string Type
        {
            get
            {
                return F_Type.Text;
            }
            set
            {
                F_Type.Text = value;
            }
        }
        public string Producer
        {
            get
            {
                return F_Producer.Text;
            }
            set
            {
                F_Producer.Text = value;
            }
        }
    }
}


