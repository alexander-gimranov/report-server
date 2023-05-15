using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using NPOI.SS.Formula;
using NPOI.SS.Formula.PTG;
using ReportsServer.Core;
using ReportsServer.FileModule.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DefinedName = DocumentFormat.OpenXml.Spreadsheet.DefinedName;

namespace ReportsServer.FileModule.Excel
{
    internal class ExcelProcessor: IFileProcessor
    {
        public string GenerateFromCollections<T>(string template, IReadOnlyDictionary<string, object> reportParams, params IEnumerable<T>[] collections)
        {
            if (collections == null) throw new ArgumentNullException(nameof(collections));
            var list = collections.ToList();
            if (list.Count == 0 || list.Count > 2) throw new ArgumentException("the number of arguments must be between 1 and 2 ");
            var first = list[0];
            var second = list.Count > 1 ? list[1] : null;
            return GenerateFromCollection(template, reportParams, first, second);
        }

        public string GenerateFromCollections<T>(IReadOnlyDictionary<string, object> reportParams, params IEnumerable<T>[] collections)
        {
            throw new NotImplementedException();
        }

        public string GenerateFromCollections<T>(params IEnumerable<T>[] collections)
        {
            throw new NotImplementedException();
        }

        public string GenerateFromReport(IPrepareReport report)
        {
            return GenerateFromCollections(report.Template, report.Params, report.Collections.ToArray());
        }

        public Task<string> GenerateFromReportAsync(IPrepareReport report)
        {
            throw new NotImplementedException();
        }

        public string GenerateFromCollection<T>(IEnumerable<T> collection, string template)
        {
            return GenerateFromCollection(template, null, collection, null);
        }

        /// <summary>
        /// Generate Excel file with data from collection and/or secondCollection
        /// </summary>
        /// <typeparam name="T">Any type of objects</typeparam>
        /// <param name="template">File path of template</param>
        /// <param name="reportParams">Dictionary with values whose keys are described in the template cell </param>
        /// <param name="collection">Collection with objects whose item write to '_Row' range</param>
        /// <param name="secondCollection">Collection with objects whose item write to '_Row' range</param>
        /// <returns>Excel file path of the current user's temporary folder</returns>
        public string GenerateFromCollection<T>(string template, IReadOnlyDictionary<string, object> reportParams, IEnumerable<T> collection, IEnumerable<T> secondCollection)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            if (!File.Exists(template)) throw new FileNotFoundException(String.Format("File {0} not found", template));

            var templateExt = Path.GetExtension(template).ToLower();
            if (!File.Exists(template)) throw new FileNotFoundException(String.Format("Template file {0} not found", template));

            var tempFile = Path.GetTempPath() + Guid.NewGuid() + templateExt;
            File.Copy(template, tempFile, true);

