﻿using Jose;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ParseBackend.Exceptions;
using ParseBackend.Exceptions.Common;
using ParseBackend.Models.Database;
using ParseBackend.Models.Response;
using ParseBackend.Services;
using ParseBackend.Utils;
using static ParseBackend.Global;

namespace ParseBackend.Controllers
{
    [ApiController]
    [Route("account/api/oauth")]
    public class OAuthController : Controller
    {
        private readonly IMongoService _mongoService;

        public OAuthController(IMongoService mongoService)
        {
            _mongoService = mongoService;
        }

        [HttpPost]
        [Route("token")]
        public async Task<ActionResult<OAuthResponse>> LoginToken()
        {
            var accountData = new UserData();
            var form = Request.Form;

            var clientId = Request.Headers.Authorization.ToString().Split(' ')[1].DecodeBase64().Split(':');

            if (clientId.Length < 2)
                throw new BaseException("Invalid.Request", "Invalid ID", 400, null);

            switch (form["grant_type"].ToString())
            {
                case "password":
                    {
                        var username = form["username"].ToString();
                        var password = form["password"].ToString();
                        if (username is null || password is null)
                            throw new InvalidCredentialsException();

                        accountData = await _mongoService.LoginAccount(username, password);

                        break;
                    };
                case "client_credentials":
                    {
                        break;
                    }
                default:
                    {
                        throw new BaseException("errors.com.epicgames.common.oauth.unsupported_grant_type", $"Unsupported grant type: {form["grant_type"]}", 400, "");
                    }
            }

            if (accountData.BannedData.IsBanned)
                throw new InvalidCredentialsException();

            var token = TokensUtils.AccessTokens.FirstOrDefault(x => x.AccountId == accountData.AccountId);

            if (token != null)
                TokensUtils.AccessTokens.Remove(token);

            var refresh = TokensUtils.RefreshTokens.FirstOrDefault(x => x.AccountId == accountData.AccountId);

            if (refresh != null)
                TokensUtils.RefreshTokens.Remove(refresh);

            var deviceId = CreateUuid();
            var accessToken = TokenCreate.CreateAccess(accountData, clientId[0], form["grant_type"].ToString(), deviceId, 8);
            var refreshToken = TokenCreate.CreateRefresh(accountData, clientId[0], form["grant_type"].ToString(), deviceId, 24);

            //await Utils.UpdateTokens(); soon

            var decodedAccess = JsonConvert.DeserializeObject<AccessToken>(JWT.Decode(accessToken));
            var decodedRefresh = JsonConvert.DeserializeObject<RefreshToken>(JWT.Decode(refreshToken));

            return new OAuthResponse
            {
                AccessToken = $"eg1~{accessToken}",
                ExpiresAt = DateTime.Parse(decodedAccess!.CreationDate).AddHours(decodedAccess.HoursExpire),
                ExpiresIn = decodedAccess.HoursExpire,
                TokenType = "bearer",
                RefreshToken = $"eg1~{refreshToken}",
                RefreshExpires = decodedRefresh!.HoursExpire,
                RefreshExpiresAt = DateTime.Parse(decodedRefresh.CreationDate).AddHours(decodedRefresh.HoursExpire),
                AccountId = accountData.AccountId,
                ClientId = clientId[0],
                InternalClient = true,
                ClientService = "fortnite",
                DisplayName = accountData.Username,
                App = "fortnite",
                InAppId = accountData.AccountId,
                DeviceId = deviceId
            };
        }

        [HttpGet]
        [Route("verify")]
        public async Task<ActionResult<OAuthVerifiedResponse>> VerifyToken()
        {
            var token = Request.Headers.Authorization.ToString().Replace("bearer ", string.Empty);
            var decodedToken = JsonConvert.DeserializeObject<AccessToken>(JWT.Decode(token.Replace("eg1~", string.Empty)));

            return new OAuthVerifiedResponse
            {
                Token = token,
                SessionId = decodedToken.Jti,
                TokenType = "bearer",
                ClientId = decodedToken.Clid,
                InternalClient = true,
                ClientService = "fortnite",
                AccountId = decodedToken.Sub,
                ExpiresAt = DateTime.Parse(decodedToken.CreationDate).AddHours(decodedToken.HoursExpire),
                ExpiresIn = decodedToken.HoursExpire,
                AuthMethod = decodedToken.Am,
                DisplayName = decodedToken.Dn,
                App = "fortnite",
                InAppId = decodedToken.Sub
            }; //device id no there but yeah
        }

        [HttpDelete]
        [Route("sessions/kill")]
        public ActionResult KillSession()
        {
            return StatusCode(204);
        }

        [HttpDelete]
        [Route("sessions/kill/{token}")]
        public async Task<ActionResult> KillSession(string token)
        {
            var findToken = TokensUtils.AccessTokens.FirstOrDefault(x => x.AccessToken == token);

            if (findToken != null)
            {
                TokensUtils.AccessTokens.Remove(findToken);

                var findRefresh = TokensUtils.RefreshTokens.FirstOrDefault(x => x.AccountId == findToken.AccountId);

                if (findRefresh != null) 
                    TokensUtils.RefreshTokens.Remove(findRefresh);

                //await UpdateTokens();
            }

            return StatusCode(204);
        }
    }
}
