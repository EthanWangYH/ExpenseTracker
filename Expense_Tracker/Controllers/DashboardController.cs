using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Expense_Tracker.Controllers
{
    public class DashboardController : Controller
    {
        private ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }
        public async Task<IActionResult> Index()
        {   //Last 7 dats
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;
            List<Transaction> selectedTranactions = await _context.Transactions.Include(i=>i.Category)
                .Where(y=>y.Date>=StartDate &&y.Date<=EndDate).ToListAsync();
            ViewBag.SelectedExpense = selectedTranactions
                .Where(t=>t.Category?.Type=="Expense")
                .GroupBy(g=>g.Category?.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().TransactionTitleWithIcon,
                    amount =k.Sum(t=>t.Amount),
                    formattedAmount = k.Sum(t=>t.Amount).ToString("C2")
                }).ToList();
                ;
            //Total income and expense
            decimal totalIncome = selectedTranactions.Where(x=>x.Category?.Type=="Income").Sum(x=>x.Amount);
            decimal totalExpense = selectedTranactions.Where(x => x.Category?.Type == "Expense").Sum(x => x.Amount);
            ViewBag.TotalIncome = totalIncome.ToString("C2");
            ViewBag.TotalExpense = totalExpense.ToString("C2");
            CultureInfo cultureInfo =CultureInfo.CreateSpecificCulture("en-US");
            cultureInfo.NumberFormat.CurrencyNegativePattern=1;
            decimal Balance = totalIncome - totalExpense;
            ViewBag.Balance = String.Format(cultureInfo,"{0:C2}",Balance);
            //Spline chart
            List<SplineChartData> incomeSumary = selectedTranactions.Where(i => i.Category?.Type == "Income").GroupBy(g => g.Date).Select(
                k => new SplineChartData
                {
                    income = (double)k.Sum(s=>s.Amount),
                    day = k.First().Date.ToString("dd-MMM"),
                }).ToList();
            List<SplineChartData> expenseSumary = selectedTranactions.Where(e => e.Category?.Type == "Expense").GroupBy(g => g.Date).Select(k=>
                new SplineChartData
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    expense = (double)k.Sum(s=>s.Amount)
                }).ToList();
            string[] selectedDays = Enumerable.Range(0,7).Select(i=>StartDate.AddDays(i).ToString("dd-MMM")).ToArray();
            var splineChartDatas = from selectedDay in selectedDays
                                      join income in incomeSumary on selectedDay equals income.day into incomeGroup
                                      from incomes in incomeGroup.DefaultIfEmpty()
                                      join expense in expenseSumary on selectedDay equals expense.day into expenseGroup
                                      from expenses in expenseGroup.DefaultIfEmpty()
                                      select new SplineChartData
                                      {
                                          day = selectedDay,
                                          income = incomes ==null? 0:incomes.income,
                                          expense = expenses==null?0:expenses.expense
                                      };
            ViewBag.SplineChartData = splineChartDatas;
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(c=>c.Category)
                .OrderByDescending(d=>d.Date)
                .Take(5).ToListAsync();
            return View();
        }

       
        public class SplineChartData
        {
            public string day;
            public double income;
            public double expense;
        }
    }
}