            using (var writeDoc = SpreadsheetDocument.Open(tempFile, true))
            {
                #region Initializing variables

                var workbookPart = writeDoc.WorkbookPart;

                var flOffset1 = false;
                var flOffset2 = false;
                var list1 = collection.ToList();
                var offset1 = list1.Count - 1;
                IList<T> list2 = null;
                var offset2 = CommonConstants.DefaultIntValue;
                if (secondCollection != null)
                {
                    list2 = secondCollection.ToList();
                    offset2 = list2.Count - 1;
                }

                var definedNames = BuildDefinedNamesTable(workbookPart.Workbook.DefinedNames);
                var paramRanges = definedNames.Where(i => i.Key.StartsWith(CommonConstants.ParamRange)).Select(i => i.Value).ToList();
                var passRanges = definedNames.Where(i => i.Key.StartsWith(CommonConstants.ReadonlyRange)).Select(i => i.Value).ToList();

                ExcelDefinedName defRow1;
                ExcelDefinedName defRow2;
                var startRow1 = CommonConstants.DefaultIntValue;
                var startRow2 = CommonConstants.DefaultIntValue;

                definedNames.TryGetValue(CommonConstants.FirstRowRange, out defRow1);
                definedNames.TryGetValue(CommonConstants.SecondRowRange, out defRow2);

                if (defRow1 != null)
                {
                    startRow1 = defRow1.StartRow;
                }
                if (defRow2 != null)
                {
                    startRow2 = defRow2.StartRow;
                }
                string sheetName = null;
                if (defRow1 != null)
                {
                    sheetName = defRow1.SheetName;
                }
                else if (defRow2 != null)
                {
                    sheetName = defRow2.SheetName;
                }
                WorksheetPart readPart;
                if (sheetName != null)
                {
                    readPart = GetWorksheetPart(workbookPart, sheetName);
                }
                else
                {
                    readPart = workbookPart.WorksheetParts.Last();
                }

                Row row4first;
                Row row4second;
                if (!TryGetRowByIndex(ref readPart, startRow1, out row4first))
                {
                    throw new NullReferenceException(string.Format("not found {0} row in sheet {1}", startRow1, sheetName));
                }
                TryGetRowByIndex(ref readPart, startRow1, out row4second);

                var writePart = workbookPart.AddNewPart<WorksheetPart>();

                #endregion

                using (var reader = OpenXmlReader.Create(readPart))
                {
                    using (var writer = OpenXmlWriter.Create(writePart))
                    {
                        while (reader.Read())
                        {
                            if ((startRow1 > CommonConstants.DefaultIntValue ||
                                 startRow2 > CommonConstants.DefaultIntValue) &&
                                reader.ElementType == typeof(SheetData))
                            {
                                if (reader.IsEndElement)
                                    continue;

                                #region Write SheetData element

                                writer.WriteStartElement(new SheetData());

                                var currentRow = string.Empty;
                                var current = CommonConstants.DefaultIntValue;
                                var offset = 0;
                                // read first row from template and copy into the new sheet
                                // start read sheetData
                                while (reader.Read() && reader.ElementType != typeof(SheetData))
                                {
                                    if (reader.IsStartElement &&
                                        (reader.ElementType == typeof(Row) || reader.ElementType == typeof(Cell) ||
                                         reader.ElementType == typeof(CellFormula)))
                                    {
                                        if (reader.ElementType == typeof(Row))
                                        {
                                            current = GetRowIndex(reader);
                                            if (!flOffset1 && current == startRow1)
                                            {
                                                currentRow = current.ToString();
                                                flOffset1 = true;
                                                offset = offset1;
                                                PassElemetAndChilds(reader, typeof(Row));

                                                if (row4first != null)
                                                {
                                                    WriteRowsFromList(writer, workbookPart, definedNames, row4first,
                                                        startRow1,
                                                        list1);
                                                }
                                                continue;
                                            }
                                            if (!flOffset2 &&
                                                (startRow2 > CommonConstants.DefaultIntValue && current == startRow2))
                                            {
                                                currentRow = current.ToString();
                                                flOffset2 = true;
                                                offset = offset2;
                                                if (flOffset1)
                                                {
                                                    offset += offset1;
                                                }
                                                PassElemetAndChilds(reader, typeof(Row));
                                                if (row4second != null)
                                                {
                                                    var startIndex = startRow2;
                                                    var itemIndexOffset = 1;
                                                    if (flOffset1)
                                                    {
                                                        startIndex += offset1;
                                                        if (list1 != null)
                                                        {
                                                            itemIndexOffset += list1.Count;
                                                        }
                                                    }
                                                    WriteRowsFromList(writer, workbookPart, definedNames,
                                                        row4second,
                                                        startIndex,
                                                        list2, itemIndexOffset);
                                                }
                                                continue;
                                            }
                                        }
                                        if (reader.ElementType == typeof(Cell) &&
                                            (paramRanges != null && paramRanges.Count > 0) &&
                                            TrySetParamValueToCell(reader, writer, paramRanges, readPart, reportParams,
                                                current, offset))
                                        {
                                            continue;
                                        }
                                        if (flOffset1 || flOffset2)
                                        {
                                            ReplaceRowIndexAndWrite(reader, writer, offset, ref currentRow, passRanges);
                                            continue;
                                        }
                                    }

                                    CopyOthersElement(reader, writer);
                                }
                                // end read sheetData
                                // write end SheetData
                                writer.WriteEndElement();

                                #endregion
                            }
                            else
                            {
                                if (TryWriteMergeCell(reader, writer, startRow1, startRow2, flOffset1, offset1,
                                    flOffset2,
                                    offset2)) continue;

                                CopyOthersElement(reader, writer);

                            }
                        }
                        var newSheet = new Sheet()
                        {
                            Id = workbookPart.GetIdOfPart(writePart),
                            SheetId = 2,
                            Name = CommonConstants.DefaultSheetName
                        };
                        if (TryGetParamValue(reportParams, CommonConstants.ParamSheetName, out var pSheetName))
                        {
                            newSheet.Name = pSheetName;
                        }

                        workbookPart.Workbook.Sheets.AppendChild(newSheet);

                    }
                }

                // Remove the template sheet reference from the workbook.
                if (TryGetSheetByName(workbookPart, CommonConstants.TemplateSheetNameParam, out var sheet))
                {
                    sheet.Remove();
                }

                // Delete the template worksheet part.
                workbookPart.DeletePart(readPart);

                workbookPart.Workbook.CalculationProperties.ForceFullCalculation = true;
                workbookPart.Workbook.CalculationProperties.FullCalculationOnLoad = true;

            }
            return tempFile;
        }

