/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.If not, see<http://www.gnu.org/licenses/>.
*/

using OdinSdk.FormLib.UI;
using System;
using System.Threading;
using System.Windows.Forms;

namespace OpenETaxBill.Certifier
{
    public static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (SingleInstance.Start() == false)
            {
                SingleInstance.ShowFirstInstance();
                return;
            }

            // Add the event handler for handling UI thread exceptions to the event.
            Application.ThreadException += Application_ThreadException;

            // Set the unhandled exception mode to force all Windows Forms errors to go through our handler.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // Add the event handler for handling non-UI thread exceptions to the event. 
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                using (MainForm mainForm = new MainForm())
                {
                    Application.Run(mainForm);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                SingleInstance.Stop();
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            DialogResult _result = DialogResult.Abort;

            try
            {
                _result = MessageBox.Show(
                        String.Format("Whoops! Please contact the developers with the following information:\n\n{0}{1}", e.Exception.Message, e.Exception.StackTrace),
                        "Application Error",
                        MessageBoxButtons.AbortRetryIgnore,
                        MessageBoxIcon.Stop
                    );
            }
            finally
            {
                if (_result == DialogResult.Abort)
                {
                    SingleInstance.Stop();
                    Application.Exit();
                }
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception _ex = (Exception)e.ExceptionObject;

                MessageBox.Show(
                        String.Format("Whoops! Please contact the developers with the following information:\n\n{0}{1}", _ex.Message, _ex.StackTrace),
                        "Fatal Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Stop
                    );
            }
            finally
            {
                SingleInstance.Stop();
                Application.Exit();
            }
        }
    }
}