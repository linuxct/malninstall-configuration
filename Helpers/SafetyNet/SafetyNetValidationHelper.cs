using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using space.linuxct.malninstall.Configuration.Exceptions;
using space.linuxct.malninstall.Configuration.Extensions;
using space.linuxct.malninstall.Configuration.Models.Persistence;
using space.linuxct.malninstall.Configuration.Models.SafetyNet;

namespace space.linuxct.malninstall.Configuration.Helpers.SafetyNet
{
    public static class SafetyNetValidationHelper
    {
        public static bool CheckSafetyNetResponseIsValid(string jwt, IDistributedCache cache, string connectionIdentifierHash)
        {
            var validationSucceeded = false;
            var safetyNetValidationResult = ParseAndVerify(jwt);
            var validationThroughSafetyNetResult = safetyNetValidationResult.BasicIntegrity && safetyNetValidationResult.CtsProfileMatch 
                && safetyNetValidationResult.ApkPackageName == "space.linuxct.malninstall";
            
            if (validationThroughSafetyNetResult)
            {
                var nonceResult = cache.GetObject<NonceResult>(safetyNetValidationResult.Nonce.ToSha256());
                validationSucceeded = nonceResult != null && nonceResult.ConnectionIdentifierHash == connectionIdentifierHash;
            }

            return validationSucceeded;
        }
        
        private static AttestationStatement ParseAndVerify(string signedAttestationStatement)
        {
            JwtSecurityToken token;
            try
            {
                token = new JwtSecurityToken(signedAttestationStatement);
            }
            catch (ArgumentException)
            {
                throw new InvalidSignatureException("Error converting to JWT");
            }
        
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = GetEmbeddedKeys(token)
            };
        
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken;
            try
            {
                tokenHandler.ValidateToken(signedAttestationStatement, validationParameters, out validatedToken);
            }
            catch (ArgumentException)
            {
                throw new InvalidSignatureException("Error validating JWT token");
            }
        
            if (!(validatedToken.SigningKey is X509SecurityKey))
            {
                throw new InvalidSignatureException("Error casting SigningKey to X509SecurityKey");
            }
            
            if (!VerifyHostname("attest.android.com", validatedToken.SigningKey as X509SecurityKey))
            {
                // The certificate isn't issued for the hostname
                // attest.android.com.
                throw new InvalidSignatureException("Hostname does not match attest.android.com");
            }
        
            var claimsDictionary = token.Claims.ToDictionary(x => x.Type, x => x.Value);
            return new AttestationStatement(claimsDictionary);
        }
        
        private static bool VerifyHostname(string hostname, X509SecurityKey securityKey)
        {
            string commonName;
            try
            {
                if (!securityKey.Certificate.Verify())
                {
                    return false;
                }
        
                commonName = securityKey.Certificate.GetNameInfo(
                    X509NameType.DnsName, false);
            }
            catch (CryptographicException)
            {
                return false;
            }
            return commonName == hostname;
        }

        private static IEnumerable<X509SecurityKey> GetEmbeddedKeys(JwtSecurityToken token)
        {
            var keys = ((JArray) token.Header["x5c"])
                .Values<string>()
                .Select(x => new X509SecurityKey(
                    new X509Certificate2(Convert.FromBase64String(x))))
                .ToArray();
            return keys;
        }
    }
}