using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ptiw.Host.Migrations
{
    public partial class StillBuilding2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DoctorId",
                schema: "public",
                table: "NpcpnAppointments",
                type: "character varying(126)",
                maxLength: 126,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AppointmentTime",
                schema: "public",
                table: "NpcpnAppointments",
                type: "character varying(126)",
                maxLength: 126,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AppointmentDayOfWeek",
                schema: "public",
                table: "NpcpnAppointments",
                type: "character varying(24)",
                maxLength: 24,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AppointmentDate",
                schema: "public",
                table: "NpcpnAppointments",
                type: "character varying(126)",
                maxLength: 126,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DoctorId",
                schema: "public",
                table: "NpcpnAppointments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(126)",
                oldMaxLength: 126);

            migrationBuilder.AlterColumn<string>(
                name: "AppointmentTime",
                schema: "public",
                table: "NpcpnAppointments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(126)",
                oldMaxLength: 126);

            migrationBuilder.AlterColumn<string>(
                name: "AppointmentDayOfWeek",
                schema: "public",
                table: "NpcpnAppointments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(24)",
                oldMaxLength: 24);

            migrationBuilder.AlterColumn<string>(
                name: "AppointmentDate",
                schema: "public",
                table: "NpcpnAppointments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(126)",
                oldMaxLength: 126);
        }
    }
}