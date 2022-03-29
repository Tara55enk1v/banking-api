using BankApp.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BankApp.Core
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Account Authenticate(string AccountNumber, string Pin)
        {
            if (string.IsNullOrEmpty(AccountNumber) || string.IsNullOrEmpty(Pin))
                return null;
            var account = _dbContext.Accounts.SingleOrDefault(x => x.AccountNumberGenerated == AccountNumber);

            if (account == null)
                return null;

            if (!VerifyPinHash(Pin, account.PinStoredHash, account.PinStoredSalt))
                return null;

            return account;
        }

        private static bool VerifyPinHash(string Pin, byte[] pinHash, byte[] pinSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(pinSalt))
            {
                var computedPinHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(Pin));
                for (int i = 0; i < computedPinHash.Length; i++)
                {
                    if (computedPinHash[i] != pinHash[i]) return false;
                }
            }
            return true;
        }

        public Account Create(Account account, string Pin, string ConfirmPin)
        {
            if (string.IsNullOrWhiteSpace(Pin)) throw new ArgumentNullException("Pin cannot be empty");

            if (_dbContext.Accounts.Any(x => x.Email == account.Email)) throw new ApplicationException("A userr with thiss email exists");

            if (!Pin.Equals(ConfirmPin)) throw new ApplicationException("Pins do not match.");

            byte[] pinHash, pinSalt;
            CreatePinHash(Pin, out pinHash, out pinSalt);

            account.PinStoredHash = pinHash;
            account.PinStoredSalt = pinSalt;

            _dbContext.Accounts.Add(account);
            _dbContext.SaveChanges();

            return account;
        }

        private static void CreatePinHash(string Pin, out byte[] pinHash, out byte[] pinSalt)
        {
            if (string.IsNullOrEmpty(Pin)) throw new ArgumentNullException("Pin");
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                pinSalt = hmac.Key;
                pinHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(Pin));
            }
        }

        public void Delete(int Id)
        {
            var account = _dbContext.Accounts.Find(Id);
            if (account != null)
            {
                _dbContext.Accounts.Remove(account);

                _dbContext.SaveChanges();
            }
        }

        public IEnumerable<Account> GetAllAccounts()
        {
            return _dbContext.Accounts.ToList();
        }

        public Account GetById(int Id)
        {
            var account = _dbContext.Accounts.Where(x => x.Id == Id).FirstOrDefault();
            return account;
        }

        public void Update(Account account, string Pin = null)
        {
            var accountToBeUpdated = _dbContext.Accounts.Find(account.Id);

            if (accountToBeUpdated == null) throw new ApplicationException("Account not found");

            if (!string.IsNullOrWhiteSpace(account.Email) && account.Email != accountToBeUpdated.Email)
            {
                if (_dbContext.Accounts.Any(x => x.Email == account.Email)) throw new ApplicationException("Email " + account.Email + " has been taken");
                accountToBeUpdated.Email = account.Email;
            }

            if (!string.IsNullOrWhiteSpace(account.PhoneNumber) && account.Email != accountToBeUpdated.PhoneNumber)
            {
                if (_dbContext.Accounts.Any(x => x.PhoneNumber == account.PhoneNumber)) throw new ApplicationException("PhoneNumber " + account.PhoneNumber + " has been taken");
                accountToBeUpdated.PhoneNumber = account.PhoneNumber;
            }

            if (!string.IsNullOrWhiteSpace(Pin))
            {
                byte[] pinHash, pinSalt;
                CreatePinHash(Pin, out pinHash, out pinSalt);

                accountToBeUpdated.PinStoredHash = pinHash;
                accountToBeUpdated.PinStoredSalt = pinSalt;
            }

            _dbContext.Accounts.Update(accountToBeUpdated);
            _dbContext.SaveChanges();
        }

        public Account GetByAccountNumber(string AccountNumber)
        {
            var account = _dbContext.Accounts.Where(x => x.AccountNumberGenerated == AccountNumber).SingleOrDefault();
            if (account == null)
            {
                return null;
            }
            return account;
        }
    }
}
