﻿using M4.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace M4.Infrastructure.Data.Context
{
    public class MagicFormulaDbContext : DbContext
    {
        public MagicFormulaDbContext(DbContextOptions<MagicFormulaDbContext> options) : base(options) { }

        public DbSet<EmailSolicitacao> EmailSolicitacao { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            FieldTypeHandler(modelBuilder, "VARCHAR(100)", typeof(string));
            FieldTypeHandler(modelBuilder, "DATETIME", typeof(DateTime));
            FieldTypeHandler(modelBuilder, "DATETIME", typeof(Nullable<DateTime>));
            FieldTypeHandler(modelBuilder, "DECIMAL(12,2)", typeof(decimal));

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MagicFormulaDbContext).Assembly);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys())) relationship.DeleteBehavior = DeleteBehavior.ClientSetNull;

            base.OnModelCreating(modelBuilder);
        }

        private static void FieldTypeHandler(ModelBuilder modelBuilder, string sqlFieldType, Type type)
        {
            foreach (var property in modelBuilder
                  .Model
                  .GetEntityTypes()
                  .SelectMany(
                     e => e.GetProperties()
                        .Where(p => p.ClrType == type)))
            {
                property.SetColumnType(sqlFieldType);
            }
        }

    }
}
