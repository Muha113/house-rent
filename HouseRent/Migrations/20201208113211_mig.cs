using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace HouseRent.Migrations
{
    public partial class mig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Advertise",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Address = table.Column<string>(nullable: false),
                    Category = table.Column<string>(nullable: false),
                    ConfirmationStatus = table.Column<string>(nullable: true),
                    FlatDetails = table.Column<string>(nullable: false),
                    FlatSize = table.Column<int>(nullable: false),
                    FlatType = table.Column<string>(nullable: false),
                    Heading = table.Column<string>(nullable: true),
                    OtherBill = table.Column<int>(nullable: false),
                    Phone = table.Column<string>(nullable: false),
                    PostTime = table.Column<DateTime>(nullable: false),
                    Rent = table.Column<int>(nullable: false),
                    UserMail = table.Column<string>(nullable: true),
                    UtilitiesBill = table.Column<int>(nullable: false),
                    YoutubeLink = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Advertise", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Address = table.Column<string>(nullable: false),
                    Avatar = table.Column<byte[]>(nullable: true),
                    Contact = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Password = table.Column<string>(nullable: false),
                    Role = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.ID);
                    table.UniqueConstraint("AlternateKey_Email", x => x.Email);
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AdvertiseID = table.Column<int>(nullable: false),
                    Anonymous = table.Column<bool>(nullable: false),
                    CommentText = table.Column<string>(nullable: true),
                    CommentTime = table.Column<DateTime>(nullable: false),
                    Commenter = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Comment_Advertise_AdvertiseID",
                        column: x => x.AdvertiseID,
                        principalTable: "Advertise",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Compliment",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AdvertiseID = table.Column<int>(nullable: false),
                    Cleanness = table.Column<int>(nullable: false),
                    Comfort = table.Column<int>(nullable: false),
                    PriceQuality = table.Column<int>(nullable: false),
                    Reviewer = table.Column<string>(nullable: true),
                    Staff = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compliment", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Compliment_Advertise_AdvertiseID",
                        column: x => x.AdvertiseID,
                        principalTable: "Advertise",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Image",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AdvertiseID = table.Column<int>(nullable: false),
                    FlatImage = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Image", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Image_Advertise_AdvertiseID",
                        column: x => x.AdvertiseID,
                        principalTable: "Advertise",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RentRage",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AdvertiseID = table.Column<int>(nullable: false),
                    RentFrom = table.Column<DateTime>(nullable: false),
                    RentTo = table.Column<DateTime>(nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentRage", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RentRage_Advertise_AdvertiseID",
                        column: x => x.AdvertiseID,
                        principalTable: "Advertise",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AdvertiseID = table.Column<int>(nullable: false),
                    ReviewStar = table.Column<int>(nullable: false),
                    Reviewer = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Review", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Review_Advertise_AdvertiseID",
                        column: x => x.AdvertiseID,
                        principalTable: "Advertise",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdvertiseRequest",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AdvID = table.Column<int>(nullable: false),
                    From = table.Column<int>(nullable: false),
                    Status = table.Column<string>(nullable: true),
                    To = table.Column<int>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    UserID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertiseRequest", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AdvertiseRequest_Advertise_AdvID",
                        column: x => x.AdvID,
                        principalTable: "Advertise",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdvertiseRequest_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdvertiseRequest_AdvID",
                table: "AdvertiseRequest",
                column: "AdvID");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertiseRequest_UserID",
                table: "AdvertiseRequest",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_AdvertiseID",
                table: "Comment",
                column: "AdvertiseID");

            migrationBuilder.CreateIndex(
                name: "IX_Compliment_AdvertiseID",
                table: "Compliment",
                column: "AdvertiseID");

            migrationBuilder.CreateIndex(
                name: "IX_Image_AdvertiseID",
                table: "Image",
                column: "AdvertiseID");

            migrationBuilder.CreateIndex(
                name: "IX_RentRage_AdvertiseID",
                table: "RentRage",
                column: "AdvertiseID");

            migrationBuilder.CreateIndex(
                name: "IX_Review_AdvertiseID",
                table: "Review",
                column: "AdvertiseID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvertiseRequest");

            migrationBuilder.DropTable(
                name: "Comment");

            migrationBuilder.DropTable(
                name: "Compliment");

            migrationBuilder.DropTable(
                name: "Image");

            migrationBuilder.DropTable(
                name: "RentRage");

            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Advertise");
        }
    }
}
