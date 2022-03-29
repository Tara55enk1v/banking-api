using AutoMapper;
using BankApp.Core;
using BankApp.DB;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace BankApp.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private IMapper _mapper;

        public TransactionsController(ITransactionService transactionService, IMapper mapper)
        {
            _transactionService = transactionService;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("create_new_transaction")]
        public IActionResult CreateNewTransation([FromBody] TransactionRequestDto transaction)
        {
            if (!ModelState.IsValid) return BadRequest(transaction);

            var transactionRequest = _mapper.Map<Transaction>(transaction);

            return Ok(_transactionService.CreateNewTransaction(transactionRequest));
        }

        [HttpPost]
        [Route("create_deposit_operation")]
        public IActionResult MakeDeposit(string AccountNumber, decimal Amount, string TransactionPin)
        {
            return Ok(_transactionService.MakeDeposit(AccountNumber, Amount, TransactionPin));
        }

        [HttpPost]
        [Route("create_funds_transfer_operation")]
        public IActionResult MakeFundsTransfer(string FromAccount, string ToAccount, decimal Amount, string TransactionPin)
        {
            if (FromAccount.Equals(ToAccount)) return BadRequest("You cannot transfer money to yourself");

            return Ok(_transactionService.MakeFundsTransfer(FromAccount, ToAccount, Amount, TransactionPin));
        }

        [HttpPost]
        [Route("create_withdrawal_operation")]
        public IActionResult MakeWithdrawal(string AccountNumber, decimal Amount, string TransactionPin)
        {
            if (!Regex.IsMatch(AccountNumber, @"^[0][1-9]\d{9}$|^[1-9]\d{9}$")) return BadRequest("Your Account Number can only be 10 digits");

            return Ok(_transactionService.MakeWithdrawal(AccountNumber, Amount, TransactionPin));
        }

    }
}