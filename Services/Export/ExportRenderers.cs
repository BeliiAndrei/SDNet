using System.Globalization;
using DocumentFormat.OpenXml.Packaging;
using QuestPDF.Fluent;
using Oxml = DocumentFormat.OpenXml;
using OxmlSheet = DocumentFormat.OpenXml.Spreadsheet;
using OxmlWord = DocumentFormat.OpenXml.Wordprocessing;
using QuestColors = QuestPDF.Helpers.Colors;
using QuestDocument = QuestPDF.Fluent.Document;
using QuestLicenseType = QuestPDF.Infrastructure.LicenseType;
using QuestPageSizes = QuestPDF.Helpers.PageSizes;
using QuestSettings = QuestPDF.Settings;
using PdfContainer = QuestPDF.Infrastructure.IContainer;

namespace SDNet.Services.Export
{
    internal enum ExportElementKind
    {
        Heading = 0,
        Paragraph = 1,
        Field = 2,
        Table = 3
    }

    internal sealed class ExportElement
    {
        public required ExportElementKind Kind { get; init; }

        public string PrimaryText { get; init; } = string.Empty;

        public string SecondaryText { get; init; } = string.Empty;

        public IReadOnlyList<string> Headers { get; init; } = [];

        public IReadOnlyList<IReadOnlyList<string>> Rows { get; init; } = [];
    }

    internal abstract class BufferedExportRendererBase : IExportRenderer
    {
        private readonly List<ExportElement> _elements = [];

        protected string DocumentTitle { get; private set; } = string.Empty;

        protected IReadOnlyList<ExportElement> Elements => _elements;

        public abstract string FileExtension { get; }

        public void BeginDocument(string title)
        {
            DocumentTitle = title;
            _elements.Clear();
        }

        public void WriteHeading(string text)
        {
            _elements.Add(new ExportElement
            {
                Kind = ExportElementKind.Heading,
                PrimaryText = Normalize(text)
            });
        }

        public void WriteParagraph(string text)
        {
            _elements.Add(new ExportElement
            {
                Kind = ExportElementKind.Paragraph,
                PrimaryText = Normalize(text)
            });
        }

        public void WriteField(string name, string value)
        {
            _elements.Add(new ExportElement
            {
                Kind = ExportElementKind.Field,
                PrimaryText = Normalize(name),
                SecondaryText = Normalize(value)
            });
        }

        public void WriteTable(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
        {
            _elements.Add(new ExportElement
            {
                Kind = ExportElementKind.Table,
                Headers = headers.Select(Normalize).ToArray(),
                Rows = rows
                    .Select(row => (IReadOnlyList<string>)row.Select(Normalize).ToArray())
                    .ToArray()
            });
        }

        public abstract Task<string> SaveAsync(
            string outputDirectory,
            string fileNameWithoutExtension,
            CancellationToken cancellationToken = default);

        protected static string BuildOutputPath(string outputDirectory, string fileNameWithoutExtension, string extension)
        {
            string safeName = fileNameWithoutExtension;
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                safeName = safeName.Replace(invalidChar, '_');
            }

            return Path.Combine(outputDirectory, $"{safeName}{extension}");
        }

