using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO.IsolatedStorage;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TCPDevice
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        string StorageFile = "App.data";
        public App()
        {
            // Initialize application-scope property
            Properties["LastOpenedProject"] = "NULL";
            Properties["LastOpenedSerial"] = "NULL";
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Restore application-scope property from isolated storage
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
            try
            {
                if (storage.FileExists(StorageFile))
                {
                    
                    using (IsolatedStorageFileStream stream = storage.OpenFile(StorageFile, FileMode.Open, FileAccess.Read))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        //storage.Remove();
                        //this.Shutdown();
                        // Restore each application-scope property individually
                        while (!reader.EndOfStream)
                        {
                            string[] keyValue = reader.ReadLine().Split(new char[] { '|' });
                            Properties[keyValue[0]] = keyValue[1];
                            
                        }
                    }
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                // Path the file didn't exist
            }
            catch (IsolatedStorageException ex)
            {
                // Storage was removed or doesn't exist
                // -or-
                // If using .NET 6+ the inner exception contains the real cause
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {

            // Persist application-scope property to isolated storage
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();
            using (IsolatedStorageFileStream stream = storage.OpenFile(StorageFile, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                
                // Persist each application-scope property individually
                foreach (string key in Properties.Keys)
                {
                    //MessageBox.Show(Properties[key].ToString());
                    writer.WriteLine("{0}|{1}", key, Properties[key]);
                    //MessageBox.Show($"{key} {Properties[key]}");
                }
                    
            }
        }
    }
}
