using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Journal_Entry.Components.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Journal_Entry.Components.Services
{
    public class PdfExportService
    {
        public PdfExportService()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        public async Task<string> ExportToPdfAsync(JournalEntry entry)
        {
            var fileName = $"Journal_{entry.CreatedAt:yyyyMMdd}.pdf";
            var path = Path.Combine(FileSystem.AppDataDirectory, fileName);

            await Task.Run(() =>
            {
                QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(QuestPDF.Helpers.PageSizes.A4);
                        page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                        page.PageColor(QuestPDF.Helpers.Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header()
                            .Text($"Journal Entry - {entry.CreatedAt:MMMM dd, yyyy}")
                            .SemiBold().FontSize(20).FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);

                        page.Content()
                            .PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre)
                            .Column(column =>
                            {
                                column.Spacing(10);

                                column.Item().Text(entry.Title ?? "Untitled")
                                    .FontSize(16).Bold();

                                column.Item().PaddingTop(10).Column(meta =>
                                {
                                    meta.Item().Text(text =>
                                    {
                                        text.Span("Date: ").Bold();
                                        text.Span(entry.CreatedAt.ToString("yyyy-MM-dd"));
                                    });

                                    meta.Item().Text(text =>
                                    {
                                        text.Span("Mood: ").Bold();
                                        text.Span(entry.PrimaryMood ?? "Not specified");
                                    });

                                    if (entry.SecondaryMoods != null && entry.SecondaryMoods.Any())
                                    {
                                        meta.Item().Text(text =>
                                        {
                                            text.Span("Secondary Moods: ").Bold();
                                            text.Span(string.Join(", ", entry.SecondaryMoods));
                                        });
                                    }

                                    if (entry.Tags != null && entry.Tags.Any())
                                    {
                                        meta.Item().Text(text =>
                                        {
                                            text.Span("Tags: ").Bold();
                                            text.Span(string.Join(", ", entry.Tags));
                                        });
                                    }

                                    meta.Item().Text(text =>
                                    {
                                        text.Span("Word Count: ").Bold();
                                        text.Span(entry.WordCount.ToString());
                                    });
                                });

                                column.Item().PaddingVertical(10).LineHorizontal(1)
                                    .LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);

                                column.Item().Text(entry.Content ?? "No content")
                                    .FontSize(11).LineHeight(1.5f);
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                                x.Span(" of ");
                                x.TotalPages();
                            });
                    });
                })
                .GeneratePdf(path);
            });

            return path;
        }

        public async Task<string> ExportMultipleToPdfAsync(List<JournalEntry> entries, string fileName = null)
        {
            fileName ??= $"Journal_Export_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var path = Path.Combine(FileSystem.AppDataDirectory, fileName);

            await Task.Run(() =>
            {
                QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(QuestPDF.Helpers.PageSizes.A4);
                        page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                        page.PageColor(QuestPDF.Helpers.Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header()
                            .Text("My Journal")
                            .SemiBold().FontSize(24).FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);

                        page.Content()
                            .PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre)
                            .Column(column =>
                            {
                                foreach (var entry in entries.OrderByDescending(e => e.CreatedAt))
                                {
                                    column.Item().PaddingBottom(20).Column(entryCol =>
                                    {
                                        entryCol.Item().Text(entry.Title ?? "Untitled")
                                            .FontSize(16).Bold();

                                        entryCol.Item().Text(entry.CreatedAt.ToString("MMMM dd, yyyy"))
                                            .FontSize(10).Italic().FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);

                                        entryCol.Item().PaddingTop(5).Text(text =>
                                        {
                                            text.Span("Mood: ").Bold();
                                            text.Span(entry.PrimaryMood ?? "Not specified");
                                        });

                                        if (entry.Tags != null && entry.Tags.Any())
                                        {
                                            entryCol.Item().Text(text =>
                                            {
                                                text.Span("Tags: ").Bold();
                                                text.Span(string.Join(", ", entry.Tags));
                                            });
                                        }

                                        entryCol.Item().PaddingTop(10)
                                            .Text(entry.Content ?? "No content")
                                            .FontSize(11).LineHeight(1.5f);

                                        entryCol.Item().PaddingTop(15).LineHorizontal(1)
                                            .LineColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
                                    });
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                                x.Span(" of ");
                                x.TotalPages();
                            });
                    });
                })
                .GeneratePdf(path);
            });

            return path;
        }
    }
}