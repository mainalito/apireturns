using apireturns.Data;
using apireturns.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;

namespace apireturns.NewFolder
{
    public class ExcelProcessingService
    {
        private readonly NPRA_LIVEContext _context;
        private readonly IConfiguration _configuration;


        // Constructor to inject the NPRA_LIVEContext
        // Constructor to inject both dependencies
        public ExcelProcessingService(NPRA_LIVEContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public NPRA_LIVEContext Context => _context;

        public async Task<ProcessResult> ProcessExcelFile(IFormFile file, ReturnSubmissionDto returnSubmission, string returnTemplateId)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "DataFiles");
            Directory.CreateDirectory(tempDir);

            var tempFilePath = Path.Combine(tempDir, $"{Guid.NewGuid()}_{file.FileName}");
            Console.WriteLine(JsonConvert.SerializeObject(tempFilePath, Formatting.Indented));

            //try
            //{
            // Save the file locally
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Load Excel workbook
            using var package = new ExcelPackage(new FileInfo(tempFilePath));
            var workbook = package.Workbook;

            // Validate Excel file
            var validationSheet = workbook.Worksheets["Validation"];
            if (validationSheet == null)
            {
                return new ProcessResult
                {
                    Success = false,
                    Message = "Missing 'Validation' sheet."
                };
            }

            //var dbTemplate = GetReturnTemplate(int.Parse(returnTemplateId));
            //if (dbTemplate == null)
            //{
            //    return new ProcessResult
            //    {
            //        Success = false,
            //        Message = $"ReturnTemplate ID {returnTemplateId} not found."
            //    };
            //}

            // Compare versions
            var excelVersion = validationSheet.Cells[3, 7].Text; // Assuming version in G3
            var excelIsValid = validationSheet.Cells[2, 7].Text; // Assuming version in G2
            var excelStatus = validationSheet.Cells[2, 6].Text; // Assuming version in F2

            Console.WriteLine(JsonConvert.SerializeObject(excelVersion, Formatting.Indented));
            Console.WriteLine(JsonConvert.SerializeObject(excelIsValid, Formatting.Indented));
            Console.WriteLine(JsonConvert.SerializeObject(excelStatus, Formatting.Indented));

            // Example usage of _context to get the ReturnTemplate by ID
            var returnTemplate = await Context.ReturnTemplates
                .FirstOrDefaultAsync(rt => rt.id == long.Parse(returnTemplateId));

            if (returnTemplate == null)
            {
                return new ProcessResult
                {
                    Success = false,
                    Message = $"No ReturnTemplate found with ID {returnTemplateId}."
                };
            }
            Console.WriteLine(JsonConvert.SerializeObject(returnTemplate, Formatting.Indented));


            if (returnTemplate.Version != excelVersion)
            {
                return new ProcessResult
                {
                    Success = false,
                    Message = "Version mismatch between template and database."
                };
            }

            if (!excelStatus.Equals("PASSED") && !excelStatus.Equals("TRUE"))
            {
                return new ProcessResult
                {
                    Success = false,
                    Message = "Ensure all validation checks have passed before uploading. "
                };
            }

            var returnSheet = await Context.ReturnSheets
                            .Where(rs => rs.ReturnId == returnSubmission.ReturnId)
                            .OrderByDescending(rs => rs.Sheet) // Sort by Sheet column in descending order
                            .FirstOrDefaultAsync();            // Get the first (highest) result


            if (returnSheet == null)
            {
                return new ProcessResult
                {
                    Success = false,
                    Message = $"No returnSheet found with ID {returnSubmission.ReturnId}."
                };
            }

            var numberOfSheetsInExcel = workbook.Worksheets.Count;
            Console.WriteLine(JsonConvert.SerializeObject(returnSheet.Sheet + 1, Formatting.Indented));
            Console.WriteLine(JsonConvert.SerializeObject(numberOfSheetsInExcel, Formatting.Indented));

            if (numberOfSheetsInExcel != returnSheet.Sheet + 1)
            {
                return new ProcessResult
                {
                    Success = false,
                    Message = "The uploaded template is incorrect. Please download and upload a new template"
                };
            }
            Console.WriteLine(JsonConvert.SerializeObject("Validation passed, proceeding with data processing...", Formatting.Indented));

