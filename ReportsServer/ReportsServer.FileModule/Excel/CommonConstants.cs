using System;

namespace ReportsServer.FileModule.Excel
{
    internal static class CommonConstants
    {
        public const int DefaultIntValue = -1;

        #region Constants for regex

        #region Regex group names
        public const string SheetNameRegexGr = "sheet";
        public const string StartColRegexGr = "sCol";
        public const string StartRowRegexGr = "sRow";
        public const string IsRangeRegexGr = "end";
        public const string EndColRegexGr = "eCol";
        public const string EndRowRegexGr = "eRow";
        #endregion

        #region Regex patterns
        public static readonly string PatternWithSheet = String.Format(
            @"^(?<{0}>[(A-z)(0-9)]+)\!\$(?<{1}>[(A-z)(0-9)]+)\$(?<{2}>[(0-9)]+)(?<{3}>$|(\:\$(?<{4}>[(A-z)(0-9)]+)\$(?<{5}>[(0-9)]+)))",
            SheetNameRegexGr, StartColRegexGr, StartRowRegexGr, IsRangeRegexGr, EndColRegexGr, EndRowRegexGr);
        public static readonly string PatternWithoutSheet = String.Format(
            @"^(?<{0}>[\$]{{0,1}}[(A-z)]+)(?<{1}>[\$]{{0,1}}[(0-9)]+)(?<{2}>$|(\:(?<{3}>[\$]{{0,1}}[(A-z)]+)(?<{4}>[\$]{{0,1}}[(0-9)]+)))",
            StartColRegexGr, StartRowRegexGr, IsRangeRegexGr, EndColRegexGr, EndRowRegexGr);
        #endregion

        #endregion

        #region Constants for working with Excel and template

        public const string ReferenceAttribute = "r";
        public const string ReferenceAttributeMerge = "ref";
        public const string TemplateSheetNameParam = "Sheet1";
        public const string DefaultSheetName = "Sheet";

        public const string CellValueBrace = "%";
        public const string FncBeginning = "f_";

        #region beginning of range names
        public const string ParamRange = "_paramValue";
        public const string ReadonlyRange = "_ro_";
        //range for first collection
        public const string FirstRowRange = "_Row";
        //range for second collection
        public const string SecondRowRange = "_Row2";
        #endregion
        
        #region Report param names
        public const string ParamSheetName = "SheetName";
        #endregion

        #endregion

    }
}
