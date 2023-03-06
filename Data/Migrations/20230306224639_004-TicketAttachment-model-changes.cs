using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CSBugTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class _004TicketAttachmentmodelchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageType",
                table: "TicketAttachments",
                newName: "FileContentType");

            migrationBuilder.RenameColumn(
                name: "ImageData",
                table: "TicketAttachments",
                newName: "FileData");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "TicketAttachments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "TicketAttachments");

            migrationBuilder.RenameColumn(
                name: "FileData",
                table: "TicketAttachments",
                newName: "ImageData");

            migrationBuilder.RenameColumn(
                name: "FileContentType",
                table: "TicketAttachments",
                newName: "ImageType");
        }
    }
}