            var returnSheets = await _context.ReturnSheets
                .Where(rs => rs.ReturnSheetTypeId == 2 && rs.ReturnId == returnSubmission.ReturnId)
                .ToListAsync();

            var modelReturnSubmission = new ReturnSubmission
            {
                EntityId = returnSubmission.EntityId,
                ReturnId = returnSubmission.ReturnId,
                Month = returnSubmission.Month,
                Quarter = returnSubmission.Quarter,
                Year = returnSubmission.Year,
                ReportPeriod = DateOnly.Parse(returnSubmission.ReportPeriod),
                FileName = returnSubmission.FileName,
                StatusId = returnSubmission.StatusId,
                CreatedBy = returnSubmission.CreatedBy,
                CreatedDate = DateOnly.FromDateTime(returnSubmission.CreatedDate),
            };

            long newSubmissionId = 0;

            if (returnSubmission.id == null)
            {
                newSubmissionId = await CreateReturnSubmissionAsync(modelReturnSubmission);

            }
            else
            {
                newSubmissionId = (long)returnSubmission.id;
            }
            var allInsertData = ProcessSheets(returnSubmission, newSubmissionId, package);

            // 2. Write data to CSV
            string csvFilePath = WriteDataToCsv(allInsertData);

            // 3. Bulk Insert from CSV
            try
            {
                await BulkInsertCsvAsync(csvFilePath);
                Console.WriteLine(newSubmissionId);
                return new ProcessResult
                {
                    Success = true,
                    Message = "File processed and stored successfully.",
                    return_submission_id = newSubmissionId,
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(newSubmissionId);
                return new ProcessResult
                {
                    Success = false,
                    Message = "Something went wrong" + ex.Message,
                
                };

            }
        }
            /// <summary>
            /// Parses a cell reference like "A2" into column="A" and row="2".
            /// </summary>
            private (string Column, int Row) ParseCellReference(string cellRef)
            {
                // E.g., cellRef = "A2" => ("A", 2)
                // or "BC15" => ("BC", 15)
                var match = System.Text.RegularExpressions.Regex.Match(cellRef, @"^([A-Z]+)(\d+)$");
                if (!match.Success)
                    throw new ArgumentException($"Invalid cell reference: {cellRef}");

                string columnLetters = match.Groups[1].Value;
                int rowNumber = int.Parse(match.Groups[2].Value);
                return (columnLetters, rowNumber);
            }
            private string WriteDataToCsv(List<ReturnSheetDatum> insertData)
            {
                // 1. Create a unique CSV file name
                string dataDirectory = @"C:\DataFiles";
                Directory.CreateDirectory(dataDirectory);


            string csvFileName = $"temp_data_{Guid.NewGuid()}.csv";

            string csvFilePath = Path.Combine(dataDirectory, csvFileName);
            Console.WriteLine(JsonConvert.SerializeObject("CSV FILE PATH" + csvFilePath, Formatting.Indented));


            using (var writer = new StreamWriter(csvFilePath))
                {
                    // 2. Write the header row
                    writer.WriteLine("id,SubmissionId,ReturnSheetId,MatrixId,RecordId,Data,Deleted,DeletedDate,CreatedDate,CreatedBy");

                    // 3. Write each record
                    foreach (var record in insertData)
                    {
                        string deletedDateString = record.DeletedDate.HasValue
                            ? record.DeletedDate.Value.ToString("yyyy-MM-dd HH:mm:ss")
                            : string.Empty;

                        // Convert `Deleted` to 0 or 1 for SQL bit column
                        int deletedBit = record.Deleted ? 1 : 0;

                        // Escape or quote Data if it can contain commas or special characters
                        string dataEscaped = record.Data.Contains(",")
                            ? $"\"{record.Data}\""
                            : record.Data;

                        writer.WriteLine($"{record.id},{record.SubmissionId},{record.ReturnSheetId},{record.MatrixId},{record.RecordId},{dataEscaped},{deletedBit},{deletedDateString},{record.CreatedDate:yyyy-MM-dd HH:mm:ss},{record.CreatedBy}");
                    }
                }

                return csvFilePath;
            }
            private async Task BulkInsertCsvAsync(string csvFilePath)
            {
                // Track start time
                var startTime = DateTime.Now;
                Console.WriteLine($"BULK INSERT started at {startTime:yyyy-MM-dd HH:mm:ss}");

                string connectionString = _configuration.GetConnectionString("MyDbContext");

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Using a large TIMEOUT if needed for huge datasets
                    string bulkInsertQuery = $@"
BULK INSERT ReturnSheetData
FROM '{csvFilePath.Replace("\\", "\\\\")}'
WITH (
    FIRSTROW = 2,
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    TABLOCK
);
";
                    using (var command = new SqlCommand(bulkInsertQuery, connection))
                    {
                        // Optionally set a command timeout for large inserts
                        command.CommandTimeout = 0; // no timeout
                        await command.ExecuteNonQueryAsync();
                    }
                }

