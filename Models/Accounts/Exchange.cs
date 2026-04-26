using Backend.Models.Setup;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Accounts
{

 public class Exchange : BaseEntity
    {


        [Required]
        public decimal Rate { get; set; }

        [Required]
        public decimal FromAmount { get; set; }

        [Required]
        public decimal ToAmount { get; set; }

        public decimal Fee { get; set; } = 0;
        public decimal Profit { get; set; } = 0;
        public decimal? NetAmount { get; set; } = 0;




        [Required]
        public Guid TransactionId { get; set; }

        [ForeignKey(nameof(TransactionId))]
        public Transaction Transaction { get; set; } = null!;

        [Required]
        public Guid FromAccountId { get; set; }

        [ForeignKey(nameof(FromAccountId))]
        public Account FromAccount { get; set; } = null!;

        [Required]
        public Guid ToAccountId { get; set; }

        [ForeignKey(nameof(ToAccountId))]
        public Account ToAccount { get; set; } = null!;





        public Guid AgencyId { get; set; }
        public Guid? BranchId { get; set; }

        [ForeignKey(nameof(AgencyId))]
        public Agency Agency { get; set; } = null!;

        [ForeignKey(nameof(BranchId))]
        public Branch? Branch { get; set; }
    }
    }

