﻿namespace EthereumT.Api.Models.Dto
{
    public class WalletDto
    {
        public int Id { get; set; }

        public string Address { get; set; }

        public decimal Balance { get; set; }
    }
}