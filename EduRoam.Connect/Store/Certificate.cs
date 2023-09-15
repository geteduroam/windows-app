using System;
using System.Security.Cryptography.X509Certificates;

namespace EduRoam.Connect.Store
{
    public readonly struct Certificate
    {
        public StoreName StoreName { get; }
        public StoreLocation StoreLocation { get; }
        public string Thumbprint { get; }
        public string SerialNumber { get; }
        public string Subject { get; }
        public string Issuer { get; }
        public DateTime NotBefore { get; }
        public DateTime NotAfter { get; }

        public Certificate(
            StoreName storeName,
            StoreLocation storeLocation,
            string thumbprint,
            string serialNumber,
            string subject,
            string issuer,
            DateTime notBefore,
            DateTime notAfter)
        {
            this.StoreName = storeName;
            this.StoreLocation = storeLocation;
            this.Thumbprint = thumbprint;
            this.SerialNumber = serialNumber;
            this.Subject = subject;
            this.Issuer = issuer;
            this.NotBefore = notBefore;
            this.NotAfter = notAfter;
        }

        public static Certificate FromCertificate(X509Certificate2? cert, StoreName storeName, StoreLocation storeLocation)
            => cert == null
                ? throw new ArgumentNullException(paramName: nameof(cert))
                : new Certificate(
                    storeName: storeName,
                    storeLocation: storeLocation,
                    thumbprint: cert.Thumbprint,
                    serialNumber: cert.SerialNumber,
                    subject: cert.Subject,
                    issuer: cert.Issuer,
                    notBefore: cert.NotBefore,
                    notAfter: cert.NotAfter);
    }
}
