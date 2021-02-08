using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;

namespace CodeAround.FluentBatch.Test.Database.Migration
{
    [Migration(201907300900)]
    public class _001_CreatePersonTable : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("Persons")
              .WithColumn("PersonId").AsString(16).NotNullable().PrimaryKey()
              .WithColumn("Name").AsString(50).NotNullable()
              .WithColumn("Surname").AsString(50).NotNullable()
              .WithColumn("BirthdayDate").AsDateTime().NotNullable();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
