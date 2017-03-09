using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Web.Services.Protocols;
using SportRadar.Common.Logs;
using WcfService;

namespace WsdlRepository
{
    public class ConsoleOutputMessageInspector : IClientMessageInspector
    {
        private static ILog Log = LogFactory.CreateLog(typeof(ConsoleOutputMessageInspector));

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            //throw new NotImplementedException();
            //request.Headers.Add(MessageHeader.CreateHeader("StationNumber", "", WcfService.WsdlRepository.Instance.Number)); 
            string requestString = request.ToString();
            if (requestString.Contains("<logZipData>"))
                return 0;

            if (requestString.Contains("OpenSession"))
            {
                Log.Debug("WSDL REQUEST OpenSession \r\n");
                return 0;
            }
            if (requestString.Contains("UpdateProfile"))
            {
                Log.Debug("WSDL REQUEST UpdateProfile \r\n");
                return 0;
            }
            if (requestString.Contains("LoginWithIdCard"))
            {
                Log.Debug("WSDL REQUEST LoginWithIdCard \r\n");
                return 0;
            }
            if (requestString.Contains("RegisterAccount"))
            {
                Log.Debug("WSDL REQUEST UpdateProfile \r\n");
                return 0;
            }

            Log.Debug("WSDL REQUEST \r\n" + requestString.Replace("\r\n", ""));
            return 0;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            //throw new NotImplementedException();
            String replyString = reply.ToString();
            if (replyString.Contains("UpdateStatisticsResult"))
            {
                Log.Debug("WSDL REPLY \r\n UpdateStatisticsResult");
                return;
            }
            if (replyString.Contains("AccountSearch"))
            {
                Log.Debug("WSDL REPLY \r\n UpdateStatisticsResult");
                return;
            }
            if (replyString.Contains("UpdateLineResponse"))
            {
                Log.Debug("WSDL REPLY \r\n UpdateLineResponse");
                return;
            }
            if (replyString.Contains("UpdateLocalizationResult"))
            {
                Log.Debug("WSDL REPLY \r\n UpdateLocalizationResult");
                return;
            }
            if (replyString.Contains("LoadProfileResponse"))
            {
                Log.Debug("WSDL REPLY \r\n LoadProfileResponse");
                return;
            }

            if (replyString.Contains("GetLiveStreamFeedResponse"))
            {
                Log.Debug("WSDL REPLY \r\n GetLiveStreamFeedResponse");
                return;
            }

            replyString = replyString.Replace("{", "{{");
            replyString = replyString.Replace("}", "}}");
            Log.Debug("WSDL REPLY \r\n " + replyString.Replace("\r\n", ""));

        }
    }
}
