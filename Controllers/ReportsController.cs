using InventorySystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Models;
using ClosedXML.Excel;
using InventorySystem.Services;
namespace InventorySystem.Controllers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

[Authorize]
public class ReportsController : Controller
{
    private readonly AppDbContext _db;

    private readonly ITransactionArchiveService _archiveService;

    public ReportsController(
        AppDbContext db,
        ITransactionArchiveService archiveService)
    {
        _db = db;
        _archiveService = archiveService;
    }


    public async Task<IActionResult> Index()
    {
        var totalItems = await _db.Items.CountAsync();

        var available = await _db.Items
            .CountAsync(x => x.Status == ItemStatus.Available);

        var borrowed = await _db.Items
            .CountAsync(x => x.Status == ItemStatus.Borrowed);


        var totalBorrowers = await _db.Borrowers.CountAsync();


        ViewBag.TotalItems = totalItems;
        ViewBag.Available = available;
        ViewBag.Borrowed = borrowed;
        ViewBag.TotalBorrowers = totalBorrowers;

        ViewBag.Borrowers = await _db.Borrowers
        .OrderBy(x => x.FullName)
        .ToListAsync();


        return View();
    }


    public async Task<IActionResult> Inventory(
        string? search,
        int? categoryId)
    {
        var query = _db.Items
            .Include(x=>x.Category)
            .AsQueryable();


        if(!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.ItemName.Contains(search) ||
                x.ItemCode.Contains(search) ||
                x.SerialNumber.Contains(search));
        }


        if(categoryId.HasValue)
        {
            query = query.Where(x =>
                x.CategoryId == categoryId);
        }


