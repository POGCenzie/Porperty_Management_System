using Microsoft.EntityFrameworkCore;
using PropertyManagementSystem.Constants;
using PropertyManagementSystem.Model;

namespace PropertyManagementSystem.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Condominium> Condominiums => Set<Condominium>();

    public DbSet<CondominiumUnit> CondominiumUnits => Set<CondominiumUnit>();

    public DbSet<Owner> Owners => Set<Owner>();

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<TenantAssignment> TenantAssignments => Set<TenantAssignment>();

    public DbSet<StatementOfAccount> StatementsOfAccount => Set<StatementOfAccount>();

    public DbSet<SoaLineItem> SoaLineItems => Set<SoaLineItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();

        user.ToTable("Users", tableBuilder =>
            tableBuilder.HasCheckConstraint(
                "CK_Users_UserGroup",
                $"[UserGroup] IN ('{UserGroups.Admin}', '{UserGroups.User}')"));

        user.HasKey(item => item.UserId);

        user.Property(item => item.UserId)
            .UseIdentityColumn();

        user.Property(item => item.Username)
            .HasMaxLength(100)
            .IsRequired();

        user.HasIndex(item => item.Username)
            .IsUnique();

        user.Property(item => item.DisplayName)
            .HasMaxLength(150)
            .IsRequired();

        user.Property(item => item.UserGroup)
            .HasMaxLength(10)
            .HasDefaultValue(UserGroups.User)
            .IsRequired();

        user.Property(item => item.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        user.Property(item => item.PasswordHash)
            .HasMaxLength(512);

        user.Property(item => item.LastUpdatedDate)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        ConfigureCondominium(modelBuilder);
        ConfigureOwner(modelBuilder);
        ConfigureCondominiumUnit(modelBuilder);
        ConfigureTenant(modelBuilder);
        ConfigureTenantAssignment(modelBuilder);
        ConfigureStatementOfAccount(modelBuilder);
    }

    private static void ConfigureCondominium(ModelBuilder modelBuilder)
    {
        var condominium = modelBuilder.Entity<Condominium>();
        condominium.ToTable("Condominiums");
        condominium.HasKey(item => item.CondominiumId);
        condominium.Property(item => item.CondominiumId).UseIdentityColumn();
        condominium.Property(item => item.Name).HasMaxLength(150).IsRequired();
        condominium.HasIndex(item => item.Name).IsUnique();
        condominium.Property(item => item.IsActive).HasDefaultValue(true).IsRequired();
        condominium.Property(item => item.LastUpdatedDate)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();
    }

    private static void ConfigureTenant(ModelBuilder modelBuilder)
    {
        var tenant = modelBuilder.Entity<Tenant>();
        tenant.ToTable("Tenants");
        tenant.HasKey(item => item.TenantId);
        tenant.Property(item => item.TenantId).UseIdentityColumn();
        tenant.Property(item => item.FullName).HasMaxLength(200).IsRequired();
        tenant.Property(item => item.ContactNumber).HasMaxLength(100);
        tenant.Property(item => item.EmailAddress).HasMaxLength(200);
        tenant.Property(item => item.IsActive).HasDefaultValue(true).IsRequired();
        tenant.Property(item => item.LastUpdatedDate)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();
    }

    private static void ConfigureCondominiumUnit(ModelBuilder modelBuilder)
    {
        var unit = modelBuilder.Entity<CondominiumUnit>();
        unit.ToTable("CondominiumUnits");
        unit.HasKey(item => item.CondominiumUnitId);
        unit.Property(item => item.CondominiumUnitId).UseIdentityColumn();
        unit.Property(item => item.UnitNumber).HasMaxLength(50).IsRequired();
        unit.Property(item => item.MeralcoSin).HasMaxLength(100);
        unit.Property(item => item.IsActive).HasDefaultValue(true).IsRequired();
        unit.Property(item => item.LastUpdatedDate)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();
        unit.HasIndex(item => new { item.CondominiumId, item.UnitNumber }).IsUnique();
        unit.HasOne(item => item.Condominium)
            .WithMany(item => item.Units)
            .HasForeignKey(item => item.CondominiumId)
            .OnDelete(DeleteBehavior.Restrict);
        unit.HasOne(item => item.Owner)
            .WithMany(item => item.Units)
            .HasForeignKey(item => item.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureOwner(ModelBuilder modelBuilder)
    {
        var owner = modelBuilder.Entity<Owner>();
        owner.ToTable("Owners");
        owner.HasKey(item => item.OwnerId);
        owner.Property(item => item.OwnerId).UseIdentityColumn();
        owner.Property(item => item.FullName).HasMaxLength(200).IsRequired();
        owner.Property(item => item.EmailAddress).HasMaxLength(200);
        owner.Property(item => item.ContactNumber).HasMaxLength(100);
        owner.Property(item => item.IsActive).HasDefaultValue(true).IsRequired();
        owner.Property(item => item.LastUpdatedDate)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();
        owner.HasIndex(item => item.FullName);
    }

    private static void ConfigureTenantAssignment(ModelBuilder modelBuilder)
    {
        var assignment = modelBuilder.Entity<TenantAssignment>();
        assignment.ToTable("TenantAssignments");
        assignment.HasKey(item => item.TenantAssignmentId);
        assignment.Property(item => item.TenantAssignmentId).UseIdentityColumn();
        assignment.Property(item => item.UnitNumber).HasMaxLength(50).IsRequired();
        assignment.Property(item => item.OwnerName).HasMaxLength(200);
        assignment.Property(item => item.OccupancyStatus).HasMaxLength(30).IsRequired();
        assignment.Property(item => item.MeralcoSin).HasMaxLength(100);
        assignment.Property(item => item.KeyRelease);
        assignment.Property(item => item.AccessCardRelease);
        assignment.Property(item => item.RentalAmountToOwner).HasPrecision(18, 2);
        assignment.Property(item => item.WithDues).HasMaxLength(100);
        assignment.Property(item => item.StayDurationUnit).HasMaxLength(10);
        assignment.Property(item => item.StartOfContract).HasColumnType("date");
        assignment.Property(item => item.EndOfContract).HasColumnType("date");
        assignment.Property(item => item.AirconMaintenanceDate).HasColumnType("date");
        assignment.Property(item => item.MaintenanceStatus).HasMaxLength(100);
        assignment.Property(item => item.OwnerAdvanceMonths);
        assignment.Property(item => item.OwnerDepositMonths);
        assignment.Property(item => item.OwnerEmailAddress).HasMaxLength(250);
        assignment.Property(item => item.OwnerContactNumber).HasMaxLength(100);
        assignment.Property(item => item.MainTenantContactNumber).HasMaxLength(100);
        assignment.Property(item => item.MainTenantEmailAddress).HasMaxLength(250);
        assignment.Property(item => item.IsActive).HasDefaultValue(true).IsRequired();
        assignment.Property(item => item.LastUpdatedDate)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();
        assignment.HasIndex(item => new { item.CondominiumId, item.UnitNumber });
        assignment.HasIndex(item => item.TenantId);
        assignment.HasOne(item => item.Condominium)
            .WithMany(item => item.TenantAssignments)
            .HasForeignKey(item => item.CondominiumId)
            .OnDelete(DeleteBehavior.Restrict);
        assignment.HasOne(item => item.Tenant)
            .WithMany(item => item.Assignments)
            .HasForeignKey(item => item.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureStatementOfAccount(ModelBuilder modelBuilder)
    {
        var statement = modelBuilder.Entity<StatementOfAccount>();
        statement.ToTable("StatementsOfAccount", tableBuilder =>
            tableBuilder.HasCheckConstraint(
                "CK_StatementsOfAccount_SoaType",
                "[SoaType] IN ('owner', 'tenant')"));
        statement.HasKey(item => item.StatementOfAccountId);
        statement.Property(item => item.StatementOfAccountId).UseIdentityColumn();
        statement.Property(item => item.SoaNumber).HasMaxLength(50).IsRequired();
        statement.HasIndex(item => item.SoaNumber).IsUnique();
        statement.Property(item => item.SoaType).HasMaxLength(10).IsRequired();
        statement.Property(item => item.StatementDate).HasColumnType("date");
        statement.Property(item => item.RentalAmount).HasPrecision(18, 2);
        statement.Property(item => item.AdvanceAmount).HasPrecision(18, 2);
        statement.Property(item => item.DepositAmount).HasPrecision(18, 2);
        statement.Property(item => item.BaseAmount).HasPrecision(18, 2);
        statement.Property(item => item.TotalDeductions).HasPrecision(18, 2);
        statement.Property(item => item.NetAmount).HasPrecision(18, 2);
        statement.Property(item => item.Status).HasMaxLength(20).IsRequired();
        statement.Property(item => item.CreatedByUsername).HasMaxLength(100).IsRequired();
        statement.Property(item => item.LastUpdatedDate)
            .HasColumnType("datetime2(0)")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();
        statement.HasIndex(item => item.TenantAssignmentId);
        statement.HasOne(item => item.TenantAssignment)
            .WithMany()
            .HasForeignKey(item => item.TenantAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        var line = modelBuilder.Entity<SoaLineItem>();
        line.ToTable("SoaLineItems");
        line.HasKey(item => item.SoaLineItemId);
        line.Property(item => item.SoaLineItemId).UseIdentityColumn();
        line.Property(item => item.Description).HasMaxLength(200).IsRequired();
        line.Property(item => item.Amount).HasPrecision(18, 2);
        line.Property(item => item.Remarks).HasMaxLength(300);
        line.HasIndex(item => item.StatementOfAccountId);
        line.HasOne(item => item.StatementOfAccount)
            .WithMany(item => item.LineItems)
            .HasForeignKey(item => item.StatementOfAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
