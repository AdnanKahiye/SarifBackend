using AutoMapper;
using AutoMapper.QueryableExtensions;
using Backend.DTOs.Requests.Accounts;
using Backend.DTOs.Responses.Accounts;
using Backend.Interfaces;
using Backend.Interfaces.Accounts;
using Backend.Models.Accounts; // Assuming this is where ExchangeRate model lives
using Backend.Persistence;
using Backend.Utiliy;
using Backend.Wrapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Services.Accounts
{
    public class AccountService : CacheService, IAccountService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        private const string ExchangeRateCacheKey = "ExchangeRateCache";
        private const string CurrencyCacheKey = "CurrencyCache";
        private const string AccountCacheKey = "AccountCache";
        private const string TransactionCacheKey = "TransactionCache";

        public AccountService(
            AppDbContext context,
            IMapper mapper,
            IMemoryCache cache,
            ICurrentUserService currentUser) : base(cache)
        {
            _context = context;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        // ================================
        // CREATE
        // ================================
        public async Task<ResponseWrapper<int>> CreateExchangeRateAsync(CreateExchangeRateDto dto)
        {
            if (string.IsNullOrEmpty(_currentUser.UserId))
            {
                return await ResponseWrapper<int>.FailureAsync("Unauthorized", "User not authenticated", 401);
            }

            return await ExecuteWriteAsync(async () =>
            {
                var entity = _mapper.Map<ExchangeRate>(dto);

                entity.UserId = _currentUser.UserId;
                entity.AgencyId = _currentUser.AgencyId;
                entity.BranchId = _currentUser.BranchId;
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;

                // Priority Logic: If DTO doesn't provide IDs, use current user's context
                entity.AgencyId ??= _currentUser.AgencyId;

                _context.ExchangeRates.Add(entity);
                await _context.SaveChangesAsync();

                RemoveByPrefix(ExchangeRateCacheKey);

                return entity.Id;
            }, "Exchange rate created successfully", "Error creating exchange rate");
        }

        // ================================
        // GET ALL (With Multi-Tenant Filter)
        // ================================
        public async Task<ResponseWrapper<PagedResponse<ExchangeRateDto>>> GetAllExchangeRateAsync(int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                cacheKey: $"{ExchangeRateCacheKey}_{_currentUser.UserId}_{page}_{pageSize}",
                action: async () =>
                {
                    var query = _context.ExchangeRates
                        .Include(x => x.Currency)
                        .Include(x => x.Branch)
                        .Include(x => x.Agency)
                        .AsNoTracking();

                    // Security Filter
                    if (!isAdmin)
                    {
                        query = query.Where(x => x.AgencyId == agencyId);
                    }

                    var totalRecords = await query.CountAsync();

                    var mapped = await query
                        .OrderByDescending(x => x.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ProjectTo<ExchangeRateDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<ExchangeRateDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} exchange rates fetched",
                cacheMessage: "Exchange rates fetched from cache",
                errorMessage: "Error fetching exchange rates"
            );
        }

        // ================================
        // UPDATE
        // ================================
        public async Task<ResponseWrapper<bool>> UpdateExchangeRateAsync(int id, UpdateExchangeRateDto dto)
        {
            return await ExecuteWriteAsync(async () =>
            {
                var entity = await _context.ExchangeRates.FindAsync(id);

                if (entity == null)
                    throw new Exception("Exchange rate not found");

                // Check ownership if not admin
                if (entity.AgencyId != _currentUser.AgencyId)
                    throw new Exception("Unauthorized access to this record");

                _mapper.Map(dto, entity);
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = _currentUser.UserId;
                entity.BranchId = _currentUser.BranchId;
                entity.AgencyId = _currentUser.AgencyId;

                await _context.SaveChangesAsync();
                RemoveByPrefix(ExchangeRateCacheKey);

                return true;
            }, "Exchange rate updated successfully", "Error updating exchange rate");
        }

        // ================================
        // DELETE
        // ================================
        public async Task<ResponseWrapper<bool>> DeleteExchangeRateAsync(int id)
        {
            return await ExecuteWriteAsync(async () =>
            {
                var entity = await _context.ExchangeRates.FindAsync(id);

                if (entity == null)
                    throw new Exception("Exchange rate not found");

                _context.ExchangeRates.Remove(entity);
                await _context.SaveChangesAsync();

                RemoveByPrefix(ExchangeRateCacheKey);

                return true;
            }, "Exchange rate deleted successfully", "Error deleting exchange rate");
        }


        // ================================
        // CREATE CURRENCY
        // ================================
        public async Task<ResponseWrapper<int>> CreateCurrencyAsync(CreateCurrencyDto dto)
        {
            if (string.IsNullOrEmpty(_currentUser.UserId))
            {
                return await ResponseWrapper<int>.FailureAsync("Unauthorized", "User not authenticated", 401);
            }

            return await ExecuteWriteAsync(async () =>
            {
                // 1. If this is marked as Base, unset any existing base currency for this user/agency
                if (dto.IsBase)
                {
                    var existingBase = await _context.Currencies
                        .Where(x => x.UserId == _currentUser.UserId && x.IsBase)
                        .ToListAsync();

                    foreach (var b in existingBase) b.IsBase = false;
                }

                // 2. Map and Setup Entity
                var entity = _mapper.Map<Currency>(dto);
                entity.UserId = _currentUser.UserId;
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;

                _context.Currencies.Add(entity);
                await _context.SaveChangesAsync();

                RemoveByPrefix(CurrencyCacheKey);

                return entity.Id;
            }, "Currency created successfully", "Error creating currency");
        }

        // ================================
        // GET ALL CURRENCIES
        // ================================
        public async Task<ResponseWrapper<PagedResponse<CurrencyDto>>> GetAllCurrencyAsync(int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            return await ExecuteWithCacheAsync(
                cacheKey: $"{CurrencyCacheKey}_{_currentUser.UserId}_P{page}_PS{pageSize}",
                action: async () =>
                {
                    var query = _context.Currencies
                        .Where(x => x.UserId == _currentUser.UserId)
                        .AsNoTracking();

                    var totalRecords = await query.CountAsync();

                    var mapped = await query
                        .OrderBy(x => x.Code)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ProjectTo<CurrencyDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<CurrencyDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} currencies fetched",
                cacheMessage: "Currencies loaded from cache",
                errorMessage: "Error fetching currencies"
            );
        }

        // ================================
        // UPDATE CURRENCY
        // ================================
        public async Task<ResponseWrapper<bool>> UpdateCurrencyAsync(int id, UpdateCurrencyDto dto)
        {
            return await ExecuteWriteAsync(async () =>
            {
                var entity = await _context.Currencies.FindAsync(id);

                if (entity == null)
                    throw new Exception("Currency not found");

                // Handle IsBase logic update
                if (dto.IsBase && !entity.IsBase)
                {
                    var existingBase = await _context.Currencies
                        .Where(x => x.UserId == _currentUser.UserId && x.Id != id && x.IsBase)
                        .ToListAsync();

                    foreach (var b in existingBase) b.IsBase = false;
                }

                _mapper.Map(dto, entity);
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = _currentUser.UserId;

                await _context.SaveChangesAsync();
                RemoveByPrefix(CurrencyCacheKey);

                return true;
            }, "Currency updated successfully", "Error updating currency");
        }

        // ================================
        // DELETE CURRENCY
        // ================================
        public async Task<ResponseWrapper<bool>> DeleteCurrencyAsync(int id)
        {
            return await ExecuteWriteAsync(async () =>
            {
                var entity = await _context.Currencies.FindAsync(id);

                if (entity == null)
                    throw new Exception("Currency not found");

                // Optional: Prevent deleting the Base currency
                if (entity.IsBase)
                    throw new Exception("Cannot delete the base currency. Assign another base currency first.");

                _context.Currencies.Remove(entity);
                await _context.SaveChangesAsync();

                RemoveByPrefix(CurrencyCacheKey);

                return true;
            }, "Currency deleted successfully", "Error deleting currency");
        }






        // ================================
        // CREATE ACCOUNT
        // ================================
        public async Task<ResponseWrapper<Guid>> CreateAccountAsync(CreateAccountDto dto)
        {
            if (string.IsNullOrEmpty(_currentUser.UserId))
            {
                return await ResponseWrapper<Guid>.FailureAsync("Unauthorized", "User not authenticated", 401);
            }

            return await ExecuteWriteAsync(async () =>
            {
                // 1. Map DTO to Entity
                var entity = _mapper.Map<Account>(dto);

                entity.Id = Guid.NewGuid();
                entity.UserId = _currentUser.UserId;
                entity.AgencyId = _currentUser.AgencyId;
                entity.BranchId = _currentUser.BranchId;
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.Nature = AccountHelper.GetNature(entity.AccountType);
                // 2. Default to current user's context if not provided
                entity.AgencyId = _currentUser.AgencyId;
                // Optionally handle BranchId logic here if required

                _context.Accounts.Add(entity);
                await _context.SaveChangesAsync();

                // 3. Invalidate relevant caches
                RemoveByPrefix(AccountCacheKey);

                return entity.Id;
            }, "Account created successfully", "Error creating account");
        }

        // ================================
        // GET ALL ACCOUNTS
        // ================================
        public async Task<ResponseWrapper<PagedResponse<AccountDto>>> GetAllAccountsAsync(int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                cacheKey: $"{AccountCacheKey}_{_currentUser.UserId}_P{page}_PS{pageSize}",
                action: async () =>
                {
                    var query = _context.Accounts
                        .Include(x => x.Agency)
                        .Include(x => x.Branch)
                        // Assuming Currency is a navigation property
                        .Include(x => x.Currency)
                        .AsNoTracking();

                    // Multi-tenant Filter
                    if (!isAdmin)
                    {
                        query = query.Where(x => x.AgencyId == agencyId);
                    }

                    var totalRecords = await query.CountAsync();

                    var mapped = await query
                        .OrderByDescending(x => x.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ProjectTo<AccountDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<AccountDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} accounts fetched",
                cacheMessage: "Accounts fetched from cache",
                errorMessage: "Error fetching accounts"
            );
        }

        // ================================
        // UPDATE ACCOUNT
        // ================================
        public async Task<ResponseWrapper<bool>> UpdateAccountAsync(Guid id, UpdateAccountDto dto)
        {
            return await ExecuteWriteAsync(async () =>
            {
                var entity = await _context.Accounts.FindAsync(id);

                if (entity == null)
                    throw new Exception("Account not found");

                // Security check for non-admins
                if (!_currentUser.IsInRole("Administrator") && entity.AgencyId != _currentUser.AgencyId)
                    throw new Exception("Unauthorized access to this account record");

                _mapper.Map(dto, entity);
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = _currentUser.UserId;
                entity.AgencyId = _currentUser.AgencyId;
                entity.BranchId = _currentUser.BranchId;

                await _context.SaveChangesAsync();
                RemoveByPrefix(AccountCacheKey);

                return true;
            }, "Account updated successfully", "Error updating account");
        }

        // ================================
        // DELETE ACCOUNT
        // ================================
        public async Task<ResponseWrapper<bool>> DeleteAccountAsync(Guid id) // Changed from int to Guid
        {
            return await ExecuteWriteAsync(async () =>
            {
                var entity = await _context.Accounts.FindAsync(id);

                if (entity == null)
                    throw new Exception("Account not found");

                // Optional: Check if account has transactions before deleting
                var hasTransactions = await _context.TransactionDetails.AnyAsync(x => x.AccountId == id);
                if (hasTransactions) throw new Exception("Cannot delete account with existing transactions.");

                _context.Accounts.Remove(entity);
                await _context.SaveChangesAsync();

                RemoveByPrefix(AccountCacheKey);

                return true;
            }, "Account deleted successfully", "Error deleting account");
        }


        private async Task<(int FromCurrencyId, int ToCurrencyId)> GetAccountCurrenciesAsync(
    Guid fromAccountId,
    Guid toAccountId)
        {
            var accounts = await _context.Accounts
                .Where(a => a.Id == fromAccountId || a.Id == toAccountId)
                .Select(a => new { a.Id, a.CurrencyId })
                .ToListAsync();

            var fromCurrency = accounts
                .FirstOrDefault(a => a.Id == fromAccountId)?.CurrencyId;

            var toCurrency = accounts
                .FirstOrDefault(a => a.Id == toAccountId)?.CurrencyId;

            if (fromCurrency == null || toCurrency == null)
                throw new Exception("Invalid account(s)");

            return (fromCurrency.Value, toCurrency.Value);
        }




        public async Task<ResponseWrapper<Guid>> CreateTransactionAsync(CreateTransactionRequest request)
        {
            if (string.IsNullOrWhiteSpace(_currentUser.UserId))
                return await ResponseWrapper<Guid>.FailureAsync("Unauthorized", "User not authenticated",
                    401);

            if (request == null)
                return await ResponseWrapper<Guid>.FailureAsync("Validation Error", "Request cannot be null");

            //  APPLY FOR EXCHANGE
            if (request.TransactionType == TransactionTypeEnum.Exchange)
            {
                if (request.Exchange == null)
                    return await ResponseWrapper<Guid>.FailureAsync("Exchange data required");

                var (fromCurrencyId, toCurrencyId) =
                    await GetAccountCurrenciesAsync(
                        request.Exchange.FromAccountId,
                        request.Exchange.ToAccountId);

                request.Exchange.FromCurrencyId = fromCurrencyId;
                request.Exchange.ToCurrencyId = toCurrencyId;
            }

            // ✅ NOW SWITCH
            return request.TransactionType switch
            {
                TransactionTypeEnum.Exchange => await CreateExchangeAsync(request),

                // KEEP YOUR OTHER TYPES
                TransactionTypeEnum.Transfer => await CreateTransferAsync(request),
                TransactionTypeEnum.Loan => await CreateLoanAsync(request),
                TransactionTypeEnum.Expense => await CreateExpenseAsync(request),
                TransactionTypeEnum.Deposit => await CreateDepositAsync(request),
                TransactionTypeEnum.Withdraw => await CreateWithdrawAsync(request),
                TransactionTypeEnum.Repayment => await CreateRepaymentAsync(request),
                TransactionTypeEnum.Revenue => await CreateRevenueAsync(request),
                TransactionTypeEnum.CashOpening => await CreateCashOpeningAsync(request),

                _ => await ResponseWrapper<Guid>.FailureAsync(
                    "Validation Error",
                    "Invalid type")
            };
        }



        private async Task<ResponseWrapper<Guid>> CreateCashOpeningAsync(CreateTransactionRequest request)
        {
            var dto = request.CashOpening;

            if (dto == null)
                return await ResponseWrapper<Guid>.FailureAsync("Cash opening required");

            var cashAccount = await GetAccountAsync(dto.CashAccountId);
            var capitalAccount = await GetAccountAsync(dto.CapitalAccountId);

            if (cashAccount.AccountType != AccountTypeEnum.Cash)
                return await ResponseWrapper<Guid>.FailureAsync("Selected account must be Cash account");

            if (capitalAccount.AccountType != AccountTypeEnum.Capital)
                return await ResponseWrapper<Guid>.FailureAsync("Selected account must be Capital account");

            if (cashAccount.CurrencyId != capitalAccount.CurrencyId)
                return await ResponseWrapper<Guid>.FailureAsync("Currency mismatch");

            var currencyId = cashAccount.CurrencyId;

            return await ExecuteWriteAsync(async () =>
            {
                using var trx = await _context.Database.BeginTransactionAsync();

                var referenceNo = GenerateReferenceNo();

                var transaction = CreateBaseTransaction(request, dto.Amount, referenceNo);

                var details = new List<TransactionDetail>
        {
            new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = dto.CashAccountId,
                CurrencyId = currencyId,
                Amount = dto.Amount,
                EntryType = 1, // Debit, cash increases
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            },

            new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = dto.CapitalAccountId,
                CurrencyId = currencyId,
                Amount = dto.Amount,
                EntryType = 2, // Credit, capital increases
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            }
        };

                _context.Transactions.Add(transaction);
                _context.TransactionDetails.AddRange(details);

                await _context.SaveChangesAsync();
                await trx.CommitAsync();

                return transaction.Id;

            }, "Cash opening created", "Error creating cash opening");
        }

        private async Task<ResponseWrapper<Guid>> CreateExchangeAsync(CreateTransactionRequest request)
        {
            var dto = request.Exchange;

            if (dto == null)
                return await ResponseWrapper<Guid>.FailureAsync("Exchange required");

            if (dto.CustomerRate <= 0)
            {
                return await ResponseWrapper<Guid>.FailureAsync(
                    "Customer rate must be greater than zero"
                );
            }

            var rates = await GetRates(dto.FromCurrencyId, dto.ToCurrencyId);

            var fromRate = rates[dto.FromCurrencyId];
            var toRate = rates[dto.ToCurrencyId];
            var agencyRate = toRate / fromRate;

            // ✅ Base conversion (USD base)
            var baseAmount = dto.FromAmount / fromRate;

            // ✅ Gross amount (before fee)
            var grossToAmount = baseAmount * toRate;

            // 🔥 Get settings
            //var settings = await GetExchangeSettings(dto.ToCurrencyId);

            //// 🔥 Calculate fee & profit (BASE currency)
            ////var fee = baseAmount * settings.FeeRate;
            ////var profit = baseAmount * settings.ProfitRate;

            //var fee = baseAmount * (settings.FeeRate / 100m);
            //var profit = baseAmount * (settings.ProfitRate / 100m);

            //var totalRevenue = fee + profit;

            //// 🔥 Convert revenue to target currency
            //var revenueInToCurrency = totalRevenue * toRate;

            //// ✅ Final amount (what customer gets)
            //var finalAmount = grossToAmount - revenueInToCurrency;



            // Lacagta macmiilka la siinayo
            var finalAmount = decimal.Round(
                dto.FromAmount * dto.CustomerRate,
                2,
                MidpointRounding.AwayFromZero
            );

            // Faa'iidada ka dhalatay farqiga labada rate
            var revenueInToCurrency = decimal.Round(
                grossToAmount - finalAmount,
                2,
                MidpointRounding.AwayFromZero
            );

            // Hubi in transaction-ku khasaaro keenin
            if (revenueInToCurrency < 0)
            {
                return await ResponseWrapper<Guid>.FailureAsync(
                    $"Customer rate causes a loss. Agency rate is {agencyRate}"
                );
            }

            // Fee hadda lama isticmaalayo
           

            // Profit-ka base currency, USD
            var profit = revenueInToCurrency / toRate;

            // Waxaa loo hayaa code-ka dambe
            var totalRevenue = revenueInToCurrency;



            if (finalAmount <= 0)
                return await ResponseWrapper<Guid>.FailureAsync("Invalid exchange calculation");

            return await ExecuteWriteAsync(async () =>
            {
                using var trx = await _context.Database.BeginTransactionAsync();

                var referenceNo = GenerateReferenceNo();
                var transaction = CreateBaseTransaction(request, baseAmount, referenceNo);

                var details = new List<TransactionDetail>
{
    // 🟢 DEBIT: You received money
    // Example: You received 100,000 SOS
    new TransactionDetail
    {
        Id = Guid.NewGuid(),
        TransactionId = transaction.Id,
        AccountId = dto.FromAccountId,
        CurrencyId = dto.FromCurrencyId,
        Amount = dto.FromAmount,
        EntryType = 1,
        UserId = _currentUser.UserId,
        CreatedAt = DateTime.UtcNow
    },

    // 🔴 CREDIT: You paid money
    // Example: You paid 3.77 USD
    new TransactionDetail
    {
        Id = Guid.NewGuid(),
        TransactionId = transaction.Id,
        AccountId = dto.ToAccountId,
        CurrencyId = dto.ToCurrencyId,
        Amount = finalAmount,
        EntryType = 2,
        UserId = _currentUser.UserId,
        CreatedAt = DateTime.UtcNow
    }
};
                // 🔥 ADD PROFIT ONLY (NO CASH DUPLICATION)
                if (totalRevenue > 0)
                {
                    var revenueAccount = await GetRevenueAccount(dto.ToCurrencyId);

                    details.Add(new TransactionDetail
                    {
                        Id = Guid.NewGuid(),
                        TransactionId = transaction.Id,
                        AccountId = revenueAccount.Id,
                        CurrencyId = dto.ToCurrencyId,
                        Amount = revenueInToCurrency,
                        EntryType = 2, // CREDIT
                        UserId = _currentUser.UserId,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // ✅ SAVE
                _context.Transactions.Add(transaction);
                _context.TransactionDetails.AddRange(details);

                _context.Exchanges.Add(new Exchange
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transaction.Id,
                    FromAccountId = dto.FromAccountId,
                    ToAccountId = dto.ToAccountId,
                    FromAmount = dto.FromAmount,
                    ToAmount = grossToAmount, // before fee
                    NetAmount = finalAmount,
                    Rate = dto.CustomerRate,
                    Fee = revenueInToCurrency,
                    Profit = profit,
                    AgencyId = _currentUser.AgencyId,
                    BranchId = _currentUser.BranchId,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await trx.CommitAsync();

                return transaction.Id;
            }, "Exchange created", "Error");
        }
        private async Task<Account> GetRevenueAccount(int currencyId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.AccountType == AccountTypeEnum.Revenue &&
                    a.CurrencyId == currencyId &&
                    a.Name.Contains("Exchange Profit"));


            if (account == null)
                throw new Exception($"Exchange Profit account lama helin currencyId: {currencyId}");

            return account;
        }

        private async Task<ExchangeSettings> GetExchangeSettings(int currencyId)
        {
            var settings = await _context.ExchangeSettings
                .FirstOrDefaultAsync(x => x.CurrencyId == currencyId && x.IsActive);

            if (settings == null)
                throw new Exception($"Exchange settings lama helin currencyId: {currencyId}");

            return settings;
        }


        private async Task<ResponseWrapper<Guid>> CreateDepositAsync(CreateTransactionRequest request)
        {
            var dto = request.Deposit;

            // ✅ NULL CHECK HORE
            if (dto == null)
                return await ResponseWrapper<Guid>.FailureAsync("Deposit required");

            var account = await GetAccountAsync(dto.AccountId);
            var currencyId = account.CurrencyId;

            // ✅ KAN SOO BIXI DIBADDA - ka hor ExecuteWriteAsync
            var customerPayableAccountId = await GetCustomerPayableAccountAsync(dto.CustomerId);

            return await ExecuteWriteAsync(async () =>
            {
                using var trx = await _context.Database.BeginTransactionAsync();

                var referenceNo = GenerateReferenceNo();

                var transaction = CreateBaseTransaction(request, dto.Amount, referenceNo);

                var details = new List<TransactionDetail>
                {

            new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = dto.AccountId,
                CurrencyId = currencyId,
                Amount = dto.Amount,
                EntryType = 1,
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            },


                    new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = customerPayableAccountId, // ✅ variable isticmaal
                CurrencyId = currencyId,
                Amount = dto.Amount,
                EntryType = 2,
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            }
                };

                _context.Transactions.Add(transaction);
                _context.TransactionDetails.AddRange(details);

                _context.Deposits.Add(new Deposit
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transaction.Id,
                    AccountId = dto.AccountId,
                    CustomerId = dto.CustomerId,
                    Amount = dto.Amount,
                    DepositNo = referenceNo,
                    CurrencyId = currencyId,
                    AgencyId = _currentUser.AgencyId,
                    BranchId = _currentUser.BranchId,
                    UserId = _currentUser.UserId,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await trx.CommitAsync();

                return transaction.Id;

            }, "Deposit created", "Error creating deposit");
        }


        private async Task<ResponseWrapper<Guid>> CreateWithdrawAsync(CreateTransactionRequest request)
        {
            var dto = request.Withdraw;

            if (dto == null)
                return await ResponseWrapper<Guid>.FailureAsync("Withdraw required");

            var account = await GetAccountAsync(dto.AccountId);
            var currencyId = account.CurrencyId;

            // ✅ GetOrCreate isticmaal - MAHA GetCustomerPayableAccount
            var customerAccountId = await GetCustomerPayableAccountAsync(dto.CustomerId);

            var balance = await _context.TransactionDetails
                 .Where(x => x.AccountId == customerAccountId)
                 .SumAsync(x => x.EntryType == 2 ? x.Amount : -x.Amount);

            if (dto.Amount > balance)
                return await ResponseWrapper<Guid>.FailureAsync("Insufficient balance");

            // ... rest of code

            return await ExecuteWriteAsync(async () =>
            {
                using var trx = await _context.Database.BeginTransactionAsync();

                var referenceNo = GenerateReferenceNo();

                var transaction = CreateBaseTransaction(request, dto.Amount, referenceNo);

                var details = new List<TransactionDetail>
        {
            // 🔴 CREDIT (Cash goes out)
            new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = dto.AccountId,
                CurrencyId = currencyId,
                Amount = dto.Amount,
                EntryType = 2,
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            },

            // 🟢 DEBIT (Reduce payable)
            new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = customerAccountId,
                CurrencyId = currencyId,
                Amount = dto.Amount,
                EntryType = 1,
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            }
        };

                _context.Transactions.Add(transaction);
                _context.TransactionDetails.AddRange(details);

                // Optional Withdraw table
                _context.Withdraws.Add(new Withdraw
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transaction.Id,
                    AccountId = dto.AccountId,
                    CustomerId = dto.CustomerId,
                    Amount = dto.Amount,
                    WithdrawNo = referenceNo,
                    CurrencyId = currencyId,
                    ReceiverName = dto.ReceiverName,
                    ReceiverIdCard = dto.ReceiverIdCard,
                    AgencyId = _currentUser.AgencyId,
                    BranchId = _currentUser.BranchId,
                    UserId = _currentUser.UserId,
                    CreatedAt = DateTime.UtcNow
                });

                // ==========================================
                // 🚀 START: UPDATE MULTIPLE DEPOSITS (FIFO)
                // ==========================================

                // 1. Waxaan la soo baxaynaa dhammaan deposits-kii u furnaa macmiilkan ee haraaga leh (Amount > 0)
                var activeDeposits = await _context.Deposits
                    .Where(b => b.CustomerId == dto.CustomerId && b.Amount > 0)
                    .OrderBy(b => b.CreatedAt) // Kuwii ugu horreeyay ee la dhigay ayaa lagu bilaabayaa
                    .ToListAsync();

                var amountToReduce = dto.Amount;

                // 2. Waxaan mid mid u dhex gelaynaa diiwaannada Deposits-ka kor ku xusan
                foreach (var deposit in activeDeposits)
                {
                    if (amountToReduce <= 0) break; // Haddii lacagtii la baxayay la wada gooyay, ka bax

                    // Maadaama Amount uu yahay nullable (decimal?), waxaan u isticmaalaynaa Value-giisa ama 0
                    var currentDepositAmount = deposit.Amount ?? 0;

                    if (currentDepositAmount >= amountToReduce)
                    {
                        // Haddii deposit-kan uu ku filan yahay lacagta la rabo oo dhan
                        deposit.Amount -= amountToReduce;
                        amountToReduce = 0; // Lacagtii la rabay waa la wada dhimay
                    }
                    else
                    {
                        // ✅ SAXID: Marka hore lacagta ka dhiman 'amountToReduce' ka jid (Substract)
                        amountToReduce -= currentDepositAmount;

                        // ✅ SAXID: Ka dib diiwanka hadda la joogo eber ka dhig mar haddii wixii ku jiray la qaatay
                        deposit.Amount = 0;
                    }
                }

         









                await _context.SaveChangesAsync();
                await trx.CommitAsync();

                return transaction.Id;

            }, "Withdraw created", "Error creating withdraw");
        }



        private async Task<ResponseWrapper<Guid>> CreateLoanAsync(CreateTransactionRequest request)
        {
            var dto = request.Loan;

            if (dto == null)
                return await ResponseWrapper<Guid>.FailureAsync("Loan required");

            var account = await GetAccountAsync(dto.AccountId);
            var currencyId = account.CurrencyId;

            var receivableAccountId = await GetCustomerReceivableAccountAsync(dto.CustomerId);

            return await ExecuteWriteAsync(async () =>
            {
                using var trx = await _context.Database.BeginTransactionAsync();

                var referenceNo = GenerateReferenceNo();

                var transaction = CreateBaseTransaction(
                    request,
                    dto.PrincipalAmount,
                    referenceNo
                );

                var details = new List<TransactionDetail>
        {
            // 🔴 CREDIT (Cash goes out)
            new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = dto.AccountId,
                CurrencyId = currencyId,
                Amount = dto.PrincipalAmount,
                EntryType = 2,
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            },

            // 🟢 DEBIT (Receivable)
            new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = receivableAccountId,
                CurrencyId = currencyId,
                Amount = dto.PrincipalAmount,
                EntryType = 1,
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            }
        };

                _context.Transactions.Add(transaction);
                _context.TransactionDetails.AddRange(details);

                _context.Loans.Add(new Loan
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transaction.Id,
                    AccountId = dto.AccountId,
                    CustomerId = dto.CustomerId,
                    PrincipalAmount = dto.PrincipalAmount,
                    InterestRate = dto.InterestRate,
                    StartDate = DateTime.UtcNow,
                    DueDate = dto.DueDate == null
                            ? null
                            : DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc),
                    LoanNo = referenceNo,
                    PaidAmount =0,
                    CurrencyId = currencyId,
                    AgencyId = _currentUser.AgencyId,
                    BranchId = _currentUser.BranchId,
                    UserId = _currentUser.UserId,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await trx.CommitAsync();

                return transaction.Id;

            }, "Loan created", "Error creating loan");
        }

        private async Task<ResponseWrapper<Guid>> CreateRepaymentAsync(CreateTransactionRequest request)
        {
            var dto = request.Repayment;

            if (dto == null)
                return await ResponseWrapper<Guid>.FailureAsync("Repayment required");

            var cashAccount = await GetAccountAsync(dto.CashAccountId);
            var currencyId = cashAccount.CurrencyId;

            var loan = await _context.Loans
                .FirstOrDefaultAsync(x => x.Id == dto.LoanId);

            if (loan == null)
                return await ResponseWrapper<Guid>.FailureAsync("Loan not found");

            // 👉 receivable account (customer)




            var receivableAccountId = await GetCustomerReceivableAccountAsync(loan.CustomerId);

            // 🔒 validation
            var remaining = loan.PrincipalAmount - loan.PaidAmount;

            if (dto.Amount > remaining)
                return await ResponseWrapper<Guid>.FailureAsync("Amount exceeds remaining loan");

            return await ExecuteWriteAsync(async () =>
            {
                using var trx = await _context.Database.BeginTransactionAsync();

                var referenceNo = GenerateReferenceNo();

                var transaction = CreateBaseTransaction(request, dto.Amount, referenceNo);

                var details = new List<TransactionDetail>
        {
            // 🟢 DEBIT (Cash comes in)
            new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = dto.CashAccountId,
                CurrencyId = currencyId,
                Amount = dto.Amount,
                EntryType = 1,
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            },

            // 🔴 CREDIT (Reduce receivable)
            new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = receivableAccountId,
                CurrencyId = currencyId,
                Amount = dto.Amount,
                EntryType = 2,
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            }
        };

                _context.Transactions.Add(transaction);
                _context.TransactionDetails.AddRange(details);


                if (loan.PaidAmount >= loan.PrincipalAmount)
                    loan.Status = LoanStatusEnum.Closed;

                // Find the business record linked to this loan or customer
                var business = await _context.Loans
                    .FirstOrDefaultAsync(b => b.Id == loan.Id); // Adjust foreign key as needed

                if (business != null)
                {
                    business.PaidAmount += dto.Amount; // Or business.TotalPaidAmount += dto.Amount;
                }




                // save repayment
                _context.LoanPayments.Add(new LoanPayment
                {
                    Id = Guid.NewGuid(),
                    LoanId = dto.LoanId,
                    Amount = dto.Amount,
                    Note = dto.Note,
                    TransactionId = transaction.Id,
                    CashAccountId = dto.CashAccountId,
                    LoanAccountId = receivableAccountId,
                    AgencyId = _currentUser.AgencyId,
                    BranchId = _currentUser.BranchId,
                    PaymentDate = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await trx.CommitAsync();

                return transaction.Id;

            }, "Repayment created", "Error creating repayment");
        }



        private async Task<ResponseWrapper<Guid>> CreateTransferAsync(CreateTransactionRequest request)
        {
            var dto = request.Transfer;

            if (dto == null)
                return await ResponseWrapper<Guid>.FailureAsync("Transfer required");

            var fromAccount = await GetAccountAsync(dto.FromAccountId);
            var toAccount = await GetAccountAsync(dto.ToAccountId);

            // 🔒 validation
            if (fromAccount.CurrencyId != toAccount.CurrencyId)
                return await ResponseWrapper<Guid>.FailureAsync("Currency mismatch");

            var currencyId = fromAccount.CurrencyId;

            return await ExecuteWriteAsync(async () =>
            {
                using var trx = await _context.Database.BeginTransactionAsync();

                var referenceNo = GenerateReferenceNo();

                var transaction = CreateBaseTransaction(request, dto.Amount, referenceNo);

                var details = new List<TransactionDetail>
        {
            // 🔴 CREDIT (FromAccount → lacag baxday)
            new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = dto.FromAccountId,
                CurrencyId = currencyId,
                Amount = dto.Amount,
                EntryType = 2,
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            },

            // 🟢 DEBIT (ToAccount → lacag soo gashay)
            new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = dto.ToAccountId,
                CurrencyId = currencyId,
                Amount = dto.Amount,
                EntryType = 1,
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            }
        };

                _context.Transactions.Add(transaction);
                _context.TransactionDetails.AddRange(details);

                _context.Transfers.Add(new Transfer
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transaction.Id,
                    FromAccountId = dto.FromAccountId,
                    ToAccountId = dto.ToAccountId,
                    Amount = dto.Amount,
                    SenderName = dto.SenderName,
                    ReceiverName = dto.ReceiverName,
                    Status = TransferStatusEnum.Completed,
                    AgencyId = _currentUser.AgencyId,
                    BranchId = _currentUser.BranchId,
                    UserId = _currentUser.UserId,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await trx.CommitAsync();

                return transaction.Id;

            }, "Transfer created", "Error creating transfer");
        }


        private async Task<ResponseWrapper<Guid>> CreateExpenseAsync(CreateTransactionRequest request)
        {
            var dto = request.Expense;

            if (dto == null)
                return await ResponseWrapper<Guid>.FailureAsync("Expense required");

            var expenseAccount = await GetAccountAsync(dto.AccountId);
            var cashAccount = await GetAccountAsync(dto.CashAccountId);

            if (expenseAccount.CurrencyId != cashAccount.CurrencyId)
                return await ResponseWrapper<Guid>.FailureAsync("Currency mismatch");

            var currencyId = cashAccount.CurrencyId;

            return await ExecuteWriteAsync(async () =>
            {
                using var trx = await _context.Database.BeginTransactionAsync();

                var referenceNo = GenerateReferenceNo();

                var transaction = CreateBaseTransaction(request, dto.Amount, referenceNo);

                var details = new List<TransactionDetail>
        {
            // 🟢 DEBIT (Expense)
            new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = dto.AccountId,
                CurrencyId = currencyId,
                Amount = dto.Amount,
                EntryType = 1,
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            },

            // 🔴 CREDIT (Cash out)
            new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = dto.CashAccountId,
                CurrencyId = currencyId,
                Amount = dto.Amount,
                EntryType = 2,
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            }
        };

                _context.Transactions.Add(transaction);
                _context.TransactionDetails.AddRange(details);

                _context.Expenses.Add(new Expense
                {
                    Id = Guid.NewGuid(),
                    Title = dto.Title,
                    Description = dto.Description,
                    Amount = dto.Amount,
                    AccountId = dto.AccountId,
                    TransactionId = transaction.Id,
                    ExpenseDate = DateTime.UtcNow,
                    AgencyId = _currentUser.AgencyId,
                    BranchId = _currentUser.BranchId,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await trx.CommitAsync();

                return transaction.Id;

            }, "Expense created", "Error creating expense");
        }

        private async Task<ResponseWrapper<Guid>> CreateRevenueAsync(CreateTransactionRequest request)
        {
            var dto = request.Revenue;

            if (dto == null)
                return await ResponseWrapper<Guid>.FailureAsync("Revenue required");

            var cashAccount = await GetAccountAsync(dto.CashAccountId);
            var revenueAccount = await GetAccountAsync(dto.RevenueAccountId);

            if (cashAccount.CurrencyId != revenueAccount.CurrencyId)
                return await ResponseWrapper<Guid>.FailureAsync("Currency mismatch");

            var currencyId = cashAccount.CurrencyId;

            return await ExecuteWriteAsync(async () =>
            {
                using var trx = await _context.Database.BeginTransactionAsync();

                var referenceNo = GenerateReferenceNo();

                var transaction = CreateBaseTransaction(request, dto.Amount, referenceNo);

                var details = new List<TransactionDetail>
        {
            // 🟢 DEBIT (Cash ↑)
            new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = dto.CashAccountId,
                CurrencyId = currencyId,
                Amount = dto.Amount,
                EntryType = 1,
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            },

            // 🔴 CREDIT (Revenue ↑)
            new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                AccountId = dto.RevenueAccountId,
                CurrencyId = currencyId,
                Amount = dto.Amount,
                EntryType = 2,
                UserId = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            }
        };

                _context.Transactions.Add(transaction);
                _context.TransactionDetails.AddRange(details);

                _context.Revenues.Add(new Revenue
                {
                    Id = Guid.NewGuid(),
                    Title = dto.Title,
                    Description = dto.Description,
                    Amount = dto.Amount,
                    RevenueAccountId = dto.RevenueAccountId,
                    CashAccountId = dto.CashAccountId,
                    SourceType = dto.SourceType,
                    TransactionId = transaction.Id,
                    AgencyId = _currentUser.AgencyId,
                    BranchId = _currentUser.BranchId,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await trx.CommitAsync();

                return transaction.Id;

            }, "Revenue created", "Error creating revenue");
        }


        private async Task<Guid> GetCustomerPayableAccount(Guid customerId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.AccountType == AccountTypeEnum.PAYABLE &&
                    a.ReferenceId == customerId
                );

            if (account == null)
                throw new Exception("Customer payable account not found");

            return account.Id;
        }


        private async Task<Guid> GetCustomerReceivableAccountAsync(Guid customerId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.AccountType == AccountTypeEnum.RECEIVABLE &&
                    a.ReferenceId == customerId &&
                    a.AgencyId == _currentUser.AgencyId
                );

            // If found, return the ID; otherwise, return an empty Guid
            return account?.Id ?? Guid.Empty;
        }

        // ← KAN KU DAR HALKAN, ka dib GetCustomerPayableAccount
        private async Task<Guid> GetCustomerPayableAccountAsync(Guid customerId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.AccountType == AccountTypeEnum.PAYABLE &&
                    a.ReferenceId == customerId &&
                    a.AgencyId == _currentUser.AgencyId
                );

            // Haddii la helo ID-ga soo culi; haddii kale soo culi Guid.Empty (00000000-0000-0000-0000-000000000000)
            return account?.Id ?? Guid.Empty;
        }

        // ================================

        private async Task<Account> GetAccountAsync(Guid accountId)
        {
            var account = await _context.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == accountId &&
                    x.AgencyId == _currentUser.AgencyId
                );

            if (account == null)
                throw new Exception("Account not found");

            return account;
        }



        private Transaction CreateBaseTransaction(CreateTransactionRequest request, decimal totalAmount, string referenceNo)
        {
            return new Transaction
            {
                Id = Guid.NewGuid(),
                ReferenceNo = referenceNo,  // ✔ no await here
                TransactionType = request.TransactionType,
                Description = request.Description,
                UserId = _currentUser.UserId,
                AgencyId = _currentUser.AgencyId,
                BranchId = _currentUser.BranchId,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = totalAmount
            };
        }

        private async Task<Dictionary<int, decimal>> GetRates(params int[] ids)
        {
            return await _context.ExchangeRates
                .Where(x => ids.Contains(x.CurrencyId))
                .GroupBy(x => x.CurrencyId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.CreatedAt).First().Rate
                );
        }



        private string GenerateReferenceNo()
        {
            return $"TRX-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6]}";
        }
        // ================================
        // GET ALL TRANSACTIONS
        // ================================
        public async Task<ResponseWrapper<PagedResponse<TransactionDto>>> GetAllTransactionsAsync(int page = 1, int pageSize = 10)
        {
            // 1. Guard Clauses
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            // 2. Role Check (Consistency with other services)
            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                cacheKey: $"{TransactionCacheKey}_{_currentUser.UserId}_P{page}_PS{pageSize}",
                action: async () =>
                {
                    // Note: Removed .Include() because .ProjectTo() handles it via AutoMapper
                    var query = _context.Transactions
                        .AsNoTracking();

                    // 3. Multi-tenant filter logic
                    if (!isAdmin)
                    {
                        query = query.Where(x => x.AgencyId == agencyId);
                    }

                    var totalRecords = await query.CountAsync();

                    var mapped = await query
                        .OrderByDescending(x => x.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        // ProjectTo creates the most efficient SQL query automatically
                        .ProjectTo<TransactionDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<TransactionDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} transactions fetched",
                cacheMessage: "Transactions loaded from cache",
                errorMessage: "Error fetching transactions"
            );
        }
        // ================================
        // UPDATE TRANSACTION
        // ================================
        public async Task<ResponseWrapper<bool>> UpdateTransactionAsync(Guid id, UpdateTransactionDto dto)
        {
            // 1. HEL EXCHANGE RATES (Base Currency Check)
            var currencyIds = dto.Details.Select(d => d.CurrencyId).Distinct().ToList();
            var rates = await _context.ExchangeRates
                .Where(r => currencyIds.Contains(r.CurrencyId))
                .ToDictionaryAsync(r => r.CurrencyId, r => r.Rate);

            // 2. DOUBLE-ENTRY VALIDATION (In USD terms)
            decimal totalDebitBase = 0;
            decimal totalCreditBase = 0;

            foreach (var d in dto.Details)
            {
                if (!rates.TryGetValue(d.CurrencyId, out decimal rate))
                    return await ResponseWrapper<bool>.FailureAsync("Rate Error", $"Exchange rate not found for Currency ID: {d.CurrencyId}");

                decimal amountInBase = d.Amount / rate;

                if (d.EntryType == 1) totalDebitBase += amountInBase;
                else if (d.EntryType == 2) totalCreditBase += amountInBase;
            }

            // Isbarbardhig (Round to 2 decimals)
            if (Math.Abs(totalDebitBase - totalCreditBase) > 0.01m)
            {
                return await ResponseWrapper<bool>.FailureAsync("Validation Error",
                    $"Updated transaction out of balance. Total Debit (USD): {totalDebitBase:N2}, Total Credit (USD): {totalCreditBase:N2}");
            }

            return await ExecuteWriteAsync(async () =>
            {
                using var dbTransaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _context.Transactions
                        .Include(x => x.Details)
                        .FirstOrDefaultAsync(x => x.Id == id);

                    if (existing == null) throw new Exception("Transaction not found");

                    // 3. SECURITY & AUTHORIZATION
                    bool isAdmin = _currentUser.IsInRole("Administrator");
                    bool isOwner = existing.UserId == _currentUser.UserId;
                    bool isSameAgency = existing.AgencyId == _currentUser.AgencyId;

                    if (!isAdmin && !(isOwner && isSameAgency))
                    {
                        throw new Exception("Unauthorized: You do not have permission to edit this transaction.");
                    }

                    // 4. CLEAN UP OLD DETAILS
                    _context.TransactionDetails.RemoveRange(existing.Details);

                    // 5. MAP DTO TO EXISTING ENTITY
                    // Nota: ReferenceNo laguma beddelo Update-ka badanaa si loo dhowro Audit-ka
                    _mapper.Map(dto, existing);

                    existing.TotalAmount = totalDebitBase; // Update total to new USD balance
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = _currentUser.UserId;

                    // 6. RE-INITIALIZE NEW DETAILS
                    foreach (var detail in existing.Details)
                    {
                        detail.Id = Guid.NewGuid();
                        detail.TransactionId = existing.Id;
                        detail.TransactionType = existing.TransactionType;
                        detail.UserId = _currentUser.UserId;
                        detail.CreatedAt = DateTime.UtcNow; // Or keep original CreatedAt if preferred
                    }

                    await _context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    RemoveByPrefix(TransactionCacheKey);
                    return true;
                }
                catch (Exception ex)
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }, "Transaction updated successfully", "Error updating transaction");
        }                 // DELETE TRANSACTION
                          // ================================
                          // Fixed DeleteTransactionAsync in AccountService
        public async Task<ResponseWrapper<bool>> DeleteTransactionAsync(Guid id) // Changed from int to Guid
        {
            return await ExecuteWriteAsync(async () =>
            {
                var entity = await _context.Transactions
                    .Include(x => x.Details) // Include details to ensure cascade or manual cleanup
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null) throw new Exception("Transaction not found");

                // Security Check
                if (!_currentUser.IsInRole("Administrator") && entity.AgencyId != _currentUser.AgencyId)
                    throw new Exception("Unauthorized: You cannot delete transactions from another agency.");

                _context.Transactions.Remove(entity);
                await _context.SaveChangesAsync();

                RemoveByPrefix(TransactionCacheKey);
                return true;
            }, "Transaction deleted successfully", "Error deleting transaction");
        }


   public async Task<ResponseWrapper<PagedResponse<AccountBalanceSummaryDto>>> GetAccountBalancesSummaryAsync(
    int page = 1,
    int pageSize = 10,
    DateTime? fromDate = null,
    DateTime? toDate = null,
    AccountTypeEnum? accountType = null)
{
    if (page <= 0) page = 1;
    if (pageSize <= 0) pageSize = 10;
    pageSize = Math.Min(pageSize, 100);

    var agencyId = _currentUser.AgencyId;
    var isAdmin = _currentUser.IsInRole("Administrator");

    return await ExecuteWithCacheAsync(
        cacheKey: $"{AccountCacheKey}_Summary_{_currentUser.UserId}_P{page}_PS{pageSize}_T{accountType}_F{fromDate?.ToString("yyyy-MM-dd")}_TO{toDate?.ToString("yyyy-MM-dd")}",

        action: async () =>
        {
            var query = _context.Accounts
                .Include(x => x.Currency)
                .AsNoTracking()
                .AsQueryable();

            if (!isAdmin)
                query = query.Where(x => x.AgencyId == agencyId);

            if (accountType.HasValue)
                query = query.Where(x => x.AccountType == accountType.Value);

            var totalRecords = await query.CountAsync();

            var pagedAccounts = await query
                .OrderBy(x => x.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var accountIds = pagedAccounts.Select(a => a.Id).ToList();

            if (!accountIds.Any())
            {
                return new PagedResponse<AccountBalanceSummaryDto>(
                    new List<AccountBalanceSummaryDto>(),
                    page,
                    pageSize,
                    totalRecords
                );
            }

            var transactionQuery = _context.TransactionDetails
                .AsNoTracking()
                .Where(td => accountIds.Contains(td.AccountId));

            if (fromDate.HasValue)
            {
                var startDate = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
                transactionQuery = transactionQuery.Where(td => td.CreatedAt >= startDate);
            }

            if (toDate.HasValue)
            {
                var nextDay = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                transactionQuery = transactionQuery.Where(td => td.CreatedAt < nextDay);
            }

            var balances = await transactionQuery
                .GroupBy(td => td.AccountId)
                .Select(g => new
                {
                    AccountId = g.Key,

                    Debit = Math.Round(
                        g.Where(x => x.EntryType == 1)
                         .Select(x => (decimal?)x.Amount)
                         .Sum() ?? 0m,
                        4
                    ),

                    Credit = Math.Round(
                        g.Where(x => x.EntryType == 2)
                         .Select(x => (decimal?)x.Amount)
                         .Sum() ?? 0m,
                        4
                    )
                })
                .ToDictionaryAsync(x => x.AccountId);

            var mappedResult = pagedAccounts.Select(acc =>
            {
                balances.TryGetValue(acc.Id, out var bal);

                var debit = bal?.Debit ?? 0m;
                var credit = bal?.Credit ?? 0m;

                var balance = acc.Nature switch
                {
                    AccountNatureEnum.Asset => debit - credit,
                    AccountNatureEnum.Expense => debit - credit,

                    AccountNatureEnum.Liability => credit - debit,
                    AccountNatureEnum.Equity => credit - debit,
                    AccountNatureEnum.Revenue => credit - debit,

                    _ => 0m
                };

                return new AccountBalanceSummaryDto
                {
                    AccountId = acc.Id,
                    AccountName = acc.Name,
                    CurrencyCode = acc.Currency?.Code ?? "N/A",
                    TotalDebit = debit,
                    TotalCredit = credit,
                    Balance = balance
                };
            }).ToList();

            return new PagedResponse<AccountBalanceSummaryDto>(
                mappedResult,
                page,
                pageSize,
                totalRecords
            );
        },

        successMessageFactory: r => $"{r.Data.Count} account balances fetched",
        cacheMessage: "Balances loaded from cache",
        errorMessage: "Error calculating balances"
    );
}

        public async Task<ResponseWrapper<PagedResponse<TransactionDetailDto>>> GetAccountStatementAsync(
                     Guid accountId,
                     int page = 1,
                     int pageSize = 10,
                     byte? entryType = null,          // 1=Debit, 2=Credit
                     DateTime? fromDate = null,
                     DateTime? toDate = null)
                        {
                            // 1. Guard Clauses
                            if (page <= 0) page = 1;
                            if (pageSize <= 0) pageSize = 10;
                            pageSize = Math.Min(pageSize, 100);

                            var agencyId = _currentUser.AgencyId;
                            var isAdmin = _currentUser.IsInRole("Administrator");

            // ✅ FIX: Normalize dates to UTC BEFORE cacheKey
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (fromDate.HasValue)
            {
                startDate = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            }

            if (toDate.HasValue)
            {
                endDate = DateTime.SpecifyKind(
                    toDate.Value.Date.AddDays(1).AddTicks(-1),
                    DateTimeKind.Utc
                );
            }

            return await ExecuteWithCacheAsync(
                // ✅ FIXED cache key (no raw DateTime)
                cacheKey: $"{AccountCacheKey}_Statement_{accountId}_P{page}_PS{pageSize}_T{entryType}_F{startDate:yyyyMMdd}_TO{endDate:yyyyMMdd}",
                action: async () =>
                {
                    // 🔒 Security Check
                    var accountExists = await _context.Accounts
                        .AnyAsync(a => a.Id == accountId && (isAdmin || a.AgencyId == agencyId));

                    if (!accountExists)
                        throw new Exception("Account not found or unauthorized access.");

                    // 📊 Base Query
                    var query = _context.TransactionDetails
                        .Include(td => td.Transaction)
                        .Include(td => td.Currency)
                        .Include(td => td.Account)
                        .Where(td => td.AccountId == accountId)
                        .AsNoTracking();

                    // 🔹 Filter: EntryType
                    if (entryType.HasValue)
                    {
                        query = query.Where(td => td.EntryType == entryType.Value);
                    }

                    // 🔹 FIXED Date Filters (UTC)
                    if (startDate.HasValue)
                    {
                        query = query.Where(td => td.CreatedAt >= startDate.Value);
                    }

                    if (endDate.HasValue)
                    {
                        query = query.Where(td => td.CreatedAt <= endDate.Value);
                    }

                    // 📊 Count
                    var totalRecords = await query.CountAsync();

                    // 📥 Load data
                    var data = await query
                        .OrderByDescending(td => td.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    // 🔁 Mapping
                    var mapped = data.Select(td => new TransactionDetailDto
                    {
                        Id = td.Id,
                        AccountId = td.AccountId,
                        AccountName = td.Account?.Name ?? "N/A",

                        Amount = td.Amount,
                        EntryType = td.EntryType,

                        CurrencyId = td.CurrencyId,
                        CurrencyCode = td.Currency?.Code ?? "N/A",

                        ReferenceNo = td.Transaction?.ReferenceNo,
                        Description = td.Transaction?.Description,

                        CreatedAt = td.CreatedAt // already UTC
                    }).ToList();

                    return new PagedResponse<TransactionDetailDto>(
                        mapped,
                        page,
                        pageSize,
                        totalRecords
                    );
                },
                successMessageFactory: r => "Statement for account fetched successfully",
                cacheMessage: "Account statement loaded from cache",
                errorMessage: "Error fetching account statement"
            );
        }


        public async Task<ResponseWrapper<PagedResponse<ExchangeDto>>> GetAllExchangesAsync(
            int page = 1,
            int pageSize = 10,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            // 1. Guard Clauses
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            // ✅ FIX: Normalize dates to UTC
            DateTime? startDate = null;
            DateTime? endDate = null;

            if (fromDate.HasValue)
            {
                startDate = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
            }

            if (toDate.HasValue)
            {
                endDate = DateTime.SpecifyKind(
                    toDate.Value.Date.AddDays(1).AddTicks(-1),
                    DateTimeKind.Utc
                );
            }

            return await ExecuteWithCacheAsync(
                // ✅ FIX: include filters in cache key
                cacheKey: $"{TransactionCacheKey}_{_currentUser.UserId}_EX_P{page}_PS{pageSize}_F{startDate:yyyyMMdd}_TO{endDate:yyyyMMdd}",
                action: async () =>
                {
                    var query = _context.Exchanges
                        .AsNoTracking();

                    // 🔒 Multi-tenant
                    if (!isAdmin)
                    {
                        query = query.Where(x => x.AgencyId == agencyId);
                    }

                    // 🔹 Date filters (UTC safe)
                    if (startDate.HasValue)
                    {
                        query = query.Where(x => x.CreatedAt >= startDate.Value);
                    }

                    if (endDate.HasValue)
                    {
                        query = query.Where(x => x.CreatedAt <= endDate.Value);
                    }

                    // 📊 Count
                    var totalRecords = await query.CountAsync();

                    // 📥 Data
                    var mapped = await query
                        .OrderByDescending(x => x.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ProjectTo<ExchangeDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<ExchangeDto>(
                        mapped,
                        page,
                        pageSize,
                        totalRecords
                    );
                },
                successMessageFactory: result => $"{result.Data.Count} exchanges fetched",
                cacheMessage: "Exchanges loaded from cache",
                errorMessage: "Error fetching exchanges"
            );
        }



        public async Task<ResponseWrapper<PagedResponse<TransferDto>>> GetAllTransfersAsync(
            int page = 1,
            int pageSize = 10,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            // 1. Guard Clauses
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            // 2. Role Check
            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                // ✅ include filters in cache key
                cacheKey: $"{TransactionCacheKey}_{_currentUser.UserId}_T_P{page}_PS{pageSize}_{fromDate}_{toDate}",
                action: async () =>
                {
                    var query = _context.Transfers
                        .AsNoTracking()
                        .AsQueryable();

                    // 3. Multi-tenant filter
                    if (!isAdmin)
                    {
                        query = query.Where(x => x.AgencyId == agencyId);
                    }

                    // ✅ 4. Date Filters with UTC FIX
                    if (fromDate.HasValue)
                    {
                        var fromUtc = fromDate.Value.Kind == DateTimeKind.Utc
                            ? fromDate.Value
                            : DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);

                        query = query.Where(x => x.CreatedAt >= fromUtc);
                    }

                    if (toDate.HasValue)
                    {
                        var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);

                        var toUtc = endOfDay.Kind == DateTimeKind.Utc
                            ? endOfDay
                            : DateTime.SpecifyKind(endOfDay, DateTimeKind.Utc);

                        query = query.Where(x => x.CreatedAt <= toUtc);
                    }

                    var totalRecords = await query.CountAsync();

                    var mapped = await query
                        .OrderByDescending(x => x.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ProjectTo<TransferDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<TransferDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} Transfer fetched",
                cacheMessage: "Transfer loaded from cache",
                errorMessage: "Error fetching transfer"
            );
        }

        public async Task<ResponseWrapper<PagedResponse<LoanDto>>> GetAllLoanAsync(
            int page = 1,
            int pageSize = 10,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                cacheKey: $"{TransactionCacheKey}_{_currentUser.UserId}_L_P{page}_PS{pageSize}_{fromDate}_{toDate}",
                action: async () =>
                {
                    var query = _context.Loans
                        .Include(x => x.Currency)
                        .AsNoTracking()
                        .AsQueryable();

                    if (!isAdmin)
                        query = query.Where(x => x.AgencyId == agencyId);

                    if (fromDate.HasValue)
                    {
                        var fromUtc = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
                        query = query.Where(x => x.CreatedAt >= fromUtc);
                    }

                    if (toDate.HasValue)
                    {
                        var toUtc = DateTime.SpecifyKind(
                            toDate.Value.Date.AddDays(1).AddTicks(-1),
                            DateTimeKind.Utc
                        );

                        query = query.Where(x => x.CreatedAt <= toUtc);
                    }

                    var totalRecords = await query.CountAsync();

                    var mapped = await query
                        .OrderByDescending(x => x.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ProjectTo<LoanDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<LoanDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} Loans fetched",
                cacheMessage: "Loans loaded from cache",
                errorMessage: "Error fetching loans"
            );
        }
        public async Task<ResponseWrapper<PagedResponse<ExpenseDto>>> GetAllExpensesAsync(
            int page = 1,
            int pageSize = 10,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            // 1. Guard Clauses
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            // 2. Role Check
            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                // ✅ include filters in cache key
                cacheKey: $"{TransactionCacheKey}_{_currentUser.UserId}_E_P{page}_PS{pageSize}_{fromDate}_{toDate}",
                action: async () =>
                {
                    var query = _context.Expenses
                        .AsNoTracking()
                        .AsQueryable();

                    // 3. Multi-tenant filter
                    if (!isAdmin)
                    {
                        query = query.Where(x => x.AgencyId == agencyId);
                    }

                    // ✅ 4. Date Filters with UTC FIX
                    if (fromDate.HasValue)
                    {
                        var fromUtc = fromDate.Value.Kind == DateTimeKind.Utc
                            ? fromDate.Value
                            : DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);

                        query = query.Where(x => x.CreatedAt >= fromUtc);
                    }

                    if (toDate.HasValue)
                    {
                        var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);

                        var toUtc = endOfDay.Kind == DateTimeKind.Utc
                            ? endOfDay
                            : DateTime.SpecifyKind(endOfDay, DateTimeKind.Utc);

                        query = query.Where(x => x.CreatedAt <= toUtc);
                    }

                    var totalRecords = await query.CountAsync();

                    var mapped = await query
                        .OrderByDescending(x => x.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ProjectTo<ExpenseDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<ExpenseDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} Expenses fetched",
                cacheMessage: "Expenses loaded from cache",
                errorMessage: "Error fetching expenses"
            );
        }


        public async Task<ResponseWrapper<PagedResponse<DepositDto>>> GetAllDepositsAsync(
       int page = 1,
       int pageSize = 10,
       DateTime? fromDate = null,
       DateTime? toDate = null)
        {
            // 1. Guard clauses
            if (page <= 0)
                page = 1;

            if (pageSize <= 0)
                pageSize = 10;

            pageSize = Math.Min(pageSize, 100);

            // 2. Current user information
            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                // V2 waxaa lagu daray si cache-kii hore aan loo soo celin
                cacheKey:
                    $"{TransactionCacheKey}_V2_{_currentUser.UserId}_D_P{page}_PS{pageSize}_{fromDate:O}_{toDate:O}",

                action: async () =>
                {
                    // 3. Main deposit query
                    var depositQuery = _context.Deposits
                        .AsNoTracking()
                        .AsQueryable();

                    // 4. Agency filter
                    if (!isAdmin)
                    {
                        depositQuery = depositQuery
                            .Where(x => x.AgencyId == agencyId);
                    }

                    // 5. From-date filter
                    if (fromDate.HasValue)
                    {
                        var fromUtc = fromDate.Value.Kind == DateTimeKind.Utc
                            ? fromDate.Value
                            : DateTime.SpecifyKind(
                                fromDate.Value,
                                DateTimeKind.Utc);

                        depositQuery = depositQuery
                            .Where(x => x.CreatedAt >= fromUtc);
                    }

                    // 6. To-date filter
                    if (toDate.HasValue)
                    {
                        var toExclusive = toDate.Value.Date.AddDays(1);

                        var toUtc = toExclusive.Kind == DateTimeKind.Utc
                            ? toExclusive
                            : DateTime.SpecifyKind(
                                toExclusive,
                                DateTimeKind.Utc);

                        depositQuery = depositQuery
                            .Where(x => x.CreatedAt < toUtc);
                    }

                    // 7. Count all filtered deposit records
                    var totalRecords = await depositQuery.CountAsync();

                    // 8. Get current page deposits
                    var deposits = await depositQuery
                        .OrderByDescending(x => x.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ProjectTo<DepositDto>(
                            _mapper.ConfigurationProvider)
                        .ToListAsync();

                    if (deposits.Count == 0)
                    {
                        return new PagedResponse<DepositDto>(
                            deposits,
                            page,
                            pageSize,
                            totalRecords);
                    }

                    // 9. Customers currently displayed on this page
                    var customerIds = deposits
                        .Select(x => x.CustomerId)
                        .Distinct()
                        .ToList();

                    // 10. Currencies currently displayed on this page
                    var currencyIds = deposits
                        .Select(x => x.CurrencyId)
                        .Distinct()
                        .ToList();

                    // 11. All deposits for these customers and currencies
                    var customerDepositsQuery = _context.Deposits
                        .AsNoTracking()
                        .Where(x =>
                            customerIds.Contains(x.CustomerId) &&
                            currencyIds.Contains(x.CurrencyId));

                    // 12. All withdrawals for these customers and currencies
                    var customerWithdrawalsQuery = _context.Withdraws
                        .AsNoTracking()
                        .Where(x =>
                            customerIds.Contains(x.CustomerId) &&
                            currencyIds.Contains(x.CurrencyId));

                    // 13. Agency security
                    if (!isAdmin)
                    {
                        customerDepositsQuery = customerDepositsQuery
                            .Where(x => x.AgencyId == agencyId);

                        customerWithdrawalsQuery = customerWithdrawalsQuery
                            .Where(x => x.AgencyId == agencyId);
                    }

                    /*
                     * Haddii aad leedahay cancelled ama rejected transactions,
                     * halkan ku dar status filter-ka saxda ah.
                     *
                     * Tusaale:
                     *
                     * customerDepositsQuery = customerDepositsQuery
                     *     .Where(x => x.Status == TransactionStatus.Completed);
                     *
                     * customerWithdrawalsQuery = customerWithdrawalsQuery
                     *     .Where(x => x.Status == TransactionStatus.Completed);
                     */

                    // 14. Total deposits grouped by customer and currency
                    var depositTotalsResult = await customerDepositsQuery
                        .GroupBy(x => new
                        {
                            x.CustomerId,
                            x.CurrencyId
                        })
                        .Select(group => new
                        {
                            group.Key.CustomerId,
                            group.Key.CurrencyId,

                            // Amount is decimal?, null becomes zero
                            Total = group.Sum(x => x.Amount ?? 0m)
                        })
                        .ToListAsync();

                    // 15. Total withdrawals grouped by customer and currency
                    var withdrawalTotalsResult = await customerWithdrawalsQuery
                        .GroupBy(x => new
                        {
                            x.CustomerId,
                            x.CurrencyId
                        })
                        .Select(group => new
                        {
                            group.Key.CustomerId,
                            group.Key.CurrencyId,

                            // Amount is decimal?, null becomes zero
                            Total = group.Sum(x => x.Amount ?? 0m)
                        })
                        .ToListAsync();

                    // 16. Convert deposit totals to dictionary
                    var depositTotals = depositTotalsResult
                        .ToDictionary(
                            x => (x.CustomerId, x.CurrencyId),
                            x => x.Total);

                    // 17. Convert withdrawal totals to dictionary
                    var withdrawalTotals = withdrawalTotalsResult
                        .ToDictionary(
                            x => (x.CustomerId, x.CurrencyId),
                            x => x.Total);

                    // 18. Add totals to every deposit record
                    foreach (var deposit in deposits)
                    {
                        var key = (
                            deposit.CustomerId,
                            deposit.CurrencyId
                        );

                        deposit.TotalDeposited =
                            depositTotals.TryGetValue(
                                key,
                                out var totalDeposited)
                                ? totalDeposited
                                : 0m;

                        deposit.WithdrawAmount =
                            withdrawalTotals.TryGetValue(
                                key,
                                out var totalWithdrawn)
                                ? totalWithdrawn
                                : 0m;

                        deposit.Balance =
                            deposit.TotalDeposited -
                            deposit.WithdrawAmount;
                    }

                    // 19. Return paginated response
                    return new PagedResponse<DepositDto>(
                        deposits,
                        page,
                        pageSize,
                        totalRecords);
                },

                successMessageFactory: result =>
                    $"{result.Data.Count} deposits fetched",

                cacheMessage: "Deposits loaded from cache",

                errorMessage: "Error fetching deposits"
            );
        }


        public async Task<ResponseWrapper<PagedResponse<WithdrawalDto>>> GetAllWithdrawAsync(
            int page = 1,
            int pageSize = 10,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            // 1. Guard Clauses
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            // 2. Role Check
            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                // ✅ include filters in cache key
                cacheKey: $"{TransactionCacheKey}_{_currentUser.UserId}_W_P{page}_PS{pageSize}_{fromDate}_{toDate}",
                action: async () =>
                {
                    var query = _context.Withdraws
                        .AsNoTracking()
                        .AsQueryable();

                    // 3. Multi-tenant filter
                    if (!isAdmin)
                    {
                        query = query.Where(x => x.AgencyId == agencyId);
                    }

                    // ✅ 4. Date Filters with UTC FIX
                    if (fromDate.HasValue)
                    {
                        var fromUtc = fromDate.Value.Kind == DateTimeKind.Utc
                            ? fromDate.Value
                            : DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);

                        query = query.Where(x => x.CreatedAt >= fromUtc);
                    }

                    if (toDate.HasValue)
                    {
                        var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);

                        var toUtc = endOfDay.Kind == DateTimeKind.Utc
                            ? endOfDay
                            : DateTime.SpecifyKind(endOfDay, DateTimeKind.Utc);

                        query = query.Where(x => x.CreatedAt <= toUtc);
                    }

                    var totalRecords = await query.CountAsync();

                    var mapped = await query
                        .OrderByDescending(x => x.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ProjectTo<WithdrawalDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<WithdrawalDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} Withdrawal fetched",
                cacheMessage: "Withdrawal loaded from cache",
                errorMessage: "Error fetching Withdrawal"
            );
        }



        public async Task<ResponseWrapper<PagedResponse<LoanPaymentDto>>> GetAllLoanPaymentAsync(
        int page = 1,
        int pageSize = 10,
        DateTime? fromDate = null,
        DateTime? toDate = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                cacheKey: $"{TransactionCacheKey}_{_currentUser.UserId}_LP_P{page}_PS{pageSize}_{fromDate}_{toDate}",
                action: async () =>
                {
                    var query = _context.LoanPayments
                        .Include(x => x.Loan)
                            .ThenInclude(x => x.Currency)
                        .AsNoTracking()
                        .AsQueryable();

                    if (!isAdmin)
                        query = query.Where(x => x.AgencyId == agencyId);

                    if (fromDate.HasValue)
                    {
                        var fromUtc = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
                        query = query.Where(x => x.CreatedAt >= fromUtc);
                    }

                    if (toDate.HasValue)
                    {
                        var toUtc = DateTime.SpecifyKind(
                            toDate.Value.Date.AddDays(1).AddTicks(-1),
                            DateTimeKind.Utc
                        );

                        query = query.Where(x => x.CreatedAt <= toUtc);
                    }

                    var totalRecords = await query.CountAsync();

                    var mapped = await query
                        .OrderByDescending(x => x.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ProjectTo<LoanPaymentDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<LoanPaymentDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} LoanPayment fetched",
                cacheMessage: "LoanPayment loaded from cache",
                errorMessage: "Error fetching loanPayment"
            );
        }


        public async Task<ResponseWrapper<PagedResponse<RevenueDto>>> GetAllRevinuesAsync(
     int page = 1,
     int pageSize = 10,
     DateTime? fromDate = null,
     DateTime? toDate = null)
        {
            // 1. Guard Clauses
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            // 2. Role Check
            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                // ✅ include filters in cache key
                cacheKey: $"{TransactionCacheKey}_{_currentUser.UserId}_R_P{page}_PS{pageSize}_{fromDate}_{toDate}",
                action: async () =>
                {
                    var query = _context.Revenues
                        .AsNoTracking()
                        .AsQueryable();

                    // 3. Multi-tenant filter
                    if (!isAdmin)
                    {
                        query = query.Where(x => x.AgencyId == agencyId);
                    }

                    // ✅ 4. Date Filters with UTC FIX
                    if (fromDate.HasValue)
                    {
                        var fromUtc = fromDate.Value.Kind == DateTimeKind.Utc
                            ? fromDate.Value
                            : DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);

                        query = query.Where(x => x.CreatedAt >= fromUtc);
                    }

                    if (toDate.HasValue)
                    {
                        var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);

                        var toUtc = endOfDay.Kind == DateTimeKind.Utc
                            ? endOfDay
                            : DateTime.SpecifyKind(endOfDay, DateTimeKind.Utc);

                        query = query.Where(x => x.CreatedAt <= toUtc);
                    }

                    var totalRecords = await query.CountAsync();

                    var mapped = await query
                        .OrderByDescending(x => x.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ProjectTo<RevenueDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<RevenueDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} Revenue fetched",
                cacheMessage: "Revenue loaded from cache",
                errorMessage: "Error fetching revenue"
            );
        }

        public async Task<ResponseWrapper<ProfitLossDto>> GetProfitLossAsync(
     DateTime? fromDate = null,
     DateTime? toDate = null)
        {
            // 1. Guard Clause
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
                throw new ArgumentException("FromDate cannot be greater than ToDate");

            // 2. Role Check
            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                cacheKey: $"{TransactionCacheKey}_{_currentUser.UserId}_PL_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}",
                action: async () =>
                {
                    var query = _context.TransactionDetails
                        .AsNoTracking();

                    // 3. Multi-tenant filter
                    if (!isAdmin)
                    {
                        query = query.Where(x => x.Account.AgencyId == agencyId);
                    }

                    // 4. Date filtering (UTC safe)
                    if (fromDate.HasValue)
                    {
                        var start = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
                        query = query.Where(x => x.CreatedAt >= start);
                    }

                    if (toDate.HasValue)
                    {
                        var end = DateTime.SpecifyKind(
                            toDate.Value.Date.AddDays(1).AddTicks(-1),
                            DateTimeKind.Utc
                        );
                        query = query.Where(x => x.CreatedAt <= end);
                    }

                    // 5. Single optimized aggregation query
                    var result = await query
                        .GroupBy(x => 1)
                        .Select(g => new ProfitLossDto
                        {
                            Revenue = g.Where(x => x.Account.Nature == AccountNatureEnum.Revenue && x.EntryType == 2)
                                       .Sum(x => (decimal?)x.Amount) ?? 0,

                            Expense = g.Where(x => x.Account.Nature == AccountNatureEnum.Expense && x.EntryType == 1)
                                       .Sum(x => (decimal?)x.Amount) ?? 0
                        })
                        .FirstOrDefaultAsync() ?? new ProfitLossDto();

                    // 6. Final calculation
                    result.Profit = result.Revenue - result.Expense;

                    return result;
                },
                successMessageFactory: result => $"Profit calculated: {result.Profit}",
                cacheMessage: "Profit & Loss loaded from cache",
                errorMessage: "Error calculating profit & loss"
            );
        }
        public async Task<ResponseWrapper<PagedResponse<DailyReportDto>>> GetDailyReportAsync(
     DateTime fromDate,
     DateTime toDate,
     int page = 1,
     int pageSize = 10)
        {
            // 1. Guard Clauses
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            if (fromDate > toDate)
                throw new ArgumentException("FromDate cannot be greater than ToDate");

            // 2. Role Check
            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                cacheKey: $"{TransactionCacheKey}_{_currentUser.UserId}_DR_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}_P{page}_PS{pageSize}",
                action: async () =>
                {
                    // 3. Convert to UTC
                    var startDate = DateTime.SpecifyKind(fromDate.Date, DateTimeKind.Utc);

                    var endDate = DateTime.SpecifyKind(
                        toDate.Date.AddDays(1).AddTicks(-1),
                        DateTimeKind.Utc
                    );

                    var query = _context.TransactionDetails
                        .AsNoTracking()
                        .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate);

                    // 4. Multi-tenant filter
                    if (!isAdmin)
                    {
                        query = query.Where(x => x.Account.AgencyId == agencyId);
                    }

                    // 5. Grouping (Daily Aggregation)
                    var groupedQuery = query
                        .GroupBy(x => x.CreatedAt.Date)
                        .Select(g => new DailyReportDto
                        {
                            Date = g.Key,
                            TotalIn = g.Where(x => x.EntryType == 1)
                                       .Sum(x => (decimal?)x.Amount) ?? 0,
                            TotalOut = g.Where(x => x.EntryType == 2)
                                        .Sum(x => (decimal?)x.Amount) ?? 0
                        })
                        .Select(x => new DailyReportDto
                        {
                            Date = x.Date,
                            TotalIn = x.TotalIn,
                            TotalOut = x.TotalOut,
                            Balance = x.TotalIn - x.TotalOut
                        });

                    // 6. Total Count (after grouping)
                    var totalRecords = await groupedQuery.CountAsync();

                    // 7. Pagination
                    var data = await groupedQuery
                        .OrderBy(x => x.Date)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    return new PagedResponse<DailyReportDto>(data, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} daily reports fetched",
                cacheMessage: "Daily report loaded from cache",
                errorMessage: "Error generating daily report"
            );
        }




        public async Task<ResponseWrapper<ProfitLossDetailedDto>> GetProfitLossDetailedAsync(
       DateTime? fromDate = null,
       DateTime? toDate = null,
       int page = 1,
       int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            pageSize = Math.Min(pageSize, 100);

            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
                throw new ArgumentException("FromDate cannot be greater than ToDate");

            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                cacheKey: $"{TransactionCacheKey}_{_currentUser.UserId}_PL_DETAIL_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}_P{page}_PS{pageSize}",
                action: async () =>
                {
                    var query = _context.TransactionDetails
                        .Include(x => x.Account)
                        .AsNoTracking();

                    if (!isAdmin)
                        query = query.Where(x => x.Account.AgencyId == agencyId);

                    if (fromDate.HasValue)
                    {
                        var start = DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc);
                        query = query.Where(x => x.CreatedAt >= start);
                    }

                    if (toDate.HasValue)
                    {
                        var end = DateTime.SpecifyKind(
                            toDate.Value.Date.AddDays(1).AddTicks(-1),
                            DateTimeKind.Utc
                        );
                        query = query.Where(x => x.CreatedAt <= end);
                    }

                    // 🔥 GET USED CURRENCIES ONLY
                    var currencies = await query
                        .Select(x => x.CurrencyId)
                        .Distinct()
                        .ToListAsync();

                    // 🔥 GET LATEST RATES (ONE QUERY)
                    var rates = await _context.ExchangeRates
                        .Where(r => currencies.Contains(r.CurrencyId))
                        .GroupBy(r => r.CurrencyId)
                        .ToDictionaryAsync(
                            g => g.Key,
                            g => g.OrderByDescending(x => x.CreatedAt).First().Rate
                        );

                    int baseCurrencyId = 1; // 👉 USD

                    // ===============================
                    // 🔥 FILTER ONLY REVENUE + EXPENSE
                    // ===============================
                    var filtered = query.Where(x =>
                        (x.Account.Nature == AccountNatureEnum.Revenue && x.EntryType == 2) ||
                        (x.Account.Nature == AccountNatureEnum.Expense && x.EntryType == 1)
                    );

                    // 📥 Load ONLY needed rows
                    var rawData = await filtered
                        .Select(x => new
                        {
                            x.Account.Name,
                            x.Account.Nature,
                            x.Amount,
                            x.CurrencyId,
                            Date = x.CreatedAt.Date
                        })
                        .ToListAsync();

                    // ===============================
                    // 🔥 CONVERT TO BASE CURRENCY
                    // ===============================
                    var converted = rawData.Select(x =>
                    {
                        var amount = x.CurrencyId == baseCurrencyId
                            ? x.Amount
                            : x.Amount / rates[x.CurrencyId];

                        return new
                        {
                            x.Name,
                            x.Nature,
                            Amount = amount,
                            x.Date
                        };
                    });

                    // ===============================
                    // 🔥 GROUP (AFTER CONVERSION)
                    // ===============================
                    var grouped = converted
                        .GroupBy(x => new { x.Name, x.Nature, x.Date })
                        .Select(g => new ProfitLossItemDto
                        {
                            AccountName = g.Key.Name,
                            Type = g.Key.Nature == AccountNatureEnum.Revenue ? "Revenue" : "Expense",
                            Amount = g.Sum(x => x.Amount),
                            Date = g.Key.Date
                        });

                    var totalRecords = grouped.Count();

                    var data = grouped
                        .OrderByDescending(x => x.Date)
                        .ThenByDescending(x => x.Amount)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

                    // ===============================
                    // 🔥 TOTALS (AFTER CONVERSION)
                    // ===============================
                    var totalRevenue = converted
                        .Where(x => x.Nature == AccountNatureEnum.Revenue)
                        .Sum(x => x.Amount);

                    var totalExpense = converted
                        .Where(x => x.Nature == AccountNatureEnum.Expense)
                        .Sum(x => x.Amount);

                    return new ProfitLossDetailedDto
                    {
                        TotalRevenue = totalRevenue,
                        TotalExpense = totalExpense,
                        Profit = totalRevenue - totalExpense,

                        Details = new PagedResponse<ProfitLossItemDto>(
                            data,
                            page,
                            pageSize,
                            totalRecords
                        )
                    };
                },
                successMessageFactory: r => $"Profit: {r.Profit}",
                cacheMessage: "Detailed P&L loaded from cache",
                errorMessage: "Error generating detailed profit & loss"
            );
        }


        public async Task<ResponseWrapper<List<AccountLookupDto>>> GetAccountsLookupAsync()
        {
            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                cacheKey: $"{AccountCacheKey}_Lookup_{_currentUser.UserId}",
                action: async () =>
                {
                    var query = _context.Accounts
                        .AsNoTracking();

                    // 🔒 Multi-tenant filter
                    if (!isAdmin)
                    {
                        query = query.Where(x => x.AgencyId == agencyId);
                    }

                    var data = await query
                        .OrderBy(x => x.Name)
                        .Select(x => new AccountLookupDto
                        {
                            Id = x.Id,
                            Name = x.Name
                        })
                        .ToListAsync();

                    return data;
                },
                successMessageFactory: result => $"{result.Count} accounts fetched",
                cacheMessage: "Accounts lookup loaded from cache",
                errorMessage: "Error fetching accounts lookup"
            );
        }



        public async Task<ResponseWrapper<List<AccountLookupDto>>> GetAccountExpenseLookupAsync()
        {
            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                cacheKey: $"{AccountCacheKey}_Lookup_Expenses_{_currentUser.UserId}",
                action: async () =>
                {
                    var query = _context.Accounts
                        .AsNoTracking()
                        .AsQueryable();

                    // 🔒 Multi-tenant
                    if (!isAdmin)
                    {
                        query = query.Where(x => x.AgencyId == agencyId);
                    }

                    // 🔥 FILTER: ONLY exchange accounts
                    query = query.Where(x =>
                        x.AccountType == AccountTypeEnum.Expense

                    );

                    var data = await query
                        .OrderBy(x => x.Name)
                        .Select(x => new AccountLookupDto
                        {
                            Id = x.Id,
                            Name = x.Name,

                            CurrencyId = x.CurrencyId,
                            CurrencyName = x.Currency.Name
                        })
                        .ToListAsync();

                    return data;
                },
                successMessageFactory: result => $"{result.Count} exchange accounts fetched",
                cacheMessage: "Exchange accounts loaded from cache",
                errorMessage: "Error fetching exchange accounts"
            );
        }



        public async Task<ResponseWrapper<List<AccountLookupDto>>> GetAccountEchangeLookupAsync()
        {
            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                cacheKey: $"{AccountCacheKey}_Lookup_Exchange_{_currentUser.UserId}",
                action: async () =>
                {
                    var query = _context.Accounts
                        .AsNoTracking()
                        .AsQueryable();

                    // 🔒 Multi-tenant
                    if (!isAdmin)
                    {
                        query = query.Where(x => x.AgencyId == agencyId);
                    }

                    // 🔥 FILTER: ONLY exchange accounts
                    query = query.Where(x =>
                        x.AccountType == AccountTypeEnum.Cash ||
                        x.AccountType == AccountTypeEnum.Bank ||
                        x.AccountType == AccountTypeEnum.Wallet 
                    );

                    var data = await query
                        .OrderBy(x => x.Name)
                        .Select(x => new AccountLookupDto
                        {
                            Id = x.Id,
                            Name = x.Name,

                            CurrencyId = x.CurrencyId,
                            CurrencyName = x.Currency.Name
                        })
                        .ToListAsync();

                    return data;
                },
                successMessageFactory: result => $"{result.Count} exchange accounts fetched",
                cacheMessage: "Exchange accounts loaded from cache",
                errorMessage: "Error fetching exchange accounts"
            );
        }




        public async Task<ResponseWrapper<List<AccountLookupDto>>> GetAccountRevenuesLookupAsync()
        {
            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                cacheKey: $"{AccountCacheKey}_Lookup_Revenues_{_currentUser.UserId}",
                action: async () =>
                {
                    var query = _context.Accounts
                        .AsNoTracking()
                        .AsQueryable();

                    // 🔒 Multi-tenant
                    if (!isAdmin)
                    {
                        query = query.Where(x => x.AgencyId == agencyId);
                    }

                    // 🔥 FILTER: ONLY exchange accounts
                    query = query.Where(x =>
                        x.AccountType == AccountTypeEnum.Revenue

                    );

                    var data = await query
                        .OrderBy(x => x.Name)
                        .Select(x => new AccountLookupDto
                        {
                            Id = x.Id,
                            Name = x.Name,

                            CurrencyId = x.CurrencyId,
                            CurrencyName = x.Currency.Name
                        })
                        .ToListAsync();

                    return data;
                },
                successMessageFactory: result => $"{result.Count} exchange accounts fetched",
                cacheMessage: "Exchange accounts loaded from cache",
                errorMessage: "Error fetching exchange accounts"
            );
        }

        public async Task<ResponseWrapper<List<CurrencyLookupDto>>> GetCurrencyLookupAsync()
        {

            return await ExecuteWithCacheAsync(
                cacheKey: $"{AccountCacheKey}_Lookup_{_currentUser.UserId}",
                action: async () =>
                {
                    var query = _context.Currencies
                        .AsNoTracking();

                    var data = await query
                        .OrderBy(x => x.Name)
                        .Select(x => new CurrencyLookupDto
                        {
                            Id = x.Id,
                            Name = x.Name
                        })
                        .ToListAsync();

                    return data;
                },
                successMessageFactory: result => $"{result.Count} accounts fetched",
                cacheMessage: "Accounts lookup loaded from cache",
                errorMessage: "Error fetching accounts lookup"
            );
        }




        public async Task<ResponseWrapper<int>> CreateExchangeSettingsAsync(CreateExchangeSettingsDto dto)
        {
            var exists = await _context.ExchangeSettings
                .AnyAsync(x => x.CurrencyId == dto.CurrencyId);

            if (exists)
                return await ResponseWrapper<int>.FailureAsync("Already exists for this currency");

            var entity = new ExchangeSettings
            {
                CurrencyId = dto.CurrencyId,
                FeeRate = dto.FeeRate,
                ProfitRate = dto.ProfitRate,
                AgencyId = _currentUser.AgencyId,
                BranchId = _currentUser.BranchId,
                UserId = _currentUser.UserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.ExchangeSettings.Add(entity);
            await _context.SaveChangesAsync();

            return await ResponseWrapper<int>.SuccessAsync(entity.Id, "Saved successfully");
        }


        public async Task<ResponseWrapper<bool>> UpdateExchangeSettingsAsync(UpdateExchangeSettingsDto dto)
        {
            var entity = await _context.ExchangeSettings
                .FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (entity == null)
                return await ResponseWrapper<bool>.FailureAsync("Not found");

            entity.FeeRate = dto.FeeRate;
            entity.ProfitRate = dto.ProfitRate;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await ResponseWrapper<bool>.SuccessAsync(true, "Updated successfully");
        }


        public async Task<List<ExchangeSettingsDto>> GetAllExchangeSettingsAsync()
        {
            return await _context.ExchangeSettings
                .Select(x => new ExchangeSettingsDto
                {
                    Id = x.Id,
                    CurrencyId = x.CurrencyId,
                    FeeRate = x.FeeRate,
                    ProfitRate = x.ProfitRate,
                    IsActive = x.IsActive,

                    CurrencyNmane = x.Currency != null ? x.Currency.Name : ""
                })
                .ToListAsync();
        }



        public async Task<ResponseWrapper<List<RecentTransactionDto>>> GetRecentTransactionsAsync()
        {
            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                cacheKey: $"{TransactionCacheKey}_{_currentUser.UserId}_Recent_10",
                action: async () =>
                {
                    var query = _context.Transactions
                        .AsNoTracking();

                    if (!isAdmin)
                    {
                        query = query.Where(x => x.AgencyId == agencyId);
                    }

                    var data = await query
                        .OrderByDescending(x => x.CreatedAt)
                        .Take(10)
                        .Select(x => new RecentTransactionDto
                        {
                            ReferenceNo = x.ReferenceNo,
                            Status = x.Status,
                            TransactionType = x.TransactionType,
                            TotalAmount = Math.Round((decimal)x.TotalAmount, 2),
                            CreatedAt = x.CreatedAt,
                            Username = x.User.FirstName

                        })
                        .ToListAsync();

                    return data;
                },
                successMessageFactory: result => $"{result.Count} recent transactions fetched",
                cacheMessage: "Recent transactions loaded from cache",
                errorMessage: "Error fetching recent transactions"
            );
        }





        public async Task<ResponseWrapper<List<AccountLookupDto>>> GetMainCashAccountsAsync()
        {
            var agencyId = _currentUser.AgencyId;

            var data = await _context.Accounts
                .Include(x => x.Currency)
                .Where(x =>
                    x.AgencyId == agencyId &&
                    x.AccountType == AccountTypeEnum.Cash)
                .OrderBy(x => x.Currency.Code)
                .ThenBy(x => x.Name)
                .Select(x => new AccountLookupDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    CurrencyId = x.CurrencyId,
                })
                .AsNoTracking()
                .ToListAsync();

            return await ResponseWrapper<List<AccountLookupDto>>.SuccessAsync(
                data,
                "Main cash accounts fetched successfully"
            );
        }




        public async Task<ResponseWrapper<List<AccountLookupDto>>> GetCapitalAccountsAsync()
        {
            var agencyId = _currentUser.AgencyId;

            var data = await _context.Accounts
                .Include(x => x.Currency)
                .Where(x =>
                    x.AgencyId == agencyId &&
                    x.AccountType == AccountTypeEnum.Capital)
                .OrderBy(x => x.Currency.Code)
                .ThenBy(x => x.Name)
                .Select(x => new AccountLookupDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    CurrencyId = x.CurrencyId,
                })
                .AsNoTracking()
                .ToListAsync();

            return await ResponseWrapper<List<AccountLookupDto>>.SuccessAsync(
                data,
                "Capital accounts fetched successfully"
            );
        }



        public async Task<ResponseWrapper<DashboardCardsDto>> GetDashboardCardsAsync()
        {
            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = todayStart.AddDays(1);

            return await ExecuteWithCacheAsync(
                cacheKey: $"{TransactionCacheKey}_{_currentUser.UserId}_Dashboard_Cards_{todayStart:yyyyMMdd}",
                action: async () =>
                {
                    var transactionDetailsQuery = _context.TransactionDetails
                        .Include(x => x.Account)
                        .Include(x => x.Currency)
                        .AsNoTracking()
                        .AsQueryable();

                    if (!isAdmin)
                    {
                        transactionDetailsQuery = transactionDetailsQuery
                            .Where(x => x.Account.AgencyId == agencyId);
                    }

                    // 1. Latest exchange rate for each currency
                    // Assumption: Rate means 1 base currency = Rate of this currency.
                    // Example: if base is USD, SOS rate may be 26000.
                    var latestRates = await _context.ExchangeRates
                        .AsNoTracking()
                        .GroupBy(x => x.CurrencyId)
                        .Select(g => new
                        {
                            CurrencyId = g.Key,
                            Rate = g.OrderByDescending(x => x.CreatedAt)
                                .Select(x => x.Rate)
                                .FirstOrDefault()
                        })
                        .ToDictionaryAsync(x => x.CurrencyId, x => x.Rate);

                    decimal ToBase(decimal amount, int currencyId)
                    {
                        if (!latestRates.TryGetValue(currencyId, out var rate) || rate <= 0)
                            return 0;

                        return amount / rate;
                    }

                    // 2. Current balance by currency: Cash + Bank + Wallet
                    var balancesByCurrency = await transactionDetailsQuery
                        .Where(x =>
                            x.Account.AccountType == AccountTypeEnum.Cash ||
                            x.Account.AccountType == AccountTypeEnum.Bank ||
                            x.Account.AccountType == AccountTypeEnum.Wallet
                        )
                        .GroupBy(x => new
                        {
                            x.CurrencyId,
                            CurrencyCode = x.Currency.Code
                        })
                        .Select(g => new DashboardCurrencyCardDto
                        {
                            CurrencyId = g.Key.CurrencyId,
                            CurrencyCode = g.Key.CurrencyCode,

                            Balance = Math.Round(
                                (
                                    g.Where(x => x.EntryType == 1)
                                        .Sum(x => (decimal?)x.Amount) ?? 0
                                )
                                -
                                (
                                    g.Where(x => x.EntryType == 2)
                                        .Sum(x => (decimal?)x.Amount) ?? 0
                                ),
                                2
                            )
                        })
                        .ToListAsync();

                    var currentBalanceBase = balancesByCurrency
                        .Sum(x => ToBase(x.Balance, x.CurrencyId));

                    // 3. Cash in / cash out today by currency
                    var cashFlowTodayByCurrency = await transactionDetailsQuery
                        .Where(x =>
                            x.CreatedAt >= todayStart &&
                            x.CreatedAt <= todayEnd &&
                            (
                                x.Account.AccountType == AccountTypeEnum.Cash ||
                                x.Account.AccountType == AccountTypeEnum.Bank ||
                                x.Account.AccountType == AccountTypeEnum.Wallet
                            )
                        )
                        .GroupBy(x => new
                        {
                            x.CurrencyId,
                            CurrencyCode = x.Currency.Code
                        })
                        .Select(g => new DashboardCurrencyCashFlowDto
                        {
                            CurrencyId = g.Key.CurrencyId,
                            CurrencyCode = g.Key.CurrencyCode,

                            CashInToday = Math.Round(
                                g.Where(x => x.EntryType == 1)
                                    .Sum(x => (decimal?)x.Amount) ?? 0,
                                2
                            ),

                            CashOutToday = Math.Round(
                                g.Where(x => x.EntryType == 2)
                                    .Sum(x => (decimal?)x.Amount) ?? 0,
                                2
                            ),

                            NetToday = Math.Round(
                                (
                                    g.Where(x => x.EntryType == 1)
                                        .Sum(x => (decimal?)x.Amount) ?? 0
                                )
                                -
                                (
                                    g.Where(x => x.EntryType == 2)
                                        .Sum(x => (decimal?)x.Amount) ?? 0
                                ),
                                2
                            )
                        })
                        .ToListAsync();

                    // 4. Total payable account in base currency
                    // Payable is liability: Credit - Debit
                    var payableByCurrency = await transactionDetailsQuery
                        .Where(x => x.Account.AccountType == AccountTypeEnum.PAYABLE)
                        .GroupBy(x => x.CurrencyId)
                        .Select(g => new
                        {
                            CurrencyId = g.Key,

                            Balance =
                                (
                                    g.Where(x => x.EntryType == 2)
                                        .Sum(x => (decimal?)x.Amount) ?? 0
                                )
                                -
                                (
                                    g.Where(x => x.EntryType == 1)
                                        .Sum(x => (decimal?)x.Amount) ?? 0
                                )
                        })
                        .ToListAsync();

                    var totalPayableAccountBase = payableByCurrency
                        .Sum(x => ToBase(x.Balance, x.CurrencyId));

                    // 5. Total receivable account in base currency
                    // Receivable is asset: Debit - Credit
                    var receivableByCurrency = await transactionDetailsQuery
                        .Where(x => x.Account.AccountType == AccountTypeEnum.RECEIVABLE)
                        .GroupBy(x => x.CurrencyId)
                        .Select(g => new
                        {
                            CurrencyId = g.Key,

                            Balance =
                                (
                                    g.Where(x => x.EntryType == 1)
                                        .Sum(x => (decimal?)x.Amount) ?? 0
                                )
                                -
                                (
                                    g.Where(x => x.EntryType == 2)
                                        .Sum(x => (decimal?)x.Amount) ?? 0
                                )
                        })
                        .ToListAsync();

                    var totalReceivableAccountBase = receivableByCurrency
                        .Sum(x => ToBase(x.Balance, x.CurrencyId));

                    // 6. Daily profit in base currency: Revenue - Expense
                    // 6. Daily profit in base currency: Revenue - Expense
                    var dailyProfitByCurrency = await transactionDetailsQuery
                        .Where(x =>
                            x.CreatedAt >= todayStart &&
                            x.CreatedAt < todayEnd
                        )
                        .GroupBy(x => x.CurrencyId)
                        .Select(g => new
                        {
                            CurrencyId = g.Key,

                            Revenue = Math.Round(
                                g.Where(x =>
                                        x.Account.Nature == AccountNatureEnum.Revenue &&
                                        x.EntryType == 2
                                    )
                                    .Sum(x =>
                                        (decimal?)Math.Round(x.Amount, 2)
                                    ) ?? 0m,
                                2
                            ),

                            Expense = Math.Round(
                                g.Where(x =>
                                        x.Account.Nature == AccountNatureEnum.Expense &&
                                        x.EntryType == 1
                                    )
                                    .Sum(x =>
                                        (decimal?)Math.Round(x.Amount, 2)
                                    ) ?? 0m,
                                2
                            )
                        })
                        .ToListAsync();

                    decimal dailyProfitBase = 0m;

                    foreach (var item in dailyProfitByCurrency)
                    {
                        var profit = checked(item.Revenue - item.Expense);

                        dailyProfitBase = checked(
                            dailyProfitBase +
                            ToBase(profit, item.CurrencyId)
                        );
                    }

                    // 7. Today transaction count
                    var transactionsQuery = _context.Transactions
                        .AsNoTracking()
                        .AsQueryable();

                    if (!isAdmin)
                    {
                        transactionsQuery = transactionsQuery
                            .Where(x => x.AgencyId == agencyId);
                    }

                    var todayTransactions = await transactionsQuery
                        .CountAsync(x =>
                            x.CreatedAt >= todayStart &&
                            x.CreatedAt <= todayEnd
                        );

                    return new DashboardCardsDto
                    {
                        TotalPayableAccountBase = Math.Round(totalPayableAccountBase, 2),
                        TotalReceivableAccountBase = Math.Round(totalReceivableAccountBase, 2),

                        CurrentBalanceBase = Math.Round(currentBalanceBase, 2),
                        DailyProfitBase = Math.Round(dailyProfitBase, 2),
                        TodayTransactions = todayTransactions,

                        BalancesByCurrency = balancesByCurrency,
                        CashFlowTodayByCurrency = cashFlowTodayByCurrency
                    };
                },
                successMessageFactory: result => "Dashboard cards fetched successfully",
                cacheMessage: "Dashboard cards loaded from cache",
                errorMessage: "Error fetching dashboard cards"
            );
        }



        public async Task<ResponseWrapper<List<CashOpeningReportDto>>> GetCashOpeningReportAsync(
    DateTime? fromDate = null,
    DateTime? toDate = null)
        {
            var agencyId = _currentUser.AgencyId;

            var query = _context.Transactions
                .Include(t => t.Details)
                    .ThenInclude(d => d.Account)
                .Include(t => t.Details)
                    .ThenInclude(d => d.Currency)
                .Where(t =>
                    t.TransactionType == TransactionTypeEnum.CashOpening &&
                    t.AgencyId == agencyId)
                .AsNoTracking();

            if (fromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc));

            if (toDate.HasValue)
            {
                var endDate = DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                query = query.Where(t => t.CreatedAt <= endDate);
            }

            var data = await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new CashOpeningReportDto
                {
                    TransactionId = t.Id,
                    ReferenceNo = t.ReferenceNo,
                    Date = t.CreatedAt,

                    CashAccountName = t.Details
                        .Where(d => d.EntryType == 1)
                        .Select(d => d.Account.Name)
                        .FirstOrDefault() ?? "",

                    CapitalAccountName = t.Details
                        .Where(d => d.EntryType == 2)
                        .Select(d => d.Account.Name)
                        .FirstOrDefault() ?? "",

                    CurrencyCode = t.Details
                        .Select(d => d.Currency.Code)
                        .FirstOrDefault() ?? "",

                    Amount = t.TotalAmount,
                    Description = t.Description
                })
                .ToListAsync();

            return await ResponseWrapper<List<CashOpeningReportDto>>.SuccessAsync(
                data,
                "Cash opening report fetched successfully"
            );
        }



        public async Task<ResponseWrapper<List<DailyCashReportDto>>> GetDailyCashReportAsync(
    DateTime? date = null,
    Guid? accountId = null,
    int? currencyId = null)
        {
            var agencyId = _currentUser.AgencyId;

            var reportDate = date?.Date ?? DateTime.UtcNow.Date;

            var startDate = DateTime.SpecifyKind(reportDate, DateTimeKind.Utc);
            var endDate = DateTime.SpecifyKind(reportDate.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            var accountsQuery = _context.Accounts
                .Include(a => a.Currency)
                .Where(a =>
                    a.AgencyId == agencyId &&
                    (
                        a.AccountType == AccountTypeEnum.Cash ||
                        a.AccountType == AccountTypeEnum.Bank ||
                        a.AccountType == AccountTypeEnum.Wallet
                    ));

            if (accountId.HasValue)
                accountsQuery = accountsQuery.Where(a => a.Id == accountId.Value);

            if (currencyId.HasValue)
                accountsQuery = accountsQuery.Where(a => a.CurrencyId == currencyId.Value);

            var accounts = await accountsQuery.AsNoTracking().ToListAsync();

            var accountIds = accounts.Select(a => a.Id).ToList();

            if (!accountIds.Any())
            {
                return await ResponseWrapper<List<DailyCashReportDto>>.SuccessAsync(
                    new List<DailyCashReportDto>(),
                    "Daily cash report fetched successfully"
                );
            }

            var details = await _context.TransactionDetails
                .Include(d => d.Transaction)
                .Where(d =>
                    accountIds.Contains(d.AccountId) &&
                    d.Transaction != null &&
                    d.Transaction.AgencyId == agencyId &&
                    d.Transaction.CreatedAt <= endDate)
                .Select(d => new
                {
                    d.AccountId,
                    d.EntryType,
                    d.Amount,
                    TransactionDate = d.Transaction.CreatedAt
                })
                .AsNoTracking()
                .ToListAsync();

            var report = accounts.Select(acc =>
            {
                var accDetails = details.Where(d => d.AccountId == acc.Id).ToList();

                var beforeToday = accDetails
                    .Where(d => d.TransactionDate < startDate);

                var today = accDetails
                    .Where(d => d.TransactionDate >= startDate && d.TransactionDate <= endDate);

                var openingDebit = beforeToday
                    .Where(x => x.EntryType == 1)
                    .Sum(x => x.Amount);

                var openingCredit = beforeToday
                    .Where(x => x.EntryType == 2)
                    .Sum(x => x.Amount);

                var openingCash = openingDebit - openingCredit;

                var cashIn = today
                    .Where(x => x.EntryType == 1)
                    .Sum(x => x.Amount);

                var cashOut = today
                    .Where(x => x.EntryType == 2)
                    .Sum(x => x.Amount);

                var closing = openingCash + cashIn - cashOut;

                return new DailyCashReportDto
                {
                    AccountId = acc.Id,
                    AccountName = acc.Name,
                    CurrencyCode = acc.Currency?.Code ?? "N/A",
                    OpeningCash = Math.Round(openingCash, 2),
                    CashIn = Math.Round(cashIn, 2),
                    CashOut = Math.Round(cashOut, 2),
                    SystemClosingCash = Math.Round(closing, 2)
                };
            }).ToList();

            return await ResponseWrapper<List<DailyCashReportDto>>.SuccessAsync(
                report,
                "Daily cash report fetched successfully"
            );
        }









    }
}

