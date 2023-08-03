namespace Nrk.FluentCore.Classes.Enums;

public enum MicrosoftAuthenticateExceptionType
{
    Unknown = 0,
    NetworkConnectionError = 1,
    XboxLiveError = 3,
    GameOwnershipError = 4,
}

public enum MicrosoftAuthenticateStep
{
    Get_Authorization_Token = 1,
    Authenticate_with_XboxLive = 2,
    Obtain_XSTS_token_for_Minecraft = 3,
    Authenticate_with_Minecraft = 4,
    Checking_Game_Ownership = 5,
    Get_the_profile = 6,
    Finished = 7
}