using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Skylight.Infrastructure.Migrations
{
	/// <inheritdoc />
	public partial class AddUserCurrenciesTable : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "user_currencies",
				columns: table => new
				{
					user_id = table.Column<int>(type: "integer", nullable: false),
					currency = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
					balance = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_user_currencies", x => new { x.user_id, x.currency });
					table.CheckConstraint("ck_user_currencies_balance_non_negative", "\"balance\" >= 0");
					table.ForeignKey(
						name: "fk_user_currencies_user",
						column: x => x.user_id,
						principalTable: "users",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(name: "user_currencies");
		}
	}
}