                var endTime = DateTime.Now;
                Console.WriteLine($"BULK INSERT completed at {endTime:yyyy-MM-dd HH:mm:ss}");
                var duration = endTime - startTime;
                Console.WriteLine($"Time taken: {duration.TotalMinutes} minutes.");
            } 
        



        /// <summary>
        /// Converts a raw Excel value based on a DataTypeId.
        /// 1 = Text, 2 = Date, 3 = Number, 4 = Formula (example).
        /// </summary>
        private string ConvertValue(object rawValue, int dataTypeId, ExcelWorksheet worksheet, string cellRef)
        {
            if (rawValue == null) return string.Empty;

            switch (dataTypeId)
            {
                case 2: // Date
                        // If rawValue is an OLE Automation date (double in Excel)
                    if (double.TryParse(rawValue.ToString(), out double dateNumber))
                    {
                        // Convert from Excel serial date to C# DateTime
                        var dt = DateTime.FromOADate(dateNumber);
                        return dt.ToString("yyyy-MM-dd");
                    }
                    // If rawValue is already a date string
                    return rawValue.ToString();

                case 4: // Formula
                        // In EPPlus, we can get the calculated value:
                        // But only if the cell has a formula. Otherwise, rawValue is enough
                    var formulaCell = worksheet.Cells[cellRef];
                    return formulaCell?.Value?.ToString() ?? string.Empty;

                default:
                    // For text/number or others, cast to string.
                    return rawValue.ToString();
            }
        }

        public List<ReturnSheetDatum> ProcessSheets(
       ReturnSubmissionDto returnSubmissionDto,
       long submissionId,
       ExcelPackage package)
        {
            var sheets = _context.ReturnSheets
                .Where(rs => rs.ReturnId == returnSubmissionDto.ReturnId && rs.ReturnSheetTypeId == 2)
                .ToList();

            var allInsertData = new List<ReturnSheetDatum>();

            foreach (var sheet in sheets)
            {
                var sheetData = ProcessSingleSheet(sheet, submissionId, package, returnSubmissionDto);
                allInsertData.AddRange(sheetData);
            }

            return allInsertData; // Return everything
        }


