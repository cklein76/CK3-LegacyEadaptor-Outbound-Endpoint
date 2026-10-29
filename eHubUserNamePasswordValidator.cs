using CargoWise.eHub.Common;
using System;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace CargoWise.eAdapterOutboundWebService
{
	public class eHubUserNamePasswordValidator : UserNamePasswordValidator
	{
		public override void Validate(string userName, string password)
		{
			if (!ValidatePassword(userName, SHA512Encryptor.Encrypt(userName + password))) throw new FaultException("ClientID or Password invalid.");
		}

		// Expects an appSettings entry "PasswordHash:<userName>" holding SHA512Encryptor.Encrypt(userName + password).
		// Use tools\New-PasswordHash.ps1 to generate the value.
		bool ValidatePassword(string userName, string hash)
		{
			if (string.IsNullOrEmpty(userName)) return false;
			var expected = ConfigurationManager.AppSettings["PasswordHash:" + userName];
			return expected != null && string.Equals(expected.Trim(), hash, StringComparison.OrdinalIgnoreCase);
		}
	}
}
