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
using System.Diagnostics;

namespace Lighthouse_Controller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenPage(object sender, EventArgs e)
        {
            using (Process OpenOtherDevicesSettings = new())
            {
                OpenOtherDevicesSettings.StartInfo.UseShellExecute = true;
                OpenOtherDevicesSettings.StartInfo.FileName = "ms-settings:privacy-customdevices";
                OpenOtherDevicesSettings.Start();
            }

        }

        private void StartLighthouses(object sender, EventArgs e)
        {
            using (Process PowerOnLighthouses = new())
            {
                PowerOnLighthouses.StartInfo.UseShellExecute = true;
                PowerOnLighthouses.StartInfo.Arguments = "on";
                PowerOnLighthouses.StartInfo.FileName = "lighthousecontrolcmd.exe";
                PowerOnLighthouses.Start();
            }
        }

        private void StopLighthouses(object sender, EventArgs e)
        {
            using (Process PowerOffLighthouses = new())
            {
                PowerOffLighthouses.StartInfo.UseShellExecute = true;
                PowerOffLighthouses.StartInfo.Arguments = "off";
                PowerOffLighthouses.StartInfo.FileName = "lighthousecontrolcmd.exe";
                PowerOffLighthouses.Start();
            }

        }

        private void InstallService(object sender, EventArgs e) { 
            using (Process InstallServiceProcess = new())
            {
                var dir = AppContext.BaseDirectory;
                InstallServiceProcess.StartInfo.UseShellExecute = true;
                InstallServiceProcess.StartInfo.Verb = "runas";
                InstallServiceProcess.StartInfo.Arguments = "-executionpolicy bypass -file {dir}\\Install-LHService.ps1";
                InstallServiceProcess.StartInfo.FileName = "powershell.exe";
                InstallServiceProcess.Start();
          
            }

        }

        private void RemoveService(object sender, EventArgs e)
        {
            using (Process RemoveServiceProcess = new())
            {
                var dir  = AppContext.BaseDirectory;
                RemoveServiceProcess.StartInfo.UseShellExecute = true;
                RemoveServiceProcess.StartInfo.Verb = "runas";
                RemoveServiceProcess.StartInfo.Arguments = "-executionpolicy bypass -file {dir}\\Remove-LHService.ps1";
                RemoveServiceProcess.StartInfo.FileName = "powershell.exe";
                RemoveServiceProcess.Start();
            }

        }
    }
}
