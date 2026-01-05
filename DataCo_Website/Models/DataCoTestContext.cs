using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace DataCo_Website.Models;

public partial class DataCoTestContext : IdentityDbContext<ApplicationUser>
{
    public DataCoTestContext()
    {
    }

    public DataCoTestContext(DbContextOptions<DataCoTestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Shipping> Shippings { get; set; }
    public virtual DbSet<Cart> Carts { get; set; }
    public virtual DbSet<CartItem> CartItems { get; set; }

    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Cấu hình relationship ApplicationUser - Customer
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Customer)
            .WithOne(c => c.ApplicationUser)
            .HasForeignKey<ApplicationUser>(u => u.CustomerId)
            .IsRequired(false);
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("category");

            entity.Property(e => e.CategoryId)
                .HasDefaultValueSql("NEXT VALUE FOR Seq_CategoryId")
                .ValueGeneratedOnAdd()
                .HasColumnName("Category_Id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Category_Name");
            entity.Property(e => e.DepartmentId).HasColumnName("Department_Id");
            
            entity.HasOne(d => d.Department).WithMany(p => p.Categories)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_category_department");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customer");

            entity.Property(e => e.CustomerId)
                .HasDefaultValueSql("NEXT VALUE FOR Seq_CustomerId")
                .ValueGeneratedOnAdd()
            //.ValueGeneratedNever()
                .HasColumnName("Customer_Id");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("First_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Last_name");
            entity.Property(e => e.Segment)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Zipcode)
                .HasMaxLength(6)
                .IsUnicode(false);

            entity.HasOne(d => d.ZipcodeNavigation).WithMany(p => p.Customers)
                .HasForeignKey(d => d.Zipcode)
                .HasConstraintName("FK_customer_location");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("department");

            entity.Property(e => e.DepartmentId)
                .HasDefaultValueSql("NEXT VALUE FOR Seq_DepartmentId")
                .ValueGeneratedOnAdd()
                //.ValueGeneratedNever()
                .HasColumnName("Department_Id");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Department_Name");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Zipcode);

            entity.ToTable("location");

            entity.Property(e => e.Zipcode)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.State)
                .HasMaxLength(3)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");

            entity.Property(e => e.OrderId)
                .HasDefaultValueSql("NEXT VALUE FOR Seq_OrderId")
                .ValueGeneratedOnAdd()
            //.ValueGeneratedNever()
                .HasColumnName("Order_Id");
            entity.Property(e => e.CustomerId).HasColumnName("Customer_Id");
            //entity.Property(e => e.DepartmentId).HasColumnName("Department_Id");
            entity.Property(e => e.Market)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OrderCity)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Order_City");
            entity.Property(e => e.OrderCountry)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Order_Country");
            entity.Property(e => e.OrderDate)
                .HasColumnType("datetime")
                .HasColumnName("Order_Date");
            entity.Property(e => e.OrderRegion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Order_Region");
            entity.Property(e => e.OrderState)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Order_State");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TypeTransaction)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Type_Transaction");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_orders_customer");

            
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_item");

            entity.Property(e => e.OrderItemId)
                .HasDefaultValueSql("NEXT VALUE FOR Seq_OrderitemId")
                .ValueGeneratedOnAdd()
                //.ValueGeneratedNever()
                .HasColumnName("Order_Item_Id");
            entity.Property(e => e.DiscountRate).HasColumnName("Discount_Rate");
            entity.Property(e => e.OrderId).HasColumnName("Order_Id");
            entity.Property(e => e.ProductId).HasColumnName("Product_Id");
            entity.Property(e => e.ProfitRatio).HasColumnName("Profit_Ratio");
            entity.Property(e => e.DepartmentId).HasColumnName("Department_Id");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_order_item_orders");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_order_item_product");

            entity.HasOne(d => d.Department).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_order_item_department");
        });
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId);

            entity.Property(e => e.CartId)
                .ValueGeneratedOnAdd(); // Tự động tăng

            entity.Property(e => e.CurrentSessionId)
                .HasMaxLength(50)
                .IsRequired(false);

            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            entity.Property(e => e.UpdatedDate)
                .HasColumnType("datetime");

            entity.HasIndex(e => e.CustomerId)
                .IsUnique()
                .HasDatabaseName("IX_Carts_CustomerId_Unique");

            entity.HasOne(d => d.Customer)
                .WithMany(p => p.Carts)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId);

            entity.Property(e => e.CartItemId)
                .ValueGeneratedOnAdd(); // Tự động tăng
            
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue("InCart");

            entity.Property(e => e.SessionId)
                .HasMaxLength(50);             

            entity.Property(e => e.AddedDate)
                .HasColumnType("datetime")
                .HasDefaultValueSql("GETDATE()");

            entity.Property(e => e.UpdatedDate)
                .HasColumnType("datetime");

            entity.Property(e => e.CheckedOutDate)
                .HasColumnType("datetime");

            entity.HasIndex(e => new { e.SessionId, e.Status })
                .HasDatabaseName("IX_CartItems_SessionId_Status");

            entity.HasIndex(e => new { e.CartId, e.Status })
                .HasDatabaseName("IX_CartItems_CartId_Status");

            entity.HasIndex(e => e.CheckedOutDate)
                .HasDatabaseName("IX_CartItems_CheckedOutDate")
                .HasFilter("[CheckedOutDate] IS NOT NULL");

            entity.HasOne(d => d.Cart)
                .WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Cart thì xóa CartItems

            entity.HasOne(d => d.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductId);
        });
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("product");

            entity.Property(e => e.ProductId)
                .HasDefaultValueSql("NEXT VALUE FOR Seq_ProductId")
                .ValueGeneratedOnAdd()
                //.ValueGeneratedNever()
                .HasColumnName("Product_Id");
            entity.Property(e => e.CategoryId).HasColumnName("Category_Id");
            entity.Property(e => e.ProductName)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Product_Name");
            entity.Property(e => e.ProductPrice).HasColumnName("Product_Price");
            entity.Property(e => e.Image)
                .HasColumnType("nvarchar(max)")
                .HasColumnName("Image");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_product_category");
        });

        modelBuilder.Entity<Shipping>(entity =>
        {
            entity.HasKey(e => e.OrderId);

            entity.ToTable("shipping");

            entity.Property(e => e.OrderId)
                .ValueGeneratedNever()
                .HasColumnName("Order_Id");
            entity.Property(e => e.DaysForShipmentScheduled).HasColumnName("Days_For_Shipment_Scheduled");
            entity.Property(e => e.DaysForShippingActual).HasColumnName("Days_For_Shipping_Actual");
            entity.Property(e => e.DeliveryStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Delivery_Status");
            entity.Property(e => e.LateDeliveryRisk).HasColumnName("Late_Delivery_Risk");
            entity.Property(e => e.OrderDate)
                .HasColumnType("datetime")
                .HasColumnName("Order_Date");
            entity.Property(e => e.ShippingDate)
                .HasColumnType("datetime")
                .HasColumnName("Shipping_Date");
            entity.Property(e => e.ShippingMode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Shipping_Mode");

            entity.HasOne(d => d.Order).WithOne(p => p.Shipping)
                .HasForeignKey<Shipping>(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_shipping_orders");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
