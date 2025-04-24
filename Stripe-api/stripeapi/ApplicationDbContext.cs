using Microsoft.EntityFrameworkCore;
using stripeapi.Entity;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<DepositRecord> DepositRecords { get; set; }
    public DbSet<ProfitAccount> ProfitAccounts { get; set; }
    public DbSet<Account> Accounts { get; set; }

    public DbSet<WithdrawRecord> WithdrawRecords { get; set; }

    public DbSet<BankCard> BankCards { get; set; }
}