using api.DTOs;
using api.DTOs.Account;
using api.DTOs.Helpers;
using api.Extensions;
using api.Interfaces;
using api.Models;
using api.Settings;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api.Repositories;

public class AccountRepository : IAccountRepository
{
    #region Vars and Constructor

    private readonly IMongoCollection<AppUser>? _collection;    
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;

    public AccountRepository(IMongoClient client, IMyMongoDbSettings dbSettings,
        UserManager<AppUser> userManager, ITokenService tokenService)
    {
        var database = client.GetDatabase(dbSettings.DatabaseName);
        _collection = database.GetCollection<AppUser>(AppVariablesExtensions.CollectionUsers);
        _userManager = userManager;
        _tokenService = tokenService;
    }

    #endregion

    public async Task<OperationResult<LoggedInDto>> RegisterAsync(RegisterDto userInput, CancellationToken cancellationToken)
    {
        LoggedInDto loggedInDto = new();

        AppUser appUser = Mappers.ConvertRegisterDtoToAppUser(userInput);

        IdentityResult? userCreatedResult = await _userManager.CreateAsync(appUser, userInput.Password);

        if (userCreatedResult.Succeeded)
        {
            IdentityResult? roleResult = await _userManager.AddToRoleAsync(appUser, "member");

            if (!roleResult.Succeeded)
                return new OperationResult<LoggedInDto>(
                    IsSuccess: false,
                    Error: new CustomError(
                        Code: ErrorCode.NetIdentityFailed,
                        Message: "Failed to create role"
                    )
                );

            string? token = await _tokenService.CreateToken(appUser, cancellationToken);

            if (!string.IsNullOrEmpty(token))
            {
                return new OperationResult<LoggedInDto>(
                    IsSuccess: true,
                    Result: Mappers.ConvertAppUserToLoggedInDto(appUser, token),
                    Error: null
                );
            }
        }
        else
        {
            string? errorMessage = userCreatedResult.Errors.FirstOrDefault()?.Description;

            return new OperationResult<LoggedInDto>(
                IsSuccess: false,
                Error: new CustomError(
                    Code: ErrorCode.NetIdentityFailed,
                    Message: errorMessage
                )
            );
        }

        return new OperationResult<LoggedInDto>(
            IsSuccess: false,
            Error: new CustomError(
                Code: ErrorCode.IsAccountCreationFailed,
                Message: "Account creation failed. Try again later."
            )
        );
    }

    public async Task<OperationResult<LoggedInDto>> LoginAsync(LoginDto userInput, CancellationToken cancellationToken)
    {
        LoggedInDto loggedInDto = new();

        AppUser? appUser;

        appUser = await _userManager.FindByEmailAsync(userInput.Email);

        if (appUser is null)
        {
            return new OperationResult<LoggedInDto>(
                false,
                Error: new CustomError(
                    Code: ErrorCode.IsNotFound,
                    Message: "Wrong Credentials"
                )
            );
        }

        bool isPassCorrect = await _userManager.CheckPasswordAsync(appUser, userInput.Password);

        if (!isPassCorrect)
        {
            return new OperationResult<LoggedInDto>(
                false,
                Error: new CustomError(
                    Code: ErrorCode.IsNotFound,
                    Message: "Wrong Credentials"
                )
            );
        }

        string? token = await _tokenService.CreateToken(appUser, cancellationToken);

        if (!string.IsNullOrEmpty(token))
        {
            return new OperationResult<LoggedInDto>(
                true,
                Mappers.ConvertAppUserToLoggedInDto(appUser, token),
                Error: null
            );
        }

        return new OperationResult<LoggedInDto>(
            false,
            Error: new CustomError(
                Code: ErrorCode.IsFailed,
                Message: "Operation failed"
            )
        );
    }

    public async Task<LoggedInDto?> ReloadLoggedInUserAsync(string hashedUserId, string token, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(hashedUserId, cancellationToken);

        if (userId is null)
            return null;

        AppUser appUser = await _collection.Find<AppUser>(appUser => appUser.Id == userId)
            .FirstOrDefaultAsync(cancellationToken);

        return appUser is null
            ? null
            : Mappers.ConvertAppUserToLoggedInDto(appUser, token);
    }

    public async Task<UpdateResult?> UpdateLastActive(string hashedUserId, CancellationToken cancellationToken)
    {
        ObjectId? userId = await _tokenService.GetActualUserIdAsync(hashedUserId, cancellationToken);

        if (userId is null) return null;

        UpdateDefinition<AppUser> newLastActive = Builders<AppUser>.Update
            .Set(appUser =>
                appUser.LastActive, DateTime.UtcNow);

        return await _collection.UpdateOneAsync<AppUser>(user =>
            user.Id == userId, newLastActive, null, cancellationToken);
    }
}