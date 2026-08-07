using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bulky.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceStripeWithRazorpay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "OrderHeaders",
                newName: "RazorpaySignature");

            migrationBuilder.RenameColumn(
                name: "PaymentIntentId",
                table: "OrderHeaders",
                newName: "RazorpayPaymentId");

            migrationBuilder.AddColumn<string>(
                name: "RazorpayOrderId",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RazorpayOrderId",
                table: "OrderHeaders");

            migrationBuilder.RenameColumn(
                name: "RazorpaySignature",
                table: "OrderHeaders",
                newName: "SessionId");

            migrationBuilder.RenameColumn(
                name: "RazorpayPaymentId",
                table: "OrderHeaders",
                newName: "PaymentIntentId");
        }
    }
}
