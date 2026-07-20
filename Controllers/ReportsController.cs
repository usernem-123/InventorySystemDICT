using InventorySystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Models;
using ClosedXML.Excel;
using InventorySystem.Services;
namespace InventorySystem.Controllers;


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
        DateTime? startDate,
        DateTime? endDate,
        string? search)
    {


        var query = _db.Transactions
            .Include(x => x.Item)
            .Include(x => x.Borrower)
            .Include(x=>x.User)
            .AsQueryable();


        if(startDate.HasValue)
        {
            query = query.Where(x =>
                x.TransactionDate >= startDate.Value);
        }


        if(endDate.HasValue)
        {
            query = query.Where(x =>
                x.TransactionDate <= endDate.Value);
        }


        if(!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Item!.ItemName.Contains(search) ||
                (x.Borrower != null &&
                x.Borrower.FullName.Contains(search)));
        }


        var transactions = await query
            .OrderByDescending(x => x.TransactionDate)
            .ToListAsync();


        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
        ViewBag.Search = search;


        return View(transactions);
    }

    public async Task<IActionResult> ExportTransactions()
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
}