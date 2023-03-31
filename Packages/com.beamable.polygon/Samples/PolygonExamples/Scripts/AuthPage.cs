using System;
using System.Linq;
using System.Text;
using Beamable;
using Beamable.Api.Auth;
using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Player;
using Beamable.Polygon.Common;
using Beamable.Server.Clients;
using Thirdweb;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PolygonExamples.Scripts
{
    /// <summary>
    /// A script that presents how to perform basic operations like connecting to a wallet, attach or detach external identity
    /// or sign a message with connected wallet
    /// </summary>
    public class AuthPage : TabPage
    {
        [SerializeField] private Button _connectWalletButton;
        [SerializeField] private Button _attachIdentityButton;
        [SerializeField] private Button _detachIdentityButton;
        [SerializeField] private Button _getExternalIdentitiesButton;
        [SerializeField] private Button _walletExplorerButton;

        [SerializeField] private TextMeshProUGUI _beamId;
        [SerializeField] private TextMeshProUGUI _walletId;

        private IAuthService _authService;
        private ThirdwebSDK _sdk;

        private async void Start()
        {
            _connectWalletButton.onClick.AddListener(OnConnectClicked);
            _attachIdentityButton.onClick.AddListener(OnAttachClicked);
            _detachIdentityButton.onClick.AddListener(OnDetachClicked);
            _walletExplorerButton.onClick.AddListener(OnWalletExplorerClicked);

            _getExternalIdentitiesButton.onClick.AddListener(OnGetExternalClicked);

            _authService = Ctx.Api.AuthService;

            await BeamContext.Default.OnReady;
            await BeamContext.Default.Accounts.OnReady;

            var external = BeamContext.Default.Accounts.Current.ExternalIdentities.FirstOrDefault(ext =>
                ext.providerNamespace == Data.Federation.Namespace
                && ext.providerService == Data.Federation.Service);
            Data.WalletId = external?.userId ?? null;
            _beamId.text = $"<b>Beam ID</b> {Ctx.Accounts.Current.GamerTag.ToString()}";

            _sdk = new ThirdwebSDK("polygon");

            OnRefresh();
        }

        private async void OnConnectClicked()
        {
            Data.Working = true;

            Data.WalletId = await _sdk
                .wallet
                .Connect(new WalletConnection
                {
                    provider = WalletProvider.MetaMask,
                    chainId = (int)Chain.Polygon
                });

            Data.Working = false;
        }

        public override void OnRefresh()
        {
            _connectWalletButton.interactable = !Data.Working && !Data.WalletConnected;
            _attachIdentityButton.interactable = !Data.Working && Data.WalletConnected && !Data.WalletAttached;
            _detachIdentityButton.interactable = !Data.Working && Data.WalletConnected && Data.WalletAttached;
            _getExternalIdentitiesButton.interactable = !Data.Working;
            _walletExplorerButton.interactable = Data.WalletConnected;

            UpdateWalletIdText();
        }

        void UpdateWalletIdText()
        {
            _walletId.text = Data.WalletConnected
                ? $"<b>Wallet Id</b> {Data.WalletId}"
                : String.Empty;
        }

        private void OnWalletExplorerClicked()
        {
            var address = $"https://mumbai.polygonscan.com/address/{Data.WalletId}";
            Application.OpenURL(address);
        }

        private async void OnAttachClicked()
        {
            Data.Working = true;
            OnLog("Attaching wallet...");
            await SendAttachRequest();
            Data.WalletAttached = CheckIfWalletHasAttachedIdentity();
            Data.Working = false;

            async Promise SendAttachRequest(ChallengeSolution challengeSolution = null)
            {
                StringBuilder builder = new();
                builder.AppendLine("Sending a request with:");
                builder.AppendLine($"Provider service: {Data.Federation.Service}");
                if (challengeSolution != null)
                {
                    builder.AppendLine($"Signed solution: {challengeSolution.solution}");
                }

                OnLog(builder.ToString());

                if (Data.WalletConnected)
                {
                    RegistrationResult result =
                        await Ctx.Accounts.AddExternalIdentity<PolygonCloudIdentity, PolygonFederationClient>(
                            Data.WalletId,
                            SolveChallenge);

                    OnLog(result.isSuccess
                        ? $"Succesfully attached an external identity... publicKey=[{Data.WalletId}]"
                        : result.innerException.Message);
                }
                else
                {
                    OnLog("No wallet connected...");
                }
            }
        }

        /// <summary>
        /// Method that shows a way to solve a challenge received from a server. It needs to be done to proof that we
        /// are true owners of a wallet. After sending it back to a server it verifies it an decides wheter solution was
        /// correct or not. Challenge token we are receiving from server is a three-part, dot separated string and has
        /// following format: {challenge}.{validUntilEpoch}.{signature} where:
        ///		{challenge}			- Base64 encoded string
        ///		{validUntilEpoch}	- valid until epoch time in milliseconds, Int64 value
        ///		{signature}			- Base64 encoded token signature
        /// </summary>
        /// <param name="challengeToken"></param>
        /// <returns></returns>
        private async Promise<string> SolveChallenge(string challengeToken)
        {
            OnLog($"Signing a challenge: {challengeToken}");

            // Parsing received challenge token to a 3 part struct
            ChallengeToken parsedToken = _authService.ParseChallengeToken(challengeToken);
            
            // Signing a challenge with a connected wallet
            string signedSignature = await _sdk.wallet.Sign(parsedToken.challenge);
            
            return signedSignature;
        }

        private async void OnDetachClicked()
        {
            Data.Working = true;
            OnLog("Detaching wallet...");
            await Ctx.Accounts.RemoveExternalIdentity<PolygonCloudIdentity, PolygonFederationClient>();

            if (!CheckIfWalletHasAttachedIdentity())
            {
                OnLog("Succesfully detached an external identity...");
            }

            Data.Working = false;
        }

        /// <summary>
        /// Method that renders currently connected to account external identities where Service is a microservice responsible
        /// for handling custom server side logic, Namespace shows which namespace will be handled (namespaces can be implemented
        /// by deriving IThirdPartyCloudIdentity interface and Public Key is a wallet address that has been connected to an
        /// account
        /// </summary>
        private void OnGetExternalClicked()
        {
            OnLog("Getting external identities info...");
            if (Ctx.Accounts.Current == null) return;

            if (Ctx.Accounts.Current.ExternalIdentities.Length != 0)
            {
                StringBuilder builder = new();
                foreach (ExternalIdentity identity in Ctx.Accounts.Current.ExternalIdentities)
                {
                    builder.AppendLine(
                        $"Service: {identity.providerService}, namespace: {identity.providerNamespace}, public key: {identity.userId}");
                }

                OnLog(builder.ToString());
            }
            else
            {
                OnLog("No external identities found...");
            }
        }

        private bool CheckIfWalletHasAttachedIdentity()
        {
            if (Ctx.Accounts.Current == null)
                return false;

            if (Ctx.Accounts.Current.ExternalIdentities.Length == 0)
                return false;

            ExternalIdentity externalIdentity = Ctx.Accounts.Current.ExternalIdentities.FirstOrDefault(i =>
                i.providerNamespace == Data.Federation.Namespace &&
                i.providerService == Data.Federation.Service &&
                i.userId == Data.WalletId);

            return externalIdentity != null;
        }
    }
}