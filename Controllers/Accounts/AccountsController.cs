using Backend.DTOs.Requests.Accounts;
using Backend.Interfaces.Accounts;
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
        public async Task<IActionResult> GetBalancesSummary([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Kani wuxuu keenayaa liiska dhammaan accounts-ka iyo balance-kooda (Dashboard)
            var result = await _accountService.GetAccountBalancesSummaryAsync(page, pageSize);
            return Ok(result);
        }


        [HttpGet("account-statement/{id}")]
        public async Task<IActionResult> GetAccountStatement(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Kani wuxuu keenayaa xogta (Ledger) hal account oo gaar ah
            var result = await _accountService.GetAccountStatementAsync(id, page, pageSize);
            return Ok(result);
        }

        // ==========================================
        // 4. TRANSACTIONS (Dhaqdhaqaaqa Lacagta)
        // ==========================================

        [HttpPost("transaction")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto dto)
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
        public async Task<IActionResult> GetExchanges([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _accountService.GetAllExchangesAsync(page, pageSize);
            return Ok(result);
        }


        [HttpGet("transfers")]
        public async Task<IActionResult> GetTransfers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _accountService.GetAllTransfersAsync(page, pageSize);
            return Ok(result);
        }


        [HttpGet("loans")]
        public async Task<IActionResult> GetLoans([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _accountService.GetAllLoanAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("expenses")]
        public async Task<IActionResult> GetExpenses([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _accountService.GetAllExpensesAsync(page, pageSize);
            return Ok(result);
        }


        [HttpGet("deposits")]
        public async Task<IActionResult> GetDeposits([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _accountService.GetAllDepositsAsync(page, pageSize);
            return Ok(result);
        }


        [HttpGet("withdrawals")]
        public async Task<IActionResult> GetWithdrawals([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _accountService.GetAllWithdrawAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("loanPayment")]
        public async Task<IActionResult> GetLoanPayment([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _accountService.GetAllLoanPaymentAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("revinues")]
        public async Task<IActionResult> GetAllRevinues([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _accountService.GetAllRevinuesAsync(page, pageSize);
            return Ok(result);
        }
    }
}