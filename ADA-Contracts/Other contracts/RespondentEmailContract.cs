using System;
using System.Collections.Generic;
using System.Text;

namespace ADA_Contracts.Other_contracts
{
    public record RespondentEmailContract(Guid RespondentId, string EmailAddress, string AccessToken);

}
