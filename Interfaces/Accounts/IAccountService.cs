using Backend.DTOs.Requests.Accounts;
using Backend.DTOs.Requests.Customers;
using Backend.DTOs.Responses.Accounts;
using Backend.DTOs.Responses.Customers;
using Backend.Models.Accounts;
using Backend.Wrapper;

namespace Backend.Interfaces.Accounts
{
    public interface IAccountService
    {
        /// <summary>
        /// Currency Methods
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<ResponseWrapper<int>> CreateCurrencyAsync(CreateCurrencyDto dto);

        Task<ResponseWrapper<PagedResponse<CurrencyDto>>> GetAllCurrencyAsync(int page = 1, int pageSize = 10);

        Task<ResponseWrapper<bool>> UpdateCurrencyAsync(int id, UpdateCurrencyDto dto);

        Task<ResponseWrapper<bool>> DeleteCurrencyAsync(int id);




        /// <summary>
        /// Exchange RatesMethods
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<ResponseWrapper<int>> CreateExchangeRateAsync(CreateExchangeRateDto dto);

        Task<ResponseWrapper<PagedResponse<ExchangeRateDto>>> GetAllExchangeRateAsync(int page = 1, int pageSize = 10);

        Task<ResponseWrapper<bool>> UpdateExchangeRateAsync(int id, UpdateExchangeRateDto dto);

        Task<ResponseWrapper<bool>> DeleteExchangeRateAsync(int id);




        /// <summary>
        /// Accounts Methods
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<ResponseWrapper<Guid>> CreateAccountAsync(CreateAccountDto dto);

        Task<ResponseWrapper<PagedResponse<AccountDto>>> GetAllAccountsAsync(int page = 1, int pageSize = 10);

        Task<ResponseWrapper<bool>> UpdateAccountAsync(Guid id, UpdateAccountDto dto);

        Task<ResponseWrapper<bool>> DeleteAccountAsync(Guid id);










        /// <summary>
        /// Transaction Methods
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<ResponseWrapper<Guid>> CreateTransactionAsync(CreateTransactionRequest request);
        Task<ResponseWrapper<int>> CreateExchangeSettingsAsync(CreateExchangeSettingsDto dto);
        Task<ResponseWrapper<bool>> UpdateExchangeSettingsAsync(UpdateExchangeSettingsDto dto);
        Task<List<ExchangeSettingsDto>> GetAllExchangeSettingsAsync();

        Task<ResponseWrapper<PagedResponse<TransactionDto>>> GetAllTransactionsAsync(int page = 1, int pageSize = 10);
        Task<ResponseWrapper<bool>> UpdateTransactionAsync(Guid id, UpdateTransactionDto dto);
        Task<ResponseWrapper<PagedResponse<AccountBalanceSummaryDto>>>
        GetAccountBalancesSummaryAsync(
            int page = 1,
            int pageSize = 10,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            AccountTypeEnum? accountType = null
        );
        Task<ResponseWrapper<PagedResponse<TransactionDetailDto>>>
        GetAccountStatementAsync(
            Guid accountId,
            int page = 1,
            int pageSize = 10,
            byte? entryType = null,        // 1=Debit, 2=Credit
            DateTime? fromDate = null,
            DateTime? toDate = null
        );

        Task<ResponseWrapper<PagedResponse<DailyReportDto>>> GetDailyReportAsync(
             DateTime fromDate,
             DateTime toDate,
             int page = 1,
             int pageSize = 10);

        Task<ResponseWrapper<ProfitLossDto>> GetProfitLossAsync(
         DateTime? fromDate = null,
         DateTime? toDate = null);

       Task<ResponseWrapper<ProfitLossDetailedDto>> GetProfitLossDetailedAsync(
DateTime? fromDate = null,
DateTime? toDate = null,
int page = 1,
int pageSize = 10);



        Task<ResponseWrapper<List<AccountLookupDto>>> GetAccountsLookupAsync();
        Task<ResponseWrapper<List<CurrencyLookupDto>>> GetCurrencyLookupAsync();
        Task<ResponseWrapper<bool>> DeleteTransactionAsync(Guid id);
        Task<ResponseWrapper<List<AccountLookupDto>>> GetAccountEchangeLookupAsync();

        Task<ResponseWrapper<PagedResponse<ExchangeDto>>> GetAllExchangesAsync(
            int page = 1,
            int pageSize = 10,
            DateTime? fromDate = null,
            DateTime? toDate = null
        ); Task<ResponseWrapper<PagedResponse<TransferDto>>> GetAllTransfersAsync(int page = 1, int pageSize = 10);
        Task<ResponseWrapper<PagedResponse<LoanDto>>> GetAllLoanAsync(int page = 1, int pageSize = 10);
        Task<ResponseWrapper<PagedResponse<ExpenseDto>>> GetAllExpensesAsync(int page = 1, int pageSize = 10);
        Task<ResponseWrapper<PagedResponse<DepositDto>>> GetAllDepositsAsync(int page = 1, int pageSize = 10);
        Task<ResponseWrapper<PagedResponse<WithdrawalDto>>> GetAllWithdrawAsync(int page = 1, int pageSize = 10);
        Task<ResponseWrapper<PagedResponse<LoanPaymentDto>>> GetAllLoanPaymentAsync(int page = 1, int pageSize = 10);

        Task<ResponseWrapper<PagedResponse<RevenueDto>>> GetAllRevinuesAsync(int page = 1, int pageSize = 10);


    }
}
