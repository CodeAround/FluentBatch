using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAround.FluentBatch.Test.Infrastructure
{
    public class Card
    {
        public System.Guid? IdCard { get; set; }

        public System.Guid IdCardType { get; set; }
        
        public string CardOwner { get; set; }

        public string CardNumber { get; set; }

        public System.DateTime CardExpiryDate { get; set; }

        public decimal CardBalance { get; set; }

        public string Currency { get; set; }

        public Nullable<System.Guid> IdBankAccounts { get; set; }

        public string BankAccountOwner { get; set; }

        public string BankName { get; set; }
    }
}
