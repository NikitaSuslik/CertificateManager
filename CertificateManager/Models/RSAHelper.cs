using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CertificateCreator.Models
{
    

    class RSAHelper
    {
        public class RSAProperty
        {

            public RSAProperty(Algorithm algorithm, long keySize, string commonName, string country, string organisation, string organisationnUnit)
            {
                RSAAlgorithm = algorithm;
                KeySize = keySize;
                CN = commonName;
                C = country;
                O = organisation;
                OU = organisationnUnit;
            }

            public enum Algorithm
            {
                MD5,
                SHA1,
                SHA256,
                SHA512
            }

            public Algorithm RSAAlgorithm
            {
                get;
                private set;
            }

            public long KeySize
            {
                get;
                private set;
            }

            public string CN
            {
                get;
                private set;
            }
            public string C
            {
                get;
                private set;
            }
            public string O
            {
                get;
                private set;
            }
            public string OU
            {
                get;
                private set;
            }

        }

        static public string[] CreateCACert(RSAProperty prop)
        {
            var RSAKey = RSA.Create((int)prop.KeySize);
            string param = $"CN={prop.CN}/O={prop.O}/OU={prop.OU}/C={prop.C}";
            HashAlgorithmName alg;
            switch (prop.RSAAlgorithm)
            {
                case RSAProperty.Algorithm.MD5:
                    alg = HashAlgorithmName.MD5;
                    break;
                case RSAProperty.Algorithm.SHA1:
                    alg = HashAlgorithmName.SHA1;
                    break;
                case RSAProperty.Algorithm.SHA256:
                    alg = HashAlgorithmName.SHA256;
                    break;
                case RSAProperty.Algorithm.SHA512:
                    alg = HashAlgorithmName.SHA512;
                    break;
                default:
                    throw new Exception("Unknown algorithm type: " + prop.RSAAlgorithm);
            }

            var certReq = new CertificateRequest(param, RSAKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            certReq.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            certReq.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(certReq.PublicKey, true));
            var exp = DateTime.Now.AddYears(10);
            var cacert = certReq.CreateSelfSigned(DateTime.Now, exp);

            string certBase64 = Convert.ToBase64String(cacert.RawData, Base64FormattingOptions.InsertLineBreaks);
            var keyCert = cacert.GetRSAPrivateKey();
            string keyBase64 = Convert.ToBase64String(keyCert.ExportRSAPrivateKey(), Base64FormattingOptions.InsertLineBreaks);
            string keySignature = keyCert.SignatureAlgorithm;

            return new string[] { certBase64, keyBase64, keySignature};
        }
    }
}
