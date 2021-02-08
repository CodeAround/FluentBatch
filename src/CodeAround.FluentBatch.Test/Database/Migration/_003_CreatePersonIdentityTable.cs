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
    [Migration(201907301100)]
    public class _003_CreatePersonIdentityTable : FluentMigrator.Migration
    {
        public override void Up()
        {
            Create.Table("PersonIdentity")
              .WithColumn("ID").AsInt32().Identity().NotNullable().PrimaryKey()
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
