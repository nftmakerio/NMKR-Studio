using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using NMKR.Shared.Classes.Blockfrost;

namespace NMKR.Shared.Blockchains
{
    public interface IBlockchainFunctions
    {
        public Task<ulong> GetWalletBalanceAsync(string walletAddress);
        public CreateNewPaymentAddressClass CreateNewWallet();

        public Task<BuildTransactionClass> MintAndSend(Nft nft, Nftproject project,
            string receiveraddress, BlockchainKeysClass paywallet, BuildTransactionClass buildtransaction);

        public Task<BuildTransactionClass> SendAllCoinsAndTokens(ulong utxo, CreateNewPaymentAddressClass wallet,
            string receiveraddress, BuildTransactionClass bt, string sendbackmessage);
        public Task<GenericTransaction> GetTransactionInformation(string txhash);
        public bool CheckForValidAddress(string address, bool mainnet);
        //public Task<ICreateCollectionResult> CreateCollection(ICreateCollection createCollection);

        public Task<string> CreateCollectionUri(CreateCollectionParameterClass createCollectionParameterClass);
        public Task<string> CreateCollectionAsync(CollectionClass collectionClass);
        public BlockchainKeysClass ConvertToBlockchainKeysClass(Nftproject project);
        public BlockchainKeysClass ConvertToBlockchainKeysClass(Burnigendpoint address);
        public BlockchainKeysClass ConvertToBlockchainKeysClass(Adminmintandsendaddress address);
        public Task<CollectionClass> CreateNewCollectionClass(Nftproject project, Adminmintandsendaddress paywallet);
        public Task<CollectionClass> CreateNewCollectionClass(Nftproject project);

        public Task<BuildTransactionClass> SendCoins(Nftaddress nftaddress,
            TxOutClass[] addresses,
            BuildTransactionClass bt);
        public Task<BuildTransactionClass> SendCoins(CreateNewPaymentAddressClass wallet,
            TxOutClass[] addresses,
            BuildTransactionClass bt);

        public Task<BuildTransactionClass> SendCoins(Adminmintandsendaddress wallet, TxOutClass[] addresses,
            BuildTransactionClass bt);

        public Task<AssetsAssociatedWithAccount[]> GetAllAssetsInWalletAsync(IConnectionMultiplexer redis,
            string address);

        public BlockfrostAssetClass GetAsset(Nftproject project, Nft nft1);
        public string GetMetadataFromCip25Metadata(string cip25metadata, Nftproject project);
        public Task<string> GetLastSenderAddressAsync(string addressOrHash);

        public Task<BuildTransactionClass> SendAllCoinsAndTokensFromNftaddress(Nftaddress address,
            string receiveraddress, BuildTransactionClass bt, string sendbackmessage);

        public Task<BuildTransactionClass> MintFromNftAddressCoreAsync(ulong amountOnAddress, Nftaddress address,
            string receiveraddress, Pricelistdiscount discount, Nftprojectsadditionalpayout[] additionalPayoutWallets,
            BuildTransactionClass bt);

        public Task<CheckCustomerAddressAddressesClass[]> GetCustomersAsync(EasynftprojectsContext db, CancellationToken cancellationToken);
        public string CheckPolicyId(string policyid);

        public Task<BuildTransactionClass> SendCoinsFromCustomerAddressAsync(Customer customer, TxOutClass[] addresses,
            BuildTransactionClass buildtransaction);

        public Task<TxInAddressesClass> GetUtxoAsync(string address);
        public Task<Nftaddress?> CreateAddress(IConnectionMultiplexer redis,
            ReserveAddressQueueClass adrreq,
            int? prepardpaymenttransactionid, EasynftprojectsContext db, Nftproject project, string reservationType,
            List<Nftreservation> selectedreservations, long sendback,
            Referer? referer);

    }
}
