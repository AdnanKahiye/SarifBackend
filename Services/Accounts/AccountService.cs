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
                entity.AgencyId =_currentUser.AgencyId;
                entity.BranchId =_currentUser.BranchId;
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
                entity.AgencyId= _currentUser.AgencyId;
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






        // ================================
        // CREATE TRANSACTION
        // ================================
        public async Task<ResponseWrapper<Guid>> CreateTransactionAsync(CreateTransactionDto dto)
        {
            // 0. Hubi Aqoonsiga User-ka
            if (string.IsNullOrEmpty(_currentUser.UserId))
                return await ResponseWrapper<Guid>.FailureAsync("Unauthorized", "User not authenticated", 401);

            // 1. Hubi Currencies-ka iyo Nooca Transaction-ka
            var distinctCurrencies = dto.Details.Select(d => d.CurrencyId).Distinct().ToList();
            bool isMultiCurrency = distinctCurrencies.Count > 1;

            // 2. Business Logic Validation (Xeerarka Noocyada)
            switch ((TransactionTypeEnum)dto.TransactionType)
            {
                case TransactionTypeEnum.Deposit:
                case TransactionTypeEnum.Withdraw:
                case TransactionTypeEnum.Transfer:
                    if (isMultiCurrency)
                        return await ResponseWrapper<Guid>.FailureAsync("Validation Error", "Noocan transaction-ka ah ma oggola laba lacag oo kala duwan.");
                    break;

                case TransactionTypeEnum.Exchange:
                    if (!isMultiCurrency)
                        return await ResponseWrapper<Guid>.FailureAsync("Validation Error", "Sarifka (Exchange) waa inuu ka koobnaadaa ugu yaraan laba lacag oo kala duwan.");
                    break;

                default:
                    // Noocyada kale sida Loan/Expense waxay u baahan karaan validation u gaar ah mustaqbalka
                    break;
            }

            // 3. HEL EXCHANGE RATES (Kaliya haddii ay lagama maarmaan tahay)
            Dictionary<int, decimal> rates = new();
            if (isMultiCurrency)
            {
                            rates = await _context.ExchangeRates
                .Where(r => distinctCurrencies.Contains(r.CurrencyId))
                .GroupBy(r => r.CurrencyId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.CreatedAt).First().Rate
    );
            }

            // 4. DOUBLE-ENTRY VALIDATION
            decimal totalDebitBase = 0;
            decimal totalCreditBase = 0;

            foreach (var d in dto.Details)
            {
                decimal amountInBase;

                if (isMultiCurrency)
                {
                    if (!rates.TryGetValue(d.CurrencyId, out decimal rate) || rate <= 0)
                        return await ResponseWrapper<Guid>.FailureAsync("Rate Error", $"Exchange rate-ka lama helin Currency ID: {d.CurrencyId}");

                    amountInBase = d.Amount / rate;
                }
                else
                {
                    amountInBase = d.Amount;
                }

                if (d.EntryType == 1) totalDebitBase += amountInBase; // 1 = Debit
                else if (d.EntryType == 2) totalCreditBase += amountInBase; // 2 = Credit
            }

            // 5. Isbarbardhig (Balance Check)
            decimal diff = Math.Abs(totalDebitBase - totalCreditBase);
            decimal allowedTolerance = isMultiCurrency ? 0.01m : 0m;

            if (diff > allowedTolerance)
            {
                return await ResponseWrapper<Guid>.FailureAsync("Validation Error",
                    $"Transaction-ku ma dheelli tirna (Out of balance). Difference: {diff:N2}");
            }

            // 6. DIIWAANGELINTA (DATABASE WRITE)
            return await ExecuteWriteAsync(async () =>
            {
                using var dbTransaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var transaction = _mapper.Map<Transaction>(dto);
                    transaction.ReferenceNo = await GenerateReferenceNoAsync();
                    transaction.Id = Guid.NewGuid();
                    transaction.UserId = _currentUser.UserId;
                    transaction.AgencyId = _currentUser.AgencyId;
                    transaction.BranchId = _currentUser.BranchId;
                    transaction.CreatedAt = DateTime.UtcNow;
                    transaction.TotalAmount = totalDebitBase; // USD Base Value

                    foreach (var detail in transaction.Details)
                    {
                        detail.Id = Guid.NewGuid();
                        detail.TransactionId = transaction.Id;
                        detail.UserId = _currentUser.UserId;
                        detail.TransactionType = transaction.TransactionType;
                        detail.CreatedAt = DateTime.UtcNow;
                        // detail.AgencyId = _currentUser.AgencyId; // Haddii aad ku haysato Detail Table-ka
                    }

                    _context.Transactions.Add(transaction);




                    // ============================================================
                    // TUSAALE: KAYDINTA METADATA-DA (EXCHANGE)
                    // ============================================================
                    if ((TransactionTypeEnum)dto.TransactionType == TransactionTypeEnum.Exchange)
                    {
                        var exchange = new Exchange
                        {
                            Id = Guid.NewGuid(),
                            TransactionId = transaction.Id,
                            Rate = dto.ExchangeRate ?? 0,
                            Fee = dto.Fee ?? 0,
                            Profit = dto.Profit ?? 0,

                            // Helitaanka Account-yada (From/To)
                            // EntryType 1 = Debit (Gashay), 2 = Credit (Baxday)
                            FromAccountId = dto.Details.FirstOrDefault(d => d.EntryType == 2)?.AccountId ?? Guid.Empty,
                            ToAccountId = dto.Details.FirstOrDefault(d => d.EntryType == 1)?.AccountId ?? Guid.Empty,

                            FromAmount = dto.Details.FirstOrDefault(d => d.EntryType == 2)?.Amount ?? 0,
                            ToAmount = dto.Details.FirstOrDefault(d => d.EntryType == 1)?.Amount ?? 0,

                            // Metadata-da kale
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            BranchId =_currentUser.BranchId,
                            AgencyId =_currentUser.AgencyId
                            
                        };

                        decimal totalRevenue = (dto.Profit ?? 0) + (dto.Fee ?? 0);

                        if (totalRevenue > 0)

                        {

                            var toDetail = dto.Details.FirstOrDefault(d => d.EntryType == 1);

                            if (toDetail == null)
                                throw new Exception("Destination account lama helin");

                            var currencyId = toDetail.CurrencyId;

                            // 🔥 Hel Revenue Account
                            var revenueAccount = await _context.Accounts
                                .FirstOrDefaultAsync(a =>
                                    a.AccountType == AccountTypeEnum.Revenue &&
                                    a.CurrencyId == currencyId &&
                                    a.Name.Contains("Exchange Profit"));

                            // ❗ HUBI HALKAN KA HOR ISTICMAAL
                            if (revenueAccount == null)
                                throw new Exception($"Exchange Profit account lama helin currencyId: {currencyId}");


                            // 👉 Debit (Cash)
                            // 👉 Debit (Cash)
                            _context.TransactionDetails.Add(new TransactionDetail
                            {
                                Id = Guid.NewGuid(),
                                TransactionId = transaction.Id,
                                AccountId = toDetail.AccountId,
                                CurrencyId = toDetail.CurrencyId, // ✅ ADD THIS
                                Amount = totalRevenue,
                                EntryType = 1,
                                UserId = _currentUser.UserId,
                                TransactionType = transaction.TransactionType,
                                CreatedAt = DateTime.UtcNow
                            });

                            // 👉 Credit (Revenue)
                            _context.TransactionDetails.Add(new TransactionDetail
                            {
                                Id = Guid.NewGuid(),
                                TransactionId = transaction.Id,
                                AccountId = revenueAccount.Id,
                                CurrencyId = currencyId, // ✅ ADD THIS
                                Amount = totalRevenue,
                                EntryType = 2,
                                UserId = _currentUser.UserId,
                                TransactionType = transaction.TransactionType,
                                CreatedAt = DateTime.UtcNow
                            });

                            // 👉 Save Revenue table
                            var autoRevenue = new Revenue
                            {
                                Id = Guid.NewGuid(),
                                TransactionId = transaction.Id,
                                Title = $"Exchange Profit/Fee - Ref: {transaction.ReferenceNo}",
                                Amount = totalRevenue,
                                RevenueAccountId = revenueAccount.Id,
                                CashAccountId = toDetail.AccountId,
                                SourceType = RevenueSourceEnum.Exchange,
                                AgencyId = _currentUser.AgencyId,
                                CreatedAt = DateTime.UtcNow
                            };

                            _context.Revenues.Add(autoRevenue);
                        }



                        _context.Exchanges.Add(exchange);
                    }

                    // TUSAALE KALE: TRANSFER
                    if ((TransactionTypeEnum)dto.TransactionType == TransactionTypeEnum.Transfer)
                    {
                        if (dto.TransferDetails == null)
                        {
                            // Hubi in xogta Transfer-ka la soo diray
                            throw new Exception("Xogta Transfer-ka waa lagama maarmaan.");
                        }

                        var transfer = new Transfer
                        {
                            Id = Guid.NewGuid(),
                            TransactionId = transaction.Id, // Ku xir Transaction-ka weyn

                            // Xogta laga soo minguuriyay TransferDetails DTO
                            SenderName = dto.TransferDetails.SenderName,
                            ReceiverName = dto.TransferDetails.ReceiverName,
                            FromAccountId = dto.TransferDetails.FromAccountId,
                            ToAccountId = dto.TransferDetails.ToAccountId,
                            Amount = dto.TransferDetails.Amount,

                            // Laamaha (Branches)
                            FromBranchId = _currentUser.BranchId,
                            ToBranchId = _currentUser.BranchId,
                            AgencyId =_currentUser.AgencyId,

                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.Transfers.Add(transfer);
                    }


                    // TUSAALE: LOAN (DAYN)
                    if ((TransactionTypeEnum)dto.TransactionType == TransactionTypeEnum.Loan)
                    {
                        if (dto.LoanDetails == null)
                            throw new Exception("Xogta Loan-ka waa lagama maarmaan.");

                        var loan = new Loan
                        {
                            Id = Guid.NewGuid(),
                            TransactionId = transaction.Id, // Ku xir xawaaladda/transaction-ka

                            LoanNo = dto.LoanDetails.LoanNo ?? $"LN-{DateTime.Now.Ticks}",
                            PrincipalAmount = dto.LoanDetails.PrincipalAmount,
                            InterestRate = dto.LoanDetails.InterestRate ?? 0,

                            StartDate = dto.LoanDetails.StartDate ?? DateTime.UtcNow,
                            DueDate = dto.LoanDetails.DueDate,
                            PaidAmount = 0, // Marka la bilaabayo waa 0

                            AccountId = dto.LoanDetails.AccountId,
                            CustomerId = dto.LoanDetails.CustomerId,

                            AgencyId = _currentUser.AgencyId,
                            BranchId = _currentUser.BranchId,

                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.Loans.Add(loan);
                    }

                    // TUSAALE: EXPENSE (KHARASH)
                    if ((TransactionTypeEnum)dto.TransactionType == TransactionTypeEnum.Expense)
                    {
                        if (dto.ExpenseDetails == null)
                            throw new Exception("Xogta Kharashka (Expense) waa lagama maarmaan.");

                        var expense = new Expense
                        {
                            Id = Guid.NewGuid(),
                            TransactionId = transaction.Id, // Link-ga muhiimka ah

                            Title = dto.ExpenseDetails.Title,
                            Description = dto.ExpenseDetails.Description,
                            Amount = dto.ExpenseDetails.Amount,

                            AccountId = dto.ExpenseDetails.AccountId, // Expense Account-ka (tusaale: Kirada)
                            ExpenseDate = dto.ExpenseDetails.ExpenseDate ?? DateTime.UtcNow,

                            AgencyId = _currentUser.AgencyId,
                            BranchId = _currentUser.BranchId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.Expenses.Add(expense);
                    }


                    // TUSAALE: DEPOSIT (LACAG DHIGASHO)
                    if ((TransactionTypeEnum)dto.TransactionType == TransactionTypeEnum.Deposit)
                    {
                        if (dto.DepositDetails == null)
                            throw new Exception("Xogta Deposit-ka waa lagama maarmaan.");

                        var deposit = new Deposit
                        {
                            Id = Guid.NewGuid(),
                            TransactionId = transaction.Id, // Link-ga Transaction-ka weyn

                            DepositNo = dto.DepositDetails.DepositNo ?? $"DEP-{DateTime.Now.Ticks}",
                            AccountId = dto.DepositDetails.AccountId,
                            CustomerId = dto.DepositDetails.CustomerId,
                            CurrencyId = dto.DepositDetails.CurrencyId,

                            AgencyId = _currentUser.AgencyId,
                            BranchId = _currentUser.BranchId,

                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.Deposits.Add(deposit);
                    }



                    if ((TransactionTypeEnum)dto.TransactionType == TransactionTypeEnum.Withdraw)
                    {
                        if (dto.WithdrawDetails == null)
                            throw new Exception("Xogta Withdraw-ga waa lagama maarmaan.");

                        var withdraw = new Withdraw
                        {
                            Id = Guid.NewGuid(),
                            TransactionId = transaction.Id,
                            WithdrawNo = dto.WithdrawDetails.WithdrawNo ?? $"WD-{DateTime.Now.Ticks}",

                            AccountId = dto.WithdrawDetails.AccountId,
                            CustomerId = dto.WithdrawDetails.CustomerId,
                            CurrencyId = dto.WithdrawDetails.CurrencyId,

                            AgencyId = _currentUser.AgencyId,
                            BranchId = _currentUser.BranchId,

                            WithdrawnAt = DateTime.UtcNow,
                            // Haddii aad metadata-da dheeraadka ah rabto:
                            ReceiverName = dto.WithdrawDetails.ReceiverName,
                            ReceiverIdCard = dto.WithdrawDetails.ReceiverIdCard
                        };

                        _context.Withdraws.Add(withdraw);
                    }


                    // REPAYMENT LOGIC
                    if ((TransactionTypeEnum)dto.TransactionType == TransactionTypeEnum.Repayment)
                    {
                        if (dto.RepaymentDetails == null)
                            throw new Exception("Xogta Repayment-ka waa lagama maarmaan.");

                        var loan = await _context.Loans
                            .FirstOrDefaultAsync(l => l.Id == dto.RepaymentDetails.LoanId);

                        if (loan == null)
                            throw new Exception("Deyntan lama helin!");

                        var remainingBalance = loan.PrincipalAmount - loan.PaidAmount;
                        if (dto.RepaymentDetails.Amount > remainingBalance)
                        {
                            throw new Exception($"Lacagta la bixinayo ({dto.RepaymentDetails.Amount}) way ka badantahay baaqiga haray ({remainingBalance})");
                        }

                        // 1. Update garee lacagta ilaa hadda la bixiyey
                        loan.PaidAmount += dto.RepaymentDetails.Amount;
                        loan.UpdatedAt = DateTime.UtcNow;

                        // --- QAYBTA CUSUB: UPDATE STATUS ---
                        // Haddii lacagta la bixiyey ay la mid tahay ama ka badato deyntii asalka ahayd
                        if (loan.PaidAmount >= loan.PrincipalAmount)
                        {
                            // Halkan geli Status-ka aad u isticmaasho "Paid" ama "Closed"
                            // Tusaale: 1 = Active, 2 = Paid/Closed
                            loan.Status = LoanStatusEnum.Closed;
                        }
                        // ------------------------------------

                        var payment = new LoanPayment
                        {
                            Id = Guid.NewGuid(),
                            TransactionId = transaction.Id,
                            LoanId = loan.Id,
                            Amount = dto.RepaymentDetails.Amount,
                            PaymentDate = DateTime.UtcNow,
                            Note = dto.RepaymentDetails.Note,
                            CashAccountId = dto.RepaymentDetails.CashAccountId,
                            LoanAccountId = dto.RepaymentDetails.LoanAccountId,
                            AgencyId = _currentUser.AgencyId,
                            BranchId = _currentUser.BranchId,
                            UserId = _currentUser.UserId
                        };

                        _context.LoanPayments.Add(payment);
                    }



                    // ============================================================
                    // REVENUE LOGIC (DIRECT) - Haddii uu yahay dakhli toos ah
                    // ============================================================
                    if ((TransactionTypeEnum)dto.TransactionType == TransactionTypeEnum.Revenue)
                    {
                        if (dto.RevenueDetails == null)
                            throw new Exception("Xogta Dakhliga (Revenue) waa lagama maarmaan.");

                        var revenue = new Revenue
                        {
                            Id = Guid.NewGuid(),
                            TransactionId = transaction.Id,
                            Title = dto.RevenueDetails.Title,
                            Amount = dto.RevenueDetails.Amount,
                            RevenueAccountId = dto.RevenueDetails.RevenueAccountId,
                            CashAccountId = dto.RevenueDetails.CashAccountId,
                            SourceType = RevenueSourceEnum.Direct,
                            AgencyId = _currentUser.AgencyId,
                            BranchId = _currentUser.BranchId,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Revenues.Add(revenue);
                    }

              



                    await _context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    RemoveByPrefix(TransactionCacheKey);
                    return transaction.Id;
                }
                catch (Exception)
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            }, "Transaction-ka waa la diiwaangeliyay", "Cillad ayaa dhacday xilliga diiwaangelinta");
        }
        private async Task<string> GenerateReferenceNoAsync()
        {
            // Raadi transaction-kii ugu dambeeyay ee maanta dhacay
            var today = DateTime.UtcNow.Date;
            var count = await _context.Transactions
                .CountAsync(t => t.CreatedAt >= today);

            // Qaabka: TRX-20260415-001 (Koodh + Taariikh + Taxane)
            string datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            string sequencePart = (count + 1).ToString("D3"); // 001, 002...

            return $"TRX-{datePart}-{sequencePart}";
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
                // ✅ FIXED CACHE KEY
                cacheKey: $"{AccountCacheKey}_Summary_{_currentUser.UserId}_P{page}_PS{pageSize}_T{accountType}_F{fromDate?.ToString("yyyy-MM-dd")}_TO{toDate?.ToString("yyyy-MM-dd")}",

                action: async () =>
                {
                    // 1. Accounts Query
                    var query = _context.Accounts
                        .Include(x => x.Currency)
                        .AsNoTracking();

                    if (!isAdmin)
                    {
                        query = query.Where(x => x.AgencyId == agencyId);
                    }

                    if (accountType.HasValue)
                    {
                        query = query.Where(x => x.AccountType == accountType.Value);
                    }

                    var totalRecords = await query.CountAsync();

                    var pagedAccounts = await query
                        .OrderBy(x => x.Name)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    var accountIds = pagedAccounts.Select(a => a.Id).ToList();

                    // 2. Transaction Query (DATE FIXED)
                    var transactionQuery = _context.TransactionDetails
                        .Where(td => accountIds.Contains(td.AccountId));

                    if (fromDate.HasValue)
                    {
                        transactionQuery = transactionQuery.Where(td => td.CreatedAt >= fromDate.Value.Date);
                    }

                    if (toDate.HasValue)
                    {
                        var endDate = toDate.Value.Date.AddDays(1).AddTicks(-1); // ✅ IMPORTANT FIX
                        transactionQuery = transactionQuery.Where(td => td.CreatedAt <= endDate);
                    }

                    // 3. Group Balances
                    var balances = await transactionQuery
                        .GroupBy(td => td.AccountId)
                        .Select(g => new
                        {
                            AccountId = g.Key,
                            Debit = g.Where(x => x.EntryType == 1).Sum(x => x.Amount),
                            Credit = g.Where(x => x.EntryType == 2).Sum(x => x.Amount)
                        })
                        .ToDictionaryAsync(x => x.AccountId);

                    // 4. Mapping
                    var mappedResult = pagedAccounts.Select(acc =>
                    {
                        balances.TryGetValue(acc.Id, out var bal);

                        var debit = bal?.Debit ?? 0;
                        var credit = bal?.Credit ?? 0;

                        var balance = acc.Nature switch
                        {
                            AccountNatureEnum.Asset => debit - credit,
                            AccountNatureEnum.Expense => debit - credit,

                            AccountNatureEnum.Liability => credit - debit,
                            AccountNatureEnum.Equity => credit - debit,
                            AccountNatureEnum.Revenue => credit - debit,

                            _ => 0
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



        public async Task<ResponseWrapper<PagedResponse<TransferDto>>> GetAllTransfersAsync(int page = 1, int pageSize = 10)
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
                    var query = _context.Transfers
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
                        .ProjectTo<TransferDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<TransferDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} Transfer fetched",
                cacheMessage: "Transfer loaded from cache",
                errorMessage: "Error fetching transfer"
            );
        }



        public async Task<ResponseWrapper<PagedResponse<LoanDto>>> GetAllLoanAsync(int page = 1, int pageSize = 10)
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
                    var query = _context.Loans
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
                        .ProjectTo<LoanDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<LoanDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} Loan fetched",
                cacheMessage: "Loan loaded from cache",
                errorMessage: "Error fetching loan"
            );
        }


        public async Task<ResponseWrapper<PagedResponse<ExpenseDto>>> GetAllExpensesAsync(int page = 1, int pageSize = 10)
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
                    var query = _context.Expenses
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
                        .ProjectTo<ExpenseDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<ExpenseDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} Expepses fetched",
                cacheMessage: "Expepses loaded from cache",
                errorMessage: "Error fetching expepses"
            );
        }



        public async Task<ResponseWrapper<PagedResponse<DepositDto>>> GetAllDepositsAsync(int page = 1, int pageSize = 10)

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
                    var query = _context.Deposits
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
                        .ProjectTo<DepositDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<DepositDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} Deposit fetched",
                cacheMessage: "Deposit loaded from cache",
                errorMessage: "Error fetching deposit"
            );
        }




        public async Task<ResponseWrapper<PagedResponse<WithdrawalDto>>> GetAllWithdrawAsync(int page = 1, int pageSize = 10)

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
                    var query = _context.Withdraws
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
                        .ProjectTo<WithdrawalDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<WithdrawalDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} Withdrawal fetched",
                cacheMessage: "Withdrawal loaded from cache",
                errorMessage: "Error fetching Withdrawal"
            );
        }




        public async Task<ResponseWrapper<PagedResponse<LoanPaymentDto>>> GetAllLoanPaymentAsync(int page = 1, int pageSize = 10)

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
                    var query = _context.LoanPayments
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
                        .ProjectTo<LoanPaymentDto>(_mapper.ConfigurationProvider)
                        .ToListAsync();

                    return new PagedResponse<LoanPaymentDto>(mapped, page, pageSize, totalRecords);
                },
                successMessageFactory: result => $"{result.Data.Count} LoanPayment fetched",
                cacheMessage: "LoanPayment loaded from cache",
                errorMessage: "Error fetching loanPayment"
            );
        }



        public async Task<ResponseWrapper<PagedResponse<RevenueDto>>> GetAllRevinuesAsync(int page = 1, int pageSize = 10)

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
                    var query = _context.Revenues
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



        public async Task<ResponseWrapper<List<AccountLookupDto>>> GetAccountEchangeLookupAsync()
        {
            var agencyId = _currentUser.AgencyId;
            var isAdmin = _currentUser.IsInRole("Administrator");

            return await ExecuteWithCacheAsync(
                cacheKey: $"{AccountCacheKey}_Lookup_Exchange_{_currentUser.UserId}",
                action: async () =>
                {
                    var query = _context.Accounts.AsNoTracking();

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
                            Name = x.Name
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




    }
}

