using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ReportsServer.FileModule.Excel
{
    internal class ExcelDefinedName
    {
        private static readonly Regex RexRangeWithSheet = new Regex(CommonConstants.PatternWithSheet, RegexOptions.Compiled);

        private static readonly Regex RexRange = new Regex(CommonConstants.PatternWithoutSheet, RegexOptions.Compiled);

        public ExcelDefinedName(string key, string reference, bool repalceEnd = true)
        {
            // If to contain ! then expected Sheet1!A1:B1, else A1:B1
            var match = reference.Contains("!") ? RexRangeWithSheet.Match(reference) : RexRange.Match(reference);
            if (!match.Success) return;

            var sheetName = match.Groups[CommonConstants.SheetNameRegexGr].Value;
            var startCol = match.Groups[CommonConstants.StartColRegexGr].Value;
            int.TryParse(match.Groups[CommonConstants.StartRowRegexGr].Value, out var startRow);
            var endCol = string.Empty;
            var endRow = CommonConstants.DefaultIntValue;
            if (repalceEnd)
            {
                endCol = startCol;
                endRow = startRow;
            }
            if (!string.IsNullOrEmpty(match.Groups[CommonConstants.IsRangeRegexGr].Value))
            {
                endCol = match.Groups[CommonConstants.EndColRegexGr].Value;
                int.TryParse(match.Groups[CommonConstants.EndRowRegexGr].Value, out endRow);
            }
            Key = key;
            SheetName = sheetName;
            StartColumn = startCol;
            StartRow = startRow;
            EndColumn = endCol;
            EndRow = endRow;
        }

        public string Key { get; set; }
        public string SheetName { get; set; }
        public string StartColumn { get; set; }
        public int StartRow { get; set; }
        public string EndColumn { get; set; }
        public int EndRow { get; set; }

        public bool Contains(string reference)
        {
            // todo: need refactor
            const char startCharInColName = 'A';
            const char endCharInColName = 'Z';
            IList<string> cells = new List<string>();
            var endChar = endCharInColName;
            if (!(EndColumn.Length > 1 && StartColumn.Length == 1))
            {
                endChar = EndColumn[0];
            }
            if (StartColumn.Length == 1)
                for (var i = StartColumn[0]; i <= endChar; i++)
                {
                    for (var k = StartRow; k <= EndRow; k++)
                    {
                        cells.Add(String.Format("{0}{1}", i, k));
                    }
                }
            if (EndColumn.Length > 1)
            {
                var startChar = startCharInColName;
                if (StartColumn.Length > 1)
                {
                    startChar = StartColumn[1];
                }

                for (var i = startChar; i <= EndColumn[0]; i++)
                for (var j = startChar; j <= EndColumn[1]; j++)
                {
                    for (var k = StartRow; k <= EndRow; k++)
                    {
                        cells.Add(String.Format("{0}{1}{2}", i, j, k));
                    }
                }
            }
            return cells.Contains(reference);
        }
    }
}
