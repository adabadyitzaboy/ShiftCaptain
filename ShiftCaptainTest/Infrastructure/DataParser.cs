using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShiftCaptainTest.Infrastructure
{
    public class DataParser
    {
        // Constants
        public const string CommentIndicator = @"//";
        public const string TableNameIndicator = "@@";
        public char FieldQualifier = '"';

        public string[] Delimiters = new[]
        {
            "\",\"",
            "\", \"",
            "\" ,\"",
            "\",  \"",
            "\" , \"",
            "\"  ,\""
        };

        // Variables
        private string[] _allLines;
        private string _filePath = string.Empty;

        /// <summary>
        /// Initializes the class.
        /// </summary>
        /// <param name="fileName">The absolute path to the file to parse.</param>
        public DataParser(string fileName, string path = "")
        {
            _filePath = Path.Combine(!string.IsNullOrEmpty(path) ? path : AppDomain.CurrentDomain.BaseDirectory,
                fileName);
            Initialize(_filePath);
        }

        private void Initialize(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("The filename parameter cannot be null or empty.", "fileName");
            }

            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(
                    string.Format("The file '{0}' could not be found.", fileName));
            }

            _allLines = File.ReadAllLines(fileName);
            _tableNames = null;
            _tables = null;

            if (LineCount == 0)
            {
                throw new DataException(
                    string.Format(
                        "The file '{0}' does not contain any lines.",
                        _filePath));
            }

            if (!TableNames.Any())
            {
                throw new DataException(
                    string.Format(
                        "The file '{0}' does not contain any tables. Please prepend your CSV files (and any tables) with '@@ \"My Table Name\"' to begin a table. See the README.txt file for more information.",
                        _filePath));
            }
        }

        public long LineCount
        {
            get { return _allLines.LongLength; }
        }

        private IEnumerable<string> _tableNames = null;

        public IEnumerable<string> TableNames
        {
            get
            {
                if (_tableNames == null)
                {
                    _tableNames = _allLines.Where(
                        l => l.Trim().StartsWith(TableNameIndicator))
                        .Select(GetCleanTableName).ToList();
                }

                return _tableNames;
            }
        }

        private string GetCleanTableName(string tableLine)
        {
            var rv = string.Empty;

            rv = tableLine.Replace(TableNameIndicator, string.Empty).Trim().Trim(FieldQualifier);

            if (rv.Contains(CommentIndicator))
            {
                string newRv = string.Empty;

                if (rv.Split(new string[] { CommentIndicator }, StringSplitOptions.RemoveEmptyEntries).Count() > 1)
                {
                    newRv =
                        rv.Split(new string[] { CommentIndicator }, StringSplitOptions.RemoveEmptyEntries)[0].Trim()
                            .Trim(FieldQualifier);
                }

                return newRv;
            }

            return rv;
        }

        private IDictionary<string, ICollection<IDictionary<string, string>>> _tables = null;
        
        public IDictionary<string, ICollection<IDictionary<string, string>>> Tables
        {
            get
            {
                if (_tables == null)
                {
                    _tables = new Dictionary<string, ICollection<IDictionary<string, string>>>();

                    var tableLines = _allLines.Where(
                        l => l.Trim().StartsWith(TableNameIndicator)).ToList();

                    bool lastTable = false;
                    int tableIndex = 0;
                    string tableName;
                    ICollection<IDictionary<string, string>> tableData;

                    foreach (var tl in tableLines)
                    {
                        tableName = GetCleanTableName(tl);
                        tableData = new Collection<IDictionary<string, string>>();

                        string colLine = string.Empty;
                        IEnumerable<string> dataLines;

                        IEnumerable<string> colNames;

                        lastTable = tableIndex == tableLines.Count - 1;

                        var csvLines = _allLines.Skip(Array.IndexOf(_allLines, tl));
                        colLine =
                            csvLines.FirstOrDefault(
                                l => !string.IsNullOrEmpty(l) &&
                                     !l.Trim().StartsWith(CommentIndicator) &&
                                     l.Trim().StartsWith(FieldQualifier.ToString()));

                        if (lastTable)
                        {
                            var startIdx = Array.IndexOf(_allLines, colLine) + 1;

                            dataLines = _allLines.Skip(startIdx).Where(
                                dl =>
                                    !string.IsNullOrEmpty(dl) &&
                                    !dl.Trim().StartsWith(CommentIndicator) &&
                                    dl.Trim().StartsWith(FieldQualifier.ToString()));
                        }
                        else
                        {
                            var startIdx = Array.IndexOf(_allLines, colLine) + 1;
                            var stopIdx = Array.IndexOf(_allLines, tableLines[tableIndex + 1]) - 1;
                            var dataLineCount = stopIdx - startIdx + 1;

                            dataLines = _allLines.Skip(startIdx).Take(dataLineCount).Where(
                                dl =>
                                    !string.IsNullOrEmpty(dl) &&
                                    !dl.Trim().StartsWith(CommentIndicator) &&
                                    dl.Trim().StartsWith(FieldQualifier.ToString()));
                        }

                        if (colLine != null)
                        {
                            colNames = colLine.Trim()
                                .Trim(FieldQualifier)
                                .Split(Delimiters, StringSplitOptions.None);

                            if (colNames.Any())
                            {
                                foreach (var dl in dataLines)
                                {
                                    dl.Trim()
                                        .TrimOnce(new char[]{'"', '\'', '0'});
                                    string[] lineSelected = dl.Trim()
                                        .TrimOnce(FieldQualifier)
                                        .Split(Delimiters, StringSplitOptions.None);
                                    var record = new Dictionary<string, string>();

                                    for (int index = 0; index < colNames.Count(); index++)
                                    {
                                        record.Add(colNames.ToArray()[index], lineSelected[index]);
                                    }

                                    tableData.Add(record);
                                }

                                _tables.Add(tableName, tableData);
                            }
                        }

                        tableIndex++;
                    }
                }

                return _tables;
            }
        }

    }
    public static class StringExtension
    {
        public static string TrimOnce(this string str, params char[] trimChars)
        {
            var rtn = Regex.Replace(str, String.Format("^[\\{0}]",String.Join(",\\",trimChars)), "");
            return Regex.Replace(rtn, String.Format("[\\{0}]$", String.Join(",\\", trimChars)), "");
        }
    }
}
