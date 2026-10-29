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
			var log = NLog.LogManager.GetCurrentClassLogger();
			if (string.IsNullOrEmpty(userName))
			{
				log.Warn("Login rejected: no user name supplied");
				return false;
			}
			var expected = ConfigurationManager.AppSettings["PasswordHash:" + userName];
			if (expected == null)
			{
				log.Warn("Login rejected: no PasswordHash entry for user name [{0}] (length {1})", userName, userName.Length);
				return false;
			}
			if (!string.Equals(expected.Trim(), hash, StringComparison.OrdinalIgnoreCase))
			{
				log.Warn("Login rejected: wrong password for user name [{0}] (length {1})", userName, userName.Length);
				return false;
			}
			log.Info("Login accepted for user name [{0}]", userName);
			return true;
		}
	}
}
