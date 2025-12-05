namespace api.DTOs.Account;

public enum ErrorCode
{
    IsRecaptchaTokenInvalid,
    IsEmailAlreadyConfirmed,
    IsWrongCreds,
    NetIdentityFailed,
    IsEmailNotConfirmed,
    IsRefreshTokenExpired,
    IsAccountCreationFailed,
    IsSessionExpired,
    IsNotFound,
    IsFailed,
    SaveAudioFailed,
    SavePhotoFailed,
    IsAlreadyExist
}