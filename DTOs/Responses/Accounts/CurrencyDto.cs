namespace Backend.DTOs.Responses.Accounts
{
    public class CurrencyDto
    {
        public int Id { get; set; }   // from BaseStaticEntity

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Symbol { get; set; }

        public int DecimalPlaces { get; set; }

        public bool IsBase { get; set; }

        // Optional
        public int AccountsCount { get; set; }


        public string? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
