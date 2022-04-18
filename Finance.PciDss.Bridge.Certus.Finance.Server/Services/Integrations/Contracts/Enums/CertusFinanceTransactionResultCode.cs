using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finance.PciDss.Bridge.Certus.Finance.Server.Services.Integrations.Contracts.Enums
{
    public enum CertusFinanceTransactionResultCode
    {
        InProcess = -4,        // Transaction is in process
        Redirected3Ds = -3,    // Transaction 3d auth response processing.
                               // Redirect to acquirer for response.
        Enrolled3Ds = -2,      // Transaction is 3DS enrolled OR checking for 3DS enrollment.
                               // Redirect to card issuer for 3DS authentication.
        Process = -1,          // Transaction is in process.
        Failed = 0,            // Transaction has failed.
        CompletedSuccessfully = 1,    // Transaction has completed successfully.
        Queued = 2,            // Transaction was successfully received and is now queued
                               // for transmission to the provider
        CreatedSuccessfully = 3, // Transaction created Successfully.
        Cancelled = 4,         // Transaction was cancelled.
        Expired = 5,           // Transaction is expired. So its failed.
        Incomplete = 6,        // Transaction is Incomplete
        PartiallyCompleted = 9 // Transaction is partially completed
    }

}
