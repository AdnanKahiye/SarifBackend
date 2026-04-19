using AutoMapper;
using Backend.DTOs.Requests;
using Backend.DTOs.Requests.Accounts;
using Backend.DTOs.Requests.Customers;
using Backend.DTOs.Requests.Identity;
using Backend.DTOs.Requests.Setup;
using Backend.DTOs.Requests.Subscriptions;
using Backend.DTOs.Responses;
using Backend.DTOs.Responses.Accounts;
using Backend.DTOs.Responses.Customers;
using Backend.DTOs.Responses.Identity;
using Backend.DTOs.Responses.Setup;
using Backend.DTOs.Responses.Subscriptions;
using Backend.Models;
using Backend.Models.Accounts;
using Backend.Models.Customers;
using Backend.Models.Setup;
using Backend.Models.Subscription;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Mapping
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Map EmployeeDto to Employee
            CreateMap<EmployeeDto, Employee>();

            CreateMap<CreateAgencyDto, Agency>();
            CreateMap<UpdateAgencyDto, Agency>();

            // Map Employee to EmployeeResponse
            CreateMap<Employee, EmployeeResponse>();

            CreateMap<CreateAgencyDto, Agency>();
            CreateMap<UpdateAgencyDto, Agency>();
            // Agency → AgencyDto
            CreateMap<Agency, AgencyDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.FirstName : string.Empty));



            //+========================= Branches mapper ===================================

            CreateMap<CreateBranchDto, Branch>();
            CreateMap<UpdateBranchDto, Branch>();
            CreateMap<Branch, BranchDto>()
            .ForMember(dest => dest.AgencyName,
                opt => opt.MapFrom(src => src.Agency.Name))
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User.FirstName));




            //===================================MENU================================================
            CreateMap<CreateMenuDto, Menu>();
            CreateMap<UpdateMenuDto, Menu>();
            CreateMap<Menu, MenuDto>();
            CreateMap<Menu, MenuSummaryDto>()
        .ForMember(dest => dest.ModuleName,
            opt => opt.MapFrom(src => src.Module != null ? src.Module.Name : string.Empty));






            //==========================Module ==========================================================
            CreateMap<CreateModuleDto, Module>();
            CreateMap<UpdateModuleDto, Module>();
            CreateMap<Module, ModuleDto>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty));



            //==========================Menu Permission==========================================================
            CreateMap<CreateMenuPermissionDto, MenuPermission>();
            CreateMap<UpdateMenuPermissionDto, MenuPermission>();
            CreateMap<MenuPermission, MenuPermissionDto>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
            .ForMember(dest => dest.MenuTitle,
                opt => opt.MapFrom(src => src.Menu != null ? src.Menu.Title : string.Empty))
            .ForMember(dest => dest.ParentId,
                      opt => opt.MapFrom(src => src.Menu.ParentId))
              .ForMember(dest => dest.ParentTitle,
                opt => opt.MapFrom(src =>
                    src.Menu.Parent != null ? src.Menu.Parent.Title : null))
             .ForMember(dest => dest.PermissionName,
                opt => opt.MapFrom(src => src.Permission != null ? src.Permission.Name : string.Empty));


            //==========================Role Permission==========================================================
            CreateMap<CreateRolePermissionDto, RolePermission>();
            CreateMap<RolePermission, RolePermissionDto>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
            .ForMember(dest => dest.RoleName,
                opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty))
             .ForMember(dest => dest.PermissionName,
                opt => opt.MapFrom(src => src.Permission != null ? src.Permission.Name : string.Empty));






            //========================== Permission==========================================================
            CreateMap<CreatePermissionDto, Permission>();
            CreateMap<UpdatePermissionDto, Permission>();
            CreateMap<Permission, PermissionDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty));









            //========================== Plan Model==========================================================
            CreateMap<CreatePlanDto, Plan>();
            CreateMap<UpdatePlanDto, Plan>();
            CreateMap<Plan, PlanDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty));




            //========================== Plan Permission Model==========================================================
            CreateMap<AssignPermissionsToPlanDto, PlanPermission>();
            CreateMap<UpdatePermissionsToPlanDto, PlanPermission>();
            CreateMap<PlanPermission, PlanPermissionDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty));





            //========================== Plan Permission Model==========================================================
            CreateMap<CreateSubscriptionDto, Subscriptions>();
            CreateMap<UpdateSubscriptionDto, Subscriptions>();
            CreateMap<Subscriptions, SubscriptionDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty));


            CreateMap<CreateCustomerDto, Customer>();
            CreateMap<UpdateCustomerDto, Customer>();
            CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(dest => dest.AgencyName,
                    opt => opt.MapFrom(src => src.Agency != null ? src.Agency.Name : string.Empty))
                .ForMember(dest => dest.BranchName,
                    opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                ;





            CreateMap<CreateCurrencyDto, Currency>();
            CreateMap<UpdateCurrencyDto, Currency>();
            CreateMap<Currency, CurrencyDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty));





            CreateMap<CreateExchangeRateDto, ExchangeRate>();
            CreateMap<UpdateExchangeRateDto, ExchangeRate>();
            CreateMap<ExchangeRate, ExchangeRateDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(dest => dest.CurrencyName,
                    opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Name : string.Empty))
                .ForMember(dest => dest.AgencyName,
                    opt => opt.MapFrom(src => src.Agency != null ? src.Agency.Name : string.Empty))
                .ForMember(dest => dest.BranchName,
                    opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty));



            CreateMap<CreateAccountDto, Account>();
            CreateMap<UpdateAccountDto, Account>();
            CreateMap<Account, AccountDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(dest => dest.CurrencyName,
                    opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Name : string.Empty))
                .ForMember(dest => dest.AgencyName,
                    opt => opt.MapFrom(src => src.Agency != null ? src.Agency.Name : string.Empty))
                .ForMember(dest => dest.BranchName,
                    opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty));



            // 1. Transaction Mapping (Hubi in Details ay raacaan)
            CreateMap<CreateTransactionDto, Transaction>()
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details));

            CreateMap<UpdateTransactionDto, Transaction>();

            // 2. Transaction Detail Mapping (Kani waa muhiim si Details-ka loo beddelo)
            CreateMap<CreateTransactionDetailDto, TransactionDetail>();

            // 1. Transaction Detail Mapping (Kani waa muhiim si Details-ka loo soo saaro)
            CreateMap<TransactionDetail, TransactionDetailDto>()
                .ForMember(dest => dest.AccountName,
                    opt => opt.MapFrom(src => src.Account.Name ?? string.Empty))
                .ForMember(dest => dest.CurrencyCode,
                    opt => opt.MapFrom(src => src.Currency.Code ?? string.Empty));

            // 2. Main Transaction Mapping
            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.AgencyName,
                    opt => opt.MapFrom(src => src.Agency.Name ?? string.Empty))
                .ForMember(dest => dest.BranchName,
                    opt => opt.MapFrom(src => src.Branch.Name ?? string.Empty))
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User.UserName ?? string.Empty))
                // Tani waxay hubinaysaa in Details-ka si toos ah loo map-gareeyo
                .ForMember(dest => dest.Details,
                    opt => opt.MapFrom(src => src.Details));







            CreateMap<Account, AccountBalanceSummaryDto>()
            .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Name))
            // Halkan waxaan ka soo qaadaynaa Currency Code-ka maadaama uu yahay Navigation Property
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Code : string.Empty))
            // Field-yada xisaabta ah (Debit, Credit, Balance) badanaa gacanta ayaa lagu buuxiyaa 
            // gudaha Service-ka, markaa halkan waan iska dhaafi karnaa ama Ignore ayaan dhihi karnaa.
            .ForMember(dest => dest.TotalDebit, opt => opt.Ignore())
            .ForMember(dest => dest.TotalCredit, opt => opt.Ignore())
            .ForMember(dest => dest.Balance, opt => opt.Ignore());




            CreateMap<Exchange, ExchangeDto>()
               .ForMember(dest => dest.UserName,
                   opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
               .ForMember(dest => dest.FromAccountName,
                   opt => opt.MapFrom(src => src.FromAccount != null ? src.FromAccount.Name : string.Empty))
               .ForMember(dest => dest.ToAccountName,
                   opt => opt.MapFrom(src => src.ToAccount != null ? src.ToAccount.Name : string.Empty));




            CreateMap<Transfer, TransferDto>()
           .ForMember(dest => dest.UserName,
               opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
           .ForMember(dest => dest.FromAccountName,
               opt => opt.MapFrom(src => src.FromAccount != null ? src.FromAccount.Name : string.Empty))
           .ForMember(dest => dest.ToAccountName,
               opt => opt.MapFrom(src => src.ToAccount != null ? src.ToAccount.Name : string.Empty));



            CreateMap<Loan, LoanDto>()
     .ForMember(dest => dest.UserName,
         opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
     .ForMember(dest => dest.AccountName,
         opt => opt.MapFrom(src => src.Account != null ? src.Account.Name : string.Empty))
     .ForMember(dest => dest.CustomerName,
         opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : string.Empty))
     .ForMember(dest => dest.TransactionDescription,
         opt => opt.MapFrom(src => src.Transaction != null ? src.Transaction.Description : string.Empty));





            CreateMap<Expense, ExpenseDto>()
           .ForMember(dest => dest.UserName,
           opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
           .ForMember(dest => dest.AccountName,
           opt => opt.MapFrom(src => src.Account != null ? src.Account.Name : string.Empty))
           .ForMember(dest => dest.TransactionDescription,
           opt => opt.MapFrom(src => src.Transaction != null ? src.Transaction.Description : string.Empty));


            CreateMap<Deposit, DepositDto>()
     .ForMember(dest => dest.UserName,
     opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
     .ForMember(dest => dest.AccountName,
     opt => opt.MapFrom(src => src.Account != null ? src.Account.Name : string.Empty))
     .ForMember(dest => dest.TransactionDescription,
     opt => opt.MapFrom(src => src.Transaction != null ? src.Transaction.Description : string.Empty))
      .ForMember(dest => dest.CustomerName,
     opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : string.Empty));





            CreateMap<Withdraw, WithdrawalDto>()
.ForMember(dest => dest.UserName,
opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
.ForMember(dest => dest.AccountName,
opt => opt.MapFrom(src => src.Account != null ? src.Account.Name : string.Empty))
.ForMember(dest => dest.TransactionDescription,
opt => opt.MapFrom(src => src.Transaction != null ? src.Transaction.Description : string.Empty))
.ForMember(dest => dest.CustomerName,
opt => opt.MapFrom(src => src.Customer != null ? src.Customer.FullName : string.Empty));

            CreateMap<LoanPayment, LoanPaymentDto>()
           .ForMember(dest => dest.UserName,
           opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
           .ForMember(dest => dest.CashAccountName,
           opt => opt.MapFrom(src => src.CashAccount != null ? src.CashAccount.Name : string.Empty))
           .ForMember(dest => dest.LoanAccountName,
           opt => opt.MapFrom(src => src.LoanAccount != null ? src.LoanAccount.Name : string.Empty))
            .ForMember(dest => dest.LoanNo,
           opt => opt.MapFrom(src => src.Loan != null ? src.Loan.LoanNo : string.Empty))

           .ForMember(dest => dest.TransactionDescription,
           opt => opt.MapFrom(src => src.Transaction != null ? src.Transaction.Description : string.Empty));




            CreateMap<Revenue, RevenueDto>()
    .ForMember(dest => dest.RevenueAccountName,
        opt => opt.MapFrom(src => src.RevenueAccount != null ? src.RevenueAccount.Name : string.Empty))
    .ForMember(dest => dest.CashAccountName,
        opt => opt.MapFrom(src => src.CashAccount != null ? src.CashAccount.Name : string.Empty))
    .ForMember(dest => dest.ReferenceNo,
        opt => opt.MapFrom(src => src.Transaction != null ? src.Transaction.ReferenceNo : string.Empty))
    .ForMember(dest => dest.UserName,
        opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
    .ForMember(dest => dest.SourceType,
        opt => opt.MapFrom(src => src.SourceType.ToString())); // Enum-ka u beddel string (Tusaale: "Exchange")


        }




    }
    }
