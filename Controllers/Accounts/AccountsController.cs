using Backend.DTOs.Requests.Accounts;
using Backend.Interfaces.Accounts;
using Backend.Models.Accounts;
using Backend.Utiliy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.Accounts
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // ==========================================
        // 1. CURRENCY (Lacagaha: USD, SOS, etc.)
        // ==========================================

        [HttpPost("currency")]
        public async Task<IActionResult> CreateCurrency([FromBody] CreateCurrencyDto dto)
        {
            var result = await _accountService.CreateCurrencyAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("currency")]
        public async Task<IActionResult> GetCurrencies([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _accountService.GetAllCurrencyAsync(page, pageSize);
            return Ok(result);
        }

        [HttpPut("currency/{id}")]
        public async Task<IActionResult> UpdateCurrency(int id, [FromBody] UpdateCurrencyDto dto)
        {
            var result = await _accountService.UpdateCurrencyAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("currency/{id}")]
        public async Task<IActionResult> DeleteCurrency(int id)
        {
            var result = await _accountService.DeleteCurrencyAsync(id);
            return Ok(result);
        }

        // ==========================================
        // 2. EXCHANGE RATES (Sarifka)
        // ==========================================

        [HttpPost("exchange-rate")]
        public async Task<IActionResult> CreateExchangeRate([FromBody] CreateExchangeRateDto dto)
        {
            var result = await _accountService.CreateExchangeRateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("exchange-rate")]
        public async Task<IActionResult> GetExchangeRates([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _accountService.GetAllExchangeRateAsync(page, pageSize);
            return Ok(result);
        }

        [HttpPut("exchange-rate/{id}")]
        public async Task<IActionResult> UpdateExchangeRate(int id, [FromBody] UpdateExchangeRateDto dto)
        {
            var result = await _accountService.UpdateExchangeRateAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("exchange-rate/{id}")]
        public async Task<IActionResult> DeleteExchangeRate(int id)
        {
            var result = await _accountService.DeleteExchangeRateAsync(id);
            return Ok(result);
        }

        // ==========================================
        // 3. ACCOUNTS (Akoonada Macmiilka/Shirkadda)
        // ==========================================

        [HttpPost("account")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
        {
            var result = await _accountService.CreateAccountAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("account")]
        public async Task<IActionResult> GetAccounts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _accountService.GetAllAccountsAsync(page, pageSize);
            return Ok(result);
        }

        [HttpPut("account/{id}")]
        public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountDto dto)
        {
            var result = await _accountService.UpdateAccountAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("account/{id}")]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            var result = await _accountService.DeleteAccountAsync(id);
            return Ok(result);
        }

        [HttpGet("balances-summary")]
        public async Task<IActionResult> GetBalancesSummary(
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 10,

          [FromQuery] DateTime? fromDate = null,
          [FromQuery] DateTime? toDate = null,

          [FromQuery] AccountTypeEnum? accountType = null // ✅ THIS
      )
        {
            var result = await _accountService.GetAccountBalancesSummaryAsync(
                page, pageSize, fromDate, toDate, accountType);

            return Ok(result);
        }


        [HttpGet("account-statement/{id}")]
        public async Task<IActionResult> GetAccountStatement(
       Guid id,
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 10,
       [FromQuery] byte? entryType = null,
       [FromQuery] DateTime? fromDate = null,
       [FromQuery] DateTime? toDate = null)
        {
            var result = await _accountService.GetAccountStatementAsync(
                id, page, pageSize, entryType, fromDate, toDate);

            return Ok(result);
        }

        // ==========================================
        // 4. TRANSACTIONS (Dhaqdhaqaaqa Lacagta)
        // ==========================================

        [HttpPost("transaction")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest dto)
        {
            var result = await _accountService.CreateTransactionAsync(dto);
            // StatusCode-ka wuxuu ka imaanayaa ResponseWrapper (tusaale: 201 Created)
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("transaction")]
        public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _accountService.GetAllTransactionsAsync(page, pageSize);
            return Ok(result);
        }

        [HttpPut("transaction/{id}")]
        public async Task<IActionResult> UpdateTransaction(Guid id, [FromBody] UpdateTransactionDto dto)
        {
            var result = await _accountService.UpdateTransactionAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("transaction/{id}")]
        public async Task<IActionResult> DeleteTransaction(Guid id)
        {
            var result = await _accountService.DeleteTransactionAsync(id);
            return Ok(result);
        }



        [HttpGet("exchanges")]
        public async Task<IActionResult> GetExchanges(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
        {
            var result = await _accountService.GetAllExchangesAsync(
                page,
                pageSize,
                fromDate,
                toDate
            );

            return Ok(result);
        }


        [HttpGet("transfers")]
        public async Task<IActionResult> GetTransfers(
         [FromQuery] int page = 1,
         [FromQuery] int pageSize = 10,
         [FromQuery] DateTime? fromDate = null,
         [FromQuery] DateTime? toDate = null)
        {
            var result = await _accountService.GetAllTransfersAsync(page, pageSize, fromDate, toDate);
            return Ok(result);
        }


        [HttpGet("loans")]
        public async Task<IActionResult> GetLoans(
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 10,
     [FromQuery] DateTime? fromDate = null,
     [FromQuery] DateTime? toDate = null)
        {
            var result = await _accountService.GetAllLoanAsync(page, pageSize, fromDate, toDate);
            return Ok(result);
        }

        [HttpGet("expenses")]
        public async Task<IActionResult> GetExpenses(
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 10,
     [FromQuery] DateTime? fromDate = null,
     [FromQuery] DateTime? toDate = null)
        {
            var result = await _accountService.GetAllExpensesAsync(page, pageSize, fromDate, toDate);
            return Ok(result);
        }

        [HttpGet("deposits")]
        public async Task<IActionResult> GetDeposits(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _accountService.GetAllDepositsAsync(page, pageSize, fromDate, toDate);
            return Ok(result);
        }


        [HttpGet("withdrawals")]
        public async Task<IActionResult> GetWithdrawals(
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 10,
       [FromQuery] DateTime? fromDate = null,
       [FromQuery] DateTime? toDate = null)
        {
            var result = await _accountService.GetAllWithdrawAsync(page, pageSize, fromDate, toDate);
            return Ok(result);
        }

        [HttpGet("loanPayment")]
        public async Task<IActionResult> GetLoanPayment(
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 10,
       [FromQuery] DateTime? fromDate = null,
       [FromQuery] DateTime? toDate = null)
        {
            var result = await _accountService.GetAllLoanPaymentAsync(page, pageSize, fromDate, toDate);
            return Ok(result);
        }

        [HttpGet("revinues")]
        public async Task<IActionResult> GetAllRevinues(
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 10,
     [FromQuery] DateTime? fromDate = null,
     [FromQuery] DateTime? toDate = null)
        {
            var result = await _accountService.GetAllRevinuesAsync(page, pageSize, fromDate, toDate);
            return Ok(result);
        }



        [HttpGet("profit-loss")]
        public async Task<IActionResult> GetProfitLoss(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate)
        {
            var result = await _accountService.GetProfitLossAsync(fromDate, toDate);
            return Ok(result);
        }

        [HttpGet("daily-report")]
        public async Task<IActionResult> GetDailyReport(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            var result = await _accountService.GetDailyReportAsync(fromDate, toDate);
            return Ok(result);
        }


        [HttpGet("profit-loss-detailed")]
        public async Task<IActionResult> GetProfitLossDetailed(
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            var result = await _accountService.GetProfitLossDetailedAsync(
                fromDate, toDate, page, pageSize);

            return Ok(result);
        }


        [HttpGet("accounts-lookup")]
        public async Task<IActionResult> GetAccountsLookup()
        {
            var result = await _accountService.GetAccountsLookupAsync();
            return Ok(result);
        }


        [HttpGet("currency-lookup")]
        public async Task<IActionResult> GetCurrencyLookup()
        {
            var result = await _accountService.GetCurrencyLookupAsync();
            return Ok(result);
        }

        [HttpGet("account-exchange-lookup")]
        public async Task<IActionResult> GetAccountExchangeLookup()
        {
            var result = await _accountService.GetAccountEchangeLookupAsync();
            return Ok(result);
        }
        [HttpGet("account-revenue-lookup")]
        public async Task<IActionResult> GetAccountRevenuesLookup()
        {
            var result = await _accountService.GetAccountRevenuesLookupAsync();
            return Ok(result);
        }

        [HttpGet("account-expenses-lookup")]
        public async Task<IActionResult> GetAccountExpensesLookup()
        {
            var result = await _accountService.GetAccountExpenseLookupAsync();
            return Ok(result);
        }



        // ==============================
        // ✅ CREATE
        // ==============================
        [HttpPost("exchange-settings")]
        public async Task<IActionResult> Create([FromBody] CreateExchangeSettingsDto dto)
        {
            var result = await _accountService.CreateExchangeSettingsAsync(dto);
            return Ok(result);
        }

        // ==============================
        // ✅ UPDATE
        // ==============================
        [HttpPut("exchange-settings")]
        public async Task<IActionResult> Update([FromBody] UpdateExchangeSettingsDto dto)
        {
            var result = await _accountService.UpdateExchangeSettingsAsync(dto);
            return Ok(result);
        }

        // ==============================
        // ✅ GET ALL
        // ==============================
        [HttpGet("exchange-settings")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _accountService.GetAllExchangeSettingsAsync();
            return Ok(result);
        }
    }
}