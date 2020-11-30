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
using System.Windows.Navigation;
using System.Windows.Shapes;

using CertificateCreator.Models;

namespace CertificateCreator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CertManager manager;
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                manager = new CertManager();
            }
            catch(Exception err)
            {
                MessageBox.Show(err.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }

        private void CreateClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] cert = RSAHelper.CreateCACert(new RSAHelper.RSAProperty(RSAHelper.RSAProperty.Algorithm.SHA1, 2048, "KKAB_CA", "RU", "KKAB", "Test"));
                CertManager mng = new CertManager();
                mng.AddCACert("Test", cert[0], cert[1], cert[2]);
            }
            catch(Exception err)
            {
                MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