        protected static string Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
        }
    }

    internal sealed class WordExportRenderer : BufferedExportRendererBase
    {
        public override string FileExtension => ".docx";

        public override Task<string> SaveAsync(
            string outputDirectory,
            string fileNameWithoutExtension,
            CancellationToken cancellationToken = default)
        {
            string outputPath = BuildOutputPath(outputDirectory, fileNameWithoutExtension, FileExtension);

            using var document = WordprocessingDocument.Create(outputPath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);
            MainDocumentPart mainPart = document.AddMainDocumentPart();
            mainPart.Document = new OxmlWord.Document(new OxmlWord.Body());
            OxmlWord.Body body = mainPart.Document.Body!;

            foreach (ExportElement element in Elements)
            {
                switch (element.Kind)
                {
                    case ExportElementKind.Heading:
                        body.Append(CreateParagraph(element.PrimaryText, isBold: true, fontSize: "30"));
                        break;
                    case ExportElementKind.Paragraph:
                        body.Append(CreateParagraph(element.PrimaryText));
                        break;
                    case ExportElementKind.Field:
                        body.Append(CreateFieldParagraph(element.PrimaryText, element.SecondaryText));
                        break;
                    case ExportElementKind.Table:
                        body.Append(CreateTable(element.Headers, element.Rows));
                        break;
                }
            }

            mainPart.Document.Save();
            return Task.FromResult(outputPath);
        }

        private static OxmlWord.Paragraph CreateParagraph(
            string text,
            bool isBold = false,
            string fontSize = "24")
        {
            OxmlWord.RunProperties runProperties = new(new OxmlWord.FontSize { Val = fontSize });
            if (isBold)
            {
                runProperties.Append(new OxmlWord.Bold());
            }

            OxmlWord.Run run = new(runProperties, new OxmlWord.Text(text) { Space = Oxml.SpaceProcessingModeValues.Preserve });
            return new OxmlWord.Paragraph(run);
        }

        private static OxmlWord.Paragraph CreateFieldParagraph(string name, string value)
        {
            OxmlWord.Run labelRun = new(
                new OxmlWord.RunProperties(new OxmlWord.Bold(), new OxmlWord.FontSize { Val = "24" }),
                new OxmlWord.Text($"{name}: ") { Space = Oxml.SpaceProcessingModeValues.Preserve });
            OxmlWord.Run valueRun = new(
                new OxmlWord.RunProperties(new OxmlWord.FontSize { Val = "24" }),
                new OxmlWord.Text(value) { Space = Oxml.SpaceProcessingModeValues.Preserve });

            return new OxmlWord.Paragraph(labelRun, valueRun);
        }

        private static OxmlWord.Table CreateTable(
            IReadOnlyList<string> headers,
            IReadOnlyList<IReadOnlyList<string>> rows)
        {
            var table = new OxmlWord.Table();
            table.AppendChild(new OxmlWord.TableProperties(
                new OxmlWord.TableBorders(
                    new OxmlWord.TopBorder { Val = OxmlWord.BorderValues.Single, Size = 4 },
                    new OxmlWord.BottomBorder { Val = OxmlWord.BorderValues.Single, Size = 4 },
                    new OxmlWord.LeftBorder { Val = OxmlWord.BorderValues.Single, Size = 4 },
                    new OxmlWord.RightBorder { Val = OxmlWord.BorderValues.Single, Size = 4 },
                    new OxmlWord.InsideHorizontalBorder { Val = OxmlWord.BorderValues.Single, Size = 4 },
                    new OxmlWord.InsideVerticalBorder { Val = OxmlWord.BorderValues.Single, Size = 4 })));

            table.Append(CreateTableRow(headers, isHeader: true));
            foreach (IReadOnlyList<string> row in rows)
            {
                table.Append(CreateTableRow(row, isHeader: false));
            }

            return table;
        }

        private static OxmlWord.TableRow CreateTableRow(IReadOnlyList<string> values, bool isHeader)
        {
            var row = new OxmlWord.TableRow();
            foreach (string value in values)
            {
                OxmlWord.RunProperties properties = new(new OxmlWord.FontSize { Val = "22" });
                if (isHeader)
                {
                    properties.Append(new OxmlWord.Bold());
                }

                row.Append(
                    new OxmlWord.TableCell(
                        new OxmlWord.Paragraph(
                            new OxmlWord.Run(properties, new OxmlWord.Text(value) { Space = Oxml.SpaceProcessingModeValues.Preserve })),
                        new OxmlWord.TableCellProperties(
                            new OxmlWord.TableCellWidth { Type = OxmlWord.TableWidthUnitValues.Auto })));
            }

            return row;
        }
    }

    internal sealed class ExcelExportRenderer : BufferedExportRendererBase
    {
        public override string FileExtension => ".xlsx";

        public override Task<string> SaveAsync(
            string outputDirectory,
            string fileNameWithoutExtension,
            CancellationToken cancellationToken = default)
        {
            string outputPath = BuildOutputPath(outputDirectory, fileNameWithoutExtension, FileExtension);

            using var document = SpreadsheetDocument.Create(outputPath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
            WorkbookPart workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new OxmlSheet.Workbook();

            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            OxmlSheet.SheetData sheetData = new();
            worksheetPart.Worksheet = new OxmlSheet.Worksheet(sheetData);

            OxmlSheet.Sheets sheets = workbookPart.Workbook.AppendChild(new OxmlSheet.Sheets());
            OxmlSheet.Sheet sheet = new()
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Export"
            };
            sheets.Append(sheet);

            uint rowIndex = 1;
            foreach (ExportElement element in Elements)
            {
                switch (element.Kind)
                {
                    case ExportElementKind.Heading:
                        AppendRow(sheetData, rowIndex++, [element.PrimaryText]);
                        rowIndex++;
                        break;
                    case ExportElementKind.Paragraph:
                        AppendRow(sheetData, rowIndex++, [element.PrimaryText]);
                        break;
                    case ExportElementKind.Field:
                        AppendRow(sheetData, rowIndex++, [element.PrimaryText, element.SecondaryText]);
                        break;
                    case ExportElementKind.Table:
                        AppendRow(sheetData, rowIndex++, element.Headers);
                        foreach (IReadOnlyList<string> row in element.Rows)
                        {
                            AppendRow(sheetData, rowIndex++, row);
                        }

                        rowIndex++;
                        break;
                }
            }

            workbookPart.Workbook.Save();
            return Task.FromResult(outputPath);
        }

        private static void AppendRow(OxmlSheet.SheetData sheetData, uint rowIndex, IReadOnlyList<string> values)
        {
            OxmlSheet.Row row = new() { RowIndex = rowIndex };
            foreach (string value in values)
            {
                row.Append(new OxmlSheet.Cell
                {
                    DataType = OxmlSheet.CellValues.InlineString,
                    InlineString = new OxmlSheet.InlineString(new OxmlSheet.Text(value))
                });
            }

            sheetData.Append(row);
        }
    }

    internal sealed class PdfExportRenderer : BufferedExportRendererBase
    {
        public override string FileExtension => ".pdf";

        public override Task<string> SaveAsync(
            string outputDirectory,
            string fileNameWithoutExtension,
            CancellationToken cancellationToken = default)
        {
            QuestSettings.License = QuestLicenseType.Community;

            string outputPath = BuildOutputPath(outputDirectory, fileNameWithoutExtension, FileExtension);
            string generatedAt = DateTime.Now.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

            QuestDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(11));

                    page.Header()
                        .PaddingBottom(10)
                        .Text(DocumentTitle)
                        .SemiBold()
                        .FontSize(18);

                    page.Content().Column(column =>
                    {
                        column.Spacing(8);

                        foreach (ExportElement element in Elements)
                        {
                            switch (element.Kind)
                            {
                                case ExportElementKind.Heading:
                                    column.Item().Text(element.PrimaryText).SemiBold().FontSize(14);
                                    break;
                                case ExportElementKind.Paragraph:
                                    column.Item().Text(element.PrimaryText);
                                    break;
                                case ExportElementKind.Field:
                                    column.Item().Text(text =>
                                    {
                                        text.Span($"{element.PrimaryText}: ").SemiBold();
                                        text.Span(element.SecondaryText);
                                    });
                                    break;
                                case ExportElementKind.Table:
                                    RenderTable(column, element);
                                    break;
                            }
                        }
                    });

                    page.Footer()
                        .AlignRight()
                        .Text($"Generated {generatedAt}");
                });
            }).GeneratePdf(outputPath);

            return Task.FromResult(outputPath);
        }

        private static void RenderTable(ColumnDescriptor column, ExportElement element)
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    for (int index = 0; index < element.Headers.Count; index++)
                    {
                        columns.RelativeColumn();
                    }
                });

                table.Header(header =>
                {
                    foreach (string headerText in element.Headers)
                    {
                        header.Cell().Element(CellStyle).Text(headerText).SemiBold();
                    }
                });

                foreach (IReadOnlyList<string> row in element.Rows)
                {
                    foreach (string value in row)
                    {
                        table.Cell().Element(CellStyle).Text(value);
                    }
                }
            });
        }

        private static PdfContainer CellStyle(PdfContainer container)
        {
            return container
                .Border(1)
                .BorderColor(QuestColors.Grey.Lighten2)
                .Padding(4);
        }
    }
}