        private bool TryGetParamValue(IReadOnlyDictionary<string, object> reportParams, string name, out string value)
        {
            value = null;
            if (reportParams != null && reportParams.TryGetValue(name, out var param))
            {
                value = param.ToString();
            }
            return !string.IsNullOrEmpty(value);
        }

        private bool TryWriteMergeCell(OpenXmlReader reader, OpenXmlWriter writer, int startRow1, int startRow2,
            bool flOffset1, int offset1, bool flOffset2, int offset2)
        {
            if (reader.ElementType == typeof(MergeCell) && TryGetAttributes(reader, out var attributes) && attributes.Keys.Contains(CommonConstants.ReferenceAttributeMerge))
            {
                var attr = attributes[CommonConstants.ReferenceAttributeMerge];
                var df = new ExcelDefinedName(null, attr.Value, false);
                if (df != null)
                {
                    if ((df.StartRow > startRow1 && df.StartRow < startRow2) && flOffset1)
                    {
                        attr.Value = $"{df.StartColumn}{(df.StartRow + offset1)}:{df.EndColumn}{(df.EndRow + offset1)}";
                    }
                    if ((df.StartRow > startRow2) && flOffset2)
                    {
                        attr.Value =
                            $"{df.StartColumn}{(df.StartRow + offset2 + (flOffset1 ? offset1 : 0))}:{df.EndColumn}{(df.EndRow + offset2 + (flOffset1 ? offset1 : 0))}";
                    }
                    attributes[CommonConstants.ReferenceAttributeMerge] = attr;
                    writer.WriteStartElement(new MergeCell(), attributes.Values);
                    return true;
                }
            }
            return false;
        }

