using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Please enter a value")]
        [Column(TypeName = "decimal(8, 2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "nvarchar(75)")]
        public string? Note { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        [NotMapped]
        public string? FormattedAmount
        {
            get
            {
                if (Category is null || Category.Type == "Income")
                    return "+" + " " + Amount.ToString("C2");
                else return "-" +" "+ Amount.ToString("C2");
            }
        }
        [NotMapped]
        public string? TransactionTitleWithIcon
        {
            get
            {
                if (Category is not null)
                {
                    return Category.TitleWithIcon;
                }
                else
                {
                    return "";
                }
            }
        }
    }
}