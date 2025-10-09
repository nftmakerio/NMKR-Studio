using System;
using System.Collections.Generic;
using NMKR.Shared.Classes.Bitcoin;
using NMKR.Shared.Classes.Solana;
using NMKR.Shared.Model;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Models;
using Solnet.Wallet;

namespace NMKR.Shared.Classes
{

    public class TxOutTokenClass
    {
        public int NftId { get; set; }
    }

    public class MintCoreNftsResult
    {
        public int NftId { get; set; }
        public string MintAddress { get; set; }
        public string TxHash { get; set; }
        public string MintTxHash { get; set; }

    }
    public class TxOutClass
    {
        public string ReceiverAddress { get; set; }
        public long Amount { get; set; }
        public int Percentage { get; set; }
        public TxInTokensClass[] Tokens { get; set; }
        public string SendbackMessage { get; set; }
    }
    public class BuildTransactionClass
    {
        public long? smartcontractmemvalue;
        public long? smartcontracttimevalue;
        public string TxHash { get; set; }
        public string SenderAddress { get; set; }
        public long Fees { get; set; } = 0;
        public string Command { get; set; } = "";
        public TxOutClass BuyerTxOut { get; set; }
        public TxOutClass ProjectTxOut { get; set; }
        public TxOutClass MintingcostsTxOut { get; set; }
        public TxOutClass Cip68ReferenceTokenTxOut { get; set; }
        public string SubmissionResult { get; set; }
        public string SignedTransaction { get; set; }
        public string LogFile { get; set; }
        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                Log(_errorMessage);
            }
        }
        public long? StakeRewards { get; set; }
        public long? Discount { get; set; }
        public long RequiredMinUtxo { get; set; }
        public string Metadata { get; set; }
        public int TxInCount { get; set; }
        public int TxOutCount { get; set; }
        public int WitnessCount { get; set; }


        public long? TokenRewards { get; set; }
        public Nftprojectsadditionalpayout[] AdditionalPayouts { get; set; }
        public long NmkrCosts { get; set; }
        public TxInTokensClass? PriceInTokens { get; set; }
        public TxInClass[] LockTxIn { get; set; }
        public RequestResult<TransactionMetaSlotInfo> SolanaTransactionMetaSlotInfo { get; set; }
        public TransactionBuilder SolanaTransaction { get; set; }
        public ulong NewCalculatedLamports { get; internal set; }
        public SolanaVerifiyCollectionResultClass SolanaVerifyCollectionResult { get; set; }
        public MintSolanaNftClass SolanaMintInfo { get; set; }

        // kann weg dann
        public PublicKey TokenSource { get; set; }
        public PublicKey TokenDestination { get; set; }
        public Account ProjectAccount { get; set; }
        public PublicKey MasterEditionKey { get; set; }
        public string MetadataStandard { get; set; }
        public List<MintCoreNftsResult> MintAssetAddress { get; set; } = new List<MintCoreNftsResult>();
        public string LastTransaction { get; set; }
        public BitcoinInscribeResult BitcoinInscribeResult { get; set; }
        public BitcoinOrderStatusResult BitcoinOrderstatusResult { get; set; }

        public BuildTransactionClass(string firstlogentry="")
        {
            LogFile = "";
            if (!string.IsNullOrEmpty(firstlogentry))
                LogFile += firstlogentry + Environment.NewLine;
        }

        public void Log(string logentry)
        {
            LogFile += logentry + Environment.NewLine;
        }

    }
}