        private bool TrySetParamValueToCell(OpenXmlReader reader, OpenXmlWriter writer,
            IList<ExcelDefinedName> paramRanges, WorksheetPart sheetPart,
            IReadOnlyDictionary<string, object> reportParams, int currentRow, int offset)
        {
            if (reportParams != null)
            {
                if (reader.IsStartElement && reader.ElementType == typeof(Cell) &&
                    TryGetAttributes(reader, out var attributes) &&
                    attributes.TryGetValue(CommonConstants.ReferenceAttribute, out var attrReference))
                {
                    foreach (var passRange in paramRanges)
                    {
                        if (passRange.Contains(attrReference.Value))
                        {
                            var orgCell = sheetPart.Worksheet.GetFirstChild<SheetData>().Descendants<Cell>()
                                .FirstOrDefault(c => c.CellReference.Value.Equals(attrReference.Value));
                            if (orgCell != null &&
                                TryGetCellValue((WorkbookPart)sheetPart.GetParentParts().First(), orgCell, out var value))
                            {
                                var newCell = new Cell();
                                ShiftReference(ref attributes, currentRow, offset, typeof(Cell));
                                newCell.SetAttributes(attributes.Values);
                                newCell.DataType = CellValues.String;
                                foreach (var param in reportParams)
                                {
                                    var str = CommonConstants.CellValueBrace + param.Key +
                                              CommonConstants.CellValueBrace;
                                    value = value.Replace(str, param.Value.ToString());
                                }
                                newCell.Append(new CellValue(value));
                                writer.WriteElement(newCell);
                                PassElemetAndChilds(reader, typeof(Cell));
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static IDictionary<string, ExcelDefinedName> BuildDefinedNamesTable(DefinedNames names)
        {
            //Build a list. 
            var definedNames = new Dictionary<string, ExcelDefinedName>();
            foreach (var openXmlElement in names)
            {
                if (!(openXmlElement is DefinedName name)) continue;
                var dn = new ExcelDefinedName(name.Name, name.InnerText, true);
                if (dn.StartColumn != null) definedNames.Add(name.Name, dn);
            }
            return definedNames;
        }

        private WorksheetPart GetWorksheetPart(WorkbookPart workbookPart, string sheetName)
        {
            var sheetId = string.Empty;
            if (!string.IsNullOrEmpty(sheetName))
            {
                if (TryGetSheetByName(workbookPart, sheetName, out var sheet))
                {
                    sheetId = sheet.Id;
                }
            }
            return !string.IsNullOrEmpty(sheetId)
                ? (WorksheetPart)workbookPart.GetPartById(sheetId)
                : null;
        }

        private bool TryGetSheetByName(WorkbookPart workbookPart, string sheetName, out Sheet sheet)
        {
            sheet = workbookPart.Workbook.Descendants<Sheet>()
                .FirstOrDefault(s => s.Name.Value.Equals(sheetName));
            return sheet != null;
        }

        private bool TryGetRowByIndex(ref WorksheetPart sheetPart, int rowIndex, out Row row)
        {
            row = null;
            if (sheetPart != null)
            {
                row = sheetPart.Worksheet.GetFirstChild<SheetData>().Descendants<Row>()
                    .FirstOrDefault(p => p.RowIndex == rowIndex);
            }
            return row != null;
        }

        private IDictionary<string, OpenXmlAttribute> GetElementAttributes(OpenXmlElement element)
        {
            return element.GetAttributes().ToDictionary(a => a.LocalName);
        }

        private bool TryGetAttributes(OpenXmlReader reader, out IDictionary<string, OpenXmlAttribute> attributes)
        {
            attributes = null;
            if (reader.IsStartElement && reader.HasAttributes)
            {
                attributes = new List<OpenXmlAttribute>(reader.Attributes).ToDictionary(a => a.LocalName);
            }
            return attributes != null;
        }

        private bool TryGetAttributesAndRefence(OpenXmlReader reader, out IDictionary<string, OpenXmlAttribute> attributes, out OpenXmlAttribute attribute)
        {
            if (TryGetAttributes(reader, out attributes) && attributes.Keys.Contains(CommonConstants.ReferenceAttribute))
            {
                attribute = attributes[CommonConstants.ReferenceAttribute];
                return true;
            }
            return false;
        }

        private int GetRowIndex(OpenXmlReader reader)
        {
            var value = reader.Attributes.FirstOrDefault(a => a.LocalName.Equals(CommonConstants.ReferenceAttribute)).Value ?? null;
            int rowIndex = CommonConstants.DefaultIntValue;
            if (value != null) Int32.TryParse(value, out rowIndex);
            return rowIndex;
        }

        private string ParseFormula(string formula, int rowOffset = 0, int colOffset = 0)
        {
            // parse formula
            Ptg[] ptgs = FormulaParser.Parse(formula, null, FormulaType.Cell, CommonConstants.DefaultIntValue);
            // re-calculate cell references
            foreach (Ptg ptg in ptgs)
            {
                if (ptg is RefPtgBase) //base class for cell reference "things"
                {
                    RefPtgBase res = (RefPtgBase)ptg;
                    if (res.IsRowRelative)
                    {
                        res.Row = res.Row + rowOffset;
                    }
                    if (res.IsColRelative)
                    {
                        res.Column = res.Column + colOffset;
                    }
                }
                else if (ptg is AreaPtg)
                {
                    AreaPtg res = (AreaPtg)ptg;
                    if (res.IsFirstRowRelative)
                    {
                        res.FirstRow = res.FirstRow + rowOffset;
                    }
                    if (res.IsFirstColRelative)
                    {
                        res.FirstColumn = res.FirstColumn + colOffset;
                    }

                    if (res.IsLastRowRelative)
                    {
                        res.LastRow = res.LastRow + rowOffset;
                    }
                    if (res.IsLastColRelative)
                    {
                        res.LastColumn = res.LastColumn + colOffset;
                    }
                }
                else
                {
                    // TODO : handle others Types
                }
            }
            return FormulaRenderer.ToFormulaString(null, ptgs);
        }

        private void ReplaceReference(ref IDictionary<string, OpenXmlAttribute> attributes, int orgRowIndex, int newRowIndex, Type elementType)
        {
            if (attributes.Keys.Contains(CommonConstants.ReferenceAttribute))
            {
                var attribute = attributes[CommonConstants.ReferenceAttribute];
                if (elementType == typeof(Cell))
                {
                    attribute.Value = attribute.Value.Replace(orgRowIndex.ToString(), newRowIndex.ToString());
                }
                if (elementType == typeof(Row))
                {
                    attribute.Value = newRowIndex.ToString();
                }
                attributes[CommonConstants.ReferenceAttribute] = attribute;
            }
        }

        private void ReplaceReference(ref IDictionary<string, OpenXmlAttribute> attributes, UInt32Value orgRowIndex, UInt32Value newRowIndex, Type elementType)
        {
            if (int.TryParse(orgRowIndex.ToString(), out var oldRowIndex) && int.TryParse(newRowIndex.ToString(), out var destRowIndex))
                ReplaceReference(ref attributes, oldRowIndex, destRowIndex, elementType);
        }


        private void ShiftReference(ref IDictionary<string, OpenXmlAttribute> attributes, int currentRow, int offset, Type elementType)
        {
            int newRowIndex = currentRow + offset;
            ReplaceReference(ref attributes, currentRow, newRowIndex, elementType);
        }

        private void ReplaceRowIndexAndWrite(OpenXmlReader reader, OpenXmlWriter writer, int offset,
            ref string currentRow, IList<ExcelDefinedName> passRanges)
        {
            if (reader.ElementType == typeof(CellFormula) && reader.IsStartElement)
            {
                string formula = reader.GetText();
                formula = ParseFormula(formula, offset);
                writer.WriteStartElement(new CellFormula());
                writer.WriteString(formula);
            }
            else if (TryGetAttributesAndRefence(reader, out var attributes, out var attr))
            {
                int current;
                if (reader.ElementType == typeof(Cell))
                {
                    int.TryParse(currentRow, out current);
                    ShiftReference(ref attributes, current, offset, reader.ElementType);
                    if (!IsReadonlyElement(passRanges, attr.Value, reader.ElementType))
                        writer.WriteStartElement(new Cell(), attributes.Values);
                    else PassElemetAndChilds(reader, reader.ElementType);
                }
                else
                {
                    currentRow = attr.Value;
                    int.TryParse(attr.Value, out current);
                    ShiftReference(ref attributes, current, offset, reader.ElementType);
                    writer.WriteStartElement(new Row(), attributes.Values);
                }
            }
        }

        private void PassElemetAndChilds(OpenXmlReader reader, Type endElementType)
        {
            while (reader.Read())
            {
                if (reader.ElementType == endElementType && reader.IsEndElement)
                    break;
            }
        }

        private bool IsReadonlyElement(IList<ExcelDefinedName> passRanges, string reference, Type elementType)
        {
            if (passRanges != null)
                foreach (var passRange in passRanges)
                {
                    if (elementType == typeof(Cell))
                    {
                        return passRange.Contains(reference);
                    }
                }
            return false;
        }

        private void CopyOthersElement(OpenXmlReader reader, OpenXmlWriter writer)
        {
            if (reader.IsStartElement)
            {
                writer.WriteStartElement(reader);

                // this bit is needed to get cell values
                if (reader.ElementType.IsSubclassOf(typeof(OpenXmlLeafTextElement)))
                {
                    writer.WriteString(reader.GetText());
                }
            }
            else if (reader.IsEndElement)
            {
                writer.WriteEndElement();
            }
        }

        private void WriteRowsFromList<T>(OpenXmlWriter writer, WorkbookPart workbookPart, IDictionary<string, ExcelDefinedName> definedNames, Row orgRow, int startIndex, IList<T> collection, int itemIndexOffset = 1)
        {
            if (orgRow != null && collection != null)
            {
                for (var i = 0; i < collection.Count; i++)
                {
                    var destRow = new Row() { RowIndex = (uint)(startIndex + i) };
                    var attributes = GetElementAttributes(orgRow);
                    ReplaceReference(ref attributes, orgRow.RowIndex, (uint)(startIndex + i), typeof(Row));

                    writer.WriteStartElement(destRow, attributes.Values);
                    WriteCells(writer, workbookPart, definedNames,
                        orgRow.Elements<Cell>(),
                        Int32.Parse(orgRow.RowIndex.ToString()), (startIndex + i),
                        (i + itemIndexOffset), collection[i]);
                    writer.WriteEndElement();
                }
            }
        }

        private void WriteCells<T>(OpenXmlWriter writer, WorkbookPart workbookPart, IDictionary<string, ExcelDefinedName> definedNames,
            IEnumerable<Cell> cells, int orgRowIndex, int destRowIndex, int itemIndex,
            T data)
        {
            foreach (var orgCell in cells)
            {
                var  attributes = GetElementAttributes(orgCell);
                ReplaceReference(ref attributes, orgRowIndex, destRowIndex, typeof(Cell));
                Cell newCell = null;
                if (TryGetCellValue(workbookPart, orgCell, out var cellValue))
                {
                    newCell = GetCellByValue(workbookPart, definedNames, cellValue, ref attributes, itemIndex, data);
                }
                if (orgCell.CellFormula != null)
                {
                    var formula = orgCell.CellFormula.InnerText;
                    formula = ParseFormula(formula, (destRowIndex - orgRowIndex));
                    newCell = new Cell();
                    newCell.SetAttributes(attributes.Values);
                    newCell.CellFormula = new CellFormula(formula);
                }
                if (newCell == null)
                {
                    newCell = (Cell)orgCell.Clone();
                    newCell.SetAttributes(attributes.Values);
                }
                writer.WriteElement(newCell);
            }
        }

        private Cell GetCellByValue<T>(WorkbookPart workbookPart, IDictionary<string, ExcelDefinedName> definedNames, string cellValue, ref IDictionary<string, OpenXmlAttribute> attributes, int itemIndex, T data)
        {
            #region names of functions used in the template
            const string fncIterator = "iterator";
            const string fncGetTimeOffsCell = "timeoff";
            #endregion
            if (!String.IsNullOrEmpty(cellValue) && cellValue.StartsWith(CommonConstants.CellValueBrace) && cellValue.EndsWith(CommonConstants.CellValueBrace))
            {
                string value = cellValue.Substring(1, cellValue.Length - 2);

                string fvalue = null;
                Cell newCell;
                EnumValue<CellValues> cellDataType = null;
                if (value.ToLower().StartsWith(CommonConstants.FncBeginning))
                {
                    // todo: extract to method
                    if (value.ToLower().StartsWith(CommonConstants.FncBeginning +fncGetTimeOffsCell))
                    {
                        value = value.Substring(10, value.Length - 11);
                        if (value.StartsWith(CommonConstants.CellValueBrace) && value.EndsWith(CommonConstants.CellValueBrace))
                        {
                            value = value.Substring(1, value.Length - 2);
                            TryGetValueFromObject(data, value, out fvalue);
                        }
                        newCell = null;
                        if (fvalue != null)
                        {
                            if (TryGetTimeOffsCell(workbookPart, definedNames, fvalue, out newCell))
                                newCell = (Cell)newCell.Clone();
                        }
                        if (newCell != null)
                        {
                            if (attributes.ContainsKey(CommonConstants.ReferenceAttribute))
                            {
                                newCell.CellReference.Value = attributes[CommonConstants.ReferenceAttribute].Value;
                            }
                            return newCell;
                        }
                    }
                }
                else
                {
                    if (value.ToLower().Equals(fncIterator))
                    {
                        fvalue = itemIndex.ToString();
                        cellDataType = CellValues.Number;
                    }
                    else
                    {
                        double d;
                        if (TryGetValueFromObject(data, value, out fvalue) && double.TryParse(fvalue, out d))
                        {
                            cellDataType = CellValues.Number;
                        }
                    }
                }
                newCell = new Cell();
                newCell.SetAttributes(attributes.Values);
                newCell.DataType = cellDataType ?? CellValues.String;
                if (fvalue != null) newCell.Append(new CellValue(fvalue));

                return newCell;
            }
            return null;
        }

        private bool TryGetValueFromObject<T>(T data, string field, out string value)
        {
            if (data is IReadOnlyDictionary<string, object>)
            {
                return TryGetValueFromExpandoObject(data as IReadOnlyDictionary<string, object>, field, out value);
            }
            else
            {
                return TryGetPropertyValueFromObject(data, field, out value);
            }
        }
        private bool TryGetValueFromExpandoObject(IReadOnlyDictionary<string, object> data, string field, out string value)
        {
            value = null;
            if (data.TryGetValue(field, out var obj))
            {
                value = obj?.ToString();
            }
            return !string.IsNullOrEmpty(value);
        }

        private bool TryGetPropertyValueFromObject<T>(T data, string propertyName, out string propertyValue)
        {
            propertyValue = null;
            var property = data.GetType().GetProperty(propertyName);
            if (property != null)
            {
                var value = property.GetValue(data);
                if (value != null)
                {
                    propertyValue = value.ToString();
                }
            }
            return !string.IsNullOrEmpty(propertyValue);
        }

        private bool TryGetTimeOffsCell(WorkbookPart workbookPart, IDictionary<string, ExcelDefinedName> definedNames, string timeoff, out Cell timeoffCell)
        {
            // todo: extract to helper
            // search for a range whose name starts with '_t'
            timeoffCell = null;
            if (definedNames.TryGetValue(string.Format("_t{0}", timeoff.Replace("-", "0")), out var defCell))
            {
                var wsPart = GetWorksheetPart(workbookPart, defCell.SheetName);
                if (TryGetRowByIndex(ref wsPart, defCell.StartRow, out var row))
                {
                    timeoffCell = row.Elements<Cell>()
                        .FirstOrDefault(
                            c => c.CellReference.Value.Equals(
                                string.Format("{0}{1}", defCell.StartColumn, defCell.StartRow)
                            )
                        );
                }
            }
            return timeoffCell != null;
        }

        private bool TryGetCellValue(WorkbookPart workbookPart, Cell cell, out string cellValue)
        {
            cellValue = null;
            if (cell.DataType != null)
            {
                switch ((CellValues)cell.DataType)
                {
                    case CellValues.SharedString:
                        if (int.TryParse(cell.InnerText, out var id))
                        {
                            var item = GetSharedStringItemById(workbookPart, id);
                            if (item != null)
                            {
                                if (item.Text != null)
                                {
                                    cellValue = item.Text.Text;
                                }
                                else if (!string.IsNullOrEmpty(item.InnerText))
                                {
                                    cellValue = item.InnerText;
                                }
                                else
                                {
                                    cellValue = item.InnerXml;
                                }
                            }
                        }
                        break;
                    case CellValues.String:
                        cellValue = cell.InnerText ?? cell.InnerXml;
                        break;
                }
            }
            return cellValue != null;
        }

        private SharedStringItem GetSharedStringItemById(WorkbookPart workbookPart, int id)
        {
            return workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAtOrDefault(id);
        }

    }
}
