using FileOperationApp.Models;
using FileOperationApp.Views.TextParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace FileOperationApp.Controllers
{
    public class TextParserController : Controller
    {
        // GET: TextParser
        public ActionResult Index()
        {
            TextParserViewModel loTextParserViewModel = new TextParserViewModel();
            ViewBag.directoryFiles = Directory.GetFiles(Server.MapPath(Constants.FilePath));
            return View();
        }

        [HttpPost]
        public ActionResult Index(TextParserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                #region Save File
                HttpPostedFileBase postedFile = viewModel.file;

                if (postedFile != null && postedFile.ContentLength > 0)
                {        
                    string fileName = Path.GetFileName(postedFile.FileName);        
                    string path = Path.Combine(Server.MapPath(Constants.FilePath), fileName);
                    int count = 1;

                    while (System.IO.File.Exists(path))
                    {
                        string tempFileName = string.Format("{0}({1})", Path.GetFileNameWithoutExtension(postedFile.FileName), count++);
                        path = Path.Combine(Server.MapPath(Constants.FilePath), tempFileName + Path.GetExtension(postedFile.FileName));
                    }
                    postedFile.SaveAs(path);

                    TempData["Success"] = "File " + Path.GetFileName(path) + " uploaded successfully!";

                }
                #endregion
            }            
            ViewBag.directoryFiles = Directory.GetFiles(Server.MapPath(Constants.FilePath));
            return View(viewModel);
        }

        [HttpGet]
        public string GetContentFromFile(string fileName, int startingLine, int endingLine)
        {
            string fileFullPath = Path.Combine(Server.MapPath(Constants.FilePath), fileName);
            string content = string.Empty;

            int lineIndex = 1;
            using (StreamReader r = new StreamReader(fileFullPath))
            {
                string line = string.Empty;

                while ((line = r.ReadLine()) != null)
                {
                    if (lineIndex >= startingLine && lineIndex <= endingLine)
                    {
                        content += line + Environment.NewLine;
                    }

                    lineIndex++;
                }
                    
            }

            return content;
        }

        [HttpGet]
        public int GetFileTotalLines(string fileName)
        {
            string fileFullPath = Path.Combine(Server.MapPath(Constants.FilePath), fileName);
            var lineCount = 0;
            using (var reader = System.IO.File.OpenText(fileFullPath))
            {
                while (reader.ReadLine() != null)
                {
                    lineCount++;
                }
            }

            return lineCount;
        }

        [HttpPost]
        public JsonResult SubmitCommand(string fileName, string commandText)
        {            
            List<FileOutputResult> resultList = new List<FileOutputResult>();            

            string[] commands = commandText.Split(Constants.NewLine);

            ProcessCommandLineByLine(fileName, commands, resultList);
            
            return Json(resultList);            
        }

        private void ProcessCommandLineByLine(string fileName, string[] commands, List<FileOutputResult> resultList)
        {            
            foreach (string command in commands)
            {
                try
                {
                    if (string.IsNullOrEmpty(command.Trim()))
                    {
                        continue;
                    }

                    IdentifyCommand(fileName, command, resultList);                    
                }
                catch (Exception ex)
                {
                    FileOutputResult fileOutputResult = new FileOutputResult();
                    fileOutputResult.isValid = false;
                    fileOutputResult.message = "Error in command - " + command;
                    //fileOutputResult.newContent = "";

                    resultList.Add(fileOutputResult);
                }
            }
        }
        private void IdentifyCommand(string fileName, string command, List<FileOutputResult> resultList)
        {   
            string trimmedCommand = command.Trim().Replace(Constants.SpaceString, string.Empty);

            switch (trimmedCommand[0])
            {
                case 's':
                    ParseAndReplace(fileName, command, trimmedCommand);
                    break;
                case 'd':
                    ParseAndDelete(fileName, command, trimmedCommand);
                    break;
                case 'i':
                    ParseAndInsert(fileName, command, trimmedCommand);
                    break;
                case 'r':
                    ParseAndReverse(fileName, command, trimmedCommand);
                    break;
                default:
                    throw new InvalidCommand("Invalid command");
                    //break;
            }

            FileOutputResult fileOutputResult = new FileOutputResult();
            fileOutputResult.isValid = true;
            fileOutputResult.message = string.Empty;
            //fileOutputResult.newContent = "";

            resultList.Add(fileOutputResult);
        }

        private void ParseAndReplace(string fileName, string command, string trimmedCommand)
        {
            if (trimmedCommand.StartsWith("sr"))
            {
                string secParam = string.Empty;
                string thirdParam = string.Empty;
                int lineNo = 0;
                string lineNoStr = string.Empty;

                switch (trimmedCommand[2])
                {
                    case Constants.SingleQuote:
                        if (trimmedCommand[2] == Constants.SingleQuote && trimmedCommand[4] == Constants.SingleQuote &&
                                        trimmedCommand[5] == Constants.SingleQuote && trimmedCommand[7] == Constants.SingleQuote
                                        ) //For char
                        {
                            secParam = trimmedCommand[3].ToString();
                            thirdParam = trimmedCommand[6].ToString();

                            if (secParam.Length == 1 && thirdParam.Length == 1)
                            {
                                lineNo = 0;
                                lineNoStr = trimmedCommand.Substring(trimmedCommand.LastIndexOf(Constants.SingleQuote) + 1);

                                if (!string.IsNullOrEmpty(lineNoStr))
                                    lineNo = Convert.ToInt32(lineNoStr);

                                ReplaceLine(fileName, secParam[0], thirdParam[0], lineNo);
                            }
                            else
                                throw new InvalidCommand("Invalid Parameters");

                        }
                        else
                            throw new InvalidCommand("Invalid Parameters");
                        break;
                    case Constants.DoubleQuote:
                        
                        string trimString = command.Substring(command.IndexOf(Constants.DoubleQuoteString) + 1);
                        secParam = trimString.Substring(0, trimString.IndexOf(Constants.DoubleQuoteString));

                        trimString = trimString.Substring(trimString.IndexOf(Constants.DoubleQuoteString) + 1);

                        trimString = trimString.Substring(trimString.IndexOf(Constants.DoubleQuoteString) + 1);
                        thirdParam = trimString.Substring(0, trimString.IndexOf(Constants.DoubleQuoteString));

                        lineNo = 0;
                        lineNoStr = trimmedCommand.Substring(trimmedCommand.LastIndexOf(Constants.DoubleQuoteString) + 1);

                        if (!string.IsNullOrEmpty(lineNoStr))
                            lineNo = Convert.ToInt32(lineNoStr);

                        ReplaceLine(fileName, secParam, thirdParam, lineNo);
                        
                        break;
                    default:
                        throw new InvalidCommand("Invalid Parameters");
                }
            }
            else
            {
                throw new InvalidCommand("Invalid command");
            }
        }
        private void ParseAndDelete(string fileName, string command, string trimmedCommand)
        {
            int lineNo = 0;
            string lineNoStr = trimmedCommand.Substring(1);

            if (!string.IsNullOrEmpty(lineNoStr))
                lineNo = Convert.ToInt32(lineNoStr);

            if (lineNo > 0) 
            {
                #region Delete                
                DeleteLine(fileName, lineNo);
                #endregion
            }
            else
            {
                throw new InvalidCommand("Invalid Parameters");
            }
        }
        private void ParseAndInsert(string fileName, string command, string trimmedCommand)
        {
            int lineNo = 0;
            string lineNoStr = trimmedCommand.Substring(1, trimmedCommand.IndexOf(Constants.DoubleQuoteString) - 1);

            if (!string.IsNullOrEmpty(lineNoStr) && trimmedCommand[trimmedCommand.Length - 1] == Constants.DoubleQuote)
                lineNo = Convert.ToInt32(lineNoStr);

            if (lineNo > 0)
            {
                string trimString = command.Substring(command.IndexOf(Constants.DoubleQuoteString) + 1);
                string secParam = trimString.Substring(0, trimString.LastIndexOf(Constants.DoubleQuoteString));

                #region Insert 
                InsertTextOnLine(fileName, secParam, lineNo);
                #endregion
            }
            else
            {
                throw new InvalidCommand("Invalid Parameters");
            }
        }
        private void ParseAndReverse(string fileName, string command, string trimmedCommand)
        {            
            if (trimmedCommand.StartsWith("rev"))
            {
                int lineNo = 0;
                string lineNoStr = trimmedCommand.Substring(3);

                if (!string.IsNullOrEmpty(lineNoStr))
                    lineNo = Convert.ToInt32(lineNoStr);

                if (lineNo > 0)
                {
                    #region Reverse
                    ReverseLine(fileName, lineNo);
                    #endregion
                }
                else
                {
                    throw new InvalidCommand("Invalid Parameters");
                }
            }
            else
            {
                throw new InvalidCommand("Invalid command");
            }
        }
        
        private void ReplaceLine(string fileName, string source, string destination, int lineNo)
        {
            int counter = 0;
            string line;

            FileStream fsOverwrite = new FileStream(Path.Combine(Server.MapPath(Constants.FilePath), "Temp.txt"), FileMode.OpenOrCreate);
            using (StreamWriter swOverwrite = new StreamWriter(fsOverwrite))
            {
                using (StreamReader file = new StreamReader(Path.Combine(Server.MapPath(Constants.FilePath), fileName)))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        counter++;

                        if (lineNo == counter || lineNo == 0)
                        {
                            swOverwrite.WriteLine(line.Replace(source, destination));
                        }
                        else
                        {
                            swOverwrite.WriteLine(line);
                        }
                    }
                }
            }

            System.IO.File.Delete(Path.Combine(Server.MapPath(Constants.FilePath), fileName));
            System.IO.File.Move(Path.Combine(Server.MapPath(Constants.FilePath), "Temp.txt"),
                        Path.Combine(Server.MapPath(Constants.FilePath), fileName));
        }
        private void ReplaceLine(string fileName, char source, char destination, int lineNo)
        {
            ReplaceLine(fileName, Convert.ToString(source), Convert.ToString(destination), lineNo);
        }
        private void DeleteLine(string fileName, int lineNo)
        {
            int counter = 0;
            string line;

            FileStream fsOverwrite = new FileStream(Path.Combine(Server.MapPath(Constants.FilePath), "Temp.txt"), FileMode.OpenOrCreate);
            using (StreamWriter swOverwrite = new StreamWriter(fsOverwrite))
            {
                using (StreamReader file = new StreamReader(Path.Combine(Server.MapPath(Constants.FilePath), fileName)))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        counter++;

                        if (lineNo == counter && lineNo > 0)
                        {
                            //Do Nothing
                        }
                        else
                        {
                            swOverwrite.WriteLine(line);
                        }
                    }
                }
            }

            System.IO.File.Delete(Path.Combine(Server.MapPath(Constants.FilePath), fileName));
            System.IO.File.Move(Path.Combine(Server.MapPath(Constants.FilePath), "Temp.txt"),
                        Path.Combine(Server.MapPath(Constants.FilePath), fileName));

        }
        private void ReverseLine(string fileName, int lineNo)
        {
            int counter = 0;
            string line;

            FileStream fsOverwrite = new FileStream(Path.Combine(Server.MapPath(Constants.FilePath), "Temp.txt"), FileMode.OpenOrCreate);
            using (StreamWriter swOverwrite = new StreamWriter(fsOverwrite))
            {
                using (StreamReader file = new StreamReader(Path.Combine(Server.MapPath(Constants.FilePath), fileName)))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        counter++;

                        if (lineNo == counter && lineNo > 0)
                        {
                            swOverwrite.WriteLine(Reverse(line));
                        }
                        else
                        {
                            swOverwrite.WriteLine(line);
                        }
                    }
                }
            }

            System.IO.File.Delete(Path.Combine(Server.MapPath(Constants.FilePath), fileName));
            System.IO.File.Move(Path.Combine(Server.MapPath(Constants.FilePath), "Temp.txt"),
                        Path.Combine(Server.MapPath(Constants.FilePath), fileName));

        }
        private void InsertTextOnLine(string fileName, string textContent, int lineNo)
        {
            int counter = 0;
            string line;

            FileStream fsOverwrite = new FileStream(Path.Combine(Server.MapPath(Constants.FilePath), "Temp.txt"), FileMode.OpenOrCreate);
            using (StreamWriter swOverwrite = new StreamWriter(fsOverwrite))
            {
                using (StreamReader file = new StreamReader(Path.Combine(Server.MapPath(Constants.FilePath), fileName)))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        counter++;

                        if (lineNo == counter && lineNo > 0)
                        {                            
                            swOverwrite.WriteLine(textContent);
                            swOverwrite.WriteLine(line);                            
                        }
                        else
                        {
                            swOverwrite.WriteLine(line);
                        }
                    }
                }

                if (lineNo > counter)
                {
                    for (int i = counter + 1; i <= lineNo; i++)
                    {
                        counter++;

                        if (lineNo == counter)
                        {
                            swOverwrite.WriteLine(textContent);
                            swOverwrite.WriteLine(line);
                        }
                        else
                        {
                            swOverwrite.WriteLine(line);
                        }
                    }
                }
            }

            System.IO.File.Delete(Path.Combine(Server.MapPath(Constants.FilePath), fileName));
            System.IO.File.Move(Path.Combine(Server.MapPath(Constants.FilePath), "Temp.txt"),
                        Path.Combine(Server.MapPath(Constants.FilePath), fileName));
        }
        private string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}