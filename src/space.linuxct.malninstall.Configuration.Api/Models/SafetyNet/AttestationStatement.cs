using System;
using System.Collections.Generic;
using System.Globalization;

namespace space.linuxct.malninstall.Configuration.Models.SafetyNet
{
    public class AttestationStatement
    {
        public Dictionary<string, string> Claims { get; private set; }

        public byte[] Nonce { get; private set; }

        public long TimestampMs { get; private set; }

        public string ApkPackageName { get; private set; }

        public byte[] ApkDigestSha256 { get; private set; }

        public byte[] ApkCertificateDigestSha256 { get; private set; }

        public bool CtsProfileMatch { get; private set; }

        public bool BasicIntegrity { get; private set; }
        
        public string EvaluationType { get; private set; }
        
        public bool HasBasicEvaluationType
        {
            get { return EvaluationType.Contains("BASIC"); }
        }
        
        public bool HasHardwareBackedEvaluationType
        {
            get { return EvaluationType.Contains("HARDWARE_BACKED"); }
        }
        
        public AttestationStatement(Dictionary<string, string> claims)
        {
            Claims = claims;

            if (claims.ContainsKey("nonce"))
            {
                Nonce = Convert.FromBase64String(claims["nonce"]);
            }

            if (claims.ContainsKey("timestampMs"))
            {
                long.TryParse(
                    claims["timestampMs"],
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var timestampMsLocal);
                TimestampMs = timestampMsLocal;
            }

            if (claims.ContainsKey("apkPackageName"))
            {
                ApkPackageName = claims["apkPackageName"];
            }

            if (claims.ContainsKey("apkDigestSha256"))
            {
                ApkDigestSha256 = Convert.FromBase64String(
                    claims["apkDigestSha256"]);
            }

            if (claims.ContainsKey("apkCertificateDigestSha256"))
            {
                ApkCertificateDigestSha256 = Convert.FromBase64String(
                    claims["apkCertificateDigestSha256"]);
            }

            if (claims.ContainsKey("ctsProfileMatch"))
            {
                bool.TryParse(
                    claims["ctsProfileMatch"],
                    out var ctsProfileMatchLocal);
                CtsProfileMatch = ctsProfileMatchLocal;
            }

            if (claims.ContainsKey("basicIntegrity"))
            {
                bool.TryParse(
                    claims["basicIntegrity"],
                    out var basicIntegrityLocal);
                BasicIntegrity = basicIntegrityLocal;
            }

            if (claims.ContainsKey("evaluationType"))
            {
                EvaluationType = claims["evaluationType"];
            }
        }
    }
}