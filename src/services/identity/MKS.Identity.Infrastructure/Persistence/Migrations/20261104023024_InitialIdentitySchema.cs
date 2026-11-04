using System;
using MKS.Identity.Domain.Shared.Constants;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MKS.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentitySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "Apps",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    MasterStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RestorerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActingAsUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImpersonatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    BreakGlassGrantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: true),
                    Details = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BreakGlassGrants",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReadOnly = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreakGlassGrants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InboxMessages",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictApplications",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ClientId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ClientSecret = table.Column<string>(type: "text", nullable: true),
                    ClientType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConsentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    DisplayNames = table.Column<string>(type: "text", nullable: true),
                    JsonWebKeySet = table.Column<string>(type: "text", nullable: true),
                    Permissions = table.Column<string>(type: "text", nullable: true),
                    PostLogoutRedirectUris = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    RedirectUris = table.Column<string>(type: "text", nullable: true),
                    Requirements = table.Column<string>(type: "text", nullable: true),
                    Settings = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictScopes",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConcurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Descriptions = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    DisplayNames = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    Resources = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictScopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformRoleAssignments",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleCode = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RestorerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformRoleAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    Thumbnail = table.Column<string>(type: "text", nullable: true),
                    MasterStatus = table.Column<int>(type: "integer", nullable: false),
                    MembershipVersion = table.Column<long>(type: "bigint", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RestorerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppFunctions",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    NameKey = table.Column<string>(type: "text", nullable: false),
                    Deprecated = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RestorerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppFunctions", x => x.Id);
                    table.UniqueConstraint("AK_AppFunctions_AppId_Code", x => new { x.AppId, x.Code });
                    table.ForeignKey(
                        name: "FK_AppFunctions_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "identity",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    MasterStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RestorerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                    table.UniqueConstraint("AK_Tenants_AppId_Id", x => new { x.AppId, x.Id });
                    table.ForeignKey(
                        name: "FK_Tenants_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "identity",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictAuthorizations",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    Scopes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictAuthorizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenIddictAuthorizations_OpenIddictApplications_Application~",
                        column: x => x.ApplicationId,
                        principalSchema: "identity",
                        principalTable: "OpenIddictApplications",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppAdminAssignments",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RestorerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAdminAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAdminAssignments_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "identity",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppAdminAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserCredentials",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "boolean", nullable: false),
                    FailedAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockedUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PasswordChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RestorerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCredentials_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserIdentifiers",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    NormalizedValue = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    IsDerived = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RestorerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIdentifiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserIdentifiers_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AppId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RestorerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppPermissions",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FunctionCode = table.Column<string>(type: "character varying(160)", nullable: true),
                    NameKey = table.Column<string>(type: "text", nullable: false),
                    Deprecated = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RestorerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPermissions", x => x.Id);
                    table.UniqueConstraint("AK_AppPermissions_AppId_Code", x => new { x.AppId, x.Code });
                    table.ForeignKey(
                        name: "FK_AppPermissions_AppFunctions_AppId_FunctionCode",
                        columns: x => new { x.AppId, x.FunctionCode },
                        principalSchema: "identity",
                        principalTable: "AppFunctions",
                        principalColumns: new[] { "AppId", "Code" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppPermissions_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "identity",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    MasterStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RestorerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.UniqueConstraint("AK_Roles_AppId_Id", x => new { x.AppId, x.Id });
                    table.ForeignKey(
                        name: "FK_Roles_Apps_AppId",
                        column: x => x.AppId,
                        principalSchema: "identity",
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Roles_Tenants_AppId_TenantId",
                        columns: x => new { x.AppId, x.TenantId },
                        principalSchema: "identity",
                        principalTable: "Tenants",
                        principalColumns: new[] { "AppId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantMemberships",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RestorerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantMemberships", x => x.Id);
                    table.UniqueConstraint("AK_TenantMemberships_TenantId_UserId", x => new { x.TenantId, x.UserId });
                    table.UniqueConstraint("AK_TenantMemberships_TenantId_UserId_AppId", x => new { x.TenantId, x.UserId, x.AppId });
                    table.ForeignKey(
                        name: "FK_TenantMemberships_Tenants_AppId_TenantId",
                        columns: x => new { x.AppId, x.TenantId },
                        principalSchema: "identity",
                        principalTable: "Tenants",
                        principalColumns: new[] { "AppId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantMemberships_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictTokens",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorizationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Payload = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    RedemptionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReferenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenIddictTokens_OpenIddictApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "identity",
                        principalTable: "OpenIddictApplications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId",
                        column: x => x.AuthorizationId,
                        principalSchema: "identity",
                        principalTable: "OpenIddictAuthorizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoleFunctions",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    FunctionCode = table.Column<string>(type: "character varying(160)", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RestorerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleFunctions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleFunctions_AppFunctions_AppId_FunctionCode",
                        columns: x => new { x.AppId, x.FunctionCode },
                        principalSchema: "identity",
                        principalTable: "AppFunctions",
                        principalColumns: new[] { "AppId", "Code" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleFunctions_Roles_AppId_RoleId",
                        columns: x => new { x.AppId, x.RoleId },
                        principalSchema: "identity",
                        principalTable: "Roles",
                        principalColumns: new[] { "AppId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionCode = table.Column<string>(type: "character varying(200)", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RestorerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_AppPermissions_AppId_PermissionCode",
                        columns: x => new { x.AppId, x.PermissionCode },
                        principalSchema: "identity",
                        principalTable: "AppPermissions",
                        principalColumns: new[] { "AppId", "Code" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_AppId_RoleId",
                        columns: x => new { x.AppId, x.RoleId },
                        principalSchema: "identity",
                        principalTable: "Roles",
                        principalColumns: new[] { "AppId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleAssignments",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RestorerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleAssignments_Roles_AppId_RoleId",
                        columns: x => new { x.AppId, x.RoleId },
                        principalSchema: "identity",
                        principalTable: "Roles",
                        principalColumns: new[] { "AppId", "Id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleAssignments_TenantMemberships_TenantId_UserId_AppId",
                        columns: x => new { x.TenantId, x.UserId, x.AppId },
                        principalSchema: "identity",
                        principalTable: "TenantMemberships",
                        principalColumns: new[] { "TenantId", "UserId", "AppId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantAdminAssignments",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RestorerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RestoredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantAdminAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantAdminAssignments_TenantMemberships_TenantId_UserId",
                        columns: x => new { x.TenantId, x.UserId },
                        principalSchema: "identity",
                        principalTable: "TenantMemberships",
                        principalColumns: new[] { "TenantId", "UserId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAdminAssignments_AppId_UserId",
                schema: "identity",
                table: "AppAdminAssignments",
                columns: new[] { "AppId", "UserId" },
                unique: true,
                filter: "\"IsActive\" = TRUE AND \"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_AppAdminAssignments_UserId",
                schema: "identity",
                table: "AppAdminAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppPermissions_AppId_FunctionCode",
                schema: "identity",
                table: "AppPermissions",
                columns: new[] { "AppId", "FunctionCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Apps_Code",
                schema: "identity",
                table: "Apps",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OccurredAt",
                schema: "identity",
                table: "AuditLogs",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_BreakGlassGrants_TenantId_UserId_Status",
                schema: "identity",
                table: "BreakGlassGrants",
                columns: new[] { "TenantId", "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_EventId",
                schema: "identity",
                table: "InboxMessages",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictApplications_ClientId",
                schema: "identity",
                table: "OpenIddictApplications",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type",
                schema: "identity",
                table: "OpenIddictAuthorizations",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictScopes_Name",
                schema: "identity",
                table: "OpenIddictScopes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_ApplicationId_Status_Subject_Type",
                schema: "identity",
                table: "OpenIddictTokens",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_AuthorizationId",
                schema: "identity",
                table: "OpenIddictTokens",
                column: "AuthorizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_ReferenceId",
                schema: "identity",
                table: "OpenIddictTokens",
                column: "ReferenceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedAt",
                schema: "identity",
                table: "OutboxMessages",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRoleAssignments_UserId_RoleCode",
                schema: "identity",
                table: "PlatformRoleAssignments",
                columns: new[] { "UserId", "RoleCode" },
                unique: true,
                filter: "\"IsActive\" = TRUE AND \"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_AppId_RoleId",
                schema: "identity",
                table: "RoleAssignments",
                columns: new[] { "AppId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_TenantId_UserId_AppId",
                schema: "identity",
                table: "RoleAssignments",
                columns: new[] { "TenantId", "UserId", "AppId" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_TenantId_UserId_RoleId",
                schema: "identity",
                table: "RoleAssignments",
                columns: new[] { "TenantId", "UserId", "RoleId" },
                unique: true,
                filter: "\"IsActive\" = TRUE AND \"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_RoleFunctions_AppId_FunctionCode",
                schema: "identity",
                table: "RoleFunctions",
                columns: new[] { "AppId", "FunctionCode" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleFunctions_AppId_RoleId_FunctionCode",
                schema: "identity",
                table: "RoleFunctions",
                columns: new[] { "AppId", "RoleId", "FunctionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_AppId_PermissionCode",
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "AppId", "PermissionCode" });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_AppId_RoleId_PermissionCode",
                schema: "identity",
                table: "RolePermissions",
                columns: new[] { "AppId", "RoleId", "PermissionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_AppId_TenantId_Code",
                schema: "identity",
                table: "Roles",
                columns: new[] { "AppId", "TenantId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_TenantAdminAssignments_TenantId_UserId",
                schema: "identity",
                table: "TenantAdminAssignments",
                columns: new[] { "TenantId", "UserId" },
                unique: true,
                filter: "\"IsActive\" = TRUE AND \"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_TenantMemberships_AppId_TenantId",
                schema: "identity",
                table: "TenantMemberships",
                columns: new[] { "AppId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantMemberships_TenantId_UserId",
                schema: "identity",
                table: "TenantMemberships",
                columns: new[] { "TenantId", "UserId" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_TenantMemberships_UserId",
                schema: "identity",
                table: "TenantMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_AppId_Code",
                schema: "identity",
                table: "Tenants",
                columns: new[] { "AppId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_UserCredentials_UserId",
                schema: "identity",
                table: "UserCredentials",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserIdentifiers_NormalizedValue",
                schema: "identity",
                table: "UserIdentifiers",
                column: "NormalizedValue",
                unique: true,
                filter: "\"IsDeleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_UserIdentifiers_UserId_Type",
                schema: "identity",
                table: "UserIdentifiers",
                columns: new[] { "UserId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_MasterStatus",
                schema: "identity",
                table: "Users",
                column: "MasterStatus");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId_RevokedAt",
                schema: "identity",
                table: "UserSessions",
                columns: new[] { "UserId", "RevokedAt" });

            migrationBuilder.Sql($$"""
                ALTER TABLE identity."TenantMemberships" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE identity."TenantMemberships" FORCE ROW LEVEL SECURITY;
                CREATE POLICY tenant_membership_isolation ON identity."TenantMemberships"
                    USING ("TenantId" = NULLIF(current_setting('app.current_tenant_id', TRUE), '')::uuid)
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.current_tenant_id', TRUE), '')::uuid);

                ALTER TABLE identity."TenantAdminAssignments" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE identity."TenantAdminAssignments" FORCE ROW LEVEL SECURITY;
                CREATE POLICY tenant_admin_assignment_isolation ON identity."TenantAdminAssignments"
                    USING ("TenantId" = NULLIF(current_setting('app.current_tenant_id', TRUE), '')::uuid)
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.current_tenant_id', TRUE), '')::uuid);

                ALTER TABLE identity."RoleAssignments" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE identity."RoleAssignments" FORCE ROW LEVEL SECURITY;
                CREATE POLICY role_assignment_isolation ON identity."RoleAssignments"
                    USING ("TenantId" = NULLIF(current_setting('app.current_tenant_id', TRUE), '')::uuid)
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.current_tenant_id', TRUE), '')::uuid);

                CREATE FUNCTION identity.prevent_last_app_admin_removal()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $function$
                DECLARE
                    target_app uuid;
                    remaining_count integer;
                BEGIN
                    IF TG_OP = 'UPDATE' AND (
                        (OLD."IsActive" AND NOT NEW."IsActive") OR
                        (NOT OLD."IsDeleted" AND NEW."IsDeleted")
                    ) THEN
                        target_app := OLD."AppId";
                    ELSIF TG_OP = 'DELETE' AND OLD."IsActive" AND NOT OLD."IsDeleted" THEN
                        target_app := OLD."AppId";
                    ELSE
                        RETURN NULL;
                    END IF;

                    PERFORM pg_advisory_xact_lock(hashtextextended('app-admin:' || target_app::text, 0));
                    SELECT COUNT(*) INTO remaining_count
                    FROM identity."AppAdminAssignments"
                    WHERE "AppId" = target_app AND "IsActive" AND NOT "IsDeleted";
                    IF remaining_count = 0 THEN
                        RAISE EXCEPTION '{{IdentityDomainErrorCodes.LastAdministrator}}'
                            USING ERRCODE = '23514';
                    END IF;
                    RETURN NULL;
                END;
                $function$;

                CREATE CONSTRAINT TRIGGER ensure_app_has_admin
                    AFTER UPDATE OR DELETE ON identity."AppAdminAssignments"
                    DEFERRABLE INITIALLY IMMEDIATE
                    FOR EACH ROW EXECUTE FUNCTION identity.prevent_last_app_admin_removal();

                CREATE FUNCTION identity.prevent_last_tenant_admin_removal()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $function$
                DECLARE
                    target_tenant uuid;
                    remaining_count integer;
                BEGIN
                    IF TG_OP = 'UPDATE' AND (
                        (OLD."IsActive" AND NOT NEW."IsActive") OR
                        (NOT OLD."IsDeleted" AND NEW."IsDeleted")
                    ) THEN
                        target_tenant := OLD."TenantId";
                    ELSIF TG_OP = 'DELETE' AND OLD."IsActive" AND NOT OLD."IsDeleted" THEN
                        target_tenant := OLD."TenantId";
                    ELSE
                        RETURN NULL;
                    END IF;

                    PERFORM pg_advisory_xact_lock(hashtextextended('tenant-admin:' || target_tenant::text, 0));
                    SELECT COUNT(*) INTO remaining_count
                    FROM identity."TenantAdminAssignments"
                    WHERE "TenantId" = target_tenant AND "IsActive" AND NOT "IsDeleted";
                    IF remaining_count = 0 THEN
                        RAISE EXCEPTION '{{IdentityDomainErrorCodes.LastAdministrator}}'
                            USING ERRCODE = '23514';
                    END IF;
                    RETURN NULL;
                END;
                $function$;

                CREATE CONSTRAINT TRIGGER ensure_tenant_has_admin
                    AFTER UPDATE OR DELETE ON identity."TenantAdminAssignments"
                    DEFERRABLE INITIALLY IMMEDIATE
                    FOR EACH ROW EXECUTE FUNCTION identity.prevent_last_tenant_admin_removal();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS ensure_app_has_admin ON identity."AppAdminAssignments";
                DROP TRIGGER IF EXISTS ensure_tenant_has_admin ON identity."TenantAdminAssignments";
                DROP FUNCTION IF EXISTS identity.prevent_last_app_admin_removal();
                DROP FUNCTION IF EXISTS identity.prevent_last_tenant_admin_removal();
                DROP POLICY IF EXISTS tenant_membership_isolation ON identity."TenantMemberships";
                DROP POLICY IF EXISTS tenant_admin_assignment_isolation ON identity."TenantAdminAssignments";
                DROP POLICY IF EXISTS role_assignment_isolation ON identity."RoleAssignments";
                """);

            migrationBuilder.DropTable(
                name: "AppAdminAssignments",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "BreakGlassGrants",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "InboxMessages",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "OpenIddictScopes",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "OpenIddictTokens",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "PlatformRoleAssignments",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "RoleAssignments",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "RoleFunctions",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "RolePermissions",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "TenantAdminAssignments",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserCredentials",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserIdentifiers",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserSessions",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "OpenIddictAuthorizations",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "AppPermissions",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "TenantMemberships",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "OpenIddictApplications",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "AppFunctions",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Tenants",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Apps",
                schema: "identity");
        }
    }
}
