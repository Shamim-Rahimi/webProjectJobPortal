﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortalWeb.Migrations
{
    /// <inheritdoc />
    public partial class token : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "registerTokenExpireTime",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "registerTokenExpireTime",
                table: "AspNetUsers");
        }
    }
}
