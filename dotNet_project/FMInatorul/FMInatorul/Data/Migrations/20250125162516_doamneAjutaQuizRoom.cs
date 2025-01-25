using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FMInatorul.Migrations
{
    public partial class doamneAjutaQuizRoom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnswerRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    ParticipantId = table.Column<int>(type: "int", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnswerRecords_IntrebariRasps_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "IntrebariRasps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_AnswerRecords_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "ParticipantId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "AnswerSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    ParticipantId = table.Column<int>(type: "int", nullable: false),
                    RoomCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnswerSubmissions_IntrebariRasps_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "IntrebariRasps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_AnswerSubmissions_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "ParticipantId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_ParticipantId",
                table: "AnswerRecords",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_QuestionId",
                table: "AnswerRecords",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerSubmissions_ParticipantId",
                table: "AnswerSubmissions",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerSubmissions_QuestionId",
                table: "AnswerSubmissions",
                column: "QuestionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnswerRecords");

            migrationBuilder.DropTable(
                name: "AnswerSubmissions");
        }
    }
}
