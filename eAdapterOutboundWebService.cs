using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.Text;

namespace CargoWise.eAdapterOutboundWebService
{
	public class eAdapterOutboundWebService : IeAdapterStreamedService
	{
		public bool Ping()
		{
			return true;
		}

        public static string ScrubFileName(string value)
        {
            StringBuilder stringBuilder = new StringBuilder(value);
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            for (int i = 0; i < (int)invalidFileNameChars.Length; i++)
            {
                char chr = invalidFileNameChars[i];
                stringBuilder.Replace(chr.ToString(), "");
            }
            return stringBuilder.ToString();
        }

        public void SendStream(SendStreamRequest request)
		{
            NLog.LogManager.GetCurrentClassLogger().Info("SendStream start");

            string name = OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name;
            string str = string.Concat(AppDomain.CurrentDomain.BaseDirectory, "Files");
            this.Validate(str);

            if (request == null) NLog.LogManager.GetCurrentClassLogger().Info("request is null");
            NLog.LogManager.GetCurrentClassLogger().Info(request.ToString());
            if (request.Messages == null) NLog.LogManager.GetCurrentClassLogger().Info("messages is null");

            NLog.LogManager.GetCurrentClassLogger().Info(request.Messages.Length);
            eHubGatewayMessage[] messages = request.Messages;
            for (int i = 0; i < (int)messages.Length; i++)
            {
                eHubGatewayMessage _eHubGatewayMessage = messages[i];
                object[] messageTrackingID = new object[] { _eHubGatewayMessage.MessageTrackingID, name, _eHubGatewayMessage.ClientID, _eHubGatewayMessage.EmailSubject, _eHubGatewayMessage.FileName, _eHubGatewayMessage.ApplicationCode };
                string str1 = eAdapterOutboundWebService.ScrubFileName(string.Format("Message!{1}!{2}!{3}!{4}!{5}!{0}.xml", messageTrackingID));
                if (!string.IsNullOrEmpty(_eHubGatewayMessage.FileName))
                {
                    str1 = _eHubGatewayMessage.FileName;
                }
                FileStream fileStream = new FileStream(string.Concat(str, "\\", str1), FileMode.Create);
                try
                {
                    _eHubGatewayMessage.MessageStream.DecodeAndDecompress().WriteTo(fileStream);
                }
                finally
                {
                    if (fileStream != null)
                    {
                        ((IDisposable)fileStream).Dispose();
                    }
                }
            }
            NLog.LogManager.GetCurrentClassLogger().Info("SendStream complete");
            //string name = OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name;
            //string item = ConfigurationManager.AppSettings["LogRequired"];
            //string str = ConfigurationManager.AppSettings["OutputFolder"];
            //if (item.ToLower() == "true")
            //{
            //    this.WriteLog("Logging b4 validation");
            //}
            //this.Validate(str);
            //eHubGatewayMessage[] messages = request.Messages;
            //for (int i = 0; i < (int)messages.Length; i++)
            //{
            //    eHubGatewayMessage _eHubGatewayMessage = messages[i];
            //    string empty = string.Empty;
            //    object[] messageTrackingID = new object[] { _eHubGatewayMessage.MessageTrackingID, name, _eHubGatewayMessage.ClientID, _eHubGatewayMessage.EmailSubject, _eHubGatewayMessage.FileName, _eHubGatewayMessage.ApplicationCode };
            //    string str2 = eAdapterOutboundWebService.ScrubFileName(string.Format("Message!{1}!{2}!{3}!{4}!{5}!{0}.xml", messageTrackingID));
            //    empty = ((_eHubGatewayMessage.FileName == string.Empty ? false : !(_eHubGatewayMessage.FileName == "")) ? _eHubGatewayMessage.FileName : str2);
            //    string str1 = Path.Combine(str, empty);
            //    if (item.ToLower() == "true")
            //    {
            //        this.WriteLog(str1);
            //    }
            //    FileStream fileStream = new FileStream(str1, FileMode.Create);
            //    try
            //    {
            //        _eHubGatewayMessage.MessageStream.DecodeAndDecompress().WriteTo(fileStream);
            //    }
            //    finally
            //    {
            //        if (fileStream != null)
            //        {
            //            ((IDisposable)fileStream).Dispose();
            //        }
            //    }
            //}
        }

        public void Validate(string destinationFolder)
		{
			if ((destinationFolder == "")) throw new FaultException("No destination folder for outbound messages has been specified. Please modify the eAdapterOutboundWebService.cs file located within the eAdapter solution and specify a destination folder for outbound messages.");
		}

        public void WriteLog(string logValueString)
        {
            StreamWriter streamWriter = new StreamWriter(ConfigurationManager.AppSettings["LogFile"], true);
            try
            {
                streamWriter.WriteLine(logValueString);
            }
            finally
            {
                if (streamWriter != null)
                {
                    ((IDisposable)streamWriter).Dispose();
                }
            }
        }
    }
}
