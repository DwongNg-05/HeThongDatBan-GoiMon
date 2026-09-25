using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeThongDatBan_GoiMon.Migrations
{
    /// <inheritdoc />
    public partial class FullProjectSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentStatus",
                table: "RestaurantTables",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StatusUpdatedAtUtc",
                table: "RestaurantTables",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateTable(
                name: "MenuItemAvailabilities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedById = table.Column<int>(type: "int", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItemAvailabilities_EmployeeAccounts_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "EmployeeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenuItemAvailabilities_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PartySize = table.Column<int>(type: "int", nullable: false),
                    ReservationStartUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReservationEndUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RestaurantTableId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConfirmedById = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_EmployeeAccounts_ConfirmedById",
                        column: x => x.ConfirmedById,
                        principalTable: "EmployeeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_RestaurantTables_RestaurantTableId",
                        column: x => x.RestaurantTableId,
                        principalTable: "RestaurantTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShiftCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StartedById = table.Column<int>(type: "int", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndedById = table.Column<int>(type: "int", nullable: true),
                    EndedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ExpectedCashAmount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    CountedCashAmount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: true),
                    CashDifferenceAmount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: true),
                    DifferenceReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shifts_EmployeeAccounts_EndedById",
                        column: x => x.EndedById,
                        principalTable: "EmployeeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shifts_EmployeeAccounts_StartedById",
                        column: x => x.StartedById,
                        principalTable: "EmployeeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TableStatusHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestaurantTableId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ChangedById = table.Column<int>(type: "int", nullable: true),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TableStatusHistories_EmployeeAccounts_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "EmployeeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TableStatusHistories_RestaurantTables_RestaurantTableId",
                        column: x => x.RestaurantTableId,
                        principalTable: "RestaurantTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiningSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestaurantTableId = table.Column<int>(type: "int", nullable: false),
                    ReservationId = table.Column<int>(type: "int", nullable: true),
                    PublicSessionToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuestCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OpenedById = table.Column<int>(type: "int", nullable: true),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiningSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiningSessions_EmployeeAccounts_OpenedById",
                        column: x => x.OpenedById,
                        principalTable: "EmployeeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiningSessions_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DiningSessions_RestaurantTables_RestaurantTableId",
                        column: x => x.RestaurantTableId,
                        principalTable: "RestaurantTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientAccountId = table.Column<int>(type: "int", nullable: true),
                    ReservationId = table.Column<int>(type: "int", nullable: true),
                    Channel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RecipientAddress = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SentAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationLogs_EmployeeAccounts_RecipientAccountId",
                        column: x => x.RecipientAccountId,
                        principalTable: "EmployeeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotificationLogs_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReservationStatusHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedById = table.Column<int>(type: "int", nullable: true),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationStatusHistories_EmployeeAccounts_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "EmployeeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReservationStatusHistories_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiningSessionId = table.Column<int>(type: "int", nullable: false),
                    ShiftId = table.Column<int>(type: "int", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SubtotalAmount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    DiscountReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FinalizedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancelledById = table.Column<int>(type: "int", nullable: true),
                    CancelledAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_DiningSessions_DiningSessionId",
                        column: x => x.DiningSessionId,
                        principalTable: "DiningSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_EmployeeAccounts_CancelledById",
                        column: x => x.CancelledById,
                        principalTable: "EmployeeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiningSessionId = table.Column<int>(type: "int", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_DiningSessions_DiningSessionId",
                        column: x => x.DiningSessionId,
                        principalTable: "DiningSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_EmployeeAccounts_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "EmployeeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TableMerges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrimaryDiningSessionId = table.Column<int>(type: "int", nullable: false),
                    SecondaryDiningSessionId = table.Column<int>(type: "int", nullable: false),
                    MergedById = table.Column<int>(type: "int", nullable: false),
                    MergedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UnmergedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UnmergedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TableMerges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TableMerges_DiningSessions_PrimaryDiningSessionId",
                        column: x => x.PrimaryDiningSessionId,
                        principalTable: "DiningSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TableMerges_DiningSessions_SecondaryDiningSessionId",
                        column: x => x.SecondaryDiningSessionId,
                        principalTable: "DiningSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TableMerges_EmployeeAccounts_MergedById",
                        column: x => x.MergedById,
                        principalTable: "EmployeeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TableMerges_EmployeeAccounts_UnmergedById",
                        column: x => x.UnmergedById,
                        principalTable: "EmployeeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    CashReceivedAmount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: true),
                    ChangeAmount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: true),
                    TransactionReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CollectedById = table.Column<int>(type: "int", nullable: false),
                    PaidAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_EmployeeAccounts_CollectedById",
                        column: x => x.CollectedById,
                        principalTable: "EmployeeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerOrderId = table.Column<long>(type: "bigint", nullable: false),
                    MenuItemId = table.Column<int>(type: "int", nullable: false),
                    MenuItemName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StatusUpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ServedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_CustomerOrderId",
                        column: x => x.CustomerOrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<long>(type: "bigint", nullable: false),
                    OrderItemId = table.Column<long>(type: "bigint", nullable: true),
                    ItemName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    LineTotalAmount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsChargeableAfterCancellation = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemStatusHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderItemId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ChangedById = table.Column<int>(type: "int", nullable: true),
                    ChangedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemStatusHistories_EmployeeAccounts_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "EmployeeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItemStatusHistories_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiningSessions_OpenedById",
                table: "DiningSessions",
                column: "OpenedById");

            migrationBuilder.CreateIndex(
                name: "IX_DiningSessions_PublicSessionToken",
                table: "DiningSessions",
                column: "PublicSessionToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiningSessions_ReservationId",
                table: "DiningSessions",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_DiningSessions_RestaurantTableId",
                table: "DiningSessions",
                column: "RestaurantTableId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_InvoiceId",
                table: "InvoiceLines",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_OrderItemId",
                table: "InvoiceLines",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CancelledById",
                table: "Invoices",
                column: "CancelledById");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_DiningSessionId",
                table: "Invoices",
                column: "DiningSessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ShiftId",
                table: "Invoices",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemAvailabilities_MenuItemId_BusinessDate",
                table: "MenuItemAvailabilities",
                columns: new[] { "MenuItemId", "BusinessDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemAvailabilities_UpdatedById",
                table: "MenuItemAvailabilities",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_RecipientAccountId",
                table: "NotificationLogs",
                column: "RecipientAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_ReservationId",
                table: "NotificationLogs",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_CustomerOrderId",
                table: "OrderItems",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MenuItemId",
                table: "OrderItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemStatusHistories_ChangedById",
                table: "OrderItemStatusHistories",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemStatusHistories_OrderItemId",
                table: "OrderItemStatusHistories",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedById",
                table: "Orders",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DiningSessionId",
                table: "Orders",
                column: "DiningSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CollectedById",
                table: "Payments",
                column: "CollectedById");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ConfirmedById",
                table: "Reservations",
                column: "ConfirmedById");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservationCode",
                table: "Reservations",
                column: "ReservationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_RestaurantTableId_ReservationStartUtc",
                table: "Reservations",
                columns: new[] { "RestaurantTableId", "ReservationStartUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationStatusHistories_ChangedById",
                table: "ReservationStatusHistories",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationStatusHistories_ReservationId",
                table: "ReservationStatusHistories",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_EndedById",
                table: "Shifts",
                column: "EndedById");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_ShiftCode",
                table: "Shifts",
                column: "ShiftCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_StartedById",
                table: "Shifts",
                column: "StartedById");

            migrationBuilder.CreateIndex(
                name: "IX_TableMerges_MergedById",
                table: "TableMerges",
                column: "MergedById");

            migrationBuilder.CreateIndex(
                name: "IX_TableMerges_PrimaryDiningSessionId",
                table: "TableMerges",
                column: "PrimaryDiningSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TableMerges_SecondaryDiningSessionId",
                table: "TableMerges",
                column: "SecondaryDiningSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TableMerges_UnmergedById",
                table: "TableMerges",
                column: "UnmergedById");

            migrationBuilder.CreateIndex(
                name: "IX_TableStatusHistories_ChangedById",
                table: "TableStatusHistories",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_TableStatusHistories_RestaurantTableId",
                table: "TableStatusHistories",
                column: "RestaurantTableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceLines");

            migrationBuilder.DropTable(
                name: "MenuItemAvailabilities");

            migrationBuilder.DropTable(
                name: "NotificationLogs");

            migrationBuilder.DropTable(
                name: "OrderItemStatusHistories");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ReservationStatusHistories");

            migrationBuilder.DropTable(
                name: "TableMerges");

            migrationBuilder.DropTable(
                name: "TableStatusHistories");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropTable(
                name: "DiningSessions");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropColumn(
                name: "CurrentStatus",
                table: "RestaurantTables");

            migrationBuilder.DropColumn(
                name: "StatusUpdatedAtUtc",
                table: "RestaurantTables");
        }
    }
}
