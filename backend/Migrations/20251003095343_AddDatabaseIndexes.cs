using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Status",
                table: "Users",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SkillsDevelopmentProviders_CreatedAt",
                table: "SkillsDevelopmentProviders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SkillsDevelopmentProviders_Email",
                table: "SkillsDevelopmentProviders",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_SkillsDevelopmentProviders_Status",
                table: "SkillsDevelopmentProviders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Order",
                table: "Modules",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Title",
                table: "Modules",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_Order",
                table: "Lessons",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_Title",
                table: "Lessons",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CreatedAt",
                table: "Departments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Status",
                table: "Departments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Type",
                table: "Departments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CreatedAt",
                table: "Courses",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Title",
                table: "Courses",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CreatedAt",
                table: "Clients",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                table: "Clients",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Status",
                table: "Clients",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_CreatedAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Role",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Status",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_SkillsDevelopmentProviders_CreatedAt",
                table: "SkillsDevelopmentProviders");

            migrationBuilder.DropIndex(
                name: "IX_SkillsDevelopmentProviders_Email",
                table: "SkillsDevelopmentProviders");

            migrationBuilder.DropIndex(
                name: "IX_SkillsDevelopmentProviders_Status",
                table: "SkillsDevelopmentProviders");

            migrationBuilder.DropIndex(
                name: "IX_Modules_Order",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Modules_Title",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_Order",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_Title",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Departments_CreatedAt",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_Name",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_Status",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_Type",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Courses_CreatedAt",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_Title",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Clients_CreatedAt",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Email",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Status",
                table: "Clients");
        }
    }
}