        return View(await query.ToListAsync());
    }


    public async Task<IActionResult> Transactions(
        string? filterType,
        int? itemId,
        int? borrowerId)
    {
        var query = _db.Transactions
            .Include(x => x.Item)
            .Include(x => x.Borrower)
            .Include(x => x.User)
            .AsQueryable();

        if (filterType == "Item" && itemId.HasValue)
        {
            query = query.Where(x => x.ItemId == itemId.Value);
        }

        if (filterType == "Borrower" && borrowerId.HasValue)
        {
            query = query.Where(x => x.BorrowerId == borrowerId.Value);
        }

        var transactions = await query
            .OrderByDescending(x => x.TransactionDate)
            .ToListAsync();

        ViewBag.FilterType = filterType ?? "Item";
        ViewBag.ItemId = itemId;
        ViewBag.BorrowerId = borrowerId;

        ViewBag.Items = await _db.Items
            .OrderBy(x => x.ItemName)
            .ToListAsync();

        ViewBag.Borrowers = await _db.Borrowers
            .OrderBy(x => x.FullName)
            .ToListAsync();

        return View(transactions);
    }

    public async Task<IActionResult> ExportTransactions(
        string? filterType,
        string? action,
        int? borrowerId)
    {
        var data = await _db.Transactions
            .Include(x=>x.Item)
            .Include(x=>x.Borrower)
            .Include(x=>x.User)
            .ToListAsync();


        using var workbook = new XLWorkbook();

        var sheet = workbook.Worksheets.Add("Transactions");


        sheet.Cell(1,1).Value="Date";
        sheet.Cell(1,2).Value="Item";
        sheet.Cell(1,3).Value="Borrower";
        sheet.Cell(1,4).Value="Action";


        int row=2;


        foreach(var t in data)
        {
            sheet.Cell(row,1).Value = t.TransactionDate.ToString("MMM dd, yyyy hh:mm tt");
            sheet.Cell(row,2).Value=t.Item?.ItemName;
            sheet.Cell(row,3).Value=t.Borrower?.FullName;
            sheet.Cell(row,4).Value=t.TransactionType.ToString();
            sheet.Cell(1,5).Value = "Processed By";

            row++;
        }


        using var stream=new MemoryStream();

        workbook.SaveAs(stream);


        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "transactions.xlsx");
    }

    public async Task<IActionResult> ArchivedTransactions(
        DateTime? startDate,
        DateTime? endDate,
        string? search)
    {
        var archives = await _archiveService.SearchAsync(
            startDate,
            endDate,
            search);

        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
        ViewBag.Search = search;

        return View(archives);
    }

    public async Task<IActionResult> TransactionsPdf(
    string? filterType,
    int? itemId,
    int? borrowerId)
{
    var query = _db.Transactions
        .Include(x => x.Item)
        .Include(x => x.Borrower)
        .Include(x => x.User)
        .AsQueryable();

    if (filterType == "Item" && itemId.HasValue)
        query = query.Where(x => x.ItemId == itemId.Value);

    if (filterType == "Borrower" && borrowerId.HasValue)
        query = query.Where(x => x.BorrowerId == borrowerId.Value);

    var transactions = await query
        .OrderByDescending(x => x.TransactionDate)
        .ToListAsync();

    var pdf = Document.Create(document =>
    {
        document.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(25);

            // ===== HEADER =====
            page.Header().Column(col =>
            {
                col.Item().Row(row =>
                {
                    // Place your logo at wwwroot/images/dictlogo.png
                    row.ConstantItem(70).Height(70)
                        .Image("wwwroot/images/dictlogo.png");

                    row.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter()
                            .Text("Department of Information and Communications Technology")
                            .Bold()
                            .FontSize(16);

                        c.Item().AlignCenter()
                            .Text("Inventory Management System")
                            .FontSize(12);

                        c.Item().AlignCenter()
                            .Text("Transaction Report")
                            .Bold()
                            .FontSize(18);
                    });
                });

                col.Item().PaddingTop(10);

                col.Item().Text($"Generated: {DateTime.Now:MMMM dd, yyyy hh:mm tt}");

                col.Item().Text($"Total Transactions: {transactions.Count}");

                if (filterType == "Item")
                    col.Item().Text("Filter: Item");

                if (filterType == "Borrower")
                    col.Item().Text("Filter: Borrower");

                col.Item().PaddingBottom(15);
            });

            // ===== TABLE =====
            page.Content().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2); // Date
                    columns.RelativeColumn(1); // Action
                    columns.RelativeColumn(2); // Item
                    columns.RelativeColumn(2); // Borrower
                    columns.RelativeColumn(1); // Qty
                    columns.RelativeColumn(2); // Remarks
                    columns.RelativeColumn(2); // Processed By
                });

                table.Header(header =>
                {
                    static IContainer HeaderStyle(IContainer c) =>
                        c.Background("#0F4C81")
                         .Border(1)
                         .BorderColor(Colors.Grey.Lighten2)
                         .Padding(5);

                    header.Cell().Element(HeaderStyle)
                        .Text("Date").Bold().FontColor(Colors.White);

                    header.Cell().Element(HeaderStyle)
                        .Text("Action").Bold().FontColor(Colors.White);

                    header.Cell().Element(HeaderStyle)
                        .Text("Item").Bold().FontColor(Colors.White);

                    header.Cell().Element(HeaderStyle)
                        .Text("Borrower").Bold().FontColor(Colors.White);

                    header.Cell().Element(HeaderStyle)
                        .Text("Qty").Bold().FontColor(Colors.White);

                    header.Cell().Element(HeaderStyle)
                        .Text("Remarks").Bold().FontColor(Colors.White);

                    header.Cell().Element(HeaderStyle)
                        .Text("Processed By").Bold().FontColor(Colors.White);
                });

                int index = 0;

                foreach (var t in transactions)
                {
                    var rowColor = index % 2 == 0
                        ? Colors.Grey.Lighten4
                        : Colors.White;

                    static IContainer CellStyle(IContainer c, string color) =>
                        c.Background(color)
                         .Border(1)
                         .BorderColor(Colors.Grey.Lighten2)
                         .Padding(5);

                    table.Cell().Element(c => CellStyle(c, rowColor))
                        .Text(t.TransactionDate.ToString("MMM dd, yyyy"));

                    var actionColor = t.TransactionType switch
                    {
                        TransactionType.Borrow => Colors.Orange.Lighten2,
                        TransactionType.Return => Colors.Green.Lighten2,
                        TransactionType.Receive => Colors.Blue.Lighten2,
                        _ => Colors.Grey.Lighten2
                    };

                    table.Cell().Element(c => CellStyle(c, actionColor))
                        .AlignCenter()
                        .Text(t.TransactionType.ToString())
                        .Bold();

                    table.Cell().Element(c => CellStyle(c, rowColor))
                        .Text(t.Item?.ItemName ?? "-");

                    table.Cell().Element(c => CellStyle(c, rowColor))
                        .Text(t.Borrower?.FullName ?? "-");

                    table.Cell().Element(c => CellStyle(c, rowColor))
                        .AlignCenter()
                        .Text(t.Quantity.ToString());

                    table.Cell().Element(c => CellStyle(c, rowColor))
                        .Text(t.Remarks ?? "-");

                    table.Cell().Element(c => CellStyle(c, rowColor))
                        .Text(t.User?.FullName ?? "-");

                    index++;
                }
            });

            // ===== FOOTER =====
            page.Footer().Column(col =>
            {
                col.Item().PaddingTop(20);

                col.Item().Row(row =>
                {
                    row.RelativeItem().AlignCenter().Column(c =>
                    {
                        c.Item().Text("________________________");
                        c.Item().Text("Prepared By")
                            .Bold();
                    });

                    row.RelativeItem().AlignCenter().Column(c =>
                    {
                        c.Item().Text("________________________");
                        c.Item().Text("Approved By")
                            .Bold();
                    });
                });

                col.Item().PaddingTop(15);

                col.Item().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });
    });

    return File(
        pdf.GeneratePdf(),
        "application/pdf",
        $"TransactionReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
}
}