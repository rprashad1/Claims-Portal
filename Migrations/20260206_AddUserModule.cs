using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimsPortal.Migrations
{
    public partial class AddUserModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Baseline migration - database already contains the user module schema.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty - schema management handled externally
        }
    }
}
