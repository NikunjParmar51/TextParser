using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FileOperationApp.Views.TextParser
{
    public class TextParserViewModel
    {
        //[FileExtensions(Extensions = "txt", ErrorMessage = "Please upload valid format")]
        public HttpPostedFileBase file { get; set; }
        public string Content { get; set; }
        public string Command { get; set; }
        
        //public string[] DirectoryFiles { get; set; }
    }
}