using System;
using System.Collections.Generic;
using System.Linq;
using Finance.PciDss.PciDssBridgeGrpc.Contracts;
using Finance.PciDss.PciDssBridgeGrpc.Models;

namespace Finance.PciDss.Bridge.Certus.Finance.Server.Services
{
    public static class RequestValidator
    {
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static ValidateResult Validate(this MakeBridgeDepositGrpcRequest request)
        {
            if (IsVerified(request.PciDssInvoiceGrpcModel.KycVerification))
            {
                return ValidateVirified(request.PciDssInvoiceGrpcModel);
            }

            return ValidateNonVirifiedWithUpdateFields(request.PciDssInvoiceGrpcModel);
        }

        private static bool IsVerified(string kycVerification)
        {
            // For all traders with kyc and deposit limit use EUR endpoint
            return (string.Equals(kycVerification, "Verified", StringComparison.OrdinalIgnoreCase));
        }

        private static ValidateResult ValidateVirified(PciDssInvoiceGrpcModel request)
        {
            var validateResult = new ValidateResult();
            if (request is null)
            {
                validateResult.Add($"{nameof(PciDssInvoiceGrpcModel)} is null");
                return validateResult;
            }

            if (string.IsNullOrEmpty(request.Address))
                validateResult.Add($"{nameof(request.Address)} is null or empty");
            if (string.IsNullOrEmpty(request.FullName))
                validateResult.Add($"{nameof(request.FullName)} is null or empty");
            if (string.IsNullOrEmpty(request.CardNumber))
                validateResult.Add($"{nameof(request.CardNumber)} is null or empty");
            if (string.IsNullOrEmpty(request.City)) validateResult.Add($"{nameof(request.City)} is null or empty");
            if (string.IsNullOrEmpty(request.PhoneNumber))
                validateResult.Add($"{nameof(request.PhoneNumber)} is null or empty");
            if (string.IsNullOrEmpty(request.Country))
                validateResult.Add($"{nameof(request.Country)} is null or empty");
            if (string.IsNullOrEmpty(request.Email)) validateResult.Add($"{nameof(request.Email)} is null or empty");
            if (string.IsNullOrEmpty(request.Cvv)) validateResult.Add($"{nameof(request.Cvv)} is null or empty");
            if (string.IsNullOrEmpty(request.Zip)) validateResult.Add($"{nameof(request.Zip)} is null or empty");

            return validateResult;
        }

        private static ValidateResult ValidateNonVirifiedWithUpdateFields(PciDssInvoiceGrpcModel request)
        {
            var validateResult = new ValidateResult();
            if (request is null)
            {
                validateResult.Add($"{nameof(PciDssInvoiceGrpcModel)} is null");
                return validateResult;
            }

            if (string.IsNullOrEmpty(request.FullName))
                validateResult.Add($"{nameof(request.FullName)} is null or empty");
            
            if (string.IsNullOrEmpty(request.CardNumber))
                validateResult.Add($"{nameof(request.CardNumber)} is null or empty");
            
            // Special card number validator for VISA            
            if (!string.IsNullOrEmpty(request.CardNumber) && 
                request.CardNumber.Length > 1 && 
                request.CardNumber[0] == '4')
            {
                validateResult.Add($"{nameof(request.CardNumber)} is VISA but not KYC approved");
            }

            if (string.IsNullOrEmpty(request.PhoneNumber))
                validateResult.Add($"{nameof(request.PhoneNumber)} is null or empty");
            if (string.IsNullOrEmpty(request.Country))
                validateResult.Add($"{nameof(request.Country)} is null or empty");
            if (string.IsNullOrEmpty(request.Email)) 
                validateResult.Add($"{nameof(request.Email)} is null or empty");
            if (string.IsNullOrEmpty(request.Cvv)) 
                validateResult.Add($"{nameof(request.Cvv)} is null or empty");

            if (string.IsNullOrEmpty(request.Address))
            {
                if (string.IsNullOrEmpty(request.Country))
                    validateResult.Add($"{nameof(request.Address)} is null or empty");
                request.Address = request.Country;
            }
            if (string.IsNullOrEmpty(request.City))
            {
                if (string.IsNullOrEmpty(request.Country))
                    validateResult.Add($"{nameof(request.City)} is null or empty");
                request.City = request.Country;
            }
            if (string.IsNullOrEmpty(request.Zip))
            {
                if (string.IsNullOrEmpty(request.Country))
                    validateResult.Add($"{nameof(request.Zip)} is null or empty");
                request.Zip = RandomString(6);
            }

            return validateResult;
        }
    }

    public sealed class ValidateResult
    {
        public IList<string> Errors { get; } = new List<string>();

        public bool IsSuccess => !Errors.Any();

        public bool IsFailed => !IsSuccess;

        public ValidateResult Add(string error)
        {
            Errors.Add(error);
            return this;
        }

        public override string ToString()
        {
            return $"ValidateResult: IsSuccess {IsSuccess}, Errors {string.Join(';', Errors)}";
        }
    }
}
