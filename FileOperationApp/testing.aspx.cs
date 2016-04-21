using FileOperationApp.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FileOperationApp
{
    public partial class testing : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Read and show each line from the file.
            string line = "";

            FileStream fsOverwrite = new FileStream(@"C:\NikunjParmar\FileOperationAppLargeFile\FileOperationApp\App_Data\uploads\Copy_of_Sample60MB.txt", FileMode.OpenOrCreate);
            using (StreamWriter swOverwrite = new StreamWriter(fsOverwrite))
            {
                using (StreamReader sr = new StreamReader(@"C:\NikunjParmar\FileOperationAppLargeFile\FileOperationApp\App_Data\uploads\Sample60MB.txt"))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        swOverwrite.WriteLine(line);
                    }
                }

                swOverwrite.WriteLine("modified file last line");
            }

            File.Delete(@"C:\NikunjParmar\FileOperationAppLargeFile\FileOperationApp\App_Data\uploads\Sample60MB.txt");
            File.Move(@"C:\NikunjParmar\FileOperationAppLargeFile\FileOperationApp\App_Data\uploads\Copy_of_Sample60MB.txt",
                        @"C:\NikunjParmar\FileOperationAppLargeFile\FileOperationApp\App_Data\uploads\Sample60MB.txt");

        }
    }
}