        private List<ReturnSheetDatum> ProcessSingleSheet(
       ReturnSheet sheet,
       long submissionId,
       ExcelPackage package,
       ReturnSubmissionDto returnSubmissionDto)
        {
            int activeSheetIndex = (int)sheet.Sheet - 1;
            if (activeSheetIndex < 0 || activeSheetIndex >= package.Workbook.Worksheets.Count)
                throw new IndexOutOfRangeException($"Sheet index {activeSheetIndex} is invalid.");

            ExcelWorksheet activeSheet = package.Workbook.Worksheets[activeSheetIndex];

            // 1. Fetch matrix info from DB
            var matrixes = Context.ReturnSheetMatrices
                .Join(
                    _context.ReturnSheetRows,
                    rsm => rsm.RowId,
                    r => r.id,
                    (rsm, r) => new { Matrix = rsm, Row = r }
                )
                .Where(joined => joined.Row.ReturnSheetId == sheet.id)
                .Select(joined => joined.Matrix)
                .ToList();

            // 2. Build a dictionary: { matrixId -> dataTypeId }
            var dataTypes = matrixes.ToDictionary(m => m.id, m => m.DataTypeId);

            // 3. Group matrixes by RowId
            var matrixGroup = matrixes
                .GroupBy(m => m.RowId)
                .ToDictionary(g => g.Key, g => g.ToList());

            if (!matrixGroup.Any())
                return new List<ReturnSheetDatum>();  // No data for this sheet

            // 4. Identify the "startFrom" matrix
            var firstGroup = matrixGroup.First().Value;
            var startFrom = firstGroup[0];
            var (startColLetters, startRowNumber) = ParseCellReference(startFrom.Cell);

            // Build a dictionary for RowId => {Cell => Id}
            var rowMatrixes = matrixGroup[startFrom.RowId];
            var cells = rowMatrixes.ToDictionary(m => m.Cell, m => m.id);

            // Also store the startFrom's ID in a special key like "A*"
            var starKey = $"{startColLetters}*";
            if (!cells.ContainsKey(starKey))
                cells.Add(starKey, startFrom.id);

            // 5. Determine the range to iterate
            int highestRow = activeSheet.Dimension.End.Row;
            int highestColumn = activeSheet.Dimension.End.Column;

            long recordId = 1;

            // **List** for collecting ReturnSheetDatum records
            var sheetInsertData = new List<ReturnSheetDatum>();

            // 6. Iterate each row
            for (int rowIdx = startRowNumber; rowIdx <= highestRow; rowIdx++)
            {
                // Check if the row is blank
                bool blankRow = false;
                var firstCellValue = activeSheet.Cells[$"{startColLetters}{rowIdx}"].Value;
                if (firstCellValue == null || string.IsNullOrWhiteSpace(firstCellValue.ToString()))
                    blankRow = true;

                if (!blankRow)
                {
                    // 7. Iterate columns
                    for (int colIdx = 1; colIdx <= highestColumn; colIdx++)
                    {
                        string colLetters = OfficeOpenXml.ExcelCellAddress.GetColumnLetter(colIdx);
                        string cellRef = $"{colLetters}{rowIdx}";
                        var rawValue = activeSheet.Cells[cellRef].Value;
                        if (rawValue == null) continue;

                        // Determine matrixId
                        long matrixId;
                        if (cellRef.Equals(startFrom.Cell, StringComparison.OrdinalIgnoreCase))
                        {
                            matrixId = startFrom.id;
                        }
                        else
                        {
                            var starCellKey = $"{colLetters}*";
                            matrixId = cells.ContainsKey(starCellKey) ? cells[starCellKey] : 0;
                        }

                        // Determine dataType
                        int dataTypeId = dataTypes.ContainsKey(matrixId) ? (int)dataTypes[matrixId] : 1;
                        var processedValue = ConvertValue(rawValue, dataTypeId, activeSheet, cellRef);
                        processedValue = processedValue.Replace("\"", "").Replace(",", "");

                        // Accumulate record
                        var dataRecord = new ReturnSheetDatum
                        {
                            SubmissionId = submissionId,
                            ReturnSheetId = sheet.id,
                            MatrixId = matrixId,
                            RecordId = recordId,
                            Data = processedValue,
                            Deleted = false,
                            DeletedDate = null,
                            CreatedDate = DateTime.Now,
                            CreatedBy = returnSubmissionDto.CreatedBy
                        };

                        sheetInsertData.Add(dataRecord);
                    }

                    recordId++;
                }
            }

            // Return the collected data for this sheet
            return sheetInsertData;
        }


        public async Task<long> CreateReturnSubmissionAsync(ReturnSubmission returnSubmission)
        {
            // Add the return submission to the DbContext
            _context.ReturnSubmissions.Add(returnSubmission);

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return the newly inserted ID
            return returnSubmission.id;
        }

        private string ManipulateValue(object cellValue, long dataTypeId)
        {
            switch (dataTypeId)
            {
                case 1: // Text
                    return cellValue.ToString();

                case 2: // Date
                    if (DateTime.TryParse(cellValue.ToString(), out DateTime dateValue))
                    {
                        return dateValue.ToString("yyyy-MM-dd"); // Format as "YYYY-MM-DD"
                    }
                    throw new FormatException($"Invalid date format: {cellValue}");

                case 3: // Number
                    if (decimal.TryParse(cellValue.ToString(), out decimal numberValue))
                    {
                        return numberValue.ToString(); // Keep as a number string
                    }
                    throw new FormatException($"Invalid number format: {cellValue}");

                case 4: // Formula
                        // Handle formula logic here
                        // Example: Simulate formula result as-is
                    return cellValue.ToString();

                default:
                    throw new NotSupportedException($"Unsupported DataTypeId: {dataTypeId}");
            }
        }


      
    }
}



