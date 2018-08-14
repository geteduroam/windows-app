using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamApp.Classes
{
    class AuthorizationUri
    {
        // Properties
        public string MainUri { get; set; }
        public string ResponseType { get; set; }
        public string CodeChallengeMethod { get; set; }
        public string Scope { get; set; }
        public string CodeChallenge { get; set; }
        public string RedirectUri { get; set; }
        public string ClientId { get; set; }
        public string State { get; set; }

        // Constructor
        public AuthorizationUri(string mainUri, string responseType, string codeChallengeMethod, string scope, string codeChallenge, string redirectUri, string clientId, string state)
        {
            MainUri = mainUri;
            ResponseType = responseType;
            CodeChallengeMethod = codeChallengeMethod;
            Scope = scope;
            CodeChallenge = codeChallenge;
            RedirectUri = redirectUri;
            ClientId = clientId;
            State = state;
        }

        public string CreateUri()
        {
            return
                MainUri + "?"
                        + "response_type=" + ResponseType
                        + "&code_challenge_method=" + CodeChallengeMethod
                        + "&scope=" + Scope
                        + "&code_challenge=" + CodeChallenge
                        + "&redirect_uri=" + RedirectUri
                        + "&client_id=" + ClientId
                        + "&state=" + State;
        }
    }
}
