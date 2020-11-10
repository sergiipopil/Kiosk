using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KioskBrains.Server.Domain.Migrations
{
    public partial class ek_entities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CentralBankExchangeRates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    LocalCurrencyCode = table.Column<string>(maxLength: 3, nullable: false),
                    ForeignCurrencyCode = table.Column<string>(maxLength: 3, nullable: false),
                    DefaultOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CentralBankExchangeRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Alpha2 = table.Column<string>(maxLength: 2, nullable: false),
                    Alpha3 = table.Column<string>(maxLength: 3, nullable: false),
                    Code = table.Column<string>(maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    IsSystem = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    SupportPhone = table.Column<string>(maxLength: 50, nullable: true),
                    TimeZoneName = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DbPersistentCacheItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Key = table.Column<string>(maxLength: 255, nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbPersistentCacheItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EkImageCacheItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    ImageKey = table.Column<string>(maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EkImageCacheItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationLogRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Direction = table.Column<int>(nullable: false),
                    ExternalSystem = table.Column<string>(maxLength: 255, nullable: false),
                    RequestedOnUtc = table.Column<DateTime>(nullable: false),
                    ProcessingTime = table.Column<TimeSpan>(nullable: false),
                    Request = table.Column<string>(nullable: true),
                    Response = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationLogRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KioskStates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    TimeUtc = table.Column<DateTime>(nullable: false),
                    LocalTime = table.Column<DateTime>(nullable: false),
                    KioskVersion = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KioskStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KioskVersionUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    ApplicationType = table.Column<int>(nullable: false),
                    VersionName = table.Column<string>(maxLength: 255, nullable: false),
                    UpdateUrl = table.Column<string>(maxLength: 1000, nullable: false),
                    ReleaseNotes = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KioskVersionUpdates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationCallbackLogRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    ReceivedOnUtc = table.Column<DateTime>(nullable: false),
                    Channel = table.Column<string>(maxLength: 50, nullable: false),
                    MessageJson = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationCallbackLogRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CentralBankExchangeRateUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CentralBankExchangeRateId = table.Column<int>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CentralBankExchangeRateUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CentralBankExchangeRateUpdates_CentralBankExchangeRates_CentralBankExchangeRateId",
                        column: x => x.CentralBankExchangeRateId,
                        principalTable: "CentralBankExchangeRates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "States",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Code = table.Column<string>(maxLength: 2, nullable: false),
                    CountryId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => x.Id);
                    table.ForeignKey(
                        name: "FK_States_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationReceivers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CustomerId = table.Column<int>(nullable: false),
                    NotificationType = table.Column<int>(nullable: false),
                    Channel = table.Column<int>(nullable: false),
                    ReceiverId = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationReceivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationReceivers_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PortalUsers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CustomerId = table.Column<int>(nullable: false),
                    Username = table.Column<string>(maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(maxLength: 255, nullable: false),
                    Role = table.Column<int>(nullable: false),
                    FullName = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortalUsers_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KioskStateComponentInfos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    KioskStateId = table.Column<int>(nullable: false),
                    ComponentName = table.Column<string>(maxLength: 100, nullable: false),
                    StatusCode = table.Column<int>(nullable: false),
                    StatusMessage = table.Column<string>(nullable: true),
                    SpecificMonitorableStateJson = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KioskStateComponentInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KioskStateComponentInfos_KioskStates_KioskStateId",
                        column: x => x.KioskStateId,
                        principalTable: "KioskStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    AddressLine1 = table.Column<string>(maxLength: 255, nullable: true),
                    AddressLine2 = table.Column<string>(maxLength: 255, nullable: true),
                    City = table.Column<string>(maxLength: 50, nullable: true),
                    ZipCode = table.Column<string>(maxLength: 10, nullable: true),
                    StateId = table.Column<int>(nullable: true),
                    CountryId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Addresses_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kiosks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CustomerId = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    ApplicationType = table.Column<int>(nullable: false),
                    SerialKey = table.Column<string>(maxLength: 128, nullable: false),
                    WorkflowComponentConfigurationsJson = table.Column<string>(nullable: false),
                    LastPingedOnUtc = table.Column<DateTime>(nullable: true),
                    CurrentStateId = table.Column<int>(nullable: true),
                    AssignedKioskVersion = table.Column<string>(maxLength: 50, nullable: true),
                    AddressId = table.Column<int>(nullable: false),
                    CommaSeparatedLanguageCodes = table.Column<string>(maxLength: 255, nullable: true),
                    CommunicationComments = table.Column<string>(maxLength: 1000, nullable: true),
                    AdminModePassword = table.Column<string>(maxLength: 8, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kiosks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kiosks_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kiosks_KioskStates_CurrentStateId",
                        column: x => x.CurrentStateId,
                        principalTable: "KioskStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kiosks_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EkTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    KioskId = table.Column<int>(nullable: false),
                    UniqueId = table.Column<string>(maxLength: 255, nullable: false),
                    AddedOnUtc = table.Column<DateTime>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    LocalStartedOn = table.Column<DateTime>(nullable: false),
                    LocalEndedOn = table.Column<DateTime>(nullable: false),
                    CompletionStatus = table.Column<int>(nullable: true),
                    IsSentToEkSystem = table.Column<bool>(nullable: false),
                    NextSendingToEkTimeUtc = table.Column<DateTime>(nullable: true),
                    ProductsJson = table.Column<string>(nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(19,2)", nullable: false),
                    TotalPriceCurrencyCode = table.Column<string>(maxLength: 3, nullable: true),
                    PromoCode = table.Column<string>(maxLength: 20, nullable: true),
                    CustomerInfoJson = table.Column<string>(nullable: true),
                    DeliveryInfoJson = table.Column<string>(nullable: true),
                    ReceiptNumber = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EkTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EkTransactions_Kiosks_KioskId",
                        column: x => x.KioskId,
                        principalTable: "Kiosks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LogRecords",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Version = table.Column<byte[]>(rowVersion: true, nullable: true),
                    KioskId = table.Column<int>(nullable: false),
                    UniqueId = table.Column<string>(maxLength: 255, nullable: false),
                    KioskVersion = table.Column<string>(maxLength: 50, nullable: true),
                    LocalTime = table.Column<DateTime>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Context = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    AdditionalDataJson = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogRecords_Kiosks_KioskId",
                        column: x => x.KioskId,
                        principalTable: "Kiosks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CountryId",
                table: "Addresses",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_StateId",
                table: "Addresses",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_CentralBankExchangeRateUpdates_CentralBankExchangeRateId",
                table: "CentralBankExchangeRateUpdates",
                column: "CentralBankExchangeRateId");

            migrationBuilder.CreateIndex(
                name: "IX_DbPersistentCacheItems_Key",
                table: "DbPersistentCacheItems",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EkImageCacheItems_ImageKey",
                table: "EkImageCacheItems",
                column: "ImageKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EkTransactions_KioskId",
                table: "EkTransactions",
                column: "KioskId");

            migrationBuilder.CreateIndex(
                name: "IX_EkTransactions_LocalStartedOn",
                table: "EkTransactions",
                column: "LocalStartedOn");

            migrationBuilder.CreateIndex(
                name: "IX_EkTransactions_CompletionStatus_IsSentToEkSystem_NextSendingToEkTimeUtc",
                table: "EkTransactions",
                columns: new[] { "CompletionStatus", "IsSentToEkSystem", "NextSendingToEkTimeUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationLogRecords_ExternalSystem",
                table: "IntegrationLogRecords",
                column: "ExternalSystem");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationLogRecords_RequestedOnUtc",
                table: "IntegrationLogRecords",
                column: "RequestedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Kiosks_AddressId",
                table: "Kiosks",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Kiosks_CurrentStateId",
                table: "Kiosks",
                column: "CurrentStateId");

            migrationBuilder.CreateIndex(
                name: "IX_Kiosks_CustomerId",
                table: "Kiosks",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Kiosks_SerialKey",
                table: "Kiosks",
                column: "SerialKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KioskStateComponentInfos_KioskStateId",
                table: "KioskStateComponentInfos",
                column: "KioskStateId");

            migrationBuilder.CreateIndex(
                name: "IX_KioskVersionUpdates_VersionName",
                table: "KioskVersionUpdates",
                column: "VersionName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogRecords_Context",
                table: "LogRecords",
                column: "Context");

            migrationBuilder.CreateIndex(
                name: "IX_LogRecords_KioskId",
                table: "LogRecords",
                column: "KioskId");

            migrationBuilder.CreateIndex(
                name: "IX_LogRecords_LocalTime",
                table: "LogRecords",
                column: "LocalTime");

            migrationBuilder.CreateIndex(
                name: "IX_LogRecords_Type",
                table: "LogRecords",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationReceivers_CustomerId",
                table: "NotificationReceivers",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PortalUsers_CustomerId",
                table: "PortalUsers",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PortalUsers_Username",
                table: "PortalUsers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_States_CountryId",
                table: "States",
                column: "CountryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CentralBankExchangeRateUpdates");

            migrationBuilder.DropTable(
                name: "DbPersistentCacheItems");

            migrationBuilder.DropTable(
                name: "EkImageCacheItems");

            migrationBuilder.DropTable(
                name: "EkTransactions");

            migrationBuilder.DropTable(
                name: "IntegrationLogRecords");

            migrationBuilder.DropTable(
                name: "KioskStateComponentInfos");

            migrationBuilder.DropTable(
                name: "KioskVersionUpdates");

            migrationBuilder.DropTable(
                name: "LogRecords");

            migrationBuilder.DropTable(
                name: "NotificationCallbackLogRecords");

            migrationBuilder.DropTable(
                name: "NotificationReceivers");

            migrationBuilder.DropTable(
                name: "PortalUsers");

            migrationBuilder.DropTable(
                name: "CentralBankExchangeRates");

            migrationBuilder.DropTable(
                name: "Kiosks");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "KioskStates");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "States");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
