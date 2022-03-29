using BankApp.DB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;

namespace BankApp.Core
{
    public class TransactionService : ITransactionService
    {
        private AppDbContext _dbContext;
        private AppSettings _settings;

        private ILogger<TransactionService> _logger;
        private IUserService _userService;
        private static string _bankSettlementAccount;

        public TransactionService(AppDbContext dbContext, ILogger<TransactionService> logger, IUserService userService, IOptions<AppSettings> settings)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userService = userService;
            _settings = settings.Value;
            _bankSettlementAccount = _settings.NetCoreBankSettlementAccount;
        }

        public Response CreateNewTransaction(Transaction transaction)
        {
            Response response = new Response();
            try
            {
                _dbContext.Transactions.Add(transaction);
                _dbContext.SaveChanges();

                response.ResponseCode = "00";
                response.ResponseMessage = "Transaction created successfully!";
                response.Data = null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED => {ex.Message}");
            }
            return response;
        }

        public Response FindTransactionByDate(DateTime date)
        {
            throw new NotImplementedException();
        }

        public Response MakeDeposit(string AccountNumber, decimal Amount, string TransactionPin)
        {
            Response response = new Response();
            Account sourceAccount; 
            Account destinationAccount; 
            Transaction transaction = new Transaction();

            var authenticateUser = _userService.Authenticate(AccountNumber, TransactionPin);

            if (authenticateUser == null)
            {
                throw new ApplicationException("Invalid Auth details");
            }

            try
            {
                sourceAccount = _userService.GetByAccountNumber(_bankSettlementAccount);
                destinationAccount = _userService.GetByAccountNumber(AccountNumber);

                sourceAccount.CurrentAccountBalance -= Amount;
                destinationAccount.CurrentAccountBalance += Amount;

                if ((_dbContext.Entry(sourceAccount).State == Microsoft.EntityFrameworkCore.EntityState.Modified) && (_dbContext.Entry(destinationAccount).State == Microsoft.EntityFrameworkCore.EntityState.Modified))
                {
                    transaction.TransactionStatus = TranStatus.Success;
                    response.ResponseCode = "00";
                    response.ResponseMessage = "Transaction Successful!";
                    response.Data = null;
                }
                else
                {
                    transaction.TransactionStatus = TranStatus.Failed;
                    response.ResponseCode = "00";
                    response.ResponseMessage = "Transaction Failed!";
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR OCCURRED => MESSAGE: {ex.Message}");
            }

            transaction.TransactionDate = DateTime.Now;
            transaction.TransactionType = TranType.Deposit;
            transaction.TransactionAmount = Amount;
            transaction.TransactionSourceAccount = _bankSettlementAccount;
            transaction.TransactionDestinationAccount = AccountNumber;
            transaction.TransactionParticulars = $"NEW Transaction FROM SOURCE {JsonConvert.SerializeObject(transaction.TransactionSourceAccount)} TO DESTINATION => {JsonConvert.SerializeObject(transaction.TransactionDestinationAccount)} ON {transaction.TransactionDate} TRAN_TYPE =>  {transaction.TransactionType} TRAN_STATUS => {transaction.TransactionStatus}";

            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();

            return response;

        }

        public Response MakeFundsTransfer(string FromAccount, string ToAccount, decimal Amount, string TransactionPin)
        {
            Response response = new Response();
            Account sourceAccount;
            Account destinationAccount;
            Transaction transaction = new Transaction();

            var authenticateUser = _userService.Authenticate(FromAccount, TransactionPin);
            if (authenticateUser == null)
            {
                throw new ApplicationException("Invalid credentials");
            }

            try
            {
                sourceAccount = _userService.GetByAccountNumber(FromAccount);
                destinationAccount = _userService.GetByAccountNumber(ToAccount);

                sourceAccount.CurrentAccountBalance -= Amount; 
                destinationAccount.CurrentAccountBalance += Amount;

                if ((_dbContext.Entry(sourceAccount).State == Microsoft.EntityFrameworkCore.EntityState.Modified) && (_dbContext.Entry(destinationAccount).State == Microsoft.EntityFrameworkCore.EntityState.Modified))
                {
                    transaction.TransactionStatus = TranStatus.Success;
                    response.ResponseCode = "00";
                    response.ResponseMessage = "Transaction Successful!";
                    response.Data = null;
                }
                else
                {
                    transaction.TransactionStatus = TranStatus.Failed;
                    response.ResponseCode = "00";
                    response.ResponseMessage = "Transaction Failed!";
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED => MESSAGE: {ex.Message}");
            }

            transaction.TransactionDate = DateTime.Now;
            transaction.TransactionType = TranType.Transfer;
            transaction.TransactionAmount = Amount;
            transaction.TransactionSourceAccount = FromAccount;
            transaction.TransactionDestinationAccount = ToAccount;
            transaction.TransactionParticulars = $"NEW Transaction FROM SOURCE {JsonConvert.SerializeObject(transaction.TransactionSourceAccount)} TO DESTINATION => {JsonConvert.SerializeObject(transaction.TransactionDestinationAccount)} ON {transaction.TransactionDate} TRAN_TYPE =>  {transaction.TransactionType} TRAN_STATUS => {transaction.TransactionStatus}";

            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();

            return response;
        }

        public Response MakeWithdrawal(string AccountNumber, decimal Amount, string TransactionPin)
        {
            Response response = new Response();
            Account sourceAccount;
            Account destinationAccount;
            Transaction transaction = new Transaction();

            var authenticateUser = _userService.Authenticate(AccountNumber, TransactionPin);
            if (authenticateUser == null)
            {
                throw new ApplicationException("Invalid Auth details");
            }

            try
            {
                sourceAccount = _userService.GetByAccountNumber(AccountNumber);
                destinationAccount = _userService.GetByAccountNumber(_bankSettlementAccount);

                sourceAccount.CurrentAccountBalance -= Amount; 
                destinationAccount.CurrentAccountBalance += Amount; 

                if ((_dbContext.Entry(sourceAccount).State == Microsoft.EntityFrameworkCore.EntityState.Modified) && (_dbContext.Entry(destinationAccount).State == Microsoft.EntityFrameworkCore.EntityState.Modified))
                {
                    transaction.TransactionStatus = TranStatus.Success;
                    response.ResponseCode = "00";
                    response.ResponseMessage = "Transaction Successful!";
                    response.Data = null;
                }
                else
                {
                    transaction.TransactionStatus = TranStatus.Failed;
                    response.ResponseCode = "00";
                    response.ResponseMessage = "Transaction Failed!";
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED => MESSAGE: {ex.Message}");
            }

            transaction.TransactionDate = DateTime.Now;
            transaction.TransactionType = TranType.Withdrawal;
            transaction.TransactionAmount = Amount;
            transaction.TransactionSourceAccount = _bankSettlementAccount;
            transaction.TransactionDestinationAccount = AccountNumber;
            transaction.TransactionParticulars = $"NEW Transaction FROM SOURCE {JsonConvert.SerializeObject(transaction.TransactionSourceAccount)} TO DESTINATION => {JsonConvert.SerializeObject(transaction.TransactionDestinationAccount)} ON {transaction.TransactionDate} TRAN_TYPE =>  {transaction.TransactionType} TRAN_STATUS => {transaction.TransactionStatus}";

            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();

            return response;
        }
    }
}